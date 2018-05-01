using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Protocol
{
    class EPing:pRuneTimeTasks
    {
        public networkServer.networkClientInterface Client;
        public string SessionID;
        public string AdditionalMessage = null;
        public DateTime TimeStamp;

        public EPing(ref networkServer.networkClientInterface Client)
        {
            TimeStamp = DateTime.Now;
            this.Client = Client;
        }
    }
}
