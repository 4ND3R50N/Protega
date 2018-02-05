﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Protega___Server.Classes;
using Support;

namespace Protega.ApplicationAdapter
{
    // Klasse muss immer ApplicationAdapter heißen, da der Server nach dem Klassennamen sucht.
    // Oder der Server muss so umgebaut werden, dass der Klassenname per config file angegeben werden kann
    public class ApplicationAdapter
    {
        public delegate void LogError(int Importance, LogCategory Category, string Message);
        public delegate void Testing(string Test);
        public event Testing TestingEvent;
        event LogError Log;
        Support.logWriter.WriteLog LogFunction;

        string IP, LoginName, LoginPass;
        int Port;

        List<int> BlockedPorts;

        string LogPath;
        int LogLevel;

        bool ServerPrepared = false;

        public LogError LogIt(string MEssage)
        {
            return Log;
        }

        #region Constructor
        /// <summary>
        /// Create the object for the application adapter
        /// </summary>
        /// <param name="LogPath">The path where the logfile should be located</param>
        /// <param name="LogLevel">The level how detailled logs should be created. 1=Rarely, 2=Medium, 3=Debug</param>
        public ApplicationAdapter(string LogPath, int LogLevel)
        {
            this.LogPath = LogPath;
            this.LogLevel = LogLevel;            
        }

        public void SetEvent(Testing _Test)
        { TestingEvent = _Test; }

        public void TestEvent(string nothing)
        {
            TestingEvent("Succeeded!");
        }
        #endregion

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
//
        public bool PrepareServer(string ServerIP, string LoginName, string LoginPass, int LoginPort, List<int> BlockedPorts, string DefaultCommand, Support.logWriter.WriteLog LogFunction)
        {
            IP = ServerIP;
            this.LoginName = LoginName;
            this.LoginPass = LoginPass;
            Port = LoginPort;
            this.BlockedPorts = BlockedPorts;
            this.LogFunction = LogFunction;

            LogFunction(1, LogCategory.OK, "Test succeeded!");

            // SSH Login SHOULD be based on certificates, not username/password!
            SshClient unixSshConnectorAccept = new SshClient(IP, LoginPort, LoginName, LoginPass);
            unixSshConnectorAccept.Connect();

            if (!unixSshConnectorAccept.IsConnected)
            {
                //LogFunction(1, LogCategory.ERROR, "Cannot connect to Linux Server!");
                return false;
            }
            //LogFunction(1, LogCategory.OK, "Linux Server connected successfully!");

            if (DefaultCommand != null && DefaultCommand.Length > 0)
            {
                bool Success = unixSshConnectorAccept.RunCommand(DefaultCommand).Error.Length == 0;
                if (!Success)
                {
                    //LogFunction(1, LogCategory.ERROR, "Cannot execute starting Query!");
                    return false;
                }
                else
                {
                    //LogFunction(2, LogCategory.OK, "Starting Query executed successfully!");
                }
            }
            else
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
                        //LogFunction(1, LogCategory.ERROR, string.Format("Could not block Port {0}", item));
                        return false;
                    }
                }

                //LogFunction(3, LogCategory.OK, "Ports successfully blocked!");
            }

            bool IPTablesSave;
            IPTablesSave = unixSshConnectorAccept.RunCommand("service iptables save").Error.Length != 0;

            if(!IPTablesSave)
            {
                //LogFunction(1, LogCategory.ERROR, "Could not save IPTables!");
                return false;
            }

            bool IPTablesStart;
            IPTablesStart = unixSshConnectorAccept.RunCommand("service iptables start").Error.Length != 0;
            if(!IPTablesStart)
            {
                //LogFunction(1, LogCategory.ERROR, "Could not start IPTables!");
                return false;
            }
            
            unixSshConnectorAccept.Disconnect();
            //LogFunction(1, LogCategory.OK, "Linux interaction successful!");

            ServerPrepared = true;
            return true;
        }

        #region User IPTable Management
        public bool AllowUser(string IP, string UserName)
        {
            if(!ServerPrepared)
            {
                //LogFunction(1, LogCategory.ERROR, "Server must be prepared at first!");
                return false;
            }

            //LogFunction(3, LogCategory.OK, "Adding User to IPTables initiated!");
            SshClient unixSshConnectorAccept = new SshClient(IP, Port, LoginName, LoginPass);
            unixSshConnectorAccept.Connect();

            if (!unixSshConnectorAccept.IsConnected)
            {
                //LogFunction(2, LogCategory.ERROR, String.Format("Could not connect to IPTables. Add IP: {0}", IP));
                return false;
            }

            bool AddToPortsSuceeded = true;
            foreach (int item in BlockedPorts)
            {
                //Bestimmte Ports blocken
                if (AddToPortsSuceeded)
                    AddToPortsSuceeded = unixSshConnectorAccept.RunCommand("iptables -I INPUT -p tcp -s " + IP + " --dport " + item + " -j ACCEPT").Error.Length == 0;
                else
                {
                    //LogFunction(2, LogCategory.ERROR, String.Format("Could not add IP to Port. Port: {0}, IP: {1}", item, IP));
                    return false;
                }
            }
            //if (AddToPortsSuceeded)
                //LogFunction(3, LogCategory.OK, String.Format("Successfully added IP {0} to Ports.", IP));

            return true;
        }

        public bool KickUser(string IP, string UserName)
        {
            //LogFunction(3, LogCategory.OK, "Kicking from IPTables initiated!");
            SshClient unixSshConnectorAccept = new SshClient(IP, Port, LoginName, LoginPass);
            unixSshConnectorAccept.Connect();

            if(!unixSshConnectorAccept.IsConnected)
            {
                //LogFunction(2, LogCategory.ERROR, String.Format("Could not connect to IPTables. KickIP: {0}", IP));
                return false;
            }

            bool KickFromPortsSuceeded = true;
            foreach (int item in BlockedPorts)
            {
                //Bestimmte Ports blocken
                if (KickFromPortsSuceeded)
                    KickFromPortsSuceeded = unixSshConnectorAccept.RunCommand("iptables -D INPUT -p tcp -s " + IP + " --dport " + item + " -j ACCEPT").Error.Length == 0;
                else
                {
                    //LogFunction(2, LogCategory.ERROR, String.Format("Could not kick from Port. Port: {0}, IP: {1}", item, IP));
                    return false;
                }
            }
            //if (KickFromPortsSuceeded)
                //LogFunction(3, LogCategory.OK, String.Format("Successfully kicked IP {0} from Ports.", IP));

            return true;
        }

        public bool BanUser() { Console.WriteLine("Ban User"); return false; }
        #endregion

    }
}
