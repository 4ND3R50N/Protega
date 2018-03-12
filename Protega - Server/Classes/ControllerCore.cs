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
using System.IO;

namespace Protega___Server.Classes.Core
{
    public class ControllerCore:IDisposable
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
        /// <summary>
        /// To initial the core we need to know many parameters to be flexible for different clients.
        /// We for example want the client to decide where to save the log file
        /// </summary>
        /// <param name="_ApplicationName"></param> Name of the application, that each client can have more then one game running on this server
        /// <param name="LatestClientVersion"></param> ////////////////////////////////////////// what is this for? usecase?
        /// <param name="_iPort"></param> port for connection to the clients. Connection is made with the NetworkServer
        /// <param name="_cProtocolDelimiter"></param> This is the delemiter used to seperate the values of each protocol for the client site! Only the c code is using this dilimiter. The recieved protocols have all ; as delemiter
        /// <param name="_EncryptionKey"></param> /////////////////////////////////////////////// 
        /// <param name="_EncryptionIV"></param> ////////////////////////////////////////////////
        /// <param name="_PingTimer"></param> The client can decide how often the clients need to ping, e.g. every second. This is to check if the client somehow killed our application
        /// <param name="SessionLength"></param> How long shall be the session ID, that is generated randomly
        /// <param name="_sDatabaseDriver"></param> only mssql supported yet. Mysql in planning
        /// <param name="_sDBHostIp"></param> ///////////////////////////////////////////////////
        /// <param name="_sDBPort"></param> /////////////////////////////////////////////////////
        /// <param name="_sDBUser"></param> /////////////////////////////////////////////////////
        /// <param name="_sDBPass"></param> /////////////////////////////////////////////////////
        /// <param name="_sDBDefaultDB"></param> ////////////////////////////////////////////////
        /// <param name="_sLogPath"></param> Where is the logging file going to be saved
        /// <param name="LogLevel"></param> What level shall be logged? Everything = 3, All but debug = 2, Critical and Error = 1, only critical = 0
        /// <param name="PathGameDll"></param> Where is the game specific dll saved? This is needed for dynamicaly use functions for all kind of games like blocking a user.
        public ControllerCore(string _ApplicationName, int LatestClientVersion, short _iPort, char _cProtocolDelimiter, string _EncryptionKey, string _EncryptionIV, int _PingTimer, int SessionLength, string _sDatabaseDriver,
            string _sDBHostIp, short _sDBPort, string _sDBUser, string _sDBPass, string _sDBDefaultDB, string _sLogPath, int LogLevel, string PathGameDll)
        {
            //Logging initialisations
            Support.logWriter Logger = new logWriter(_sLogPath, LogLevel);
            Logger.Seperate();
            Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.SERVER, "Logging class initialized!");
            DBEngine dBEngine = null;
            
            //Database Initialisations
            if(_sDatabaseDriver == "mysql")
            {
               // mysql isn't supported yet
               // CCstDatabase.DatabaseEngine = new DBMysqlDataManager(_sDBHostIp,_sDBUser,_sDBPass,_sDBPort,_sDBDefaultDB);

            }else if(_sDatabaseDriver == "mssql")
            {
                // create the mssql manager
                dBEngine = new DBMssqlDataManager(_sDBHostIp, _sDBUser, _sDBPass, _sDBPort, _sDBDefaultDB);
            }


            //Database test
            if (dBEngine.testDBConnection())
            {
                Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.DATABASE, "Database test successfull!");
            }else
            {
                Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.DATABASE, "Database test was not successfull!");
                return;
            }
            // try to get the application ID for the given application out of the DB
            Application = SApplication.GetByName(_ApplicationName, dBEngine);
            // QUESTION: usually the procedure creates a new ID for a new application so this case should never apear?
            if(Application==null)
            {
                Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.DATABASE, "The application name was not found in the database!");
                return;
            }

            Logger.ApplicationID = Application.ID;
            // Create a new config object to be able to use specific functions like logging in all classes by getting the insance via application information
            CCstData Config = new CCstData(Application, dBEngine, Logger);
            // test if connection to database is still working after safing it in the config
            if (CCstData.GetInstance(Application.ID).DatabaseEngine.testDBConnection())
            {
                CCstData.GetInstance(Application.ID).Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.SERVER, "Instance successfully created!");
            } else
            {
                Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.SERVER, "Instance could not be created!");
                return;
            }

            // fill the config with the needed values
            CCstData.GetInstance(Application.ID).LatestClientVersion = LatestClientVersion;
            CCstData.GetInstance(Application.ID).EncryptionKey = _EncryptionKey;
            CCstData.GetInstance(Application.ID).EncryptionIV = _EncryptionIV;
            CCstData.GetInstance(Application.ID).PingTimer = _PingTimer;
            CCstData.GetInstance(Application.ID).SessionIDLength = SessionLength;

            // Check if the user really included the path with the needed URL.            
            if(!File.Exists(PathGameDll))
            {
                // this would be a critical error because we need the functions of the DLL to be a productive server
                Logger.writeInLog(1, LogCategory.CRITICAL, LoggerType.SERVER, String.Format("Game Dll not found! Path: {0}", PathGameDll));
                return;
            }
            // create the path to the config file. It always MUST have the name: config.ini
            string ConfigPath = Path.Combine(Path.GetDirectoryName(PathGameDll), "config.ini");
            if (!File.Exists(ConfigPath))
            {
                // this would be a critical error because we need the functions of the config file to be a productive server
                Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, String.Format("Game Config file not found! Path: ", ConfigPath));
                return;
            }
            // save the found DLL in our universal config object
            CCstData.GetInstance(Application).GameDLL = new Utility.ApplicationAdapter(Path.GetFullPath(PathGameDll), Application);
            // check if the constructor of the given DLL is working. This is important to make sure that we included the right DLL
            if (!CCstData.GetInstance(Application).GameDLL.ConstructorSuccessful)
                return;

            CCstData.GetInstance(Application).GameDLL.PrepareServer(ConfigPath);
            


                //Block Linux Ports
                /*SshClient unixSshConnectorAccept = new SshClient(LinuxIP, LinuxPort, LinuxLogin, LinuxPass);

                try
                {
                    unixSshConnectorAccept.Connect();
                    if (!unixSshConnectorAccept.IsConnected)
                        return;
                    unixSshConnectorAccept.RunCommand("/root/.firewall.sh");
                    Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.GAMEDLL, "IP Table command executed!");
                    //if (Res.Error.Length != 0)
                        //CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.CRITICAL, LoggerType.GAMEDLL, "Cannot execute start command!");
                }
                catch (Exception e)
                {
                    Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.GAMEDLL, "Cannot connect to Linux Server");
                    return;
                }
                finally
                {
                    unixSshConnectorAccept.Disconnect();
                }

                string PuttyStringBuilder = "";
                //PuttyStringBuilder += " service iptables stop";
                //PuttyStringBuilder += " && iptables -F";
                //PuttyStringBuilder += " && iptables -Z";
                //PuttyStringBuilder += " && iptables -X";
                //PuttyStringBuilder += " && iptables -A INPUT -p tcp --destination-port 80 -j DROP";
                //PuttyStringBuilder += " && iptables -I INPUT -p all -s 112.211.180.233 -j ACCEPT";
                //PuttyStringBuilder += " && iptables -I INPUT -p all -s 62.138.6.50 -j ACCEPT";
                //PuttyStringBuilder += " && iptables -I INPUT -p all -s 167.88.15.104 -j ACCEPT";
                //PuttyStringBuilder += " && iptables -I INPUT -p all -s 142.44.136.74 -j ACCEPT";
                //PuttyStringBuilder += " && iptables -I INPUT -p all -s 169.255.124.234 -j ACCEPT";
                //PuttyStringBuilder += " && iptables -I INPUT -p all -s 169.255.124.206 -j ACCEPT";
                //PuttyStringBuilder += " && iptables -I INPUT -p all -s 167.88.15.102 -j ACCEPT";
                //PuttyStringBuilder += " && service iptables save";
                //PuttyStringBuilder += " && service iptables start";

                //SshCommand testing = unixSshConnectorAccept.RunCommand(PuttyStringBuilder);
                //string Result1 = testing.Result;
                //string Err = testing.Error;
                //SshCommand test = unixSshConnectorAccept.RunCommand("service iptables stop");

                //SshCommand Test4 = unixSshConnectorAccept.RunCommand("cd ../etc/ppp/");
                //SshCommand Test3 = unixSshConnectorAccept.RunCommand("ls -l");
                //string Res = Test3.Result;
                //string Res2 = Test2.Result;

                //SshCommand Test1=unixSshConnectorAccept.RunCommand("./PX2000.sh");
                //string Error = Test1.Error;
                //string Error2 = Test2.Error;



                //Clear IPTables
                //unixSshConnectorAccept.RunCommand("iptables -F");
                //unixSshConnectorAccept.RunCommand("iptables -Z");
                //unixSshConnectorAccept.RunCommand("iptables -X");
                //unixSshConnectorAccept.RunCommand("iptables -F FORWARD");

                //unixSshConnectorAccept.RunCommand("iptables -F OUTPUT");
                //Block World-Ports
                if (DeactivatePortBlocking)
                    return;

                List<string> Ports = new List<string>();
                Ports.Add("12001");
                Ports.Add("12002");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                Ports.Add("12003");
                //Ports.Add("entextnetwk");


                foreach (string item in Ports)
                {
                    //Bestimmte Ports blocken
                    //unixSshConnectorAccept.RunCommand("iptables -I INPUT -p tcp --dport " + item + " -j DROP");
                    //unixSshConnectorAccept.RunCommand("iptables -A INPUT -p tcp --destination-port " + item + " -j DROP");
                }

                //unixSshConnectorAccept.RunCommand("service iptables save");
                //unixSshConnectorAccept.RunCommand("service iptables start");

                //unixSshConnectorAccept.Disconnect();
                */

                //Network Initialisations, this list includes all active connections to the client part of our project
                // this is needed for sending them protocols because there is no permanent connection to each client
                // it is also needed to check if all of them are still pinging
                ActiveConnections = new List<networkServer.networkClientInterface>();
            sAesKey = "";
            this.cProtocolDelimiter = _cProtocolDelimiter;
            this.cDataDelimiter = _cProtocolDelimiter;
            TcpServer = new networkServer(NetworkProtocol, sAesKey, Application.ID, IPAddress.Any, _iPort, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
            // QUESTION: What is this doing?
            ProtocolController.SendProtocol += this.SendProtocol;
            Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.SERVER, "TCP Server ready for start!");
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
            //  NetworkProtocol("#218", ref dummy);
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
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.SERVER, "Server has been started successfully!");
            }
            else
            {
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.SERVER, "The server was not able to start!");
            }

        }

        #region Destructor
        public void Dispose()
        {
            //Kick all connected clients and remove the objects
            foreach (networkServer.networkClientInterface item in ActiveConnections)
            {
                item.Dispose();
            }

            //Close network server
            TcpServer.Dispose();

            //Remove instance from CCstData
            if (CCstData.InstanceExists(Application.Hash))
                CCstData.InstanceClose(Application.ID);
        }
        #endregion

        #region Protocol
        public void NetworkProtocol(ref networkServer.networkClientInterface NetworkClient, string message)
        {
            //Public for the Unit Tests
            CCstData.GetInstance(Application).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "Protocol received: " + message);
            try
            {
                //Öañ4\u001b3[\b\nÎbÞö}\u0010VDYZ‚\u009d\u0005sQ˜e@p•\u001e\ab{ó¥Ÿ›¨YÉ`\\wõˆ¹éî\0
                if (message[message.Length-1]=='\0')
                {
                    message = message.Substring(0, message.Length - 1);
                }
                //Decrypt received protocol
                // QUESTION: Can we somehow see if we get something that isn't encrypted like traffic?
                // QUESTION: Is it possible to decrypt something that isn't enchrypted and send it to the ProtocolController?
                // QUESTION: Or does the decryption function checck all this?
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
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, "Protocol Decryption failed! Message: " + message + ", Error: " + e.ToString());
                return;
            }
            CCstData.GetInstance(Application).Logger.writeInLog(2, LogCategory.OK, Support.LoggerType.SERVER, "Protocol received decrypted: " + message);
            ProtocolController.ReceivedProtocol(NetworkClient, message);
        }

        public void SendProtocol(string Protocol, networkServer.networkClientInterface ClientInterface)
        {
            //encrypt protocol
            string EncryptedProt = AES_Converter.EncryptWithCBC(CCstData.GetInstance(Application).EncryptionKey, CCstData.GetInstance(Application).EncryptionIV, Protocol);
            string LengthAddition = EncryptedProt.Length.ToString();
            while (LengthAddition.Length < 3)
            {
                LengthAddition = "0" + LengthAddition;
            }
            EncryptedProt = LengthAddition + EncryptedProt;
            CCstData.GetInstance(Application).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Protocol encrypted: {0} ({1})", EncryptedProt, Protocol));

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
