﻿using Protega___Server.Classes.Core;
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
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authenticating new user ({0})", prot.GetUserID()));
            ArrayList Objects = prot.GetValues();
            if(Objects.Count!=4)
            {
                //Log error - protocol size not as expected
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
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
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, "Double expected but received " + Objects[3].ToString());
                return false;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authentification protocol correct. ApplicationHash={0}, Architecture={1}, Language={2}, Version={3}", ApplicationHash, architecture, language, version));

            //Check if user exists and add it to the list
            return AddUserToActiveConnections(ClientInterface, ApplicationHash, prot.GetUserID(), architecture, language, version);
        }

        bool AddUserToActiveConnections(networkServer.networkClientInterface ClientInterface, string ApplicationHash, string ComputerID, string architecture, String language, double version)
        {
            if (!CCstData.InstanceExists(ApplicationHash))
            {
                //Instance does not exist. The player must have manipulated the protocol!
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Invalid application hash received in authentification protocol! ComputerID: {0}, ApplicationHash: {1}", ComputerID, ApplicationHash));
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
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.CLIENT, "Authentification: User is already added to list!");
                    SendProtocol("201;2;Contact Admin", ClientInterface);
                    return false;
                }
            }
            
            EPlayer dataClient = SPlayer.Authenticate(ComputerID, ApplicationHash, architecture, language, "");
            
            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, "Authentification: Hardware ID exists multiple times in the database");
                SendProtocol("201;3;Contact Admin", ClientInterface);
                return false;
            }
            dataClient.Application.Hash = ApplicationHash;

            //Check if user is banned
            if (dataClient.isBanned == true)
            {
                //Do something and dont let him enter
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Authentification: Banned user tried to authentificate. User: {0}", dataClient.ID));
                //Send protocol to client that user is banned
                SendProtocol("201;4;Too many hacks", ClientInterface);
                return false;
            }

            //Add EPlayer to ClientInterface and to the list
            ClientInterface.User = dataClient;

            //Generate unique Session ID for network communication
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "Authentification: Start creating a unique session ID");
            while (true)
            {
                string SessionID = AdditionalFunctions.GenerateSessionID(CCstData.GetInstance(ApplicationHash).SessionIDLength);
                //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
                if (ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList().Count == 0)
                {
                    ClientInterface.SessionID = SessionID;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "Unique session ID created + " + SessionID);
                    break;
                }
            }

            //Add the new connection to the list of connected connections
            ClientInterface.SetPingTimer(CCstData.GetInstance(dataClient.Application.ID).PingTimer, "167.88.15.106", "root", "Wn51b453gpEdZTB5Bl", 22, KickUser);


            bool IpExistsAlready= ActiveConnections.Select(Client => Client.IP == ClientInterface.IP).ToList().Count > 0;
            
            if (!IpExistsAlready)
            {
                //If there is already an IP exception, we dont need another
                ClientInterface.unixSshConnectorAccept.Connect();
                if (ClientInterface.unixSshConnectorAccept.IsConnected)
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
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.CLIENT, "Client could not be connected to the Linux Server. Session ID: " + ClientInterface.SessionID);
                    return false;
                }
            }
            else
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, "Authentication: IP already exists");
            }

            ActiveConnections.Add(ClientInterface);

            SendProtocol("200;" + ClientInterface.SessionID, ClientInterface);
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "Protocol sent. Session ID + " + ClientInterface.SessionID);
            return true;
        }

        void KickUser(networkServer.networkClientInterface ClientInterface)
        {
            System.Threading.Thread.Sleep(1000);
            bool IpExistsAlready = ActiveConnections.Select(Client => Client.IP == ClientInterface.IP && Client.SessionID != ClientInterface.SessionID).ToList().Count > 0;
            if (!IpExistsAlready)
            {
                //If there is another user with the same IP, we have to keep it in the IPTables
                ClientInterface.unixSshConnectorAccept.Connect();
                if (!ClientInterface.unixSshConnectorAccept.IsConnected)
                {
                    //Log error

                }
                List<int> Ports = new List<int>();
                Ports.Add(12001);
                Ports.Add(12002);
                Ports.Add(12003);

                foreach (int item in Ports)
                {
                    ClientInterface.unixSshConnectorAccept.RunCommand("iptables -D INPUT -p tcp -s " + ClientInterface.IP + " --dport " + item + " -j ACCEPT");
                }
                ClientInterface.unixSshConnectorAccept.Disconnect();
                ClientInterface.unixSshConnectorAccept.Dispose();
            }
            CCstData.GetInstance(ClientInterface.User.Application.ID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "User kicked. Session ID: " + ClientInterface.SessionID);
            ActiveConnections.Remove(ClientInterface);
            ClientInterface.Dispose();
        }

        #endregion

        private bool CheckPing(networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "Ping: Protocol received. User: " + prot.GetUserID());

            networkServer.networkClientInterface ClientInterface = Client;
            Client.Counter++;
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "Ping: User found in the list.");

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
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "Ping resetted.");

                //zhCCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, "Additional Infos: "+AdditionalInfo);

                if (AdditionalInfo.Length == 0)
                    SendProtocol("300", ClientInterface);
                else
                    SendProtocol(String.Format("301;{0}",AdditionalInfo), ClientInterface);

                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.CLIENT, "Ping: User does not exist in the active connections");
            return false;
        }


        #region Hack Detections
        private bool HackDetection_Heuristic(networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "H-Detection received. User: " + prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = Client;
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "H-Detection: User found in the active connections");
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 2)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("H-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol("401;5", ClientInterface);
                    KickUser(ClientInterface);
                    return false;
                }

                //The section ID defines which hack detection method triggered
                int SectionID;
                if (!Int32.TryParse(Objects[0].ToString(), out SectionID))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("H-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol("401;13", ClientInterface);
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

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "H-Detection: Saved protocol values: ProcessName: " + ProcessName + ", WindowName: " + WindowName + ", ClassName: " + ClassName + ", MD5Value: " + MD5Value);

                int Counter = 0;
                while (!SHackHeuristic.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, ProcessName, WindowName, ClassName, MD5Value))
                {
                    Counter++;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, String.Format("H-Detection: Insertion in database failed! Attempt: {0}, Protocol: {1}", Counter, prot.GetOriginalString()));
                    if (Counter > 3)
                    {
                        SendProtocol("401;6", ClientInterface);
                        KickUser(ClientInterface);
                        return false;
                    }
                }

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.DATABASE, "H-Detection: Database interaction successful");
                SendProtocol("400;8", ClientInterface);

                KickUser(ClientInterface);
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.SERVER, "H-Detection: User not found in active connections!");
            SendProtocol("401;7", ClientInterface);
            KickUser(ClientInterface);
            return false;
        }


        private bool HackDetection_File(networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "F-Detection received. User: " + prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = Client;
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "F-Detection: User found in the active connections");
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 2)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("F-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol("401;5", ClientInterface);
                    KickUser(ClientInterface);
                    return false;
                }

                //The section ID defines which hack detection method triggered
                int SectionID;
                if (!Int32.TryParse(Objects[0].ToString(), out SectionID))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("F-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol("401;16", ClientInterface);
                    KickUser(ClientInterface);
                }

                string Content =  Convert.ToString(Objects[1]);

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "F-Detection: Saved protocol values: CaseID: " + SectionID + ", Content: " + Content);

                int Counter = 0;
                while (!SHackFile.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, SectionID, Content))
                {
                    Counter++;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, String.Format("F-Detection: Insertion in database failed! Attempt: {0}, Protocol: {1}", Counter, prot.GetOriginalString()));
                    if (Counter > 3)
                    {
                        SendProtocol("401;15", ClientInterface);
                        KickUser(ClientInterface);
                        return false;
                    }
                }

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.DATABASE, "F-Detection: Database interaction successful");
                SendProtocol("400;14", ClientInterface);

                KickUser(ClientInterface);
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.SERVER, "F-Detection: User not found in active connections!");
            SendProtocol("401;17", ClientInterface);
            KickUser(ClientInterface);
            return false;
        }

        private bool HackDetection_VirtualMemory(networkServer.networkClientInterface Client, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "V-Detection received. User: "+prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = Client;
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 4)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.CLIENT, String.Format("V-Detection: Unexpected size of protocol. Expected are 4 but it was {0}. Protocol: {1}", Objects.Count, prot.GetOriginalString()));
                    SendProtocol("401;9", ClientInterface);
                    KickUser(ClientInterface);
                    return false;
                }

                string BaseAddress = Convert.ToString(Objects[0]);
                string Offset = Convert.ToString(Objects[1]);
                string DetectedValue = Convert.ToString(Objects[2]);
                string DefaultValue = Convert.ToString(Objects[3]);

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "V-Detection: Saved protocol successfully. Values: BaseAddress: " + BaseAddress + ", Offset: " + Offset + ", DetectedValue: " + DetectedValue + ", DefaultValue: " + DefaultValue);
                
                int Counter = 0;
                while (!SHackVirtual.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, BaseAddress, Offset, DetectedValue, DefaultValue))
                {
                    Counter++;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.DATABASE, String.Format("V-Detection: Insertion in database failed! Protocol: {0}", prot.GetOriginalString()));
                    if (Counter > 3)
                    {
                        SendProtocol("401;10", ClientInterface);
                        KickUser(ClientInterface);
                        return false;
                    }
                }
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "V-Detection: Database interaction successful");

                SendProtocol("400;11", ClientInterface);
                KickUser(ClientInterface);
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.SERVER, "V-Detection: User not found in active connections!");
            SendProtocol("401;12", ClientInterface);
            KickUser(ClientInterface);
            return false;
        }
        #endregion
    }
}
