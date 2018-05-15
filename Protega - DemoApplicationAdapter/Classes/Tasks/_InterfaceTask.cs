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
            Ports.Clear();
            Ports.Add(50001);
            Ports.Add(50002);
            Ports.Add(50003);
            Ports.Add(50004);
            Ports.Add(50005);
            Ports.Add(50006);
            Ports.Add(50007);
            Ports.Add(50008);
            Ports.Add(50009);
            Ports.Add(50010);
            Ports.Add(50011);
            Ports.Add(50012);
            Ports.Add(50013);
            Ports.Add(50014);
            Ports.Add(50015);
            Ports.Add(50016);
            Ports.Add(50017);
            Ports.Add(50018);
            Ports.Add(50019);
            Ports.Add(50020);

            char InOut='a';
            if (this is InsertConnection)
                InOut = 'I';
            else if (this is RemoveConnection)
                InOut = 'D';
            else
                return false;

            //IPTables allow max 15 entries
            List<string> lPorts = SplitAmountOfPorts(Ports, Length);

            LinuxQuery = "";
            foreach (var item in lPorts)
            {
                LinuxQuery += "iptables -" + InOut.ToString() + " INPUT -p tcp -s " + IP.ToString() + " --match multiport --dport " + item + " -j ACCEPT && ";
            }
            LinuxQuery = LinuxQuery.TrimEnd(' ').TrimEnd('&');
            return true;
        }
        

        static List<string> SplitAmountOfPorts(List<int> Input, int Length)
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
