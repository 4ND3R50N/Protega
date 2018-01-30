using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Support;
using System.Net;
using System.Net.Sockets;
using Protega___Server.Classes.Protocol;
using Renci.SshNet;

namespace Protega___Server.Classes.Core
{
    public class ControllerCore
    {
        //Variablen
        public networkServer TcpServer;
        public List<networkServer.networkClientInterface> ActiveConnections;
        public ProtocolController ProtocolController;
        
        private string sAesKey;
        private char   cProtocolDelimiter;
        private char   cDataDelimiter;
        public Classes.Entity.EApplication Application;

        //Konstruktor
        public ControllerCore(string _ApplicationName, short _iPort, char _cProtocolDelimiter, char _cDataDelimiter, string _sAesKey, string _sDatabaseDriver,
            string _sDBHostIp, short _sDBPort, string _sDBUser, string _sDBPass, string _sDBDefaultDB, string _sLogPath, int LogLevel)
        {
            //Logging initialisations
            Support.logWriter Logger = new logWriter(_sLogPath, LogLevel);
            Logger.Seperate();
            Logger.writeInLog(1, LogCategory.OK, "Logging class initialized!");
            DBEngine dBEngine = null;
            
            //Database Initialisations
            if(_sDatabaseDriver == "mysql")
            {
               // CCstDatabase.DatabaseEngine = new DBMysqlDataManager(_sDBHostIp,_sDBUser,_sDBPass,_sDBPort,_sDBDefaultDB);

            }else if(_sDatabaseDriver == "mssql")
            {
                dBEngine = new DBMssqlDataManager(_sDBHostIp, _sDBUser, _sDBPass, _sDBPort, _sDBDefaultDB);
            }


            //Database test
            if (dBEngine.testDBConnection())
            {
                Logger.writeInLog(1, LogCategory.OK, "Database test successfull!");
            }else
            {
                Logger.writeInLog(1, LogCategory.ERROR, "Database test was not successfull!");
                return;
            }

            Application = SApplication.GetByName(_ApplicationName, dBEngine);
            if(Application==null)
            {
                Logger.writeInLog(1, LogCategory.ERROR, "The application name was not found in the database!");
                return;
            }


            CCstData Config = new CCstData(Application, dBEngine, Logger);

            if (CCstData.GetInstance(Application.ID).DatabaseEngine.testDBConnection())
            {
                CCstData.GetInstance(Application.ID).Logger.writeInLog(1, LogCategory.OK, "Instance successfully created!");
            } else
            {
                Logger.writeInLog(1, LogCategory.ERROR, "Instance could not be created!");
                return;
            }

            //Block Linux Ports

            SshClient unixSshConnectorAccept = new SshClient("62.138.6.50", 2223, "root", "xCodeZer0");
            unixSshConnectorAccept.Connect();
            if(!unixSshConnectorAccept.IsConnected)
            {
                Logger.writeInLog(1, LogCategory.ERROR, "Cannot connect to Linux Server");
                return;
            }


            unixSshConnectorAccept.RunCommand("sudo service iptables stop");
            //Clear IPTables
            unixSshConnectorAccept.RunCommand("sudo iptables -F");
            unixSshConnectorAccept.RunCommand("sudo iptables -Z");
            unixSshConnectorAccept.RunCommand("sudo iptables -X");
            //unixSshConnectorAccept.RunCommand("iptables -F FORWARD");

            //unixSshConnectorAccept.RunCommand("iptables -F OUTPUT");




            //Block World-Ports
            List<string> Ports = new List<string>();
            Ports.Add("sudo 12001");
            Ports.Add("sudo 12002");
            Ports.Add("sudo 12003");
            //Ports.Add("entextnetwk");
            
            foreach (string item in Ports)
            {
                //Bestimmte Ports blocken
                //unixSshConnectorAccept.RunCommand("sudo iptables -I INPUT -p tcp --dport " + item + " -j DROP");
                unixSshConnectorAccept.RunCommand("sudo iptables -A INPUT -p tcp --destination-port " + item + " -j DROP");
            }

            unixSshConnectorAccept.RunCommand("sudo service iptables save");
            unixSshConnectorAccept.RunCommand("sudo service iptables start");

            unixSshConnectorAccept.Disconnect();


                //Network Initialisations
                ActiveConnections = new List<networkServer.networkClientInterface>();
            sAesKey = _sAesKey;
            this.cProtocolDelimiter = _cProtocolDelimiter;
            this.cDataDelimiter = _cDataDelimiter;
            TcpServer = new networkServer(NetworkProtocol, _sAesKey, IPAddress.Any, _iPort, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
            ProtocolController.SendProtocol += this.SendProtocol;
            Logger.writeInLog(1, LogCategory.OK, "TCP Server ready for start!");
            Logger.Seperate();
            ProtocolController = new ProtocolController(ref ActiveConnections, Application.ID);

            /*//TESTCASE
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            //Registration
            ProtocolController.ReceivedProtocol(dummy, "500;98765;Test;Windoofs 7;Deutsch;1");
            string SessionID = "ASDASD";
            ActiveConnections[0].SessionID = SessionID;

            ProtocolController.ReceivedProtocol(dummy, String.Format("600;{0}", SessionID));
            System.Threading.Thread.Sleep(1000);
            ProtocolController.ReceivedProtocol(dummy, String.Format("600;{0}", SessionID));

            //ProtocolController.ReceivedProtocol(dummy, String.Format("701;{0};Prozess", SessionID));

            dummy.SessionID = SessionID;
            ProtocolController.ReceivedProtocol(dummy, String.Format("701;{0};Process;Window;Class;MD5",SessionID));
            */
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
        
        public void Start()
        {
            if(TcpServer.startListening())
            {
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, "Server has been started successfully!");
            }
            else
            {
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.ERROR, "The server was not able to start!");
            }
           
        }

        #region Protocol
        public void NetworkProtocol(networkServer.networkClientInterface NetworkClient, string message)
        {
            //Public for the Unit Tests
            CCstData.GetInstance(Application).Logger.writeInLog(2, LogCategory.OK, "Protocol received: " + message);
            try
            {
                //Öañ4\u001b3[\b\nÎbÞö}\u0010VDYZ‚\u009d\u0005sQ˜e@p•\u001e\ab{ó¥Ÿ›¨YÉ`\\wõˆ¹éî\0
                if (message[message.Length-1]=='\0')
                {
                    message = message.Substring(0, message.Length - 1);
                }
                //Decrypt received protocol

                List<char> Chars = message.ToList();
                message = AES_Converter.DecryptFromCBC(CCstData.GetInstance(Application).EncryptionKey, CCstData.GetInstance(Application).EncryptionIV, message);
                if (message[message.Length - 1] == '\0')
                {
                    message = message.Substring(0, message.Length - 1);
                }
            }
            catch (Exception e)
            {
                //If decryption failed, something was probably manipulated -> Log it
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.CRITICAL, "Protocol Decryption failed! Message: " + e.ToString());
                return;
            }

            CCstData.GetInstance(Application).Logger.writeInLog(2, LogCategory.OK, "Protocol received decrypted: " + message);
            ProtocolController.ReceivedProtocol(NetworkClient, message);
        }

        public void SendProtocol(string Protocol, networkServer.networkClientInterface ClientInterface)
        {
            //encrypt protocol
            string EncryptedProt = AES_Converter.EncryptWithCBC(CCstData.GetInstance(Application).EncryptionKey, CCstData.GetInstance(Application).EncryptionIV, Protocol) + "~";
            CCstData.GetInstance(Application).Logger.writeInLog(3, LogCategory.OK, String.Format("Protocol encrypted: {0} ({1})", EncryptedProt, Protocol));

            TcpServer.sendMessage(EncryptedProt, ClientInterface);
        }
        #endregion

        #region Support functions
        //private List<string> GetProtocolData(string message)
        //{
        //    return message.Split(cProtocolDelimiter).ToList();
        //}

        //private string GetProtocolShortcut(string message)
        //{
        //    return message.Split(cProtocolDelimiter)[0];
        //}
        //private string GetProtocolMessage(string message)
        //{
        //    try
        //    {
        //        return message.Substring(GetProtocolShortcut(message).Length + 1);
        //    }
        //    catch (ArgumentOutOfRangeException)
        //    {
        //        return "-";
        //    }
            
        //}
        #endregion
        
    }
}
