using Protega___Server.Classes.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protega___Server.Classes.Entity;
using Support;
using System.Threading;
using Renci.SshNet;

namespace Protega___Server.Classes.Protocol
{
    public class ProtocolController
    {
        List<networkServer.networkClientInterface> ActiveConnections;
        int ApplicationID;
        char ProtocolDelimiter;
        Thread AuthManager, RuntimeManager;
        //Queue<Classes.Protocol.> RuntimeQueue;
        Queue<Classes.Protocol.pLoginLogout> AuthQueue;

        public ProtocolController(char ProtocolDelimiter, ref List<networkServer.networkClientInterface> ActiveConnections, int _ApplicationID)
        {
            this.ProtocolDelimiter = ProtocolDelimiter;
            this.ActiveConnections = ActiveConnections;
            ApplicationID = _ApplicationID;
            //RuntimeQueue = new Queue<pRuneTimeTasks>();
            AuthQueue = new Queue<Classes.Protocol.pLoginLogout>();

            AuthManager = new Thread(LoginLogoutManagement);
            AuthManager.Start();
        }
        
        public delegate void SendProt(string Protocol, networkServer.networkClientInterface ClientInterface);
        public static event SendProt SendProtocol = null;

        public bool ReceivedProtocol(networkServer.networkClientInterface NetworkClient, string protocolString)
        {
            Protocol protocol = new Protocol(protocolString, ProtocolDelimiter);
            if (!protocol.Split())
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, "Received invalid protocol synthax: " + protocolString);
                return false;
            }

            switch (protocol.GetKey())
            {
                case 600:
                    return CheckPing(ref NetworkClient, protocol); 
                case 500:
                    AuthenticateUser(NetworkClient, protocol);
                    return true;
                case 701:
                    return HackDetection_Heuristic(NetworkClient, protocol); 
                case 702:
                    return HackDetection_VirtualMemory(NetworkClient, protocol);
                case 703:
                    return HackDetection_File(NetworkClient, protocol);
                default:
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, "Received invalid protocol: " + protocolString);
                    return false; 
            }
            
        }
        
        public bool CheckIfUserExists(string SessionID, ref networkServer.networkClientInterface ClientInterface)
        {
            //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
            for (int i = 0; i < ActiveConnections.Count; i++)
            {
                if (ActiveConnections[i].SessionID == SessionID)
                {
                    ActiveConnections[i].networkSocket.Close();
                    ActiveConnections[i].networkSocket.Dispose();
                    ActiveConnections[i].networkSocket = ClientInterface.networkSocket;
                    ClientInterface = ActiveConnections[i];
                    return true;
                }
            }
            return false;
        }
        
        #region Thread Protocol managing
        void LoginLogoutManagement()
        {
            while (true)
            {
                while (AuthQueue.Count > 0)
                {
                    pLoginLogout Task = AuthQueue.Dequeue();
                    if (Task is pAuthentication)
                        TaskAuthenticateUser(Task as pAuthentication);
                    else if (Task is pDisconnection)
                        TaskKickUser(Task as pDisconnection);
                    else
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Impossible LoginLogout-Queue item found!"));
                }
                Thread.Sleep(1000);
            }
        }

        void RunTimeManagement()
        {

        }
        #endregion

        #region Protocol Packing
        private bool AuthenticateUser(networkServer.networkClientInterface ClientInterface, Protocol prot)
        {
            ClientInterface.CheckIP(ApplicationID);
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Received new Authentication. User ({0}), IP: {1}", prot.GetUserID(), ClientInterface.IP.ToString()));
            pAuthentication AuthProt = new pAuthentication(ref ClientInterface, prot);

            //Add the Auth protocol into the queue which is checked by a separate thread
            AuthQueue.Enqueue(AuthProt);
            return true;
        }

        void KickUser(networkServer.networkClientInterface ClientInterface)
        {
            CCstData.GetInstance(ClientInterface.User.Application.ID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Disconnection triggered!. {0} ({1} - {2})", ClientInterface.User.ID, ClientInterface.SessionID, ClientInterface.IP));
            pDisconnection DisconnectionTask = new pDisconnection(ref ClientInterface);
            AuthQueue.Enqueue(DisconnectionTask);
            return;
        }
        #endregion

        bool AddUserToActiveConnections(ref networkServer.networkClientInterface ClientInterface, string ApplicationHash, string ComputerID, string architecture, String language, double version)
        {
            if (!CCstData.InstanceExists(ApplicationHash))
            {
                //Instance does not exist. The player must have manipulated the protocol!
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Invalid application hash received in authentification protocol! ComputerID: {0}, ApplicationHash: {1}", ComputerID, ApplicationHash));
                return false;
            }

            if (CCstData.GetInstance(ApplicationHash).LatestClientVersion != version)
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.ERROR, Support.LoggerType.CLIENT, String.Format("Invalid version! Having {0}, expected {1}. Hardware ID {2}", version, CCstData.GetInstance(ApplicationHash).LatestClientVersion, ComputerID));
                SendProtocol(String.Format("201{0}35{0}Antihack Client version outdated!", ProtocolDelimiter), ClientInterface);
                return false;
            }

            ////Check if user is already connected
            //foreach (networkServer.networkClientInterface item in ActiveConnections)
            //{
            //    if(item.User.ID==ComputerID
            //        && item.User.Application.Hash==ApplicationHash)
            //    {
            //        //User is already registered
            //        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.CLIENT, "Authentification: User is already added to list!");
            //        SendProtocol("201;2;Still logged in. Please try again", ClientInterface);
            //        return false;
            //    }
            //}

            CCstData.GetInstance(ApplicationID).Logger.writeInLog(5, LogCategory.OK, Support.LoggerType.DATABASE, "Authentification: Checking user in the database");
            ClientInterface.CheckIP(ApplicationID);
            EPlayer dataClient = SPlayer.Authenticate(ComputerID, ApplicationHash, architecture, language, ClientInterface.IP.ToString());
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(5, LogCategory.OK, Support.LoggerType.DATABASE, "Authentification: User found!");

            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, "Authentification: Hardware ID exists multiple times in the database");
                SendProtocol(String.Format("201{0}3{0}Contact Admin", ProtocolDelimiter), ClientInterface);
                return false;
            }
            dataClient.Application.Hash = ApplicationHash;

            //Check if user is banned
            if (dataClient.isBanned == true)
            {
                //Do something and dont let him enter
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authentification: Banned user tried to authentificate. User: {0}", dataClient.ID));
                //Send protocol to client that user is banned
                SendProtocol(String.Format("201{0}4{0}Too many hacks", ProtocolDelimiter), ClientInterface);
                return false;
            }

            //Add EPlayer to ClientInterface and to the list
            ClientInterface.User = dataClient;

            //Generate unique Session ID for network communication
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Authentification: Start creating a unique session ID");
            while (true)
            {
                string SessionID = AdditionalFunctions.GenerateSessionID(CCstData.GetInstance(ApplicationHash).SessionIDLength);
                //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
                if (ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList().Count == 0)
                {
                    ClientInterface.SessionID = SessionID;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, String.Format("New user authentificated! HardwareID: {0}, Session ID: {1}", dataClient.ID, SessionID));
                    break;
                }
            }

            //Add the new connection to the list of connected connections
            ClientInterface.SetPingTimer(CCstData.GetInstance(dataClient.Application.ID).PingTimer, KickUser);

            bool IpExistsAlready = false;
            foreach (var Client in ActiveConnections)
            {
                if (Client.IP == ClientInterface.IP)
                    IpExistsAlready = true;
            }


            //Linux takes ages to connect. Therefore contact the client before it sends another request

            if (!IpExistsAlready)
            {
                if (!CCstData.GetInstance(ApplicationID).GameDLL.AllowUser(ClientInterface.IP, ClientInterface.User.ID))
                {
                    //Do something and dont let him enter
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, String.Format("Linux exception failed. User: {0}", dataClient.ID));
                    //Send protocol to client that user is banned
                    SendProtocol(String.Format("201{0}30{0}Access verification failed", ProtocolDelimiter), ClientInterface);
                    return false;
                }
            }
            else
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authentication: IP already exists ({0})", ClientInterface.IP.ToString()));

            ActiveConnections.Add(ClientInterface);

            SendProtocol(String.Format("200{0}{1}", ProtocolDelimiter, ClientInterface.SessionID), ClientInterface);
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authenticated new user. Computer ID: {0}, Session ID: {1}, IP: {2}", ClientInterface.User.ID, ClientInterface.SessionID, ClientInterface.IP.ToString()));

            /*if (!IpExistsAlready)
            {
                //If there is already an IP exception, we dont need another
                try
                {
                    ClientInterface.unixSshConnectorAccept.Connect();
                }
                catch (Exception)
                {
                    
                }
                if (ClientInterface.unixSshConnectorAccept.IsConnected)
                {
                    List<int> Ports = new List<int>();
                    Ports.Add(50001);
                    Ports.Add(50002);
                    Ports.Add(50003);
                    Ports.Add(50004);
                    Ports.Add(50005);
                    Ports.Add(50006);
                    Ports.Add(50007);
                    Ports.Add(50008);
                    Ports.Add(50009);
                    Ports.Add(50010);
                    Ports.Add(50011);
                    Ports.Add(50012);
                    Ports.Add(50013);
                    Ports.Add(50014);
                    Ports.Add(50015);
                    Ports.Add(50016);
                    Ports.Add(50017);
                    Ports.Add(50018);
                    Ports.Add(50019);
                    Ports.Add(50020);
                    string LinuxPorts = "";
                    foreach (int item in Ports)
                    {
                        LinuxPorts += "iptables -I INPUT -p tcp -s " + ClientInterface.IP + " --dport " + item + " -j ACCEPT && ";
                    }
                    if(LinuxPorts.Length > 0)
                    {
                        LinuxPorts = LinuxPorts.TrimEnd(' ');
                        LinuxPorts = LinuxPorts.TrimEnd('&');
                        using (SshCommand Result = ClientInterface.unixSshConnectorAccept.RunCommand(LinuxPorts))
                        {
                            if (Result.Error.Length > 0)
                                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, "Linux exception failed! Session ID: " + ClientInterface.SessionID + ", Error: " + Result.Error);
                            else
                                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.GAMEDLL, "Linux exception successful. Session ID: " + ClientInterface.SessionID + ", Result: " + Result.Result);
                        }
                    }

                    ClientInterface.unixSshConnectorAccept.Disconnect();
                }
                else
                {
                    //Fehlerinfo
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.CLIENT, "Client could not be connected to the Linux Server. Session ID: " + ClientInterface.SessionID);
                    return false;
                }
            }
            else
            {
                string AllIPs = "";
                foreach (var item in ActiveConnections)
                {
                    AllIPs += String.Format(" User: {0}, IP: {1} -", item.User.ID, item.IP);
                }
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authentication: IP already exists ({0})", AllIPs));
            }*/

            return true;
        }


        #region Protocol proceeding functions
        bool TaskAuthenticateUser(pAuthentication prot)
        {
            if(!prot.Initialize())
            {
                //Input parameters had an error
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Invalid application hash received in authentification protocol! ComputerID: {0}, ApplicationHash: {1}", prot.HardwareID, prot.ApplicationHash));
                return false;
            }

            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.DATABASE, String.Format("Authentificating user {0} ({1})", prot.HardwareID, prot.Client.IP));

            if (!CCstData.InstanceExists(prot.ApplicationHash))
            {
                //Instance does not exist. The player must have manipulated the protocol!
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Invalid application hash received in authentification protocol! ComputerID: {0}, ApplicationHash: {1}", prot.HardwareID, prot.ApplicationHash));
                return false;
            }

            if (CCstData.GetInstance(prot.ApplicationHash).LatestClientVersion != prot.version)
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.ERROR, Support.LoggerType.CLIENT, String.Format("Invalid version! Having {0}, expected {1}. Hardware ID {2}", prot.version, CCstData.GetInstance(ApplicationID).LatestClientVersion, prot.HardwareID));
                SendProtocol(String.Format("201{0}35{0}Antihack Client version outdated!", ProtocolDelimiter), prot.Client);
                return false;
            }

            ////Check if user is already connected
            //foreach (networkServer.networkClientInterface item in ActiveConnections)
            //{
            //    if(item.User.ID==ComputerID
            //        && item.User.Application.Hash==ApplicationHash)
            //    {
            //        //User is already registered
            //        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.CLIENT, "Authentification: User is already added to list!");
            //        SendProtocol("201;2;Still logged in. Please try again", ClientInterface);
            //        return false;
            //    }
            //}

            CCstData.GetInstance(ApplicationID).Logger.writeInLog(5, LogCategory.OK, Support.LoggerType.DATABASE, "Authentification: Checking user in the database");
            EPlayer dataClient = SPlayer.Authenticate(prot.HardwareID, prot.ApplicationHash, prot.architecture, prot.language, prot.Client.IP.ToString());
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(5, LogCategory.OK, Support.LoggerType.DATABASE, "Authentification: User found!");

            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, "Authentification: Hardware ID exists multiple times in the database");
                SendProtocol(String.Format("201{0}3{0}Contact Admin", ProtocolDelimiter), prot.Client);
                return false;
            }
            dataClient.Application.Hash = prot.ApplicationHash;

            //Check if user is banned
            if (dataClient.isBanned == true)
            {
                //Do something and dont let him enter
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authentification: Banned user tried to authentificate. User: {0}", dataClient.ID));
                //Send protocol to client that user is banned
                SendProtocol(String.Format("201{0}4{0}Too many hacks", ProtocolDelimiter), prot.Client);
                return false;
            }

            //Add EPlayer to ClientInterface and to the list
            prot.Client.User = dataClient;

            //Generate unique Session ID for network communication
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Authentification: Start creating a unique session ID");
            while (true)
            {
                string SessionID = AdditionalFunctions.GenerateSessionID(CCstData.GetInstance(prot.ApplicationHash).SessionIDLength);
                //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
                if (ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList().Count == 0)
                {
                    prot.Client.SessionID = SessionID;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, String.Format("New user authentificated! HardwareID: {0}, Session ID: {1}", dataClient.ID, SessionID));
                    break;
                }
            }

            //Add the new connection to the list of connected connections
            prot.Client.SetPingTimer(CCstData.GetInstance(dataClient.Application.ID).PingTimer, KickUser);

            bool IpExistsAlready = false;
            foreach (var Client in ActiveConnections)
            {
                if (Client.IP == prot.Client.IP)
                    IpExistsAlready = true;
            }


            //Linux takes ages to connect. Therefore contact the client before it sends another request

            if (!IpExistsAlready)
            {
                if (!CCstData.GetInstance(ApplicationID).GameDLL.AllowUser(prot.Client.IP, prot.Client.User.ID))
                {
                    //Do something and dont let him enter
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, String.Format("Linux exception failed. User: {0}", dataClient.ID));
                    //Send protocol to client that user is banned
                    SendProtocol(String.Format("201{0}30{0}Access verification failed", ProtocolDelimiter), prot.Client);
                    return false;
                }
            }
            else
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authentication: IP already exists ({0})", prot.Client.IP.ToString()));

            ActiveConnections.Add(prot.Client);

            SendProtocol(String.Format("200{0}{1}", ProtocolDelimiter, prot.Client.SessionID), prot.Client);
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authenticated new user {0} ({1} - {2}). Time: {3} secs", prot.Client.User.ID, prot.Client.SessionID, prot.Client.IP.ToString(), prot.TimePassedSecs()));
            return true;
        }

        void TaskKickUser (pDisconnection DisconnectionTask)
        {
            CCstData.GetInstance(DisconnectionTask.Client.User.Application.ID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Kick triggered for user {0} ({1} - {2})", DisconnectionTask.Client.User.ID, DisconnectionTask.Client.SessionID, DisconnectionTask.Client.IP));
            if (DisconnectionTask.Client.KickTriggered)
            {
                CCstData.GetInstance(DisconnectionTask.Client.User.Application.ID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Kick triggered multiple times!. {0} ({1} - {2})", DisconnectionTask.Client.User.ID, DisconnectionTask.Client.SessionID, DisconnectionTask.Client.IP));
                return;
            }

            int Index=-1;
            for (int i = 0; i < ActiveConnections.Count; i++)
            {
                if(ActiveConnections[i].SessionID==DisconnectionTask.Client.SessionID)
                {
                    ActiveConnections[i].KickTriggered = true;
                    Index = i;
                }
            }
            if (Index == -1)
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Kick triggered for not existing user: {0} ({1} - {2}", DisconnectionTask.Client.User.ID, DisconnectionTask.Client.SessionID, DisconnectionTask.Client.IP));
            
            DisconnectionTask.Client.ResetPingTimer();
            //System.Threading.Thread.Sleep(1000);
            
            bool IpExistsAlready = false;
            foreach (var item in ActiveConnections)
            {
                if (item.IP == DisconnectionTask.Client.IP)
                    if (item.SessionID != DisconnectionTask.Client.SessionID)
                        IpExistsAlready = true;
            }


            if (!RemoveNetworkinterfaceBySession(DisconnectionTask.Client.SessionID))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Disconnection: User not found. User {0} ({1} - {2})", DisconnectionTask.Client.User.ID, DisconnectionTask.Client.SessionID, DisconnectionTask.Client.IP));

                string ErrorMessageDetail = String.Format("Searched User: {0}, {1}, {2}\n", DisconnectionTask.Client.SessionID, DisconnectionTask.Client.User.ID, DisconnectionTask.Client.IP);
                for (int i = 0; i < ActiveConnections.Count; i++)
                {
                    ErrorMessageDetail += String.Format("User{0}: {1}, {2}, {3}. KickTrigger: {4}, LastPing: {5} ({6}), ", i, ActiveConnections[i].SessionID, ActiveConnections[i].User.ID, ActiveConnections[i].IP, ActiveConnections[i].KickTriggered.ToString(), ActiveConnections[i]._LastPing, (DateTime.Now - ActiveConnections[i]._LastPing).TotalSeconds);
                }
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.CRITICAL, Support.LoggerType.SERVER, ErrorMessageDetail);
            }

            if (!IpExistsAlready)
            {
                if(!CCstData.GetInstance(ApplicationID).GameDLL.KickUser(DisconnectionTask.Client.IP,DisconnectionTask.Client.User.ID))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, String.Format("Linux exception removal failed. IP {0}, User: {1}", DisconnectionTask.Client.IP, DisconnectionTask.Client.User.ID));
                }
            }
            
            DisconnectionTask.Client.Dispose();
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("User disconnected. User {0} ({1} - {2}). Time {3} secs", DisconnectionTask.Client.User.ID, DisconnectionTask.Client.SessionID, DisconnectionTask.Client.IP, DisconnectionTask.TimePassedSecs()));
        }
        
        bool RemoveNetworkinterfaceBySession(string Session)
        {
            foreach (networkServer.networkClientInterface item in ActiveConnections)
            {
                if (item.SessionID == Session)
                {
                    ActiveConnections.Remove(item);
                    return true;
                }
            }
            return false;
        }


        #endregion

        private bool CheckPing(ref networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Ping: Protocol received. User: " + prot.GetUserID());

            //networkServer.networkClientInterface ClientInterface = Client;
            if (CheckIfUserExists(prot.UserID, ref Client))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Ping: User found in the list.");

                int AdditionalInfos = prot.HasValues() ? Convert.ToInt32(prot.GetValues()[0]) : -1;
                string AdditionalInfo = "";
                switch (AdditionalInfos)
                {
                    case 1:
                        AdditionalInfo = ";123";
                        break;
                    default:
                        break;
                }

                //Reset the Ping timer

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Ping: Resetting timer.");
                Client.ResetPingTimer();
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Ping resetted.");

                //zhCCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, "Additional Infos: "+AdditionalInfo);
                if (AdditionalInfo.Length == 0)
                    SendProtocol("300", Client);
                else
                    SendProtocol(String.Format("301{0}{1}", ProtocolDelimiter, AdditionalInfo), Client);

                return true;
            }

            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.CLIENT, String.Format("Ping: User does not exist in the active connections ({0})", prot.GetUserID()));
            try
            {
                Client.Dispose();
            }
            catch (Exception)
            {

            }
            return false;
        }


        #region Hack Detections
        private bool HackDetection_Heuristic(networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Heuristic-Detection received. User: " + prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = Client;
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "H-Detection: User found in the active connections");
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 2)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("H-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol(String.Format("401{0}5",ProtocolDelimiter), ClientInterface);
                    KickUser(ClientInterface);
                    return false;
                }

                //The section ID defines which hack detection method triggered
                int SectionID;
                if (!Int32.TryParse(Objects[0].ToString(), out SectionID))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("H-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol(String.Format("401{0}13",ProtocolDelimiter), ClientInterface);
                    KickUser(ClientInterface);
                }

                string ProcessName = null;
                string WindowName = null;
                string ClassName = null;
                string MD5Value = null;

                switch (SectionID)
                {
                    case 1:
                        ProcessName = Convert.ToString(Objects[1]);
                        break;
                    case 2:
                        MD5Value = Convert.ToString(Objects[1]);
                        break;
                    default:
                        break;
                }

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, "H-Detection: User: "+ClientInterface.User.ID+", Session: "+ClientInterface.SessionID+" - Saved protocol values: ProcessName: " + ProcessName + ", WindowName: " + WindowName + ", ClassName: " + ClassName + ", MD5Value: " + MD5Value);

                int Counter = 0;
                while (!SHackHeuristic.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, ProcessName, WindowName, ClassName, MD5Value))
                {
                    Counter++;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, String.Format("H-Detection: Insertion in database failed! Attempt: {0}, Protocol: {1}", Counter, prot.GetOriginalString()));
                    if (Counter > 3)
                    {
                        SendProtocol(String.Format("401{0}6",ProtocolDelimiter), ClientInterface);
                        KickUser(ClientInterface);
                        return false;
                    }
                }

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.DATABASE, "H-Detection: Database interaction successful");
                SendProtocol(String.Format("400{0}8",ProtocolDelimiter), ClientInterface);

                KickUser(ClientInterface);
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.SERVER, "H-Detection: User not found in active connections!");
            SendProtocol(String.Format("401{0}7{0}UID: {1}",ProtocolDelimiter, ClientInterface.SessionID), ClientInterface);
            KickUser(ClientInterface);
            return false;
        }


        private bool HackDetection_File(networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "File-Detection received. User: " + prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = Client;
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "F-Detection: User found in the active connections");
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 2)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("F-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol(String.Format("401{0}5",ProtocolDelimiter), ClientInterface);
                    KickUser(ClientInterface);
                    return false;
                }

                //The section ID defines which hack detection method triggered
                int SectionID;
                if (!Int32.TryParse(Objects[0].ToString(), out SectionID))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("F-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol(String.Format("401{0}16",ProtocolDelimiter), ClientInterface);
                    KickUser(ClientInterface);
                }

                string Content =  Convert.ToString(Objects[1]);

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, "F-Detection: User: " + ClientInterface.User.ID + ", Session: " + ClientInterface.SessionID + " -Saved protocol values: CaseID: " + SectionID + ", Content: " + Content);

                int Counter = 0;
                while (!SHackFile.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, SectionID, Content))
                {
                    Counter++;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, String.Format("F-Detection: Insertion in database failed! Attempt: {0}, Protocol: {1}", Counter, prot.GetOriginalString()));
                    if (Counter > 3)
                    {
                        SendProtocol(String.Format("401{0}15", ProtocolDelimiter), ClientInterface);
                        KickUser(ClientInterface);
                        return false;
                    }
                }

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.DATABASE, "F-Detection: Database interaction successful");
                SendProtocol(String.Format("401{0}14", ProtocolDelimiter), ClientInterface);

                KickUser(ClientInterface);
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.SERVER, "F-Detection: User not found in active connections!");
            SendProtocol(String.Format("401{0}17", ProtocolDelimiter), ClientInterface);
            KickUser(ClientInterface);
            return false;
        }

        private bool HackDetection_VirtualMemory(networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Virtual-Detection received. User: "+prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = Client;
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 4)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("V-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol(String.Format("401{0}9", ProtocolDelimiter), ClientInterface);
                    KickUser(ClientInterface);
                    return false;
                }

                string BaseAddress = Convert.ToString(Objects[0]);
                string Offset = Convert.ToString(Objects[1]);
                string DetectedValue = Convert.ToString(Objects[2]);
                string DefaultValue = Convert.ToString(Objects[3]);

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, "V-Detection: User: " + ClientInterface.User.ID + ", Session: " + ClientInterface.SessionID + " -Saved protocol successfully. Values: BaseAddress: " + BaseAddress + ", Offset: " + Offset + ", DetectedValue: " + DetectedValue + ", DefaultValue: " + DefaultValue);
                
                int Counter = 0;
                while (!SHackVirtual.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, BaseAddress, Offset, DetectedValue, DefaultValue))
                {
                    Counter++;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, String.Format("V-Detection: Insertion in database failed! Protocol: {0}", prot.GetOriginalString()));
                    if (Counter > 3)
                    {
                        SendProtocol(String.Format("401{0}10", ProtocolDelimiter), ClientInterface);
                        KickUser(ClientInterface);
                        return false;
                    }
                }
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "V-Detection: Database interaction successful");

                SendProtocol(String.Format("401{0}11", ProtocolDelimiter), ClientInterface);
                KickUser(ClientInterface);
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.SERVER, "V-Detection: User not found in active connections!");
            SendProtocol(String.Format("401{0}12", ProtocolDelimiter), ClientInterface);
            KickUser(ClientInterface);
            return false;
        }
        #endregion
    }
}
