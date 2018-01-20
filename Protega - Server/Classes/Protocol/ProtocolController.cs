using Protega___Server.Classes.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protega___Server.Classes.Entity;
using Support;

namespace Protega___Server.Classes.Protocol
{
    public class ProtocolController
    {
        List<networkServer.networkClientInterface> ActiveConnections;
        Support.logWriter Logger;

        public ProtocolController(ref List<networkServer.networkClientInterface> ActiveConnections)
        {
            Logger = new logWriter("ProtocolController");
            Logger.writeInLog(true, LoggingStatus.OKAY, "Logging class initialized!");
            this.ActiveConnections = ActiveConnections;
        }


        //private ControllerCore core;
        //public ProtocolController(ControllerCore core) {
        //    this.core = core;
        //}
        // just for me that I don´t forget any of the calls
        //int[] protocolKeysClientToServer = {500, 600, 701,702,703,704,705 };
        //int[] protocolKeysServerToClient = { 200, 201, 300, 301, 400, 401, 402 };
        //QUESTION: I thought the computerID is already included in the protocol -> don't need to pass it as parameter??
        public delegate void SendProt(string Protocol, networkServer.networkClientInterface ClientInterface);
        public static event SendProt SendProtocol = null;

        public bool ReceivedProtocol(networkServer.networkClientInterface NetworkClient, string protocolString)
        {
            Logger.writeInLog(true, LoggingStatus.OKAY, "Receieved protocol ["+protocolString+"] from session: "+NetworkClient.SessionID);
            Protocol protocol = new Protocol(protocolString);
            switch (protocol.GetKey())
            {
                case 600:
                    Logger.writeInLog(true, LoggingStatus.OKAY, "Received protocol for ping");
                    return CheckPing(protocol); 
                case 500:
                    Logger.writeInLog(true, LoggingStatus.OKAY, "Received protocol for authentication");
                    return AuthenticateUser(NetworkClient, protocol); 
                case 701:
                    Logger.writeInLog(true, LoggingStatus.OKAY, "Received protocol for heuristic hack detection");
                    return HackDetection_Heuristic(protocol); 
                case 702:
                    Logger.writeInLog(true, LoggingStatus.OKAY, "Received protocol for virtual hack detection");
                    return HackDetection_VirtualMemory(protocol); 
                default:
                    Logger.writeInLog(true, LoggingStatus.ERROR, "Received unvalid protocol");
                    return false; 
            }
            
        }

        // QUESTION: why do we give session ID here? It is saved in ClientInterface isn't it???????????????????????????????????????????????
        public bool CheckIfUserExists(string SessionID, ref networkServer.networkClientInterface ClientInterface)
        {
            Logger.writeInLog(true, LoggingStatus.OKAY, "Called method CheckIfUserExists for session "+ClientInterface.SessionID);
            //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
            List<networkServer.networkClientInterface> lList = ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList();
            if (lList.Count == 1)
                ClientInterface = lList[0];
            if(lList.Count == 1)
            {
                Logger.writeInLog(true, LoggingStatus.OKAY, "Client Interface " +ClientInterface.SessionID+" exists in active connections");

            }else if (lList.Count == 0)
            {
                // QUESTION: Is this one already an error or just a warning????????????????????????????????????????????????????????????????
                Logger.writeInLog(true, LoggingStatus.WARNING, "Client Interface " + ClientInterface.SessionID + " DOES NOT exists in active connections");

            }
            else
            {
                // QUESTION: Is this one already an error or just a warning????????????????????????????????????????????????????????????????
                Logger.writeInLog(true, LoggingStatus.ERROR, "Client Interface " + ClientInterface.SessionID + " is saved "+lList.Count+" times in active connections");
            }
            //True if an interface exists, false if not or more
            return lList.Count == 1;
        }

        //private static void SendProtocol(String protocolString, int userID) {
        // send the protocoll to the given user
        // using the controller core to send messages??????????
        //}

        #region Authenticate User
        //public delegate void _RegisterUser(string ComputerID, Boolean architecture, String language, double version, Boolean auth);
        //public event _RegisterUser RegisterUser=null;

        private bool AuthenticateUser(networkServer.networkClientInterface ClientInterface, Protocol prot)
        {
            Logger.writeInLog(true, LoggingStatus.OKAY, "Called method AuthenticateUser for session " + ClientInterface.SessionID);
            ArrayList Objects = prot.GetValues();
            if(Objects.Count!=4)
            {
                //Log error - protocol size not as expected
                Logger.writeInLog(true, LoggingStatus.ERROR, "Unexpected size of protocol. Expected are 4 but it was "+Objects.Count);
                return false;
            }

            //Computer ID, Computer Architecture, Language, Version
            string ApplicationHash = Objects[0].ToString();
            string architecture = Objects[1].ToString();
            string language = Objects[2].ToString();            
            double version;
            if (!Double.TryParse(Objects[3].ToString(), out version))
            {
                //Log error - protocol index 3 is not as expected
                Logger.writeInLog(true, LoggingStatus.ERROR, "Unexpected type of version (Objects[3]). Should be double but was "+version);
                return false;
            }
            Logger.writeInLog(true, LoggingStatus.OKAY, "Saved protocol values: ApplicationHash: "+ApplicationHash+", architecture: "+architecture+", language: "+language+", version: "+version);

            //Check if user exists and add it to the list
            return AddUserToActiveConnections(ClientInterface, ApplicationHash, prot.GetUserID(), architecture, language, version);
        }

        bool AddUserToActiveConnections(networkServer.networkClientInterface ClientInterface, string ApplicationHash, string ComputerID, string architecture, String language, double version)
        {
            Logger.writeInLog(true, LoggingStatus.OKAY, "Called method AddUserToActiveConnections for session " + ClientInterface.SessionID);
            if (!CCstData.InstanceExists(ApplicationHash))
            {
                //Instance does not exist. The player must have manipulated the protocol!
                Logger.writeInLog(true, LoggingStatus.ERROR, "Instance does not exist. The player must have manipulated the protocol");
                return false;
            }


            //Check if user is already connected
            foreach (networkServer.networkClientInterface item in ActiveConnections)
            {
                if(item.User.ID==ComputerID
                    && item.User.Application.Hash==ApplicationHash)
                {
                    //User is already registered
                    //Kick User?
                    // QUESTION: Why are we using this logger??????????????????????????????????????????????????????????????????????????????????
                    // QUESTION: Is this one already an error or just a warning????????????????????????????????????????????????????????????????
                    CCstData.GetInstance(ApplicationHash).Logger.writeInLog(true, LoggingStatus.WARNING, "User is already added to list");
                    return false;
                }
            }
            
            EPlayer dataClient = SPlayer.Authenticate(ComputerID, ApplicationHash, architecture, language, "");
            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                Logger.writeInLog(true, LoggingStatus.ERROR, "Computer ID exists multiple times in the database");
                SendProtocol("201;Contact Admin", ClientInterface);
                return false;
            }

            //Check if user is banned
            if (dataClient.isBanned == true)
            {
                //Do something and dont let him enter
                // QUESTION: Is this one already an error or just a warning????????????????????????????????????????????????????????????????
                Logger.writeInLog(true, LoggingStatus.WARNING, "User is banned");
                //Send protocol to client that user is banned
                //SendProtocol("201;Too many hacks", ClientInterface);
                return false;
            }

            //Add EPlayer to ClientInterface and to the list
            ClientInterface.User = dataClient;

            //Generate unique Session ID for network communication
            Logger.writeInLog(true, LoggingStatus.OKAY, "Start creating an unique session ID");
            while (true)
            {
                string SessionID = AdditionalFunctions.GenerateSessionID(CCstData.GetInstance(ApplicationHash).SessionIDLength);
                //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
                if (ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList().Count == 0)
                {
                    ClientInterface.SessionID = SessionID;
                    Logger.writeInLog(true, LoggingStatus.OKAY, "Created unique session ID " + SessionID);
                    break;
                }
            }

            //Add the new connection to the list of connected connections
            ClientInterface.SetPingTimer(CCstData.GetInstance(dataClient.Application.ID).PingTimer);
            ActiveConnections.Add(ClientInterface);
            return true;
        }

        #endregion

        private bool CheckPing(Protocol prot)
        {
            Logger.writeInLog(true, LoggingStatus.OKAY, "Called method CheckPing");
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();


            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                Logger.writeInLog(true, LoggingStatus.OKAY, "Given user exists in connections");
                //If additional infos are asked, what is needed is identified by an ID
                /*
                 * Different logic is needed here. Admin is able to add additional information for user to a queue
                 * and when a successful ping is send down, all new one for this client are added.
                 * Not ment for version 1.
                 */
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

                return true;
                //Send to the client that the Ping was successful
                //If requested, additional infos will be sent
                //SendProtocol(String.Format("{0}{1}", "300", AdditionalInfo), ClientInterface);
            }
            Logger.writeInLog(true, LoggingStatus.ERROR, "Given user DOES NOT exist in connections");
            return false;
        }


        #region Hack Detections
        private bool HackDetection_Heuristic(Protocol prot)
        {
            Logger.writeInLog(true, LoggingStatus.OKAY, "Called method HackDetection_Heuristic");
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                Logger.writeInLog(true, LoggingStatus.OKAY, "User exists");
                ArrayList Objects = prot.GetValues();
                if(Objects.Count!=4)
                {
                    //Log error - protocol size not as expected
                    Logger.writeInLog(true, LoggingStatus.ERROR, "Unexpected size of protocol. Expected are 4 but it was " + Objects.Count);

                    return false;
                }

                string ProcessName = Convert.ToString(Objects[0]);
                string WindowName = Convert.ToString(Objects[1]);
                string ClassName = Convert.ToString(Objects[2]);
                string MD5Value = Convert.ToString(Objects[3]);

                Logger.writeInLog(true, LoggingStatus.OKAY, "Saved protocol values: ProcessName: " + ProcessName + ", WindowName: " + WindowName + ", ClassName: " + ClassName + ", MD5Value: " + MD5Value);

                if (!SHackHeuristic.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, ProcessName, WindowName, ClassName, MD5Value))
                {
                    Logger.writeInLog(true, LoggingStatus.ERROR, "Isertion of the hack in database DID NOT work");
                    return false;
                    //Log error - Hack insertion did not work
                }
                Logger.writeInLog(true, LoggingStatus.OKAY, "Hack successfully inserted in database");
                return true;
            }
            Logger.writeInLog(true, LoggingStatus.ERROR, "User DOES NOT exist");
            return false;
        }

        private bool HackDetection_VirtualMemory(Protocol prot)
        {
            Logger.writeInLog(true, LoggingStatus.OKAY, "Called method HackDetection_VirtualMemory");
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 4)
                {
                    //Log error - protocol size not as expected
                    Logger.writeInLog(true, LoggingStatus.ERROR, "Unexpected size of protocol. Expected are 4 but it was " + Objects.Count);

                    return false;
                }

                string BaseAddress = Convert.ToString(Objects[0]);
                string Offset = Convert.ToString(Objects[1]);
                string DetectedValue = Convert.ToString(Objects[2]);
                string DefaultValue = Convert.ToString(Objects[3]);

                Logger.writeInLog(true, LoggingStatus.OKAY, "Saved protocol values: BaseAddress: " + BaseAddress + ", Offset: " + Offset + ", DetectedValue: " + DetectedValue + ", DefaultValue: " + DefaultValue);

                if (!SHackVirtual.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, BaseAddress, Offset, DetectedValue, DefaultValue))
                {
                    Logger.writeInLog(true, LoggingStatus.ERROR, "Isertion of the hack in database DID NOT work");
                    return false;
                    //Log error - Hack insertion did not work
                }
                Logger.writeInLog(true, LoggingStatus.OKAY, "Hack successfully inserted in database");
                return true;
            }
            Logger.writeInLog(true, LoggingStatus.ERROR, "User DOES NOT exist");
            return false;
        }
        #endregion
    }
}
