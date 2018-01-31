using Protega___Server.Classes.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protega___Server.Classes.Entity;
using Support;
using Renci.SshNet;

namespace Protega___Server.Classes.Protocol
{
    public class ProtocolController
    {
        List<networkServer.networkClientInterface> ActiveConnections;
        int ApplicationID;

        public ProtocolController(ref List<networkServer.networkClientInterface> ActiveConnections, int _ApplicationID)
        {
            this.ActiveConnections = ActiveConnections;
            ApplicationID = _ApplicationID;
        }
        
        public delegate void SendProt(string Protocol, networkServer.networkClientInterface ClientInterface);
        public static event SendProt SendProtocol = null;

        public bool ReceivedProtocol(networkServer.networkClientInterface NetworkClient, string protocolString)
        {
            Protocol protocol = new Protocol(protocolString);
            switch (protocol.GetKey())
            {
                case 600:
                    return CheckPing(NetworkClient, protocol); 
                case 500:
                    return AuthenticateUser(NetworkClient, protocol); 
                case 701:
                    return HackDetection_Heuristic(protocol); 
                case 702:
                    return HackDetection_VirtualMemory(protocol); 
                default:
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, "Received invalid protocol: " + protocolString);
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
                    ActiveConnections[i].networkSocket = ClientInterface.networkSocket;
                    ClientInterface = ActiveConnections[i];
                    return true;
                }
            }
            return false;
        }
        

        #region Authenticate User
        private bool AuthenticateUser(networkServer.networkClientInterface ClientInterface, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, String.Format("Authenticating new user ({0})", prot.GetUserID()));
            ArrayList Objects = prot.GetValues();
            if(Objects.Count!=4)
            {
                //Log error - protocol size not as expected
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, String.Format("Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                return false;
            }

            //Computer ID, Computer Architecture, Language, Version
            string ApplicationHash = Objects[1].ToString();
            string architecture = Objects[2].ToString();
            string language = Objects[3].ToString();            
            double version;
            if (!Double.TryParse(Objects[0].ToString(), out version))
            {
                //Log error - protocol index 3 is not as expected
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, "Double expected but received " + Objects[3].ToString());
                return false;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, String.Format("Authentification protocol correct. ApplicationHash={0}, Architecture={1}, Language={2}, Version={3}", ApplicationHash, architecture, language, version));

            //Check if user exists and add it to the list
            return AddUserToActiveConnections(ClientInterface, ApplicationHash, prot.GetUserID(), architecture, language, version);
        }

        bool AddUserToActiveConnections(networkServer.networkClientInterface ClientInterface, string ApplicationHash, string ComputerID, string architecture, String language, double version)
        {
            if (!CCstData.InstanceExists(ApplicationHash))
            {
                //Instance does not exist. The player must have manipulated the protocol!
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, String.Format("Invalid application hash received in authentification protocol! ComputerID: {0}, ApplicationHash: {1}", ComputerID, ApplicationHash));
                return false;
            }

            ///asCCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, "201 sent");
            //SendProtocol("201;1;Es war einmal ein Engellein, fdg hatte keine Flügel mehr", ClientInterface);
            ////SendProtocol("201;1;Contact Admin#!sG36&§$-ENDE", ClientInterface);
            //return false;

            //Check if user is already connected
            foreach (networkServer.networkClientInterface item in ActiveConnections)
            {
                if(item.User.ID==ComputerID
                    && item.User.Application.Hash==ApplicationHash)
                {
                    //User is already registered
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, "Authentification: User is already added to list!");
                    SendProtocol("201;2;Contact Admin", ClientInterface);
                    return false;
                }
            }
            
            EPlayer dataClient = SPlayer.Authenticate(ComputerID, ApplicationHash, architecture, language, "");
            
            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, "Authentification: Hardware ID exists multiple times in the database");
                SendProtocol("201;3;Contact Admin", ClientInterface);
                return false;
            }
            dataClient.Application.Hash = ApplicationHash;

            //Check if user is banned
            if (dataClient.isBanned == true)
            {
                //Do something and dont let him enter
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, String.Format("Authentification: Banned user tried to authentificate. User: {0}", dataClient.ID));
                //Send protocol to client that user is banned
                SendProtocol("201;4;Too many hacks", ClientInterface);
                return false;
            }

            //Add EPlayer to ClientInterface and to the list
            ClientInterface.User = dataClient;

            //Generate unique Session ID for network communication
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "Authentification: Start creating a unique session ID");
            while (true)
            {
                string SessionID = AdditionalFunctions.GenerateSessionID(CCstData.GetInstance(ApplicationHash).SessionIDLength);
                //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
                if (ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList().Count == 0)
                {
                    ClientInterface.SessionID = SessionID;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "Unique session ID created + " + SessionID);
                    break;
                }
            }

            //Add the new connection to the list of connected connections
            ClientInterface.SetPingTimer(CCstData.GetInstance(dataClient.Application.ID).PingTimer, "167.88.15.106", "root", "Wn51b453gpEdZTB5Bl", 22, KickUser);
            ClientInterface.unixSshConnectorAccept.Connect();
            if(ClientInterface.unixSshConnectorAccept.IsConnected)
            {
                List<int> Ports = new List<int>();
                Ports.Add(12001);
                Ports.Add(12002);
                Ports.Add(12003);

                foreach (int item in Ports)
                {
                    ClientInterface.unixSshConnectorAccept.RunCommand("iptables -I INPUT -p tcp -s " + ClientInterface.IP + " --dport " + item + " -j ACCEPT");
                }

                ClientInterface.unixSshConnectorAccept.Disconnect();
            }
            else
            {
                //Fehlerinfo
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, "Client could not be connected to the Linux Server. Session ID: " + ClientInterface.SessionID);
                return false;
            }

            ActiveConnections.Add(ClientInterface);

            SendProtocol("200;" + ClientInterface.SessionID, ClientInterface);
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "Protocol sent. Session ID + " + ClientInterface.SessionID);
            return true;
        }

        void KickUser(networkServer.networkClientInterface Client)
        {
            ActiveConnections.Remove(Client);
        }

        #endregion

        private bool CheckPing(networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "Ping: Protocol received. User: " + prot.GetUserID());

            networkServer.networkClientInterface ClientInterface = Client;
            Client.Counter++;
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "Ping: User found in the list.");

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
                ClientInterface.ResetPingTimer();
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "Ping resetted.");

                //zhCCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "Additional Infos: "+AdditionalInfo);

                if (AdditionalInfo.Length == 0)
                    SendProtocol("300", ClientInterface);
                else
                    SendProtocol(String.Format("301;{0}",AdditionalInfo), ClientInterface);

                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.ERROR, "Ping: User does not exist in the active connections");
            return false;
        }


        #region Hack Detections
        private bool HackDetection_Heuristic(Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "H-Detection received. User: "+ prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "H-Detection: User found in the active connections");
                ArrayList Objects = prot.GetValues();
                if(Objects.Count!=4)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, String.Format("H-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));

                    return false;
                }

                string ProcessName = Convert.ToString(Objects[0]);
                string WindowName = Convert.ToString(Objects[1]);
                string ClassName = Convert.ToString(Objects[2]);
                string MD5Value = Convert.ToString(Objects[3]);

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "H-Detection: Saved protocol values: ProcessName: " + ProcessName + ", WindowName: " + WindowName + ", ClassName: " + ClassName + ", MD5Value: " + MD5Value);

                if (!SHackHeuristic.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, ProcessName, WindowName, ClassName, MD5Value))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, String.Format("H-Detection: Insertion in database failed! Protocol: {0}", prot.GetOriginalString()));
                    return false;
                    //Log error - Hack insertion did not work
                }
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "H-Detection: Database interaction successful");
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, "H-Detection: User not found in active connections!");
            return false;
        }

        private bool HackDetection_VirtualMemory(Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "V-Detection received. User: "+prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 4)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, String.Format("V-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));

                    return false;
                }

                string BaseAddress = Convert.ToString(Objects[0]);
                string Offset = Convert.ToString(Objects[1]);
                string DetectedValue = Convert.ToString(Objects[2]);
                string DefaultValue = Convert.ToString(Objects[3]);

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "V-Detection: Saved protocol successfully. Values: BaseAddress: " + BaseAddress + ", Offset: " + Offset + ", DetectedValue: " + DetectedValue + ", DefaultValue: " + DefaultValue);

                if (!SHackVirtual.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, BaseAddress, Offset, DetectedValue, DefaultValue))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, String.Format("V-Detection: Insertion in database failed! Protocol: {0}", prot.GetOriginalString()));
                    return false;
                    //Log error - Hack insertion did not work
                }
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "V-Detection: Database interaction successful");
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, "V-Detection: User not found in active connections!");
            return false;
        }
        #endregion
    }
}
