using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Protocol
{
    abstract class InterfaceRunTimeTasks
    {
        public InterfaceRunTimeTasks() { }

        public Protocol prot;
        public networkServer.networkClientInterface Client;
        public string Session;

        public string TimePassed()
        {
            return prot.TimePassedMs();
        }
    }
}
