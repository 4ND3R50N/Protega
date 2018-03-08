using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Protega___Server.Classes;
using Support;
using System.Net;

namespace Protega.ApplicationAdapter
{
    // Klasse muss immer ApplicationAdapter heißen, da der Server nach dem Klassennamen sucht.
    // Oder der Server muss so umgebaut werden, dass der Klassenname per config file angegeben werden kann
    public class ApplicationAdapter:IDisposable
    {
        logWriter.WriteLog LogFunction;

        SshClient LinuxInterface;
        static readonly object _lock = new object();

        List<int> PortsToBlock = new List<int>();
        string LinuxIP, LinuxLoginName, LinuxPassword;
        short LinuxPort;
        string DefaultCommand;

        bool ServerPrepared = false;

        #region Constructor & Destructor
        /// <summary>
        /// Create the object for the application adapter
        /// </summary>
        /// <param name="LogPath">The path where the logfile should be located</param>
        /// <param name="LogLevel">The level how detailled logs should be created. 1=Rarely, 2=Medium, 3=Debug</param>
        public ApplicationAdapter()
        {  }

        public void Dispose()
        {
            if (LinuxInterface != null)
            {
                if (LinuxInterface.IsConnected)
                    LinuxInterface.Disconnect();
                LinuxInterface.Dispose();
            }
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
        public bool PrepareServer(string ConfigPath, string ConfigIniSection, Support.logWriter.WriteLog LogFunction)
        {
            this.LogFunction = LogFunction;


            if (!LoadConfig(ConfigPath, ConfigIniSection))
            {
                LogFunction(1, LogCategory.CRITICAL, LoggerType.GAMEDLL, String.Format("Could not load config! Stated path: {0}, Section {1}", ConfigPath, ConfigIniSection));
                return false;
            }

            // SSH Login SHOULD be based on certificates, not username/password
            LinuxInterface = new SshClient(LinuxIP, LinuxPort, LinuxLoginName, LinuxPassword);

            try
            {
                LinuxInterface.Connect();
                if (!LinuxInterface.IsConnected)
                {
                    LogFunction(1, LogCategory.CRITICAL, LoggerType.GAMEDLL, "Linux connection failed!");
                    LinuxInterface.Dispose();
                    return false;
                }
            }
            catch (Exception e)
            {

                LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, String.Format("Cannot connect to Linux Server! ({0})", e.ToString()));
                LinuxInterface.Dispose();
                return false;
            }

            LogFunction(2, LogCategory.OK, LoggerType.GAMEDLL, "Linux Server connected successfully!");

            if (DefaultCommand != null && DefaultCommand.Length > 0)
            {
                using (SshCommand Result = LinuxInterface.RunCommand(DefaultCommand))
                {
                    bool Success = Result.Result.Length > 0;
                    if (!Success)
                    {
                        LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, String.Format("Cannot execute the starting Query! Error: {0}", Result.Error));
                        return false;
                    }
                    else
                    {
                        LogFunction(2, LogCategory.OK, LoggerType.GAMEDLL, "Linux Default command executed successfully!");
                    }
                }
            }
            /*else
            {
                unixSshConnectorAccept.RunCommand("service iptables stop");
            }

            if (BlockedPorts != null)
            {
                bool PortBlockingSucceeded = true;
                foreach (int item in BlockedPorts)
                {
                    //Bestimmte Ports blocken
                    if (PortBlockingSucceeded)
                        PortBlockingSucceeded = unixSshConnectorAccept.RunCommand("iptables -A INPUT -p tcp --destination-port " + item + " -j DROP").Error.Length == 0;
                    else
                    {
                        LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, string.Format("Could not block Port {0}", item));
                        return false;
                    }
                }

                LogFunction(3, LogCategory.OK, LoggerType.GAMEDLL, "Ports successfully blocked!");
            }

            bool IPTablesSave;
            IPTablesSave = unixSshConnectorAccept.RunCommand("service iptables save").Error.Length != 0;

            if(!IPTablesSave)
            {
                LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, "Could not save IPTables!");
                return false;
            }

            bool IPTablesStart;
            IPTablesStart = unixSshConnectorAccept.RunCommand("service iptables start").Error.Length != 0;
            if(!IPTablesStart)
            {
                LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, "Could not start IPTables!");
                return false;
            }
            unixSshConnectorAccept.Disconnect();
            */
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
            if (!short.TryParse(iniEngine.IniReadValue(Section, "LinuxPort"), out LinuxPort))
            {
                return false;
            }
            DefaultCommand = iniEngine.IniReadValue(Section, "PathDefaultCommand");

            bool bPortError = false;
            foreach (string Port in iniEngine.IniReadValue(Section, "Ports").Split(';'))
            {
                int tmpPort;
                if (!Int32.TryParse(Port, out tmpPort))
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

        #endregion

        #region User IPTable Management
        public bool AllowUser(IPAddress ClientIP, string UserName = null)
        {
            if (!ServerPrepared)
            {
                LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, "Server must be prepared at first!");
                return false;
            }

            lock (_lock)
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
                foreach (int item in PortsToBlock)
                {
                    LinuxCommand += "iptables -I INPUT -p tcp -s " + ClientIP.ToString() + " --dport " + item + " -j ACCEPT && ";
                }

                if (LinuxCommand.Length > 0)
                {
                    LinuxCommand = LinuxCommand.TrimEnd(' ').TrimEnd('&');
                    using (SshCommand Result = LinuxInterface.RunCommand(LinuxCommand))
                    {
                        if (Result.Error.Length > 0)
                        {
                            LogFunction(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, "Linux exception failed! Session ID: " + ClientIP + ", Error: " + Result.Error);
                            return false;
                        }
                    }
                }

                //if (AddToPortsSuceeded)
                LogFunction(3, LogCategory.OK, LoggerType.GAMEDLL, String.Format("IPTable exception successful for User {0}", (UserName == null ? ClientIP.ToString() : UserName)));

                return true;
            }
        }

        public bool KickUser(IPAddress ClientIP, string UserName = null)
        {
            if (!ServerPrepared)
            {
                LogFunction(1, LogCategory.ERROR, LoggerType.GAMEDLL, "Server must be prepared at first!");
                return false;
            }

            lock (_lock)
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
                foreach (int item in PortsToBlock)
                {
                    LinuxCommand += "iptables -D INPUT -p tcp -s " + ClientIP.ToString() + " --dport " + item + " -j ACCEPT && ";
                }

                if (LinuxCommand.Length > 0)
                {
                    LinuxCommand = LinuxCommand.TrimEnd(' ').TrimEnd('&');
                    using (SshCommand Result = LinuxInterface.RunCommand(LinuxCommand))
                    {
                        if (Result.Error.Length > 0)
                        {
                            LogFunction(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, "IPTable kick failed! Session ID: " + ClientIP + ", Error: " + Result.Error);
                            return false;
                        }
                    }
                }

                //if (AddToPortsSuceeded)
                LogFunction(3, LogCategory.OK, LoggerType.GAMEDLL, String.Format("IPTable kick successful for User {0}", (UserName == null ? ClientIP.ToString() : UserName)));

                return true;
            }
        }
        #endregion
        
        public bool BanUser() { Console.WriteLine("Ban User"); return false; }

    }
}
