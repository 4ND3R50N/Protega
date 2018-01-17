using Protega___Server.Classes.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protega___Server.Classes.Entity;

namespace Protega___Server.Classes.Protocol
{
    public class ProtocolController
    {
        List<networkServer.networkClientInterface> ActiveConnections;

        public ProtocolController(ref List<networkServer.networkClientInterface> ActiveConnections)
        {
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

        public bool ReceivedProtocol(networkServer.networkClientInterface NetworkClient, string protocolString) {
            Protocol protocol = new Protocol(protocolString);
            switch (protocol.GetKey())
            {
                case 600: return CheckPing(protocol); break;
                case 500: return AuthenticateUser(NetworkClient, protocol); break;
                case 701: return HackDetection_Heuristic(protocol); break;
                case 702: return HackDetection_VirtualMemory(protocol); break;
                default: Console.WriteLine("Invalid key for client to server communication."); return false; break;
            }
            
        }

        public bool CheckIfUserExists(string SessionID, ref networkServer.networkClientInterface ClientInterface)
        {            
            //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
            List<networkServer.networkClientInterface> lList = ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList();
            if (lList.Count == 1)
                ClientInterface = lList[0];

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
            ArrayList Objects = prot.GetValues();
            if(Objects.Count!=4)
            {
                //Log error - protocol size not as expected

                return false;
            }

            //Computer ID, Computer Architecture, Language, Version
            string ApplicationName = Objects[0].ToString();
            string architecture = Objects[1].ToString();
            String language = Objects[2].ToString();
            
            double version;
            if(!Double.TryParse(Objects[3].ToString(), out version))
            {
                //Log error - protocol index 3 is not as expected

                return false;
            }
            
            //Check if user exists and add it to the list
            return AddUserToActiveConnections(ClientInterface, ApplicationName, prot.GetUserID(), architecture, language, version);
        }

        bool AddUserToActiveConnections(networkServer.networkClientInterface ClientInterface, string ApplicationName, string ComputerID, string architecture, String language, double version)
        {
            //Check if user is already connected
            foreach (networkServer.networkClientInterface item in ActiveConnections)
            {
                if(item.User.ID==ComputerID
                    && item.User.ApplicationName==ApplicationName)
                {
                    //User is already registered
                    //Kick User?
                    CCstLogging.Logger.writeInLog(true, "User is already added to list!");
                    return false;
                }
            }

            EPlayer dataClient = SPlayer.Authenticate(ComputerID, ApplicationName, architecture, language, "");
            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                SendProtocol("201;Contact Admin", ClientInterface);
                return false;
            }

            //Check if user is banned
            if (dataClient.isBanned == true)
            {
                //Do something and dont let him enter

                //Send protocol to client that user is banned
                //SendProtocol("201;Too many hacks", ClientInterface);
                return false;
            }

            //Add EPlayer to ClientInterface and to the list
            ClientInterface.User = dataClient;
            ClientInterface.User.ApplicationName = ApplicationName;

            //Generate unique Session ID for network communication
            while (true)
            {
                string SessionID = AdditionalFunctions.GenerateSessionID(CCstConfig.SessionIDLength);

                //Checks if that connection exists already. Gives back the amount of matching ClientInterfaces
                if (ActiveConnections.Where(Client => Client.SessionID == SessionID).ToList().Count == 0)
                {
                    ClientInterface.SessionID = SessionID;
                    break;
                }
            }

            //Add the new connection to the list of connected connections
            ActiveConnections.Add(ClientInterface);
            return true;
        }

        #endregion

        private bool CheckPing(Protocol prot)
        {
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();


            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                //If additional infos are asked, what is needed is identified by an ID
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
            return false;
        }


        #region Hack Detections
        private bool HackDetection_Heuristic(Protocol prot)
        {
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                ArrayList Objects = prot.GetValues();
                if(Objects.Count!=4)
                {
                    //Log error - protocol size not as expected

                    return false;
                }

                string ProcessName = Convert.ToString(Objects[0]);
                string WindowName = Convert.ToString(Objects[1]);
                string ClassName = Convert.ToString(Objects[2]);
                string MD5Value = Convert.ToString(Objects[3]);

                if(!SHackHeuristic.Insert(ClientInterface.User.ID, ClientInterface.User.ApplicationName, ProcessName, WindowName, ClassName, MD5Value))
                {
                    return false;
                    //Log error - Hack insertion did not work
                }
                return true;
            }
            return false;
        }

        private bool HackDetection_VirtualMemory(Protocol prot)
        {
            networkServer.networkClientInterface ClientInterface = new networkServer.networkClientInterface();
            if (CheckIfUserExists(prot.UserID, ref ClientInterface))
            {
                ArrayList Objects = prot.GetValues();
                if (Objects.Count != 4)
                {
                    //Log error - protocol size not as expected

                    return false;
                }

                string BaseAddress = Convert.ToString(Objects[0]);
                string Offset = Convert.ToString(Objects[1]);
                string DetectedValue = Convert.ToString(Objects[2]);
                string DefaultValue = Convert.ToString(Objects[3]);

                if (!SHackVirtual.Insert(ClientInterface.User.ID, ClientInterface.User.ApplicationName, BaseAddress, Offset, DetectedValue, DefaultValue))
                {
                    return false;
                    //Log error - Hack insertion did not work
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}
