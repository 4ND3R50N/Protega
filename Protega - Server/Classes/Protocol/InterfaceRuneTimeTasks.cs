using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Protocol
{
    abstract class pRuneTimeTasks
    {
        public pRuneTimeTasks() { }

        protected Protocol prot;
        protected networkServer.networkClientInterface Client;
        
        public int TimePassedSecs()
        {
            return (int)Math.Round(prot.TimeNeededSecs().TotalSeconds);
        }
    }
}
