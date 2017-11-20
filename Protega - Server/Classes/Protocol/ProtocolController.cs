using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Protocol
{
    class ProtocolController
    {
        // just for me that I don´t forget any of the calls
        int[] protocolKeysClientToServer = {500, 600, 701,702,703,704,705 };
        int[] protocolKeysServerToClient = { 200, 201, 300, 301, 400, 401, 402 };

        public void RecievedProtocol(String protocolString) {
            Protocol protocol = new Protocol(protocolString);
                switch (protocol.GetKey())
                {
                    case 500: AuthenticateUser(protocol); break;
                    case 600: CheckPing(protocol);  break;
                    case 701: FoundHackDetaction(protocol); break;
                    case 702: FoundHackDetaction(protocol); break;
                    case 703: FoundHackDetaction(protocol); break;
                    case 704: FoundHackDetaction(protocol); break;
                    case 705: FoundHackDetaction(protocol); break;
                    default: Console.WriteLine("Invalid key for client to server communication."); break;
                }
            
        }

        private void SendProtocol(String protocolString, int userID) {
            // send the protocoll to the given user
        }

        private void AuthenticateUser(Protocol prot)
        {
            //Computer ID, Computer Architecture, Language, Version
            try
            {
                int computerID = (int)prot.GetValues()[0];
                Boolean architecture = (Boolean)prot.GetValues()[1];
                String language = (String)prot.GetValues()[2];
                double version = (double)prot.GetValues()[3];
                Boolean auth = true;
                // TODO: check if computerID is saved in database
                // TODO: Save the other parameters in the database
                // check if user is authorized to play the game by checking also the #of hack detections
                if (auth)
                {
                    SendProtocol("200;16 Bit Alias Key", computerID);
                }
                else
                {
                    SendProtocol("201;IssueID getting from the database response.", computerID);
                }
            }
            catch(IndexOutOfRangeException ex)
            {
                System.Console.WriteLine("Something went wrong getting the parameters for protocol to authenticate a user.");
                System.Console.WriteLine(ex.Message);
            }
            
        }

        private void CheckPing(Protocol prot)
        {
            int computerID = (int)prot.GetValues()[0];
            // save somewhere that user pinged successfully
            // check if some other messages should be send
            Boolean sendMessage = true;
            if (sendMessage)
            {
                SendProtocol("301;Some messages added divided by ;", computerID);
            }
            else
            {
                SendProtocol("300", computerID);
            }
        }

        private void FoundHackDetaction(Protocol prot)
        {
            int computerID = (int)prot.GetValues()[0];
            // get number of found hack detections and the result will lead to different answers to the client
            int numberOfDetections = 0;
            numberOfDetections++;
            // save new number for this user

            // save all other parameter in the database

            if(numberOfDetections == 1)
            {
                SendProtocol("400", computerID);
            }else if((numberOfDetections >= 2) && (numberOfDetections <= 4))
            {
                int time = numberOfDetections*1000;
                SendProtocol("401;" + time, computerID);
            }
            else
            {
                SendProtocol("402", computerID);
            }
        }
    }
}
