using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Support;
using System.Net;
using System.Net.Sockets;
using Protega___Server.Classes.Protocol;

namespace Protega___Server.Classes.Core
{
    public class ControllerCore
    {

        //Variablen
        networkServer TcpServer;
        List<networkServer.networkClientInterface> ActiveConnections;
        ProtocolController ProtocolController;
        
        private string sAesKey;
        private char   cProtocolDelimiter;
        private char   cDataDelimiter;

        //Konstruktor
        public ControllerCore(short _iPort, char _cProtocolDelimiter, char _cDataDelimiter, string _sAesKey, string _sDatabaseDriver,
            string _sDBHostIp, short _sDBPort, string _sDBUser, string _sDBPass, string _sDBDefaultDB, string _sLogPath)
        {
            //Logging initialisations
            CCstLogging.Logger = new logWriter(_sLogPath);
            CCstLogging.Logger.writeInLog(true, "Logging class initialized!");
            //Database Initialisations
            if(_sDatabaseDriver == "mysql")
            {
               // CCstDatabase.DatabaseEngine = new DBMysqlDataManager(_sDBHostIp,_sDBUser,_sDBPass,_sDBPort,_sDBDefaultDB);

            }else if(_sDatabaseDriver == "mssql")
            {
                CCstDatabase.DatabaseEngine = new DBMssqlDataManager(_sDBHostIp, _sDBUser, _sDBPass, _sDBPort, _sDBDefaultDB);
            }
            //Database test
            if (CCstDatabase.DatabaseEngine.testDBConnection())
            {
                CCstLogging.Logger.writeInLog(true, "Database test successfull!");
            }else
            {
                CCstLogging.Logger.writeInLog(true, "ERROR: Database test was not successfull!");
                return;
            }

            //Network Initialisations
            ActiveConnections = new List<networkServer.networkClientInterface>();
            sAesKey = _sAesKey;
            this.cProtocolDelimiter = _cProtocolDelimiter;
            this.cDataDelimiter = _cDataDelimiter;
            TcpServer = new networkServer(NetworkProtocol, _sAesKey, IPAddress.Any, _iPort, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ProtocolController.SendProtocol += this.SendProtocol;
            CCstLogging.Logger.writeInLog(true, "TCP Server ready for start!");
            ProtocolController = new ProtocolController(ref ActiveConnections);

            //TESTCASE
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            //Registration
            ProtocolController.RecievedProtocol(new networkServer.networkClientInterface(), "500;98765;Windoofs 7;Deutsch;1");
            ProtocolController.RecievedProtocol(new networkServer.networkClientInterface(), "500;98765;Windoofs 7;Deutsch;1");
            //Auth
            //  networkProtocol("#104;Anderson2;Lars;Pickelin;miau1234;l.pickelin@web.de", ref dummy);
            //  networkProtocol("#102;Anderson2;miau1x234", ref dummy);
            //Content
            //Get all rooms
            //  networkProtocol("#201", ref dummy);
            //Get all rooms of a specific user
            //  NetworkProtocol("#211;18", ref dummy);
            //Add new or update room
            //NetworkProtocol("#203;5;1;Avelinas Test raum;Hallo Welt;1;http://www.AvelinaLerntArrays.net", ref dummy);
            //Get all workouts of room id 2
            //  NetworkProtocol("#205;Hadd e", ref dummy);
            //Get Levels of workout with id 1
            //  NetworkProtocol("#207;1", ref dummy);
            //Get all excercises of workout 1
            //  NetworkProtocol("#209;1", ref dummy);
            //Delete room
            //NetworkProtocol("#213;114", ref dummy);

        }

        // QUESTION: what exactly is the authenticating here? It looks for me more like:
        // is there someone saved? No? -> save him! And if there is already someone saved
        // I don't add him so the list only have one active connection? What is that for?

            //Answer
            //I only added this to test with Lars (max 1 connection).
            //Because currently every protocol is seen as a new authentication
        void AuthenticateClient(networkServer.networkClientInterface Client)
        {
            if(ActiveConnections.Count==0)
            ActiveConnections.Add(Client);
        }

        public void Start()
        {
            if(TcpServer.startListening())
            {
                CCstLogging.Logger.writeInLog(true, "Server has been started successfully!");
            }
            else
            {
                CCstLogging.Logger.writeInLog(true, "ERROR: The server was not able to start!");
            }
           
        }

        #region Protocol
        private void NetworkProtocol(networkServer.networkClientInterface NetworkClient, string message)
        {
            try
            {
                message = message.TrimEnd('\0');
                //Decrypt received protocol

                List<char> Chars = message.ToList();
                message = AES_Converter.DecryptFromCBC(CCstConfig.EncryptionKey, CCstConfig.EncryptionIV, message);
            }
            catch (Exception e)
            {
                //If decryption failed, something was probably manipulated -> Log it
                CCstLogging.Logger.writeInLog(true, "Protocol Decryption failed! From xy - message: " + e.ToString());
                return;
            }

            CCstLogging.Logger.writeInLog(true, "Protocol received: " + message);
            ProtocolController.RecievedProtocol(NetworkClient, message);
        }

        
        void AddUserToActiveConnections(string ComputerID, Boolean architecture, String language, double version, Boolean auth)
        {

            networkServer.networkClientInterface Client = new networkServer.networkClientInterface();
            Client.User.ID = ComputerID;

            if(ActiveConnections.Contains(Client))
            {
                //User is already registered
                //Kick User?
                CCstLogging.Logger.writeInLog(true, "User is already added to list!");
                return;
            }
            else
            {
                //Client.User.
            }




            ActiveConnections.Add(Client);
        }


        public void SendProtocol(string Protocol, networkServer.networkClientInterface ClientInterface)
        {
            //List<networkServer.networkClientInterface> Clients = ActiveConnections.Where(asd => asd.User.ID == computerID).ToList();
            //if (Clients.Count == 1)
            //{
            //encrypt protocol
            Protocol = AES_Converter.EncryptWithCBC(CCstConfig.EncryptionKey, CCstConfig.EncryptionIV, Protocol) + "~";
                //TcpServer.sendMessage(Protocol, Clients[0]);
                if (ActiveConnections.Count>0)
                    TcpServer.sendMessage(Protocol, ClientInterface);
            //}
            //else
            //{
            //    //Usually there cannot be 2 clients with the same Computer ID
            //    CCstLogging.Logger.writeInLog(true, String.Format("SendProtocol - found Clients with same Computer ID: {0}, ID: {1}, Protocol: {2}", Clients.Count, computerID, Protocol));
            //}
                
        }
        #endregion

        //private void NetworkProtocol(string message)
        //{
        //    ProtocolController contr = new ProtocolController(this);

        //    contr.RecievedProtocol(message);
        //}


        #region Support functions
        //private List<string> GetProtocolData(string message)
        //{
        //    return message.Split(cProtocolDelimiter).ToList();
        //}

        private string GetProtocolShortcut(string message)
        {
            return message.Split(cProtocolDelimiter)[0];
        }
        private string GetProtocolMessage(string message)
        {
            try
            {
                return message.Substring(GetProtocolShortcut(message).Length + 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "-";
            }
            
        }
        #endregion
        
    }
}
