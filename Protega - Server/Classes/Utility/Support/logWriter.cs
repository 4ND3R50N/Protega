using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Support
{
    class logWriter{

        string path;

        public logWriter(string path) {
            this.path = path;    
        }

        public void writeInLog(bool consoleOutput, string text)
        {
            if (consoleOutput)
            {
                conOut(text);
                logFile(text);
            }
            else
            {
                logFile(text);
            }
        }

        private void conOut(string text)
        {
            DateTime DateTime = DateTime.Now;
            Console.WriteLine("[" + DateTime + "]: " + text);
        }

        private void logFile(string text)
        {
            DateTime DateTime = DateTime.Now;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
            {
                file.WriteLine("[" + DateTime + "]: " + text);
            }
        }
    }
}
