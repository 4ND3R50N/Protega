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

        public void RecievedProtocol(networkServer.networkClientInterface NetworkClient, string protocolString) {
            Protocol protocol = new Protocol(protocolString);
                switch (protocol.GetKey())
                {
                    case 500: AuthenticateUser(NetworkClient, protocol); break;
                    case 600: CheckPing(protocol);  break;
                    case 701: FoundHackDetaction(protocol); break;
                    case 702: FoundHackDetaction(protocol); break;
                    case 703: FoundHackDetaction(protocol); break;
                    case 704: FoundHackDetaction(protocol); break;
                    case 705: FoundHackDetaction(protocol); break;
                    default: Console.WriteLine("Invalid key for client to server communication."); break;
                }
            
        }

        bool CheckIfUserExists(string HardwareID, out networkServer.networkClientInterface ClientInterface)
        {
            //Check if a hardware ID exists in ActiveConnections
            ClientInterface = null;
            
            List<networkServer.networkClientInterface> lList = ActiveConnections.Where(Client => Client.User.ID == HardwareID).ToList();
            if (lList.Count == 1)
                ClientInterface = lList[0];
            return lList.Count == 1;
        }

        //private static void SendProtocol(String protocolString, int userID) {
        // send the protocoll to the given user
        // using the controller core to send messages??????????
        //}

        #region Authenticate User
        public delegate void _RegisterUser(string ComputerID, Boolean architecture, String language, double version, Boolean auth);
        public event _RegisterUser RegisterUser=null;

        private void AuthenticateUser(networkServer.networkClientInterface ClientInterface, Protocol prot)
        {
            //Computer ID, Computer Architecture, Language, Version
            string architecture = prot.GetValues()[0].ToString();
            String language = prot.GetValues()[1].ToString();
            double version = Convert.ToDouble(prot.GetValues()[2]);

            //Check if user is already connected
            
            if (CheckIfUserExists(prot.GetComputerID(), out ClientInterface))
            {
                //User is already registered
                //Kick User?
                CCstLogging.Logger.writeInLog(true, "User "+prot.GetComputerID()+" is already added to list!");
                return;
            }

            EPlayer dataClient = SPlayer.Authenticate(prot.GetComputerID(), architecture, language, "127.0.0.1");
            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                SendProtocol("201;Contact Admin!", ClientInterface);
                return;
            }

            //Add EPlayer to ClientInterface and to the list
            ClientInterface.User = dataClient;
            ActiveConnections.Add(ClientInterface);

            //SendProtocol("200;16 Bit Alias Key", ClientInterface);

        }

        void AddUserToActiveConnections(networkServer.networkClientInterface ClientInterface, string ComputerID, string architecture, String language, double version, Boolean auth)
        {
            //Check if user is already connected
            ActiveConnections
                .ForEach(Client =>
                {
                    if (Client.User.ID == ComputerID)
                    {
                        //User is already registered
                        //Kick User?
                        CCstLogging.Logger.writeInLog(true, "User is already added to list!");
                        return;
                    }
                });

            EPlayer dataClient = SPlayer.Authenticate(ComputerID, architecture, language, "");
            if (dataClient == null)
            {
                //If a computer ID exists multiple times in the database, a null object is returned
                SendProtocol("201;Contact Admin", ClientInterface);
                return;
            }

            //Add EPlayer to ClientInterface and to the list
            ClientInterface.User = dataClient;
            ActiveConnections.Add(ClientInterface);
        }

            #endregion

            private void CheckPing(Protocol prot)
        {
            networkServer.networkClientInterface ClientInterface;
            if (CheckIfUserExists(prot.ComputerID, out ClientInterface))
            {
                // save somewhere that user pinged successfully
                // check if some other messages should be send
                Boolean sendMessage = true;
                if (sendMessage)
                {
                    SendProtocol("301;Some messages added divided by ;", ClientInterface);
                }
                else
                {
                    SendProtocol("300", ClientInterface);
                }
            }
        }

        private void FoundHackDetaction(Protocol prot)
        {
            networkServer.networkClientInterface ClientInterface;
            if (CheckIfUserExists(prot.ComputerID, out ClientInterface))
            {
                int computerID = (int)prot.GetValues()[0];
                // get number of found hack detections and the result will lead to different answers to the client
                int numberOfDetections = 0;
                numberOfDetections++;
                // save new number for this user

                // save all other parameter in the database

                if (numberOfDetections == 1)
                {
                    SendProtocol("400", ClientInterface);
                }
                else if ((numberOfDetections >= 2) && (numberOfDetections <= 4))
                {
                    int time = numberOfDetections * 1000;
                    SendProtocol("401;" + time, ClientInterface);
                }
                else
                {
                    SendProtocol("402", ClientInterface);
                }
            }
        }
    }
}
