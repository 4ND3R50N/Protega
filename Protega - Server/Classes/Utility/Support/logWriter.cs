using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Support
{
    public class logWriter
    {

        string path;
        string classs;

        public logWriter(string classs)
        {
            this.path = Directory.GetCurrentDirectory();
            this.classs = classs;
        }

        public logWriter(string path, string classs)
        {
            this.path = path;
            this.classs = classs;
        }

        public void writeInLog(bool consoleOutput, LoggingStatus status, string text)
        {
            if (consoleOutput)
            {
                conOut(status, text);
            }
            //logFile(text);
            logFile(status, text);
        }

        private void conOut(LoggingStatus status, string text)
        {
            DateTime DateTime = DateTime.Now;
            Console.WriteLine(String.Format("[{0}]: {1} - [{2}]: {3}!", DateTime, status, classs, text));
        }

        private void logFile(LoggingStatus status, string text)
        {
            DateTime DateTime = DateTime.Now;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
            {
                file.WriteLine(String.Format("[{0}]: {1} - [{2}]: {3}!", DateTime, status, classs, text));
            }
        }
        //private void LogInDatabase(string Text)
    }

    public enum LoggingStatus
    {
        OKAY, WARNING, ERROR
    }
}
