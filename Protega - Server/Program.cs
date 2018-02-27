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
        static List<ControllerCore> AppsRunning = new List<ControllerCore>();

        static void Main(string[] args)
        {
            StartServer();

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
                    default:
                        Console.WriteLine("Command '" + Command + "' unknown!");
                        Console.WriteLine("Available: Online, ConfigReload (refreshes Version, PingTimer, EncryptionKey/IV from Config.ini");
                        break;
                }
            }
        }

        static bool StartServer()
        {
            List<string> Sections = GetSections(Path.Combine(Environment.CurrentDirectory, "config.ini"));
            Support.iniManager iniEngine = new Support.iniManager(Path.Combine(Environment.CurrentDirectory, "config.ini"));
            foreach (string item in Sections)
            {
                string ApplicationName = item;

                bool isActive = iniEngine.IniReadValue(item, "isActive") == "1";
                if (!isActive)
                    continue;

                int Version;
                if (!Int32.TryParse(iniEngine.IniReadValue(item, "Version"), out Version))
                {
                    continue;
                }

                short InputPort;
                if (!Int16.TryParse(iniEngine.IniReadValue(item, "InputPort"), out InputPort))
                {
                    continue;
                }
                char ProtocolDelimiter;
                if (!char.TryParse(iniEngine.IniReadValue(item, "ProtocolDelimiter"), out ProtocolDelimiter))
                {
                    continue;
                }
                string EncryptionKey = iniEngine.IniReadValue(item, "EncryptionKey");
                string EncryptionIV = iniEngine.IniReadValue(item, "EncryptionIV");
                int PingTimer;
                if (!int.TryParse(iniEngine.IniReadValue(item, "PingTimer"), out PingTimer))
                {
                    continue;
                }
                int SessionLength;
                if (!int.TryParse(iniEngine.IniReadValue(item, "SessionLength"), out SessionLength))
                {
                    continue;
                }
                string DatabaseDriver = iniEngine.IniReadValue(item, "DatabaseDriver");
                string DatabaseIP = iniEngine.IniReadValue(item, "DatabaseIP");
                short DatabasePort;
                if (!short.TryParse(iniEngine.IniReadValue(item, "DatabasePort"), out DatabasePort))
                {
                    continue;
                }
                string DatabaseLoginName = iniEngine.IniReadValue(item, "DatabaseLoginName");
                string DatabasePassword = iniEngine.IniReadValue(item, "DatabasePassword");
                string DatabaseDefault = iniEngine.IniReadValue(item, "DatabaseDefault");
                string LogFile = iniEngine.IniReadValue(item, "LogFile");
                int LogLevel;
                if (!int.TryParse(iniEngine.IniReadValue(item, "LogLevel"), out LogLevel))
                {
                    continue;
                }

                //CABAL DEFAULT!
                string LinuxIP = iniEngine.IniReadValue(item, "LinuxIP");
                string LinuxLoginName = iniEngine.IniReadValue(item, "LinuxLoginName");
                string LinuxPassword = iniEngine.IniReadValue(item, "LinuxPassword");
                short LinuxPort;
                if (!short.TryParse(iniEngine.IniReadValue(item, "LinuxPort"), out LinuxPort))
                {
                    continue;
                }

                bool bPortError = false;
                List<int> Ports = new List<int>();
                foreach (string Port in iniEngine.IniReadValue(item, "Ports").Split(';'))
                {
                    int tmpPort;
                    if (!Int32.TryParse(Port, out tmpPort))
                    {
                        bPortError = true;
                        continue;
                    }
                    Ports.Add(tmpPort);
                }
                if (bPortError)
                    continue;


                bool DeactivatePortBlocking = iniEngine.IniReadValue(item, "DeactivatePortBlocking") == "1";

                ControllerCore Controller = new ControllerCore(ApplicationName, Version, InputPort, ProtocolDelimiter, EncryptionKey, EncryptionIV, PingTimer, SessionLength, DatabaseDriver, DatabaseIP, DatabasePort, DatabaseLoginName, DatabasePassword, DatabaseDefault, LogFile, LogLevel, LinuxIP, LinuxPort, LinuxLoginName, LinuxPassword, Ports, DeactivatePortBlocking);

                try
                {
                    Controller.Start();
                    AppsRunning.Add(Controller);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

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
                        Classes.CCstData.GetInstance(AppsRunning[0].Application).LatestClientVersion = NewVersion;
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
                    if (FormerEncryptionIV != EncryptionIV)
                    {
                        Classes.CCstData.GetInstance(AppsRunning[0].Application).PingTimer = PingTimer;
                        Console.WriteLine("CONFIG update: Using now PingTimer " + PingTimer.ToString() + "ms");
                    }
                }

            }
            return true;
        }

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
    }
}
