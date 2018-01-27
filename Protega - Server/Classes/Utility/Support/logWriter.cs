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
        string path;
        int LogLevel;

        public logWriter(string path, int LogLevel)
        {
            this.path = path;
            this.LogLevel = LogLevel;
        }

        public void writeInLog(int Importance, LogCategory Category, string Message)
        {
            if (Importance > LogLevel)
                return;
            
            string OutMessage = string.Format("[{0} {1}]: ({2}) - {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), Category, Message);
                     
            conOut(OutMessage);
            logFile(OutMessage);
            //if (Importance >= 3)
            //    LogDatabase(BasicInformation, DetailledInformation);
        }

        private void conOut(string Message)
        {
            Console.WriteLine(Message);
        }

        void LogDatabase(string BasicInformation, string DetailledInformation)
        {
            //@Sunny Überleg dir wie wir Logs in der Datenbank speichern wollen
            //Welche Infos sind wirklich relevant? Wie übergeben wir die?
            //Wir können gern das aktuelle Logging anpassen, wenn der Aufbau anders sein soll
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
}
