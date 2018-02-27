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
            
            string OutMessage = string.Format("[{0} {1}]: ({2}) - {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), Category, Message);
    
            if(!Message.Contains("Protocol received"))
                conOut(OutMessage);
            logFile(OutMessage);
        }

        private void conOut(string Message)
        {
            Console.WriteLine(Message);
        }

        void LogDatabase(int Importance, LogCategory Category, LoggerType LType, string Message)
        {
            if (ApplicationID != 0)
                Protega___Server.Classes.SLoggerData.Insert(ApplicationID, Category, LType, Importance, Message);
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
