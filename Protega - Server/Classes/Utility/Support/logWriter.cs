using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace Support
{
    public class logWriter
    {
        public delegate void WriteLog(int Importance, LogCategory Category, LoggerType LType, string Message);
        public WriteLog writeLog;
        string path;
        public int LogLevel;
        public int ApplicationID = 0;

        public logWriter(string path, int LogLevel)
        {
            this.path = path;
            this.LogLevel = LogLevel;
            writeLog += writeInLog;
        }

        public void writeInLog(int Importance, LogCategory Category, LoggerType LType, string Message)
        {
            if (Category == LogCategory.ERROR || Category == LogCategory.CRITICAL)
            {
                LogDatabase(Importance, Category, LType, Message);
            }

            //1=Important, 2=Medium, 3=Debug Infos
            if (Importance > LogLevel)
                return;
            
            string OutMessage = string.Format("[{0} {1}-{4}]: ({2}) - {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), Category, Message, DateTime.Now.Millisecond);
    
            //if(!Message.Contains("Protocol received"))
                conOut(OutMessage);
            logFile(OutMessage);
        }

        private void conOut(string Message)
        {
            Console.WriteLine(Message);
        }

        void LogDatabase(int Importance, LogCategory Category, LoggerType LType, string Message)
        {
            Protega___Server.Classes.Entity.ELoggerData LogResult=null;
            if (ApplicationID != 0)
                LogResult = Protega___Server.Classes.SLoggerData.Insert(ApplicationID, Category, LType, Importance, Message);

            if (LogResult==null)
            {
                string OutMessage = string.Format("[{0} {1}-{4}]: ({2}) - {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), "CRITICAL (m)", "Log Result null!", DateTime.Now.Millisecond);
                logFile(OutMessage);
            }

            if(LogResult.ID=="-1")
            {
                string OutMessage = string.Format("[{0} {1}-{4}]: ({2}) - {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), "CRITICAL (m)", "Log Result -1!", DateTime.Now.Millisecond);
                logFile(OutMessage);
            }
            if(LogLevel==4)
            {
                string OutMessage = string.Format("[{0} {1}-{4}]: ({2}) - {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), "OK", "Logging succeeded. ID: " + LogResult.ID, DateTime.Now.Millisecond);
                logFile(OutMessage);
            }

        }

        private void logFile(string Message)
        {
            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.WriteLine(Message);
            }
        }

        public void Seperate()
        {
            logFile("---------------------------------------------------------------");
        }

    }

    public enum LogCategory
    { OK, ERROR, CRITICAL }

    public enum LoggerType
    { SERVER, CLIENT, GAMEDLL, DATABASE }
}
