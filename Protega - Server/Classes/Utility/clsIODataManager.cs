using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Protes_cmdServer.git.classes_support
{
    class ioDataManager
    {
        //Variable
        //--Objects
        StreamWriter writeMainLog;
        StreamWriter writeBannReport;

        //--config
        private const string guidBlackListName = "guidBlacklist.txt";
        private const string ipBlackListName = "bannedIPList.txt";
        private const string gamePortListName = "gamePorts.txt";
        private const string binomMD5Name = "binomMD5.txt";
        private const string tmpBannListName = "tmpBannDescription.txt";

        private const string mainLogName = "main.log";
        private const string bannLogName = "bann.log";
        //StreamWriter writePlayerReportLog;
        //--Private
        private string baseDirectory = Environment.CurrentDirectory;
        private string guidBlacklist;
        private string ipBlackList;
        private string portList;
        private string tmpList;
        private string binomDecriptKey;

        public bool useConsole { get; set; }

        public ioDataManager(string mainLogPath, string dataPath)
        {
            Console.WriteLine("Initialize ioDataManager...");
            if (!File.Exists(baseDirectory + mainLogPath + mainLogName))
            {
                File.Create(baseDirectory + mainLogPath + mainLogName);
            }
            if (!File.Exists(baseDirectory + mainLogPath + bannLogName))
            {
                File.Create(baseDirectory + mainLogPath + bannLogName);
            }
            Thread.Sleep(3000);
            useConsole = true;
            writeMainLog = new StreamWriter(baseDirectory + mainLogPath + mainLogName);
            writeBannReport = new StreamWriter(baseDirectory + mainLogPath + bannLogName);
            guidBlacklist = baseDirectory +  dataPath + guidBlackListName;
            portList = baseDirectory + dataPath + gamePortListName;
            ipBlackList = baseDirectory + dataPath + ipBlackListName;         
            binomDecriptKey = baseDirectory + dataPath + binomMD5Name;
            tmpList = baseDirectory + dataPath + tmpBannListName;
            Console.WriteLine("-> Successfull!");
        }

        public void writeInMainlog(string text, bool showConsole)
        {
            if (useConsole == true)
            {
                writeMainLog.WriteLine("[" + DateTime.UtcNow + "] " + text);
                if (showConsole != false)
                {
                    Console.WriteLine("[" + DateTime.UtcNow + "] " + text);
                }
            }

            writeMainLog.Flush();
        }

        public void writeInBannlog(string text, bool showConsole)
        {
            if (useConsole == true)
            {
                writeBannReport.WriteLine("[" + DateTime.Now + "] " + text);
                if (showConsole != false)
                {
                    Console.WriteLine("[" + DateTime.UtcNow.ToString("HH:mm") + "] " + text);
                }
            }
            
            writeBannReport.Flush();
        }

        public string getMD5Key()
        {
            using (StreamReader _MD5KeySR = new StreamReader(binomDecriptKey))
            {
                return _MD5KeySR.ReadToEnd();
            }
        }

        public List<string> getGUIDBlacklist()
        {
            StreamReader _GuidBlacklistSR = new StreamReader(guidBlacklist);
            List<string> guids = new List<string>();
            string[] arr;

            arr = _GuidBlacklistSR.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arr)
            {
                guids.Add(item);
            }
            return guids;
        }

        public List<string> getIPBlacklist()
        {
            StreamReader _IPBlackListSR = new StreamReader(ipBlackList);
            List<string> IPS = new List<string>();
            string[] arr;

            arr = _IPBlackListSR.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arr)
            {
                IPS.Add(item);
            }
            return IPS;
        }

        public List<string> getPorts()
        {
            StreamReader _Ports = new StreamReader(portList);
            List<string> PortList = new List<string>();
            string[] arr;
            arr = _Ports.ReadToEnd().Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in arr)
            {
                PortList.Add(item);
            }
            
            return PortList;
        }

        public Dictionary<string, string> getTempBanList()

        {
            StreamReader _GuidBlacklistSR = new StreamReader(tmpList);
            Dictionary<string, string> MatchedEntry = new Dictionary<string, string>();
            string[] arr;
            arr = _GuidBlacklistSR.ReadToEnd().Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in arr)
            {
                string[] info;

                info = item.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                MatchedEntry.Add(info[1], info[2]);

            }
            return MatchedEntry;
        }
    }
}
