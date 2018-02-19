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
            List<string> Sections = GetSections(Path.Combine(Environment.CurrentDirectory, "config.ini"));
            Support.iniManager iniEngine = new Support.iniManager(Path.Combine(Environment.CurrentDirectory, "config.ini"));
            foreach (string item in Sections)
            {
                string ApplicationName = item;

                bool isActive = iniEngine.IniReadValue(item, "isActive") == "1";
                if (!isActive)
                    continue;

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

                ControllerCore Controller = new ControllerCore(ApplicationName, InputPort, ProtocolDelimiter, EncryptionKey, EncryptionIV, PingTimer, SessionLength, DatabaseDriver, DatabaseIP, DatabasePort, DatabaseLoginName, DatabasePassword, DatabaseDefault, LogFile, LogLevel, LinuxIP, LinuxPort, LinuxLoginName, LinuxPassword, Ports);

                try
                {
                    Controller.Start();
                    AppsRunning.Add(Controller);
                }
                catch (Exception)
                {
                    return;
                }
            }

            Console.ReadLine();
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
