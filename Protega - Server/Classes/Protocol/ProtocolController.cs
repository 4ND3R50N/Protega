﻿using Protega___Server.Classes.Core;
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
        int ApplicationID;

        public ProtocolController(ref List<networkServer.networkClientInterface> ActiveConnections, int _ApplicationID)
        {
            this.ActiveConnections = ActiveConnections;
            ApplicationID = _ApplicationID;
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
            Protocol protocol = new Protocol(protocolString);
            switch (protocol.GetKey())
            {
                case 600:
                    return CheckPing(protocol); 
                case 500:
                    return AuthenticateUser(NetworkClient, protocol); 
                case 701:
                    return HackDetection_Heuristic(protocol); 
                case 702:
                    return HackDetection_VirtualMemory(protocol); 
                default:
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, "Received unvalid protocol");
                    return false; 
            }
            
        }

        // QUESTION: why do we give session ID here? It is saved in ClientInterface isn't it???????????????????????????????????????????????
        public bool CheckIfUserExists(string SessionID, ref networkServer.networkClientInterface ClientInterface)
        {
            //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
            List<networkServer.networkClientInterface> lList = ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList();
            if (lList.Count == 1)
                ClientInterface = lList[0];
            //if(lList.Count == 1)
            //{
            //    Logger.writeInLog(true, LoggingStatus.LOW, "Client Interface " +ClientInterface.SessionID+" exists in active connections");

            //}else if (lList.Count == 0)
            //{
            //    // QUESTION: Is this one already an error or just a warning????????????????????????????????????????????????????????????????
            //    Logger.writeInLog(true, LoggingStatus.MIDDLE, "Client Interface " + ClientInterface.SessionID + " DOES NOT exists in active connections");

            //}
            //else
            //{
            //    // QUESTION: Is this one already an error or just a warning????????????????????????????????????????????????????????????????
            //    Logger.writeInLog(true, LoggingStatus.CRITICAL, "Client Interface " + ClientInterface.SessionID + " is saved "+lList.Count+" times in active connections");
            //}
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

        private bool AuthenticateUser( networkServer.networkClientInterface ClientInterface, Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "Called method AuthenticateUser");
            ArrayList Objects = prot.GetValues();
            if(Objects.Count!=4)
            {
                //Log error - protocol size not as expected
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, "Unexpected size of protocol. Expected are 4 but it was "+Objects.Count);
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
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, "Double expected but received " + Objects[3].ToString());
                return false;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, String.Format("Authentification protocol correct. ApplicationHash={0}, Architecture={1}, Language={2}, Version={3}", ApplicationHash, architecture, language, version));

            //Check if user exists and add it to the list
            return AddUserToActiveConnections(ClientInterface, ApplicationHash, prot.GetUserID(), architecture, language, version);
        }

        bool AddUserToActiveConnections(networkServer.networkClientInterface ClientInterface, string ApplicationHash, string ComputerID, string architecture, String language, double version)
        {
            if (!CCstData.InstanceExists(ApplicationHash))
            {
                //Instance does not exist. The player must have manipulated the protocol!
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, "Invalid application hash received in authentification protocol!");
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
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, "Authentification: User is already added to list!");
                    return false;
                }
            }
            
            EPlayer dataClient = SPlayer.Authenticate(ComputerID, ApplicationHash, architecture, language, "");
            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, "Authentification: Hardware ID exists multiple times in the database");
                SendProtocol("201;Contact Admin", ClientInterface);
                return false;
            }

            //Check if user is banned
            if (dataClient.isBanned == true)
            {
                //Do something and dont let him enter
                // QUESTION: Is this one already an error or just a warning????????????????????????????????????????????????????????????????
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, String.Format("Authentification: Banned user tried to authentificate. User: {0}", dataClient.ID));
                //Send protocol to client that user is banned
                //SendProtocol("201;Too many hacks", ClientInterface);
                return false;
            }

            //Add EPlayer to ClientInterface and to the list
            ClientInterface.User = dataClient;

            //Generate unique Session ID for network communication
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "Authentification: Start creating a unique session ID");
            while (true)
            {
                string SessionID = AdditionalFunctions.GenerateSessionID(CCstData.GetInstance(ApplicationHash).SessionIDLength);
                //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
                if (ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList().Count == 0)
                {
                    ClientInterface.SessionID = SessionID;
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "Unique session ID created + " + SessionID);
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
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "Ping: Protocol received. User: " + prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();


            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "Ping: User found in the list.");
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
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, "Ping: User does not exist in the active connections");
            return false;
        }


        #region Hack Detections
        private bool HackDetection_Heuristic(Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "H-Detection received. User: "+ prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "H-Detection: User found in the active connections");
                ArrayList Objects = prot.GetValues();
                if(Objects.Count!=4)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, "H-Detection: Unexpected size of protocol. Expected are 4 but it was " + Objects.Count);

                    return false;
                }

                string ProcessName = Convert.ToString(Objects[0]);
                string WindowName = Convert.ToString(Objects[1]);
                string ClassName = Convert.ToString(Objects[2]);
                string MD5Value = Convert.ToString(Objects[3]);

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "H-Detection: Saved protocol values: ProcessName: " + ProcessName + ", WindowName: " + WindowName + ", ClassName: " + ClassName + ", MD5Value: " + MD5Value);

                if (!SHackHeuristic.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, ProcessName, WindowName, ClassName, MD5Value))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(1, "H-Detection: Insertion in database failed!");
                    return false;
                    //Log error - Hack insertion did not work
                }
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "H-Detection: Database interaction successful");
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, "H-Detection: User not found in active connections!");
            return false;
        }

        private bool HackDetection_VirtualMemory(Protocol prot)
        {
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "V-Detection received. User: "+prot.GetUserID());
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 4)
                {
                    //Log error - protocol size not as expected
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, "V-Detection: Unexpected size of protocol. Expected are 4 but it was " + Objects.Count);

                    return false;
                }

                string BaseAddress = Convert.ToString(Objects[0]);
                string Offset = Convert.ToString(Objects[1]);
                string DetectedValue = Convert.ToString(Objects[2]);
                string DefaultValue = Convert.ToString(Objects[3]);

                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "V-Detection: Saved protocol successfully. Values: BaseAddress: " + BaseAddress + ", Offset: " + Offset + ", DetectedValue: " + DetectedValue + ", DefaultValue: " + DefaultValue);

                if (!SHackVirtual.Insert(ClientInterface.User.ID, ClientInterface.User.Application.ID, BaseAddress, Offset, DetectedValue, DefaultValue))
                {
                    CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, "V-Detection: Insertion in database failed!");
                    return false;
                    //Log error - Hack insertion did not work
                }
                CCstData.GetInstance(ApplicationID).Logger.writeInLog(3, "V-Detection: Database interaction successful");
                return true;
            }
            CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, "V-Detection: User not found in active connections!");
            return false;
        }
        #endregion
    }
}
