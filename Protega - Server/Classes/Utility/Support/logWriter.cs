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

        public void writeInLog(int Importance, string Message)
        {
            switch (Importance)
            {
                case 1:
                    Message = "Low" + Message;
                    break;
                case 2:
                    Message = "Medium" + Message;
                    break;
                case 3:
                    Message = "Critical" + Message;
                    break;
                default:
                    break;
            }

            
            conOut(Message);
            logFile(Message);
            //if (Importance >= 3)
            //    LogDatabase(BasicInformation, DetailledInformation);
        }

        private void conOut(string Message)
        {
            Message= String.Format("[{0} {1}]]: {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), Message);
            Console.WriteLine(Message);
        }

        void LogDatabase(string BasicInformation, string DetailledInformation)
        {
            
        }

        private void logFile(string Message)
        {

            Message = String.Format("[{0} {1}]]: {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), Message);
            using (StreamWriter file = new StreamWriter(path))
            {
                file.WriteLine(Message);
            }
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
            //{
            //    file.WriteLine(String.Format("[{0}]: {1} - [{2}]: {3}!", DateTime, status, classs, text));
            //}
        }

        
    }
}
