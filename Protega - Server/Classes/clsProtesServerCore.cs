/*using System;
using System.Collections.Generic;
using System.Linq;
using Protes_cmdServer.git.classes_support;
using Protes_cmdServer.git.classes_network;
using System.Threading;
using System.Net;
using Protes_cmdServer.git.classes_db;
using Renci.SshNet;
using System.Diagnostics;

namespace Protes_cmdServer.git.classes_Server
{


    class ProtesServerCore
    {
        //Variables
       
        private string clientVersion = "";
        private string binomEncodeKey = "";
        private string basePathUri = Environment.CurrentDirectory;
        public bool serverBootedCorrectly = false;
        public bool serverIsRunning = false;
        private short clientTimeout = 0;

        //Management tool
        public bool managerIsLoggedIn = false;
        public short managementClientTimeout = 0;
        Stopwatch managementClientSession;

        private string sshRootIP = "";
        private string sshRootUser = "";
        private string sshRootPW = "";

        private string ftpIP = "";
        private string ftpID = "";
        private string ftpPass = "";
        private short ftpPort = 0;

        //Objects
        List<network_Server.networkClientInterface> connectionCollection;
        List<string> ipStorage;
        Queue<network_Server.networkClientInterface> sshAcceptQueue;
        Queue<network_Server.networkClientInterface> sshDcQueue;
        SshClient unixSshConnectorAccept;
        SshClient unixSshConnectorDc;

        //Main managers
        private ioDataManager dataManager;
        private network_Server networkManager;
        private PDBEngine databaseManager;

        //Threads
        private static Thread clientHandler;
        private static Thread sshDcHandler;
        private static Thread sshAcceptHandler;
        
        public ProtesServerCore(string logPath, string dataPath, string dataBinomKey,  string networkEncodeKey, short networkPort, short networkClientTimeout, string networkClientVersion, short managementClientTimeout,
            string dbDriver,
            string sqlIP, string sqlUser, string sqlPass, short sqlPort, string sqlDB_Protes, string sqlDB_Game,
            string sshIP, string sshUser, string sshPass,
            string ftpIP, string ftpID, string ftpPass, short ftpPort)
        {

            dataManager = new ioDataManager(logPath, dataPath);
            dataManager.writeInMainlog("[ProtesServerCore] Module initialized -> Staring logging -> Initialize all necessary objects...", true);
            networkManager = new network_Server(networkProtocol, networkEncodeKey, IPAddress.Any,
                networkPort,System.Net.Sockets.AddressFamily.InterNetwork,System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp);
            if(dbDriver == "mssql")
            {
                dataManager.writeInMainlog("DB Driver found: MSSQL -> Starting and testing mssql engine...", true);
                databaseManager = new DBMssqlProtesManager(sqlIP, sqlUser, sqlPass, sqlPort, sqlDB_Protes, sqlDB_Game, ref dataManager);

                if(!databaseManager.testDBConnection())
                {
                    return;
                }
            }
            else if(dbDriver == "mysql")
            {
                dataManager.writeInMainlog("DB Driver found: MYSQL -> Starting and testing the mysql engine...", true);
                databaseManager = new clsDBMysqlProtesManager(sqlIP, sqlUser, sqlPass, sqlPort, sqlDB_Protes, sqlDB_Game, ref dataManager);
                if (!databaseManager.testDBConnection())
                {
                    return;
                }
            }
            else
            {
                Console.WriteLine("Unknown DB Driver. Check your INI!");
                return;
            }
            //Initialize threads
            dataManager.writeInMainlog("Initialize threads...", true);
            clientHandler = new Thread(clientThread);
            sshAcceptHandler = new Thread(sshAcceptThread);
            sshDcHandler = new Thread(sshDisconnectThread);
            //Get important data from ini file
            dataManager.writeInMainlog("Get important data...", true);
            this.managementClientTimeout = managementClientTimeout;
            sshRootIP = sshIP;
            sshRootUser = sshUser;
            sshRootPW = sshPass;
            this.ftpIP = ftpIP;
            this.ftpID = ftpID;
            this.ftpPass = ftpPass;
            this.ftpPort = ftpPort; 


            clientTimeout = networkClientTimeout;
            clientVersion = networkClientVersion;
            binomEncodeKey = dataBinomKey;

            //Object init
            managementClientSession = new Stopwatch();
            connectionCollection = new List<network_Server.networkClientInterface>();
            sshAcceptQueue = new Queue<network_Server.networkClientInterface>();
            sshDcQueue = new Queue<network_Server.networkClientInterface>();
            ipStorage = new List<string>();
            serverBootedCorrectly = true;
            dataManager.writeInMainlog("Server successfully prepared for start!", true);

           
        }
        
        public void start()
        {
            //Starting network services
            dataManager.writeInMainlog("Starting Server...", true);
            if (networkManager.startListening())
            {
                dataManager.writeInMainlog("Network engine successfully started", true);
            }
            else
            {
                dataManager.writeInMainlog("Server was not able to start. Check the current status or try again on a different port!", true);
                return;
            }

            //Blocking ports
            dataManager.writeInMainlog("Block ports on " + sshRootIP + " ...", true);
            unixSshConnectorAccept = new SshClient(sshRootIP, sshRootUser, sshRootPW);
            unixSshConnectorAccept.Connect();
            //call iptable block + protection for cabal
            unixSshConnectorAccept.RunCommand("cd /root/");
            unixSshConnectorAccept.RunCommand("./PX2000.sh");



            foreach (var item in dataManager.getPorts())
            {
                unixSshConnectorAccept.RunCommand("iptables -I INPUT -p tcp --dport " + item + " -j DROP");
            }
            unixSshConnectorAccept.Disconnect();

            //Starting threads
            clientHandler.Start();
            sshAcceptHandler.Start();
            sshDcHandler.Start();
            dataManager.writeInMainlog("Server started!", true);
            serverIsRunning = true;
        }


        //Threads
        private void clientThread()
        {
            while (serverIsRunning)
            {
                Thread.Sleep(100);


                #region Ping check + Vote Reminder
                try
                {
                    foreach (var connection in connectionCollection)
                    {
                        if (connection.ping.Elapsed.Seconds >= 10)
                        {
                            closeConnection(connection);
                            break;
                        }
                        //if (connection.voteReminder.Elapsed.Minutes >= VoteRemindCount)
                        //{
                        //    VoteReminder(connection);
                        //}

                    }
                }
                catch (Exception)
                {

                    dataManager.writeInMainlog("Warning, Ping Routine error!", false);
                }

                #endregion
                //Game related routines
                #region Login Compare Cabal Online
                try
                {

                   
                        for (int i = 0; i < connectionCollection.Count; i++)
                        {
                            bool flag = false;

                            //Suche nach Account
                            foreach (var Account in databaseManager.getOnlineGameAccounts())
                            {


                                if (string.Equals(connectionCollection[i].gameUser, Account, StringComparison.OrdinalIgnoreCase))
                                {

                                    flag = true;
                                    connectionCollection[i].wasLoggedIn = true;
                                }
                            }
                            //Es wurde kein Account gefunden!
                            if (!flag)
                            {
                                if (connectionCollection[i].connectionTime.Elapsed.Minutes >= clientTimeout && connectionCollection[i].wasLoggedIn)
                                {
                                    connectionCollection[i].connectionTime.Reset();
                                    connectionCollection[i].connectionTime.Start();
                                    connectionCollection[i].wasLoggedIn = false;
                                    break;
                                }

                                if (connectionCollection[i].connectionTime.Elapsed.Minutes >= clientTimeout && connectionCollection[i].wasLoggedIn != true)
                                {
                                    dataManager.writeInMainlog("Login routine: irregularity found. User: " + connectionCollection[i].gameUser + " IP: " + connectionCollection[i].ip, false);
                                    networkManager.sendMessage( "#203error¶The server noticed that youre not logged in with the account you used in launcher. You get disconnected!", connectionCollection[i]);
                                    connectionCollection.Remove(connectionCollection[i]);
                                    Thread.Sleep(1000);

                                    //closeConnection(connectionCollection[i]);
                                    break;
                                }

                            }

                        }
                    
                }
                catch (Exception)
                {
                    dataManager.writeInMainlog("Warning: Login check routine failed!", false);
                }


                #endregion

                #region Management Client checks
                if(managementClientSession.Elapsed.Minutes >= managementClientTimeout)
                {
                    managerIsLoggedIn = false;
                    managementClientSession.Stop();
                    managementClientSession.Reset();
                }
                #endregion
            }

        }

        private void sshAcceptThread()
        {

            while (serverIsRunning)
            {

                if (sshAcceptQueue.Count > 0)
                {
                    
                    string wanIP = sshAcceptQueue.Peek().ip;
                    string mssqlIP = sshAcceptQueue.Peek().mssql_ip;
                    int counter = 0;
                    bool Success = false;
                    bool ipAlreadyExisting = false;
                    List<network_Server.networkClientInterface> tmp = new List<network_Server.networkClientInterface>(connectionCollection);
                    ////Check if ip is already in exceptions
                    for(int i = 0; i < tmp.Count; i++)
                    {
                        // WENN die validate mssql eine wanip in connectioncollection ist, wird
                        if (wanIP == tmp[i].ip || wanIP == tmp[i].mssql_ip)
                        {
                            counter++;
                            if (counter == 2)
                            {
                                ipAlreadyExisting = true;
                                break;
                            }

                        }
                    }


                    if (ipAlreadyExisting != true)
                    {
                        do
                        {
                            try
                            {
                                unixSshConnectorAccept = new SshClient(sshRootIP, sshRootUser, sshRootPW);
                                unixSshConnectorAccept.Connect();

                                foreach (var item in dataManager.getPorts())
                                {
                                    try
                                    {
                                        unixSshConnectorAccept.RunCommand("iptables -I INPUT -p tcp -s " + wanIP + " --dport " + item + " -j ACCEPT");
                                        if (mssqlIP != "" && mssqlIP != wanIP)
                                        {
                                            unixSshConnectorAccept.RunCommand("iptables -I INPUT -p tcp -s " + mssqlIP + " --dport " + item + " -j ACCEPT");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        dataManager.writeInMainlog("Error while applying exeptions! " + e.ToString(), true);
                                        Thread.Sleep(2000);
                                        Success = false;

                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                dataManager.writeInMainlog("Access to Unix root failed. Error: " + e.ToString(), true);
                                Success = false;
                            }
                            Success = true;
                            unixSshConnectorAccept.Disconnect();
                            dataManager.writeInMainlog("SSH Accept for IP: " + wanIP + " and MIP: " + mssqlIP + " successfull!", false);

                        } while (Success == false);
                    }
                    sshAcceptQueue.Dequeue();
                }                
            }
        }

        private void sshDisconnectThread()
        {

            while (serverIsRunning)
            {
                if (sshDcQueue.Count > 0)
                {
                    string wanIP = sshDcQueue.Peek().ip;
                    string mssqlIP = sshDcQueue.Peek().mssql_ip;
                    bool Success = false;

                    //ipStorage.Remove(wanIP);
                    do
                    {

                        try
                        {
                            unixSshConnectorDc = new SshClient(sshRootIP, sshRootUser, sshRootPW);
                            unixSshConnectorDc.Connect();

                            foreach (var item in dataManager.getPorts())
                            {

                                try
                                {
                                    unixSshConnectorDc.RunCommand("iptables -D INPUT -p tcp -s " + wanIP + " --dport " + item + " -j ACCEPT");
                                    if (mssqlIP != "" && mssqlIP != wanIP)
                                    {
                                        unixSshConnectorDc.RunCommand("iptables -D INPUT -p tcp -s " + mssqlIP + " --dport " + item + " -j ACCEPT");
                                    }
                                }
                                catch (Exception e)
                                {
                                    dataManager.writeInMainlog("Error while disapplying exeptions! " + e.ToString(), true);
                                    Thread.Sleep(2000);
                                    Success = false;
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            dataManager.writeInMainlog("Kickout failed! Retry... " + e.ToString(), true);
                            Thread.Sleep(2000);
                            Success = false;

                        }

                        unixSshConnectorDc.Disconnect();
                        sshDcQueue.Dequeue();
                        Success = true;
                        dataManager.writeInMainlog("SSH Kickout for IP: " + wanIP + " and MIP: " + mssqlIP + " successfull!", false);


                    } while (Success == false);


                }
            }

        }

        //Network block

        private void networkProtocol(string prot, ref network_Server.networkClientInterface client)
        {
            string[] parts = null;
            string ProtID = protocolAnalysing(prot, ref parts);

            //Protocoltypes:
            switch (ProtID)
            {
                //AS = Registration of client
                case "#101": protocol_101_authReceive(parts, client); break;
                //PS = Normal Ping
                case "#103": protocol_103_pingResponse(client); break;
                //Send decript key + md5
                case "#201": protocol_201_sendDecryptAndMD5(client); break;
                //HS = Hack detection from client
                case "#204": protocol_204_hackdetected(parts, client); break;
                //MANAGEMENT TOOL
                case "#301": protocol_301_loginManagementTool(parts, client); break;
                case "#303": protocol_303_startServer(client); break;
                case "#305": protocol_305_stopServer(client); break;
                case "#309": protocol_309_sendMessageToClients(parts, client); break;
                case "#313": protocol_313_screenAPlayer(parts, client); break;
                case "#315": protocol_315_screenAllPlayers(client); break;
                case "#319": protocol_319_kickoutAPlayer(parts, client); break;
                case "#323": protocol_323_getOnlinePlayerAmount(client); break;
                case "#XO":
                //Player report protocol
                case "#RS":

                    //string ReportedAccountName = "";
                    //string ReportedName = parts[0].Remove(0, 3);
                    ////Verbindungsaufbau
                    //using (SqlConnection ReportConnMssql =
                    //    new SqlConnection("Server=" + DBEngine._MssqlIP + ";Database=" + DBEngine._MssqlDBGame + ";User Id=" + DBEngine._MssqlUser + ";Password=" + DBEngine._MssqlPW + ";"))
                    //{
                    //    try
                    //    {
                    //        ReportConnMssql.Open();
                    //    }
                    //    catch (Exception)
                    //    {
                    //        _IOEngine.writeMainLog("Connection couldnt be open for a session <BannAccount>.", true);
                    //        return false;
                    //    }
                    //    //Get Reported Account name
                    //    ReportedAccountName = DBEngine.getAccountByCharacter(ReportConnMssql, ReportedName);
                    //    //Check, if online
                    //    foreach (ConnectionInfo element in _connections)
                    //    {

                    //        if (string.Equals(element.GameUser, ReportedAccountName, StringComparison.OrdinalIgnoreCase))
                    //        {
                    //            Send(element, "#SR");
                    //            //Write db entry
                    //            DBEngine.updateReportTable(ReportConnMssql, Conn.GameUser, ReportedName, parts[1]);
                    //        }
                    //        else { *//*War name entschlüsseln + neu suchen*//* }
                    //    }

                    //}
                    ////Write report log
                    //_IOEngine.writePlayerReportLog("Player (" + Conn.GameUser + ") reported (Character name: " + ReportedName + " || Account ID: " + ReportedAccountName + ")! Reason: " + parts[1], true);
                    //_IOEngine.writeMainLog("Player (" + Conn.GameUser + ") reported (Character name: " + ReportedName + " || Account ID: " + ReportedAccountName + ")! Reason: " + parts[1], false);
                    break;

                    

                default:
                    break;

            }
        }

        private void closeConnection(network_Server.networkClientInterface client)
        {

            dataManager.writeInMainlog("Disconnecting Client. IP: " + client.ip + " User: " + client.gameUser, false);
            networkManager.closeConnection(client);
            connectionCollection.Remove(client);
            sshDcQueue.Enqueue(client);
            databaseManager.setStatusOffline(client.guid);
            dataManager.writeInMainlog("Successfull for Client IP: " + client.ip + " and User: " + client.gameUser, false);

        }

        //Protocol functions - Antihack
        private bool protocol_101_authReceive(string[] parts, network_Server.networkClientInterface client)
        {
            
                #region Variablen/Pre setup
                short sendDecision = 0;
                int punishment = 0;
                //ConnectionInfo Conn = client.Client;
                //temporary bann variables
                Dictionary<string, string> TimeContent = new Dictionary<string, string>();
                int Time = 0;
                string BannText = "";            
                #endregion
            
                #region Version + GUID Blacklist check
                //Version check
                if (parts[0].Remove(0,4) != clientVersion)
                {
                    dataManager.writeInMainlog("Login failed for version " + parts[0].Remove(0,2) + " UserID: " + parts[5], true);
                    networkManager.sendMessage("#102false¶Clientversion is not up to date!", client);
                    return false;
                }
                //Guid Blacklist check
                foreach (var guid in dataManager.getGUIDBlacklist())
                {
                    if (parts[2] == guid)
                    {
                        dataManager.writeInMainlog("Blacklisted Guid! || GUID: " + parts[2] + " || LatestGameUser: " + parts[5], true);
                        networkManager.sendMessage("#203error¶Error: Youre blocked. Get in contact to the support to get more information!", client);
                        Thread.Sleep(1000);
                        //networkManager.sendMessage(Conn, "#ARfalse¶Error: BL! Contact admin!"); Brauchen wir das?!
                        return false;
                    }
                }

                //IP blacklist check
                foreach (var ip in dataManager.getIPBlacklist())
                {
                    if (parts[1] == ip)
                    {
                        dataManager.writeInMainlog("Blacklisted IP! || GUID: " + parts[1] + " || LatestGameUser: " + parts[5], true);
                        networkManager.sendMessage("#203Error: Youre blocked. Get in contact to the support to get more information!", client);
                        Thread.Sleep(1000);
                        //networkManager.sendMessage(Conn, "#ARfalse¶Error: BL! Contact admin!"); Brauchen wir das?!
                        return false;
                    }
                }
                #endregion

                #region Save data + register/update client in db
                //Data from protocol gets saved in the class
                client.ip = parts[1];
                Guid.TryParse(parts[2], out client.guid);
                client.os = parts[3];
                client.architecture = parts[4];
                client.gameUser = parts[5];

                if (client.guid == Guid.Empty)
                {
                    //Create new user
                    client.guid = databaseManager.createUserAccount(client.guid, client.ip, client.os, client.architecture, client.gameUser);
                    sendDecision = 1;
                }
                else
                {
                    //Update user
                    if(!databaseManager.updateUserAccount(client.guid, client.ip, client.os, client.architecture, client.gameUser))
                    {
                        return false;
                    }                
                    sendDecision = 0;
                }
                #endregion

                #region Check, if account has a temp bann
                //Getting Punishment and timelist for check!
                punishment = databaseManager.getPunishmentID(client.guid);
                TimeContent = dataManager.getTempBanList();

                //Temporary Bann check!   
                int counter = 0;
                foreach (var entry in TimeContent)
                {
                    if (counter == punishment)
                    {
                        Time = Convert.ToInt32(entry.Key);
                        BannText = entry.Value;
                    }
                    counter++;
                }

                //Entscheidung (Zeit drüber/ oder ok)
                DateTime BannDate = new DateTime();
                DateTime Now = new DateTime();
                Now = DateTime.Now;

                //Punishment =1
                if (Time == -1)
                {
                    networkManager.sendMessage("#203info¶You already has 1 strike! If you hack again, you get a temporary channel ban!", client);
                    Thread.Sleep(1000);
                }
                else
                {
                    BannDate = databaseManager.getLastBanndate(client.guid);
                    if (BannDate > Now)
                    {
                        dataManager.writeInMainlog("Connection refused. Temporary ban. IP: " + client.ip, true);
                        networkManager.sendMessage("#203" + BannText + " Your Ban ends on " + BannDate.Add(new TimeSpan(0, Time, 0)) + ".", client);
                        //Send(Conn, "#ARfalse¶" + BannText);
                        return false;
                    }
                }
                #endregion

                #region Root exeption wird erstellt + bestätigung an client gesendet
                client.mssql_ip = databaseManager.getMSSQLIP(client.gameUser).Replace(" ", string.Empty);

                //Activate connection to root
                connectionCollection.Add(client);
                sshAcceptQueue.Enqueue(client);


                client.connectionTime.Start();
                client.ping.Start();
                client.voteReminder.Start();

                dataManager.writeInMainlog("Registration from " + client.ip + ", User: " + client.gameUser + " successfull.", false);
                if (sendDecision == 1)
                {
                    networkManager.sendMessage("#102true¶0¶" + client.guid + "¶" + ftpIP + "¶" + ftpID + "¶" + ftpPass + "¶" + ftpPort, client);
                }
                else
                {
                    networkManager.sendMessage("#102true¶" + punishment + "¶" + "0" + "¶" + ftpIP + "¶" + ftpID + "¶" + ftpPass + "¶" + ftpPort, client); //ftp daten hinzufügen
                }
                #endregion

                //VoteReminder -  disabled
                //Thread.Sleep(2000);
                //VoteReminder(Conn);
                return true;

            }
        
        private void protocol_103_pingResponse(network_Server.networkClientInterface client)
        {
            networkManager.sendMessage("#104", client);
            client.ping.Restart(); //Enabled
        }

        private void protocol_201_sendDecryptAndMD5(network_Server.networkClientInterface client)
        {
            networkManager.sendMessage("#202" + dataManager.getMD5Key() + "¶" + binomEncodeKey, client);
        }

        private void protocol_204_hackdetected(string[] parts, network_Server.networkClientInterface client)
        {
            //Main process
            dataManager.writeInMainlog("Hack detected: " + client.ip + "|| HackID: " + parts[0].Remove(0,4) + "|| UserID: " + client.gameUser, true);
            int tmp = databaseManager.hackDetected(parts[0].Remove(0, 4), parts[1], client.guid, client.ip, client.gameUser);
            networkManager.sendMessage("#205" + tmp.ToString(), client);
        }

        //Protocol function - Management tool
        private void protocol_301_loginManagementTool(string[] parts, network_Server.networkClientInterface client)
        {
            if (databaseManager.loginManager(parts[0].Remove(0, 4), parts[1], Convert.ToInt16(parts[2])))
            {
                managerIsLoggedIn = true;
                managementClientSession.Start();
                networkManager.sendMessage("#302true", client);
                return;
            }
            networkManager.sendMessage("#302false", client);
        }

        private void protocol_303_startServer(network_Server.networkClientInterface client)
        {
            //needs to be implemented
        }

        private void protocol_305_stopServer(network_Server.networkClientInterface client)
        {
            //Needs to be implemented
        }

        private void protocol_309_sendMessageToClients(string[] parts, network_Server.networkClientInterface client)
        {
            //Needs to be implemented
        }

        private void protocol_313_screenAPlayer(string[] parts, network_Server.networkClientInterface client)
        {
            if (managerIsLoggedIn)
            {
                managementClientSession.Reset();
                takeScreenshotFrom(parts[0].Remove(0, 4));
                networkManager.sendMessage("#314true", client);
                return;
            }
            networkManager.sendMessage("#314false", client);
        }

        private void protocol_315_screenAllPlayers(network_Server.networkClientInterface client)
        {
            if (managerIsLoggedIn)
            {
                managementClientSession.Reset();
                takeScreenShotFromAll();
                networkManager.sendMessage("#316true", client);
                return;
            }
            networkManager.sendMessage("#316false", client);
        }

        private void protocol_319_kickoutAPlayer(string[] parts, network_Server.networkClientInterface client)
        {
            if (managerIsLoggedIn)
            {
                managementClientSession.Reset();
                kickoutAPlayer(parts[0].Remove(0, 4));
                networkManager.sendMessage("#320true", client);
                return;
            }
            networkManager.sendMessage("#320false", client);
        }

        private void protocol_323_getOnlinePlayerAmount(network_Server.networkClientInterface client)
        {
            if (managerIsLoggedIn)
            {
                managementClientSession.Reset();
                networkManager.sendMessage("#324" + getOnlineValue().ToString(), client);
                return;
            }
            networkManager.sendMessage("#324false", client);
        }

        //public user commands
        public int getOnlineValue()
        {
            int count = 0;
            foreach (var connection in connectionCollection)
            {
                count++;
            }
            return count;
        }

        public List<string> showOnlineUsers()
        {
            List<string> onlineIDs = new List<string>();
            foreach (var connection in connectionCollection)
            {
                onlineIDs.Add(connection.gameUser);
            }
            return onlineIDs;
        }

        public void kickoutAPlayer(string searchString)
        {
            foreach (var connection in searchConnections(searchString))
            {
                Console.WriteLine("Kickout Client: " + connection.guid + " || GameUser: " + connection.gameUser + " || IP: " + connection.ip);
                closeConnection(connection);
            }
        }

        public void takeScreenshotFrom(string searchString)
        {
            foreach (var connection in searchConnections(searchString))
            {
                Console.WriteLine("Took Screenshot from Client: " + connection.guid + " || GameUser: " + connection.gameUser + " || IP: " + connection.ip);
                networkManager.sendMessage("#206", connection);
            }
        }

        public void takeScreenShotFromAll()
        {
            foreach (var connection in connectionCollection)
            {
                Console.WriteLine("Took Screenshot from Client: " + connection.guid + " || GameUser: " + connection.gameUser + " || IP: " + connection.ip);
                networkManager.sendMessage("#206", connection);
            }
        }

        //Support functions

        private static string protocolAnalysing(string Protocol, ref string[] parts)
        {
            //Check, if the protocol has informations after the first contraction
            if (Protocol.Length > 4)
            {
                parts = Protocol.Split((char)20);
                return parts[0].Remove(4);
            }
            else
            {
                parts = new string[1];
                return Protocol;
            }
        }

        private List<network_Server.networkClientInterface> searchConnections(string info)
        {
            List<network_Server.networkClientInterface> connections = new List<network_Server.networkClientInterface>();

            foreach (var conn in connectionCollection)
            {
                if (conn.guid.ToString() == info || conn.gameUser == info || conn.ip == info)
                {
                    connections.Add(conn);
                }

            }
            return connections;
        }
    }
}
*/