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
    public class ControllerCore : IDisposable
    {
        //Variablen
        public networkServer TcpServer;
        public List<networkServer.networkClientInterface> ActiveConnections = null;
        public _ProtocolController ProtocolController;

        private string sAesKey;
        private char cProtocolDelimiter;
        private char cDataDelimiter;
        public Classes.Entity.EApplication Application;

        public bool ConfigureSuccessful = false;
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
            
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            //Logging initialisations
            Support.logWriter Logger = new logWriter(_sLogPath, Path.Combine(Path.GetDirectoryName(_sLogPath), "DetectionLog"), LogLevel);
            Logger.Seperate();
            Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.SERVER, "Logging class initialized!");
            DBEngine dBEngine = null;

            //Database Initialisations
            if (_sDatabaseDriver == "mysql")
            {
                // mysql isn't supported yet
                // CCstDatabase.DatabaseEngine = new DBMysqlDataManager(_sDBHostIp,_sDBUser,_sDBPass,_sDBPort,_sDBDefaultDB);

            } else if (_sDatabaseDriver == "mssql")
            {
                // create the mssql manager
                dBEngine = new DBMssqlDataManager(_sDBHostIp, _sDBUser, _sDBPass, _sDBPort, _sDBDefaultDB);
            }


            Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.SERVER, "Testing DB connection...");
            //Database test
            if (dBEngine.testDBConnection())
            {
                Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.DATABASE, "Database test successfull!");
            } else
            {
                Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.DATABASE, "Database test was not successfull!");
                return;
            }
            // try to get the application ID for the given application out of the DB
            Application = SApplication.GetByName(_ApplicationName, dBEngine);


            if (Application == null)
            {
                Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.DATABASE, "The application name was not found in the database!");
                return;
            }
            Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.DATABASE, "Application ID: " + Application.ID + ", Name: " + Application.Name);

            Logger.ApplicationID = Application.ID;
            // Create a new config object to be able to use specific functions like logging in all classes by getting the instance via application information
            CCstData Config = new CCstData(Application, dBEngine, Logger);

            // Check if the config management works by repeating the database test
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
            if (!File.Exists(PathGameDll))
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



            ActiveConnections = new List<networkServer.networkClientInterface>();
            sAesKey = "";
            this.cProtocolDelimiter = _cProtocolDelimiter;
            this.cDataDelimiter = _cProtocolDelimiter;
            TcpServer = new networkServer(NetworkProtocol, sAesKey, Application.ID, IPAddress.Any, _iPort, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _ProtocolController.SendProtocol += this.SendProtocol;
            Logger.writeInLog(1, LogCategory.OK, Support.LoggerType.SERVER, "TCP Server ready for start!");
            Logger.Seperate();
            ProtocolController = new _ProtocolController(cProtocolDelimiter, ref this.ActiveConnections, Application.ID);

            if (!CCstData.GetInstance(Application).GameDLL.PrepareServer(ConfigPath, ProtocolController.ReceiveGameDLLCallback))
                return;
            //ProtocolController.ReceivedProtocol(null, "500;23");
            ConfigureSuccessful = true;
        }

        public void Start()
        {
            if (TcpServer.startListening())
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
            CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.SERVER, "Disposing Server!");
            int Attempt = 0;
            //Kick all connected clients and remove the objects
            while (ActiveConnections.Count > 0 && Attempt++ < 5)
            {
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.SERVER, "Disposing Server - kicking players attempt " + Attempt.ToString());
                KickAllPlayers();
                System.Threading.Thread.Sleep(5000);
            }


            CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.SERVER, "Disposing Server - disposing network client!");
            //Close network server
            TcpServer.Dispose();

            CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.ERROR, Support.LoggerType.SERVER, "Disposing Server - removing instance from list - finished!");
            //Remove instance from CCstData
            if (CCstData.InstanceExists(Application.Hash))
                CCstData.InstanceClose(Application.ID);
        }

        public int KickAllPlayers()
        {
            int AmountofKicks = 0;
            foreach (var item in ActiveConnections)
            {
                ProtocolController.KickUser(item);
                AmountofKicks++;
            }
            return AmountofKicks;
        }
        #endregion

        #region Protocol

        #region Protection layer against people who manipulate protocols
        class HackPlayers { public IPAddress IP; public int Counter; public DateTime LastAttempt; }
        class SuspiciousPlayers
        {
            public List<HackPlayers> Hackers = new List<HackPlayers>();

            public int AllowProtocol(IPAddress IP, bool HackAttempt = false)
            {
                int Attempt = 0;
                //Check if sender is marked as hacker
                for (int i = 0; i < Hackers.Count; i++)
                {
                    if (Hackers[i].IP.ToString() == IP.ToString())
                    {
                        //If yes and the current protocol is a hack attempt, notice it
                        if (HackAttempt)
                        {
                            Hackers[i].Counter++;
                            Hackers[i].LastAttempt = DateTime.Now;
                        }
                        Attempt = Hackers[i].Counter;
                        //Deny any protocol of the hacker
                        return Attempt;
                    }
                }

                //If player is not hacker & did not try to hack, proceed the protocol
                if (!HackAttempt)
                    return Attempt;

                //If the player tried to hack, add to the list
                Hackers.Add(new HackPlayers() { IP = IP, Counter = 1, LastAttempt = DateTime.Now });
                //Deny protocol of the hacker
                return 1;
            }

            public bool BlockIP(IPAddress IP)
            {
                bool IPExists = false;
                foreach (var item in Hackers)
                {
                    if (item.IP.ToString() == IP.ToString())
                        IPExists = true;
                }
                if (IPExists)
                    return false;

                Hackers.Add(new HackPlayers() { IP = IP, Counter = 1, LastAttempt = DateTime.Now });
                return true;
            }

            public bool RemoveIP(IPAddress IP)
            {
                foreach (var item in Hackers)
                {
                    if (item.IP.ToString() == IP.ToString())
                    {
                        Hackers.Remove(item);
                        return true;
                    }
                }
                return false;
            }

            public void RemoveAll()
            {
                Hackers.Clear();
            }
        }

        static SuspiciousPlayers suspiciousPlayers = new SuspiciousPlayers();
        #endregion

        public void NetworkProtocol(ref networkServer.networkClientInterface NetworkClient, string message, DateTime TimeStampStart)
        {
            //Assign current IP to the NetworkClient
            NetworkClient.CheckIP(Application.ID);
            int HackAttempts = 0;
            try
            {
                if (message == null || message.Length == 0)
                    throw new Exception("Protocol message null or length=0");

                if (message[message.Length - 1] == '\0')
                {
                    message = message.Substring(0, message.Length - 1);
                }
                //Decrypt received protocol
                message = AES_Converter.DecryptFromCBC(CCstData.GetInstance(Application).EncryptionKey, CCstData.GetInstance(Application).EncryptionIV, message);
                if (message[message.Length - 1] == '\0')
                {
                    message = message.Substring(0, message.Length - 1);
                }
            }
            catch (Exception e)
            {
                //If decryption failed, something was probably manipulated
                //Store the IP as potential hacker to block further protocols
                HackAttempts = suspiciousPlayers.AllowProtocol(NetworkClient.IP, true);

                //Log the attempt
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.CRITICAL, Support.LoggerType.SERVER, "Protocol Decryption failed! (" + AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now) + ") Message: " + message + " (" + NetworkClient.IP + ") Attempt " + HackAttempts.ToString() + ", Error: " + e.ToString());
                NetworkClient.Dispose();
                return;
            }

            //Check if the protocol sender previously tried to manipulate protocols
            if ((HackAttempts = suspiciousPlayers.AllowProtocol(NetworkClient.IP)) > 0)
            {
                CCstData.GetInstance(Application).Logger.writeInLog(2, LogCategory.ERROR, Support.LoggerType.SERVER, String.Format("Hacker sent encryptable protocol. IP {0} Hack Attempts {1}, message {2} ({3})", NetworkClient.IP, HackAttempts, message, AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now)));
                NetworkClient.Dispose();
                return;
            }

            CCstData.GetInstance(Application).Logger.writeInLog(3, LogCategory.OK, Support.LoggerType.SERVER, "Protocol received decrypted: " + message + " (" + AdditionalFunctions.CalcDifferenceMS(TimeStampStart, DateTime.Now) + ")");

            //Proceed protocol
            ProtocolController.ReceivedProtocol(NetworkClient, message, TimeStampStart);
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
            CCstData.GetInstance(Application).Logger.writeInLog(4, LogCategory.OK, Support.LoggerType.SERVER, String.Format("Protocol encrypted: {0} ({1})", EncryptedProt, Protocol));

            TcpServer.sendMessage(EncryptedProt, ClientInterface);
        }
        #endregion

        #region Support functions
        public void CheckPings()
        {
            string PingCheck = "";
            int PingTimer = CCstData.GetInstance(Application).PingTimer;

            foreach (var item in ActiveConnections)
            {
                TimeSpan PingStatus = (DateTime.Now - item._LastPing);
                if (PingStatus.TotalMilliseconds + 2000 > PingTimer)
                {
                    PingCheck += String.Format("User {0}, IP {1}, LoginTime {2}, LastPing {3}, Difference {4} sec - ", item.User.ID, item.IP, item.ConnectedTime.ToShortTimeString(), item._LastPing.ToShortTimeString(), PingStatus.TotalSeconds);
                }
            }
            if (PingCheck.Length == 0)
                PingCheck = "PingCheck: Everything is alright!";
            else
                PingCheck = "PingCheck: Inconsistencies! (Max PingTimer: " + PingTimer.ToString() + " ms) \n " + PingCheck;

            CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, PingCheck);
        }

        public void GetBlockedIPs()
        {
            if (suspiciousPlayers.Hackers.Count == 0)
            {
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, "Check blocked IPs: Nobody is blocked!");
                return;
            }
            
            string Feedback = "The following IPs are blocked:\n";
            foreach (var item in suspiciousPlayers.Hackers)
            {
                Feedback += String.Format("IP {0}, attempts {1}, last attempt {2}\n", item.IP, item.Counter, item.LastAttempt.ToShortTimeString());
            }


            CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, Feedback);
        }

        public bool BlockIP(string sIP)
        {
            IPAddress IP;
            if (!IPAddress.TryParse(sIP, out IP))
            {
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, String.Format("BlockIP: {0} is not an IP!", sIP));
                return false;
            }
            bool bBlockSucceeded = suspiciousPlayers.BlockIP(IP);
            if (bBlockSucceeded)
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, String.Format("BlockIP: {0} blocked!", sIP));
            else
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, String.Format("BlockIP: {0} could not be blocked!", sIP));
            return bBlockSucceeded;
        }

        public bool RemoveBlockIP(string sIP)
        {
            IPAddress IP;
            if (!IPAddress.TryParse(sIP, out IP))
            {
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, String.Format("Remove BlockIP: {0} is not an IP!", sIP));
                return false;
            }
            bool bBlockSucceeded = suspiciousPlayers.RemoveIP(IP);
            if (bBlockSucceeded)
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, String.Format("Remove BlockIP: {0} unblocked!", sIP));
            else
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, String.Format("Remove BlockIP: {0} could not be unblocked!", sIP));
            return bBlockSucceeded;
        }

        public void BlockIPClear()
        {
            CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.OK, LoggerType.SERVER, String.Format("Clear BlockIP: Removed {0} entries!", suspiciousPlayers.Hackers.Count));
            suspiciousPlayers.RemoveAll();
        }
        #endregion
        
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            string DateFormatLog = String.Format("{0:dd.MM HH:mm:ss (fff)}", DateTime.Now);

            Console.WriteLine(String.Format("arrived! {0} - {1}", Application != null, Application.ID));
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine(e.Message);

            string Error = "Unhandled Exception: isTerminating " + args.IsTerminating.ToString() + ", Error " + e.Message;
            if (e.InnerException != null)
                Error += e.InnerException.Message+"\n";
            Error += "StackTrace " + e.StackTrace + "\n";
            //Error += "TargetSite " + e.TargetSite.Name;
            if(Application!=null)
                CCstData.GetInstance(Application).Logger.writeInLog(1, LogCategory.CRITICAL, LoggerType.SERVER, Error);
           
            Console.WriteLine("Exit this");
            //Environment.Exit(1);

        }
    }
}
