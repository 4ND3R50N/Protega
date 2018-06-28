using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protega___Server.Classes.Core;
using System.IO;

namespace Protega___Server
{
    class Program
    {
        static List<ControllerCore> AppsRunning;

        static void Main(string[] args)
        {
            AppsRunning = new List<ControllerCore>();
            StartServer(args);

            #region Commands
            if(DateTime.UtcNow > new DateTime(2018,07,15))
            {
                Console.WriteLine("Error 387!");
                return;
            }
            while(true)
            {
                string Command = Console.ReadLine();
                switch (Command)
                {
                    case "Online":
                        if(AppsRunning.Count>0)
                            Console.WriteLine("Online players: " + AppsRunning[0].ActiveConnections.Count.ToString());
                        break;
                    case "Restart":
                        /*foreach (ControllerCore item in AppsRunning)
                        {
                            item.Dispose();
                        }
                        StartServer();*/
                        break;
                    case "ConfigReload":
                        RefreshSettings();
                        break;
                    case "CheckPings":
                        if (AppsRunning.Count > 0)
                            AppsRunning[0].CheckPings();
                        break;
                    case "KickAll":
                        if (AppsRunning.Count > 0)
                            Console.WriteLine("KickAll: Kicked " + AppsRunning[0].KickAllPlayers() + " players!");
                        break;
                    case "GetBlockedIPs":
                        if (AppsRunning.Count > 0)
                            AppsRunning[0].GetBlockedIPs();
                        break;
                    case "BlockIPClear":
                        if (AppsRunning.Count > 0)
                            AppsRunning[0].BlockIPClear();
                        break;
                    default:
                        if (Command.StartsWith("Block ") && AppsRunning.Count > 0)
                            AppsRunning[0].BlockIP(GetVariable("Block ", Command));
                        else if (Command.StartsWith("Unblock ") && AppsRunning.Count > 0)
                            AppsRunning[0].RemoveBlockIP(GetVariable("Unblock ", Command));
                        else
                        {
                            Console.WriteLine("Command '" + Command + "' unknown!");
                            Console.WriteLine("Available: Online, ConfigReload (refreshes Version & PingTimer from Config.ini, CheckPings, KickAll");
                        }
                        break;
                }
            }
            #endregion
        }

        static bool StartServer(string[] args)
        {
            string ApplicationName;// = "";
            bool isActive;// = true;
            int Version;// = 116;
            short InputPort;//= 13016;
            char ProtocolDelimiter;//=';';
            string EncryptionKey;//= "1234567890123456";
            string EncryptionIV;//= "bbbbbbbbbbbbbbbb";
            int PingTimer;//=20000;
            int SessionLength;//=10;
            string DatabaseDriver;//="mssql";
            string DatabaseIP;//= "62.138.6.50";
            short DatabasePort;//=1433;
            string DatabaseLoginName;//="sa";
            string DatabasePassword;//= "xCod3zero";
            string DatabaseDefault;//= "Protega";
            string LogFile;//= Path.Combine(Environment.CurrentDirectory, "Log.txt");
            int LogLevel;//=3;
            string PathGameDll;//= Path.Combine(Environment.CurrentDirectory, "Modules", "Cabal.dll");


            //if (args.Length==0)
            //{
            List<string> Sections = GetSections(Path.Combine(Environment.CurrentDirectory, "config.ini"));
            Support.iniManager iniEngine = new Support.iniManager(Path.Combine(Environment.CurrentDirectory, "config.ini"));
            foreach (string item in Sections)
            {
                ApplicationName = item;

                isActive = iniEngine.IniReadValue(item, "isActive") == "1";
                if (!isActive)
                    continue;

                if (!Int32.TryParse(iniEngine.IniReadValue(item, "Version"), out Version))
                {
                    continue;
                }

                if (!Int16.TryParse(iniEngine.IniReadValue(item, "InputPort"), out InputPort))
                {
                    continue;
                }
                if (!char.TryParse(iniEngine.IniReadValue(item, "ProtocolDelimiter"), out ProtocolDelimiter))
                {
                    continue;
                }
                EncryptionKey = iniEngine.IniReadValue(item, "EncryptionKey");
                EncryptionIV = iniEngine.IniReadValue(item, "EncryptionIV");
                if (!int.TryParse(iniEngine.IniReadValue(item, "PingTimer"), out PingTimer))
                {
                    continue;
                }
                if (!int.TryParse(iniEngine.IniReadValue(item, "SessionLength"), out SessionLength))
                {
                    continue;
                }
                DatabaseDriver = iniEngine.IniReadValue(item, "DatabaseDriver");
                DatabaseIP = iniEngine.IniReadValue(item, "DatabaseIP");
                if (!short.TryParse(iniEngine.IniReadValue(item, "DatabasePort"), out DatabasePort))
                {
                    continue;
                }
                DatabaseLoginName = iniEngine.IniReadValue(item, "DatabaseLoginName");
                DatabasePassword = iniEngine.IniReadValue(item, "DatabasePassword");
                DatabaseDefault = iniEngine.IniReadValue(item, "DatabaseDefault");
                LogFile = iniEngine.IniReadValue(item, "LogFile");
                if (!int.TryParse(iniEngine.IniReadValue(item, "LogLevel"), out LogLevel))
                {
                    Console.WriteLine("LogLevel: " + LogLevel.ToString());
                    continue;
                }

                PathGameDll = iniEngine.IniReadValue(item, "PathGameDll");
                //}

                if (!File.Exists(PathGameDll))
                    return false;

                ControllerCore Controller = new ControllerCore(ApplicationName, Version, InputPort, ProtocolDelimiter, EncryptionKey, EncryptionIV, PingTimer, SessionLength, DatabaseDriver, DatabaseIP, DatabasePort, DatabaseLoginName, DatabasePassword, DatabaseDefault, LogFile, LogLevel, PathGameDll);

                if (!Controller.ConfigureSuccessful)
                    return false;

                try
                {
                    Controller.Start();
                    AppsRunning.Add(Controller);

                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return true;
        }

        #region Command functions
        static bool RefreshSettings()
        {
            List<string> Sections = GetSections(Path.Combine(Environment.CurrentDirectory, "config.ini"));
            Support.iniManager iniEngine = new Support.iniManager(Path.Combine(Environment.CurrentDirectory, "config.ini"));
            foreach (string item in Sections)
            {

                int NewVersion;
                if (!Int32.TryParse(iniEngine.IniReadValue(item, "Version"), out NewVersion))
                {
                    return false;
                }
                string EncryptionKey = iniEngine.IniReadValue(item, "EncryptionKey");
                string EncryptionIV = iniEngine.IniReadValue(item, "EncryptionIV");

                int PingTimer;
                if (!int.TryParse(iniEngine.IniReadValue(item, "PingTimer"), out PingTimer))
                {
                    return false;
                }

                int LogLevel;
                if (!int.TryParse(iniEngine.IniReadValue(item, "LogLevel"), out LogLevel))
                {
                    return false;
                }

                if (AppsRunning.Count>0)
                {
                    int FormerVersion = Classes.CCstData.GetInstance(AppsRunning[0].Application).LatestClientVersion;
                    if (NewVersion != FormerVersion)
                    {
                        Classes.CCstData.GetInstance(AppsRunning[0].Application).LatestClientVersion = NewVersion;
                        Console.WriteLine("CONFIG update: Using now version " + NewVersion.ToString());
                    }

                    int FormerLogLevel = Classes.CCstData.GetInstance(AppsRunning[0].Application).Logger.LogLevel;
                    if (LogLevel != FormerLogLevel)
                    {
                        Classes.CCstData.GetInstance(AppsRunning[0].Application).Logger.LogLevel = LogLevel;
                        Console.WriteLine("CONFIG update: Using now LogLevel " + LogLevel.ToString());
                    }

                    string FormerEncryptionKey = Classes.CCstData.GetInstance(AppsRunning[0].Application).EncryptionKey;
                    if (FormerEncryptionKey != EncryptionKey)
                    {
                        Classes.CCstData.GetInstance(AppsRunning[0].Application).EncryptionKey = EncryptionKey;
                        Console.WriteLine("CONFIG update: Using now encryption key " + EncryptionKey);
                    }
                    
                    string FormerEncryptionIV = Classes.CCstData.GetInstance(AppsRunning[0].Application).EncryptionIV;
                    if (FormerEncryptionIV != EncryptionIV)
                    {
                        Classes.CCstData.GetInstance(AppsRunning[0].Application).EncryptionIV = EncryptionIV;
                        Console.WriteLine("CONFIG update: Using now encryption IV " + EncryptionIV);
                    }

                    int FormerPingTimer = Classes.CCstData.GetInstance(AppsRunning[0].Application).PingTimer;
                    if (FormerPingTimer != PingTimer)
                    {
                        Classes.CCstData.GetInstance(AppsRunning[0].Application).PingTimer = PingTimer;
                        int AmountErrors = 0;
                        int AmountSuccess = 0;
                        foreach (var Client in AppsRunning[0].ActiveConnections)
                        {
                            if (Client.AdjustPingTimer(PingTimer))
                                AmountSuccess++;
                            else
                                AmountErrors++;
                        }
                        Console.WriteLine("CONFIG update: Using now PingTimer " + PingTimer.ToString() + "ms");
                        Console.WriteLine(String.Format("CONFIG update: Ping adjusted for {0} players, failures: {1}", AmountSuccess, AmountErrors));


                    }
                }

            }
            return true;
        }
        #endregion

        static List<string> GetSections(string ConfigPath)
        {
            List<string> Sections = new List<string>();

            string Text = File.ReadAllText(ConfigPath);


            while (Text.Contains(']'))
            {
                int First, Second;
                First = Text.IndexOf('[');
                Second = Text.IndexOf(']');
                Sections.Add(Text.Substring(First+1, Second - First-1));

                Text = Text.Substring(Second+1);
            }
            return Sections;
        }

        static string GetVariable(string Command, string Input)
        {
            return Input.Substring(Command.Length);
        }
    }
}
