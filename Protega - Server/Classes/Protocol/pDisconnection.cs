using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Protega___Server.Classes.Protocol
{
    class pDisconnection:InterfaceLoginLogout
    {
        DateTime LogoutTriggered;
        
        public pDisconnection(ref networkServer.networkClientInterface Client)
        {
            LogoutTriggered = DateTime.Now;
            this.Client = Client;
        }

        public string TimePassed()
        {
            return Math.Round((DateTime.Now - LogoutTriggered).TotalMilliseconds) + "ms";
        }
    }
}
