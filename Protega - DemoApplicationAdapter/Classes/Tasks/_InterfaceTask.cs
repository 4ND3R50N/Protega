using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Protega.ApplicationAdapter.Classes.Tasks
{
    public abstract class _InterfaceTask
    {
        public string Username;
        public IPAddress IP;
        public DateTime TimeStamp;
        public string LinuxQuery="";
        public Protega___Server.Classes.Utility.ApplicationAdapter.Task Task;

        public _InterfaceTask(IPAddress IP, DateTime TimeStamp, string Username=null)
        {
            this.IP = IP;
            this.Username = Username;
            this.TimeStamp = TimeStamp;
            if (this is InsertConnection)
                Task = Protega___Server.Classes.Utility.ApplicationAdapter.Task.InsertConnection;
            else if (this is RemoveConnection)
                Task = Protega___Server.Classes.Utility.ApplicationAdapter.Task.RemoveConnection;
        }
        
        public bool BuildLinuxQuery(List<int> Ports, int Length)
        {

            char InOut='a';
            if (this is InsertConnection)
                InOut = 'I';
            else if (this is RemoveConnection)
                InOut = 'D';
            else
                return false;

            //IPTables allow max 15 entries
            List<string> lPorts = SplitAmountOfPorts(Ports, Length);

            foreach (var item in lPorts)
            {
                LinuxQuery += "iptables -" + InOut.ToString() + " INPUT -p tcp -s " + IP.ToString() + " --match multiport --dport " + item + " -j ACCEPT && ";
            }
            LinuxQuery = LinuxQuery.TrimEnd(' ').TrimEnd('&');
            return true;
        }
        

        List<string> SplitAmountOfPorts(List<int> Input, int Length)
        {
            List<string> Output = new List<string>();
            int Counter = 0;
            string Concat = "";
            foreach (int item in Input)
            {
                if (Concat.Length == 0)
                    Concat = item.ToString();
                else
                    Concat += "," + item.ToString();

                if (++Counter == Length)
                {
                    Output.Add(Concat);
                    Concat = "";
                }
            }
            if (Concat.Length > 0)
                Output.Add(Concat);
            return Output;
        }
    }
}
