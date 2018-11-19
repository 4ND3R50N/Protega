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
    public class _ProtocolController
    {
        List<networkServer.networkClientInterface> ActiveConnections;
        int ApplicationID;
        char ProtocolDelimiter;
        Thread LoginManager, LogoutManager, RuntimeManager;
        Queue<Classes.Protocol.InterfaceRunTimeTasks> RuntimeQueue;
        Queue<Classes.Protocol.InterfaceLoginLogout> LoginQueue;
        Queue<Classes.Protocol.InterfaceLoginLogout> LogoutQueue;

        public _ProtocolController(char ProtocolDelimiter, ref List<networkServer.networkClientInterface> ActiveConnections, int _ApplicationID)
        {
            this.ProtocolDelimiter = ProtocolDelimiter;
            this.ActiveConnections = ActiveConnections;
            ApplicationID = _ApplicationID;
            RuntimeQueue = new Queue<InterfaceRunTimeTasks>();
            LoginQueue = new Queue<InterfaceLoginLogout>();
            LogoutQueue = new Queue<InterfaceLoginLogout>();

            LoginManager = new Thread(LoginManagement);
            LogoutManager = new Thread(LogoutManagement);
            RuntimeManager = new Thread(RunTimeManagement);

            LoginManager.Start();
            LogoutManager.Start();
            RuntimeManager.Start();
        }

        public delegate void SendProt(string Protocol, networkServer.networkClientInterface ClientInterface);
        public static event SendProt SendProtocol = null;

        public bool ReceivedProtocol(networkServer.networkClientInterface NetworkClient, string protocolString, DateTime TimestampStart)
        {
            Protocol protocol = new Protocol(protocolString, ProtocolDelimiter, TimestampStart);
            if (!protocol.Split())
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, "Received invalid protocol synthax: " + protocolString);
                return false;
            }

            switch (protocol.GetKey())
            {
                case 600:
                    return ResetPing(ref NetworkClient, protocol);
                case 500:
                    AuthenticateUser(NetworkClient, protocol);
                    return true;
                case 701:
                    return HackDetection_Heuristic(ref NetworkClient, protocol);
                case 702:
                    return HackDetection_VirtualMemory(NetworkClient, protocol);
                case 703:
                    return HackDetection_File(NetworkClient, protocol);
                case 666:
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, "Critical Error a173, contact Ferity!");
                    Environment.Exit(1);
                    return true;
                default:
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, "Received invalid protocol: " + protocolString);
                    return false;
            }

        }

        public bool CheckIfUserExists(string SessionID, ref networkServer.networkClientInterface ClientInterface, bool ReplaceSocket=true)
        {
            //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
            for (int i = 0; i < ActiveConnections.Count; i++)
            {
                if (ActiveConnections[i].SessionID == SessionID)
                {
                    if (ReplaceSocket)
                    {
                        try
                        {
                            if (ActiveConnections[i].networkSocket != null)
                            {
                                ActiveConnections[i].networkSocket.Close();
                                ActiveConnections[i].networkSocket.Dispose();
                            }
                        }
                        catch (Exception e)
                        {
                            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.ERROR, LoggerType.SERVER, "CheckIfUserExists Socket replace failed! Error " + e.Message);
                        }
                    }
                    //if (ClientInterface.networkSocket == null || !ClientInterface.networkSocket.Connected)
                    //    CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.ERROR, LoggerType.SERVER, String.Format("CheckIfUserExists New Socket null or not connected. Null {0}", (ClientInterface.networkSocket == null ? "true" : "false")));

                    ActiveConnections[i].networkSocket = ClientInterface.networkSocket;
                    ClientInterface = ActiveConnections[i];
                    return true;
                }
            }
            return false;
        }

        #region Thread Protocol managing
        object SyncRunTime = new object();
        object SyncLogin = new object();
        object SyncLogout = new object();

        void LoginManagement()
        {
            while (true)
            {
                while (LoginQueue.Count > 0)
                {
                    
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, String.Format("New Login Task. Open: {0}", LoginQueue.Count));
                    int Index = LoginQueue.Count - 1;
                    InterfaceLoginLogout Task;
                    lock (SyncLogin)
                    {
                        //Task = LoginQueue[Index];
                        //LoginQueue.Remove(Task);
                        Task = LoginQueue.Dequeue();
                    }
                    if (Task is pAuthentication)
                        TaskAuthenticateUser(Task as pAuthentication);
                    else if (Task is pDisconnection)
                        TaskKickUser(Task as pDisconnection);
                    else
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, "Impossible Login-Queue item found!");
                }
                Thread.Sleep(1000);
            }
        }

        void LogoutManagement()
        {
            while (true)
            {
                while (LogoutQueue.Count > 0)
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, String.Format("New Logout Task. Open: {0}", LogoutQueue.Count));
                    InterfaceLoginLogout Task;
                    lock (SyncLogout)
                    {
                        Task = LogoutQueue.Dequeue();
                    }
                    if (Task is pDisconnection)
                        TaskKickUser(Task as pDisconnection);
                    else
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, "Impossible Logout-Queue item found!");
                }
                Thread.Sleep(1000);
            }
        }
        
        void RunTimeManagement()
        {
            while (true)
            {
                while (RuntimeQueue.Count > 0)
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, String.Format("New Runtime Task. Open: {0}", RuntimeQueue.Count));
                    InterfaceRunTimeTasks Task;
                    lock (SyncRunTime)
                    {
                        Task = RuntimeQueue.Dequeue();
                    }
                    if (Task is pPing)
                        TaskResetPing(Task as pPing);
                    else if (Task is pHackDetectionHeuristic)
                        TaskHackDetection_Heuristic(Task as pHackDetectionHeuristic);
                    else
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, "Impossible Runtime-Queue item found!");
                }
                Thread.Sleep(200);
            }
        }
        #endregion

        #region Protocol Packing
        private bool AuthenticateUser(networkServer.networkClientInterface ClientInterface, Protocol prot)
        {
            ClientInterface.CheckIP(ApplicationID);
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Received new Authentication. User ({0}), IP: {1}", prot.GetUserID(), ClientInterface.IP.ToString()));
            pAuthentication AuthProt = new pAuthentication(ref ClientInterface, prot);

            //Add the Auth protocol into the queue which is checked by a separate thread
            lock (SyncLogin)
            {
                LoginQueue.Enqueue(AuthProt);
            }
            return true;
        }

        public void KickUser(networkServer.networkClientInterface ClientInterface)
        {
            CCstData.GetInstance(ClientInterface.User.Application.ID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Disconnection triggered!. {0} ({1} - {2})", ClientInterface.User.ID, ClientInterface.SessionID, ClientInterface.IP));
            pDisconnection DisconnectionTask = new pDisconnection(ref ClientInterface);
            lock (SyncLogout)
            {
                LogoutQueue.Enqueue(DisconnectionTask);
            }
            return;
        }

        private bool ResetPing(ref networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Ping: Protocol received. User: " + prot.GetUserID());
            pPing ping = new pPing(ref Client, prot);
            //lock (SyncRunTime)
            //{
            //    RuntimeQueue.Enqueue(ping);
            //}
            TaskResetPing(ping);
            return true;
        }

        private bool HackDetection_Heuristic(ref networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Heuristic-Detection received. User: " + prot.GetUserID());
            pHackDetectionHeuristic detection = new pHackDetectionHeuristic(ref Client, prot);
            TaskHackDetection_Heuristic(detection);
            //RuntimeQueue.Enqueue(detection);
            return true;
        }
        #endregion

        #region Protocol proceeding functions
        bool TaskAuthenticateUser(pAuthentication prot)
        {
            try
            {
                prot.Initialize();
            }
            catch (Exception e)
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Error initializing authentification protocol! Error: {0}, Protocol {1}", e.Message, prot.prot.GetOriginalString()));
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
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.CLIENT, String.Format("Invalid version! Having {0}, expected {1}. Hardware ID {2}", prot.version, CCstData.GetInstance(ApplicationID).LatestClientVersion, prot.HardwareID));
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

            int Counter = 0;
            EPlayer dataClient;
            do
            {
                dataClient = SPlayer.Authenticate(prot.HardwareID, prot.ApplicationHash, prot.architecture, prot.language, prot.Client.IP.ToString());

            } while (dataClient == null && Counter++ < 3);


            CCstData.GetInstance(ApplicationID).Logger.writeInLog(5, LogCategory.OK, Support.LoggerType.DATABASE, "Authentification: User found!");

            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, "Authentification: DB error or two users with the same Hardware ID exist!");
                SendProtocol(String.Format("201{0}3{0}Contact Admin", ProtocolDelimiter), prot.Client);
                return false;
            }
            dataClient.Application.Hash = prot.ApplicationHash;
            dataClient.GameIP = (prot.IPtoGame.ToString() == "127.0.0.1" ? prot.Client.IP : prot.IPtoGame);
            if (prot.IPtoGame.ToString() == "127.0.0.1")
            {
                //The Client sends 127.0.0.1 if the IP cannot be fetched
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, LoggerType.SERVER, String.Format("Auth: Could not get GameIP. User {0}, IP {1}, GameIP {2}", prot.Client.User.ID, prot.Client.IP.ToString(), prot.IPtoGame.ToString()));
                dataClient.GameIP = prot.Client.IP;
            }
            else
            {
                dataClient.GameIP = prot.IPtoGame;
                if (prot.Client.IP.ToString() != prot.IPtoGame.ToString())
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, LoggerType.SERVER, String.Format("Auth: Different IPs. User {0}, IP {1}, GameIP {2}", prot.Client.User.ID, prot.Client.IP.ToString(), prot.IPtoGame.ToString()));
            }


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
            //prot.Client.SetPingTimer(CCstData.GetInstance(dataClient.Application.ID).PingTimer, KickUser);

            bool IpExistsAlready = false;
            foreach (var Client in ActiveConnections)
            {
                if (Client.User.GameIP == prot.Client.User.GameIP)
                    IpExistsAlready = true;
            }


            //Linux takes ages to connect. Therefore contact the client before it sends another request

            //ActiveConnections.Add(prot.Client);
            //SendProtocol(String.Format("200{0}{1}", ProtocolDelimiter, prot.Client.SessionID), prot.Client);
            if (!IpExistsAlready)
            {
                if (!CCstData.GetInstance(ApplicationID).GameDLL.AllowUser(prot.Client.User.GameIP, prot.Client.SessionID, prot.TimeStampStart()))
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


            prot.Client.Initialize(CCstData.GetInstance(dataClient.Application.ID).PingTimer, KickUser);

            ActiveConnections.Add(prot.Client);

            SendProtocol(String.Format("200{0}{1}", ProtocolDelimiter, prot.Client.SessionID), prot.Client);
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authenticated new user {0} ({1} - {2}). {3}", prot.Client.User.ID, prot.Client.SessionID, prot.Client.IP.ToString(), prot.TimePassed()));
            return true;
        }

        void TaskKickUser (pDisconnection DisconnectionTask)
        {
            CCstData.GetInstance(DisconnectionTask.Client.User.Application.ID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Kick triggered for user {0} ({1} - {2})", DisconnectionTask.Client.User.ID, DisconnectionTask.Client.SessionID, DisconnectionTask.Client.IP));
            if (DisconnectionTask.Client.KickTriggered)
            {
                CCstData.GetInstance(DisconnectionTask.Client.User.Application.ID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Kick triggered multiple times!. {0} ({1} - {2})", DisconnectionTask.Client.User.ID, DisconnectionTask.Client.SessionID, DisconnectionTask.Client.IP));
            }
            DisconnectionTask.Client.KickTriggered = true;

            if (!CheckIfUserExists(DisconnectionTask.Client.SessionID, ref DisconnectionTask.Client, false))
            {
                CCstData.GetInstance(DisconnectionTask.Client.User.Application.ID).Logger.writeInLog(2, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Kick triggered for not existing user: {0} ({1} - {2})", DisconnectionTask.Client.User.ID, DisconnectionTask.Client.SessionID, DisconnectionTask.Client.IP));
            }
            
            DisconnectionTask.Client.ResetPingTimer();
            //System.Threading.Thread.Sleep(1000);
            
            bool IpExistsAlready = false;
            foreach (var item in ActiveConnections)
            {
                if (item.IP == DisconnectionTask.Client.IP)
                    if (item.SessionID != DisconnectionTask.Client.SessionID)
                        IpExistsAlready = true;
            }

            if (!IpExistsAlready)
            {
                if (!CCstData.GetInstance(ApplicationID).GameDLL.KickUser(DisconnectionTask.Client.User.GameIP, DisconnectionTask.Client.SessionID, DisconnectionTask.TimeStampStart()))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, String.Format("Linux exception removal failed. IP {0}, User: {1}", DisconnectionTask.Client.IP, DisconnectionTask.Client.User.ID));
                }
            }

            DisconnectionTask.Client.Dispose();
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("User disconnected. User {0} ({1} - {2}). {3}", DisconnectionTask.Client.User.ID, DisconnectionTask.Client.SessionID, DisconnectionTask.Client.IP, DisconnectionTask.TimePassed()));
        }
        
        private bool TaskResetPing(pPing ping)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Ping proceeding. User: " + ping.Session);

            //networkServer.networkClientInterface ClientInterface = Client;
            if (CheckIfUserExists(ping.Session, ref ping.Client))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(5, LogCategory.OK, Support.LoggerType.SERVER, "Ping: User found in the list.");

                if (ping.Initialize())
                {
                    //Ping has additional tasks. Do something

                }

                //Reset the Ping timer
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "Ping: Resetting timer.");

                ping.Client.ResetPingTimer();

                //zhCCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, "Additional Infos: "+AdditionalInfo);
                if (ping.AdditionalMessage==null)
                    SendProtocol("300", ping.Client);
                else
                    SendProtocol(String.Format("301{0}{1}", ProtocolDelimiter, ping.AdditionalMessage), ping.Client);

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Ping resetted User {0} ({1} - {2}). {3}", ping.Client.User.ID, ping.Client.SessionID, ping.Client.IP, ping.TimePassed()));
                return true;
            }

            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.ERROR, Support.LoggerType.SERVER, String.Format("Ping: User does not exist in the active connections ({0} {1})", ping.Session, ping.TimePassed()));
            try
            {
                ping.Client.Dispose();
            }
            catch (Exception e)
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.CLIENT, String.Format("Ping: Could not dispose not found client! Error {0}", e.Message));
            }
            return false;
        }

        #region Hack Detections
        private bool TaskHackDetection_Heuristic(pHackDetectionHeuristic pdata)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "Heuristic-Detection proceeding. User: " + pdata.prot.GetUserID());
            if (CheckIfUserExists(pdata.prot.UserID, ref pdata.Client))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, "H-Detection: User found in the active connections");
                int ErrorCode=0;
                if (!pdata.Initialize(out ErrorCode))
                {
                    //Error in received protocol
                    switch (ErrorCode)
                    {
                        case 1:
                            CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("H-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", pdata.prot.GetValues().Count, pdata.prot.GetOriginalString()));
                            SendProtocol(String.Format("401{0}5", ProtocolDelimiter), pdata.Client);
                            KickUser(pdata.Client);
                            return false;
                        case 2:
                            CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("H-Detection: SectionID was no int! Protocol: {0}", pdata.prot.GetOriginalString()));
                            SendProtocol(String.Format("401{0}13", ProtocolDelimiter), pdata.Client);
                            KickUser(pdata.Client);
                            return false;
                        default:
                            return false;
                    }
                }

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, "H-Detection: User: " + pdata.Client.User.ID + ", Session: " + pdata.Client.SessionID + " - Saved protocol values: ProcessName: " + pdata.hackData.ProcessName + ", WindowName: " + pdata.hackData.WindowName + ", ClassName: " + pdata.hackData.ClassName + ", MD5Value: " + pdata.hackData.MD5Value);

                int Counter = 0;
                while (!SHackHeuristic.Insert(pdata.hackData))
                {
                    Counter++;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, String.Format("H-Detection: Insertion in database failed! Attempt: {0}, Protocol: {1}", Counter, pdata.prot.GetOriginalString()));
                    if (Counter > 3)
                    {
                        SendProtocol(String.Format("401{0}6", ProtocolDelimiter), pdata.Client);
                        KickUser(pdata.Client);
                        return false;
                    }
                }

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.DATABASE, "H-Detection: Database interaction successful");
                SendProtocol(String.Format("400{0}8", ProtocolDelimiter), pdata.Client);

                KickUser(pdata.Client);
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.SERVER, "H-Detection: User not found in active connections!");
            SendProtocol(String.Format("401{0}7{0}UID: {1}", ProtocolDelimiter, pdata.Client.SessionID), pdata.Client);
            KickUser(pdata.Client);
            return false;
        }
        #endregion
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
        
        #region Hack Detections
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
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.SERVER, String.Format("V-Detection: User not found in active connections! User {0} ({1}), prot {2}", prot.UserID, Client.IP, prot.GetOriginalString()));
            SendProtocol(String.Format("401{0}12", ProtocolDelimiter), ClientInterface);
            KickUser(ClientInterface);
            return false;
        }
        #endregion
        
        #region Callbacks
        public void ReceiveGameDLLCallback(string Username, System.Net.IPAddress IP, Classes.Utility.ApplicationAdapter.Task task, Classes.Utility.ApplicationAdapter.Result Result,DateTime TimeStampStart)
        {
            if (task == Utility.ApplicationAdapter.Task.InsertConnection)
            {
                networkServer.networkClientInterface Client = new networkServer.networkClientInterface();
                if (Result == Utility.ApplicationAdapter.Result.SUCCESS)
                {
                    if (CheckIfUserExists(Username, ref Client, false))
                    {
                        //Authentication success and user exists
                        Client.LoginCompleted = true;
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, LoggerType.SERVER, String.Format("Linux exception successful: {0} ({1} - {2}) {3}", Client.User.ID, Client.SessionID, Client.IP, Classes.AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now)));
                    }
                    else
                    {
                        //Authentication success but user does not exist
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, LoggerType.SERVER, String.Format("Linux exception successful but user not found: {0} ({1}) {2}", Username, IP, Classes.AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now)));
                        CCstData.GetInstance(ApplicationID).GameDLL.KickUser(IP, Username,DateTime.Now);
                        Client.Dispose();
                    }
                }
                else
                {
                    if (CheckIfUserExists(Username, ref Client, false))
                    {
                        //Authentication failed but user exists
                        //Send protocol to kick user since IPException was not successful
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, LoggerType.SERVER, String.Format("Linux exception failed: {0} ({1} - {2}) {3}", Client.User.ID, Client.SessionID, Client.IP, Classes.AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now)));
                        KickUser(Client);
                    }
                    else
                    {
                        //Authentication failed and user does not exist
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, LoggerType.SERVER, String.Format("Linux exception failed and user not found: {0} ({1}) {2}", Username, IP, Classes.AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now)));
                    }
                }
            }
            else if (task == Utility.ApplicationAdapter.Task.RemoveConnection)
            {
                networkServer.networkClientInterface Client = new networkServer.networkClientInterface();
                if (Result == Utility.ApplicationAdapter.Result.SUCCESS)
                {
                    if (CheckIfUserExists(Username, ref Client, false))
                    {
                        //Kick succeeded and user exists
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, LoggerType.SERVER, String.Format("Linux exception removal success: {0} ({1} - {2}) {3}", Client.User.ID, Client.SessionID, Client.IP, Classes.AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now)));


                        if (!RemoveNetworkinterfaceBySession(Client.SessionID))
                        {
                            CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Disconnection: User not found. User {0} ({1} - {2})", Client.User.ID, Client.SessionID, Client.IP));

                            string ErrorMessageDetail = String.Format("Searched User: {0}, {1}, {2}\n", Client.SessionID, Client.User.ID, Client.IP);
                            for (int i = 0; i < ActiveConnections.Count; i++)
                            {
                                ErrorMessageDetail += String.Format("User{0}: {1}, {2}, {3}. KickTrigger: {4}, LastPing: {5} ({6}), ", i, ActiveConnections[i].SessionID, ActiveConnections[i].User.ID, ActiveConnections[i].IP, ActiveConnections[i].KickTriggered.ToString(), ActiveConnections[i]._LastPing, (DateTime.Now - ActiveConnections[i]._LastPing).TotalSeconds);
                            }
                            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.CRITICAL, Support.LoggerType.SERVER, ErrorMessageDetail);
                        }
                    }
                    else
                    {
                        //Kick succeeded but user does not exist
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, LoggerType.SERVER, String.Format("Linux exception removal success but user not found: {0} ({1}) {2}", Username, IP, Classes.AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now)));
                        Client.Dispose();
                    }
                }
                else
                {
                    if (CheckIfUserExists(Username, ref Client, false))
                    {
                        //Kick failed but user exists - retry
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, LoggerType.SERVER, String.Format("Linux exception removal failed: {0} ({1} - {2}) {3}", Client.User.ID, Client.SessionID, Client.IP, Classes.AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now)));
                        Client.KickTriggered = false;
                        KickUser(Client);
                    }
                    else
                    {
                        //Kick failed and user does not exist
                        CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, LoggerType.SERVER, String.Format("Linux exception removal success but user not found: {0} ({1}) {2}", Username, IP, Classes.AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now)));
                        Client.Dispose();
                    }
                }
            }
        }


        #endregion
    }
}
