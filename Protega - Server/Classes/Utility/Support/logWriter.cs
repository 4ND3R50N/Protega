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
        string banPath;
        public int LogLevel;
        public int ApplicationID = 0;

        const string DateFormatInLog = "{0:dd.MM HH:mm:ss (fff)}";
        const string DateFormatInConsole = "{0:HH:mm:ss}";

        public logWriter(string path, string banPath, int LogLevel)
        {
            this.path = path;
            this.banPath = banPath;
            this.LogLevel = LogLevel;
            writeLog += writeInLog;
        }

        public void writeInLog(int Importance, LogCategory Category, LoggerType LType, string Message)
        {
            string DateFormatLog = String.Format(DateFormatInLog, DateTime.Now);
            string DateFormatConsole = String.Format(DateFormatInConsole, DateTime.Now);
            if (Category == LogCategory.ERROR || Category == LogCategory.CRITICAL)
            {
                LogDatabase(Importance, Category, LType, Message, DateFormatLog);
            }

            //1=Important, 2=Medium, 3=Debug Infos
            if (Importance > LogLevel)
                return;

            if (Importance <= 3)
                conOut(String.Format("{0} {1} - {2}", DateFormatConsole, Category, Message));
            logFile(String.Format("[{0}] {1} - {2}", DateFormatLog, Category, Message));
        }

        private void conOut(string Message)
        {
            Console.WriteLine(Message);
        }

        void LogDatabase(int Importance, LogCategory Category, LoggerType LType, string Message, string DateFormat)
        {
            Protega___Server.Classes.Entity.ELoggerData LogResult = null;
            try
            {
                if (ApplicationID != 0)
                    LogResult = Protega___Server.Classes.SLoggerData.Insert(ApplicationID, Category, LType, Importance, Message);
                if (LogResult == null)
                    return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            if (LogResult == null)
            {
                string OutMessage = string.Format("[{0}]: ({1}) - {2}", DateFormat, "CRITICAL (m)", "Log Result null!");
                logFile(OutMessage);
            }

            if (LogResult.ID == "-1")
            {
                string OutMessage = string.Format("[{0}]: ({1}) - {2}", DateFormat, "CRITICAL (m)", "Log Result -1!");
                logFile(OutMessage);
            }
            if (LogLevel == 4)
            {
                string OutMessage = string.Format("[{0}]: ({1}) - {2}", DateFormat, "Ok", "DB Logging succeeded. ID: " + LogResult.ID);
                logFile(OutMessage);
            }

        }

        private System.Threading.ReaderWriterLockSlim lock_ = new System.Threading.ReaderWriterLockSlim();
        private void logFile(string Message)
        {
            lock_.EnterWriteLock();
            try
            {
                //while (LogInUse)
                //{
                //    System.Threading.Thread.Sleep(100);
                //}
                //LogInUse = true;
                using (StreamWriter file = new StreamWriter(path, true))
                {
                    file.WriteLine(Message);

                }
                //LogInUse = false;

            }
            catch (Exception e)
            {
                writeInLog(2, LogCategory.ERROR, LoggerType.SERVER, "Couldnt write log in file! " + Message);
            }
            finally
            {
                lock_.ExitWriteLock();
            }
        }

        private System.Threading.ReaderWriterLockSlim Banlock_ = new System.Threading.ReaderWriterLockSlim();
        private void logFileBan(string Message)
        {
            Banlock_.EnterWriteLock();
            try
            {
                //while (LogInUse)
                //{
                //    System.Threading.Thread.Sleep(100);
                //}
                //LogInUse = true;
                using (StreamWriter file = new StreamWriter(banPath, true))
                {
                    file.WriteLine(Message);

                }
                //LogInUse = false;

            }
            catch (Exception e)
            {
                writeInLog(2, LogCategory.ERROR, LoggerType.SERVER, "Couldnt write log in file! " + Message);
            }
            finally
            {
                Banlock_.ExitWriteLock();
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
