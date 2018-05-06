using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Protega___Server.Classes;
using Support;
using System.Net;
using System.Threading;
using Protega.ApplicationAdapter.Classes.Tasks;
using Protega.ApplicationAdapter.Classes;

namespace Protega.ApplicationAdapter
{
    // Klasse muss immer ApplicationAdapter heißen, da der Server nach dem Klassennamen sucht.
    // Oder der Server muss so umgebaut werden, dass der Klassenname per config file angegeben werden kann
    class LinuxOperator : IDisposable
    {
        public SshClient Client;
        public bool OpenForRequest;
        System.Timers.Timer tmrReload;
        logWriter.WriteLog LogFunction;

        public LinuxOperator(string LinuxIP, short LinuxPort, string LinuxLoginName, string LinuxPassword, logWriter.WriteLog _LogFunction)
        {
            this.LogFunction = _LogFunction;
            Client = new SshClient(LinuxIP, LinuxPort, LinuxLoginName, LinuxPassword);
            tmrReload = new System.Timers.Timer();
            tmrReload.Interval = 5000;
            tmrReload.Start();
        }

        void tmrReload_Ping()
        {
            tmrReload.Stop();
            LogFunction(2, LogCategory.OK, LoggerType.GAMEDLL, "Checking timer");
            if (!Client.IsConnected)
                try
                {
                    LogFunction(5, LogCategory.OK, LoggerType.GAMEDLL, "Reconnecting timer");
                    Client.Connect();
                }
                catch (Exception e)
                {
                    LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "Reconnecting timer failed! Error: " + e.Message);
                }
            tmrReload.Start();
        }

        public void Dispose()
        {
            Client.Disconnect();
            Client.Dispose();
        }

        public bool Reconnect()
        {
            Client.Disconnect();
            Client.Connect();
            return Client.IsConnected;
        }

        public bool Initialize()
        {
            try
            {
                Client.Connect();
                if (Client.IsConnected)
                {
                    OpenForRequest = true;
                    return true;
                }
                else
                {
                    Client.Dispose();
                    return false;
                }
            }
            catch (Exception e)
            {
                Client.Dispose();
                return false;
            }
        }
    }

    public class ApplicationAdapter : IDisposable
    {
        logWriter.WriteLog LogFunction;
        Protega___Server.Classes.Utility.ApplicationAdapter.DllFeedback FeedbackAction;

        //Queue<LinuxOperator> LinuxInterfaces = new Queue<LinuxOperator>();
        static readonly object _lockLogin = new object();

        Queue<_InterfaceTask> qTasks;

        List<int> PortsToBlock = new List<int>();
        string LinuxIP, LinuxLoginName, LinuxPassword;
        short LinuxPort;
        string DefaultCommand;
        int AmountSshInstances;
        int AmountSshInstacesToKeep;

        bool ServerPrepared = false;

        Thread LinuxManager;

        #region Constructor & Destructor
        /// <summary>
        /// Create the object for the application adapter
        /// </summary>
        /// <param name="LogPath">The path where the logfile should be located</param>
        /// <param name="LogLevel">The level how detailled logs should be created. 1=Rarely, 2=Medium, 3=Debug</param>
        public ApplicationAdapter()
        { qTasks = new Queue<_InterfaceTask>();
        }

        public void Dispose()
        {
            //if (LinuxInterface != null)
            //{
            //    if (LinuxInterface.IsConnected)
            //        LinuxInterface.Disconnect();
            //    LinuxInterface.Dispose();
            //}
        }
        #endregion

        #region Startup Functions
        /// <summary>
        /// Connect to Linux Server, execute starting command and block given ports
        /// </summary>
        /// <param name="ServerIP">IP of the Linux Server</param>
        /// <param name="LoginName">LoginName to the Linux Server</param>
        /// <param name="LoginPass">LoginPass to the Linux Server</param>
        /// <param name="LoginPort">Port of the Linux Server</param>
        /// <param name="BlockedPorts">Ports to be blocked. Null if not needed</param>
        /// <param name="DefaultCommand">A Linux command that should be executed in the beginning</param>
        /// <param name="LogFunction">Function to Log errors. (int Importance, LogCategory Category, string Message)</param>
        /// <returns>Bool Successful</returns>
        ///
        public bool PrepareServer(string ConfigPath, string ConfigIniSection, Support.logWriter.WriteLog LogFunction, Protega___Server.Classes.Utility.ApplicationAdapter.DllFeedback dllFeedback)
        {
            this.LogFunction = LogFunction;
            this.FeedbackAction = dllFeedback;
            

            if (!LoadConfig(ConfigPath, ConfigIniSection))
            {
                LogFunction(1, LogCategory.CRITICAL, LoggerType.GAMEDLL, "Error while reading dll config! Stated path: " + ConfigPath + ", Section " + ConfigIniSection);
                return false;
            }
            LogFunction(1, LogCategory.OK, LoggerType.GAMEDLL, "Loading DLL");

            SshConnectionManager ConnectionManager = new SshConnectionManager(LinuxIP, LinuxPort, LinuxLoginName, LinuxPassword, AmountSshInstacesToKeep, LogFunction);

            LogFunction(1, LogCategory.OK, LoggerType.GAMEDLL, "DLL - Trying to create " + AmountSshInstances.ToString() + " sshClient instances...");
            if (AmountSshInstances != 0 && ConnectionManager.CreateInstances(AmountSshInstances) == 0)
            {
                LogFunction(1, LogCategory.OK, LoggerType.GAMEDLL, "Could not connect to Linux server!");
                return false;
            }

            // SSH Login SHOULD be based on certificates, not username/password
            //int Counter = 0;
            //while (Counter < 4)
            //{
            //    LinuxOperator sshClient = new LinuxOperator(LinuxIP, LinuxPort, LinuxLoginName, LinuxPassword, LogFunction);
            //    if (sshClient.Initialize())
            //    {
            //        LinuxInterfaces.Enqueue(sshClient);
            //        Counter++;
            //        LogFunction(1, LogCategory.OK, LoggerType.GAMEDLL, "Linux Interface initiated nb " + Counter.ToString());
            //    }
            //    else
            //        LogFunction(1, LogCategory.CRITICAL, LoggerType.GAMEDLL, "Linux connection failed! Connection "+Counter.ToString());
            //}
            //LinuxOperator LinuxInterface = LinuxInterfaces.Dequeue();
            SshConnection LinuxInterface = ConnectionManager.GetAvailableSshClient();

            try
            {
                if (!LinuxInterface.Client.IsConnected)
                    LinuxInterface.Client.Connect();
                if (!LinuxInterface.Client.IsConnected)
                {
                    LogFunction(1, LogCategory.CRITICAL, LoggerType.GAMEDLL, "Linux connection failed!");
                    LinuxInterface.Dispose();
                    return false;
                }
            }
            catch (Exception e)
            {

                LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, "Cannot connect to Linux Server! (" + e.Message + ")");
                LinuxInterface.Dispose();
                LinuxInterface.Dispose();
                return false;
            }


            LogFunction(2, LogCategory.OK, LoggerType.GAMEDLL, "Linux Server connected successfully!");

            if (DefaultCommand != null && DefaultCommand.Length > 0)
            {
                using (SshCommand Result = LinuxInterface.Client.RunCommand(DefaultCommand))
                {
                    LinuxInterface.isAvailable = true;
                    bool Success = Result.Result.Length > 0;
                    if (!Success)
                    {
                        LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, "Cannot execute the starting Query! Error: "+ Result.Error);
                        return false;
                    }
                    else
                    {
                        LogFunction(2, LogCategory.OK, LoggerType.GAMEDLL, "Linux Default command executed successfully!");
                    }
                }

            }
            LinuxInterface.isAvailable = true;

            LinuxManager = new Thread(() => AllowManager(ConnectionManager));

            LinuxManager.Start();

            LogFunction(1, LogCategory.OK, LoggerType.GAMEDLL, "Linux interaction successful!");
            ServerPrepared = true;
            
            return true;

        }

        
        bool LoadConfig(string Path, string Section)
        {
            iniManager iniEngine = new iniManager(Path);
            LinuxIP = iniEngine.IniReadValue(Section, "LinuxIP");
            LinuxLoginName = iniEngine.IniReadValue(Section, "LinuxLoginName");
            LinuxPassword = iniEngine.IniReadValue(Section, "LinuxPassword");
            if (!short.TryParse(iniEngine.IniReadValue(Section, "LinuxPort"), out LinuxPort) || LinuxPort < 1)
            {
                return false;
            }
            if (!Int32.TryParse(iniEngine.IniReadValue(Section, "AmountSshInstancesAtStart"), out AmountSshInstances) || AmountSshInstances < 0)
            {
                return false;
            }
            if (!Int32.TryParse(iniEngine.IniReadValue(Section, "AmountSshInstancesToKeep"), out AmountSshInstacesToKeep) || AmountSshInstacesToKeep < 0)
            {
                return false;
            }
            DefaultCommand = iniEngine.IniReadValue(Section, "PathDefaultCommand");

            bool bPortError = false;
            foreach (string Port in iniEngine.IniReadValue(Section, "Ports").Split(';'))
            {
                int tmpPort;
                if (!Int32.TryParse(Port, out tmpPort) || tmpPort < 1)
                {
                    bPortError = true;
                    return false;
                }
                PortsToBlock.Add(tmpPort);
            }
            if (bPortError)
                return false;

            return true;
        }

        void ReaddItems(List<_InterfaceTask> Tasks)
        {
            foreach (_InterfaceTask item in Tasks)
            {
                lock(LockLoginClient)
                {
                    qTasks.Enqueue(item);
                }
            }
        }
        #endregion

        #region User IPTable Management


        #region Threadmanager
        private readonly object LockLoginClient = new object();
        public void AllowManager(SshConnectionManager ConnectionManager)
        {
            while (true)
            {
                while (qTasks.Count > 0)
                {
                    List<_InterfaceTask> listTasks = new List<_InterfaceTask>();
                    lock (LockLoginClient)
                    {
                        //Split the queue into parts of 10
                        int Counter = 0;
                        while (qTasks.Count > 0 && Counter++ < 10)
                        {
                            listTasks.Add(qTasks.Dequeue());
                        }
                    }

                    //Create the execution query for each item
                    foreach (_InterfaceTask item in listTasks)
                    {
                        if(!item.BuildLinuxQuery(PortsToBlock, 15))
                        {
                            LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "Dll: Assigned Task was not specified! User " + item.Username + " (" + item.IP.ToString() + ")");
                            listTasks.Remove(item);
                        }
                    }

                    //If all tasks had to be removed, continue the loop
                    if (listTasks.Count == 0)
                        continue;


                    //int Attempt = 0;
                    //Attempt++;
                    //LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "AllowManager: No Linuxinterface, attempt " + Attempt.ToString());
                    //Thread HandleSubTasks = new Thread(() => Classes.Utility.LinuxInterface(param1, param2));
                    SshConnection sshConnection = ConnectionManager.GetAvailableSshClient();
                    Classes.Utility.LinuxInterface Interface = new Classes.Utility.LinuxInterface(LogFunction, FeedbackAction, ReaddItems);
                    Interface.DoTasks(listTasks, sshConnection);
                }
                Thread.Sleep(2000);
            }
        }
        
        #endregion


        public bool AllowUser(IPAddress ClientIP, string UserName, DateTime Timestamp)
        {
            if (!ServerPrepared)
            {
                LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, "Server must be prepared at first!");
                return false;
            }

            InsertConnection NewTask = new InsertConnection(ClientIP, Timestamp, UserName);

            lock (LockLoginClient)
            {
                qTasks.Enqueue(NewTask);
            }
            return true;
        }

        public bool KickUser(IPAddress ClientIP, string UserName, DateTime Timestamp)
        {
            if (!ServerPrepared)
            {
                LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, "Server must be prepared at first!");
                return false;
            }

            Classes.Tasks.RemoveConnection NewTask = new RemoveConnection(ClientIP, Timestamp, UserName);
            lock (LockLoginClient)
            {
                qTasks.Enqueue(NewTask);
            }

            return true;
        }



        #endregion
        
        public bool BanUser() { Console.WriteLine("Ban User"); return false; }

        #region Backup
        /*
        void HandleAllow(List<AllowTask> allowTask, ref SshClient LinuxInterface)
        {
            //if (!LinuxInterface.Client.IsConnected)
            //    if(!LinuxInterface.Reconnect())
            //    {
            //        LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "AllowTask: LinuxInterface reconnect not possible!");
            //        return;
            //    }


            lock (_lockLogin)
            {
                LogFunction(3, LogCategory.OK, LoggerType.GAMEDLL, "Adding User to IPTables initiated!");
                if (!LinuxInterface.IsConnected)
                {
                    try
                    {
                        LinuxInterface.Connect();
                    }
                    catch (Exception e)
                    {
                        LogFunction(1, LogCategory.CRITICAL, LoggerType.GAMEDLL, "Cannot reconnect to Linux server!");
                    }
                }

                string LinuxCommand = "";
                string IPs = "", Usernames = "";
                foreach (AllowTask Task in allowTask)
                {
                    IPs += Task.IP;
                    Usernames += Task.Playername;
                    foreach (var item in SplitAmountOfPorts(PortsToBlock, 15))
                    {
                        if (LinuxCommand.Length == 0)
                            LinuxCommand += "iptables -I INPUT -p tcp -s " + Task.IP.ToString() + " --match multiport --dport " + item + " -j ACCEPT && ";
                        else
                            LinuxCommand += " && iptables -I INPUT -p tcp -s " + Task.IP.ToString() + " --match multiport --dport " + item + " -j ACCEPT";
                    }
                }

                if (LinuxCommand.Length > 0)
                {
                    using (SshCommand Result = LinuxInterface.RunCommand(LinuxCommand))
                    {
                        if (Result.Error.Length > 0)
                        {
                            LogFunction(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, "Linux exception failed! Session ID: " + IPs + ", Error: " + Result.Error);
                            return;
                        }
                    }
                }

                //LinuxInterfaces.Enqueue(LinuxInterface);

                //if (AddToPortsSuceeded)
                LogFunction(3, LogCategory.OK, LoggerType.GAMEDLL, String.Format("IPTable exception successful for User {0}", (Usernames.Length == 0 ? IPs : Usernames)));


                return;
                //}
            }
        }

        void HandleKick(KickTask kickTask, ref SshClient LinuxInterface)
        {
            //if (!LinuxInterface.IsConnected)
            //    if (!LinuxInterface.Reconnect())
            //    {
            //        LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "AllowTask: LinuxInterface reconnect not possible!");
            //        return;
            //    }
            //while (LinuxInterfaces.Count == 0)
            //{
            //    LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "No LinuxInterface available!");
            //    System.Threading.Thread.Sleep(1000);
            //}

            //LinuxOperator LinuxInterface = LinuxInterfaces.Dequeue();


            lock (_lockLogout)
            {
                LogFunction(3, LogCategory.OK, LoggerType.GAMEDLL, "Kicking User from IPTables initiated!");
                if (!LinuxInterface.IsConnected)
                {
                    try
                    {
                        LinuxInterface.Connect();
                    }
                    catch (Exception e)
                    {
                        LogFunction(1, LogCategory.CRITICAL, LoggerType.GAMEDLL, "Cannot reconnect to Linux server!");
                    }
                }

                string LinuxCommand = "";
                foreach (var item in SplitAmountOfPorts(PortsToBlock, 15))
                {
                    if (LinuxCommand.Length == 0)
                        LinuxCommand += "iptables -I INPUT -p tcp -s " + kickTask.IP.ToString() + " --match multiport --dport " + item + " -j ACCEPT && ";
                    else
                        LinuxCommand += " && iptables -I INPUT -p tcp -s " + kickTask.IP.ToString() + " --match multiport --dport " + item + " -j ACCEPT";
                }

                if (LinuxCommand.Length > 0)
                {
                    using (SshCommand Result = LinuxInterface.RunCommand(LinuxCommand))
                    {
                        if (Result.Error.Length > 0)
                        {
                            LogFunction(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, "IPTable kick failed! Session ID: " + kickTask.IP + ", Error: " + Result.Error);
                            return;
                        }
                    }
                }

                //LinuxInterfaces.Enqueue(LinuxInterface);

                //if (AddToPortsSuceeded)
                LogFunction(3, LogCategory.OK, LoggerType.GAMEDLL, String.Format("IPTable kick successful for User {0}", (kickTask.Playername == null ? kickTask.IP.ToString() : kickTask.IP.ToString())));

                return;
                //}
            }
        }*/
        #endregion
    }
}
