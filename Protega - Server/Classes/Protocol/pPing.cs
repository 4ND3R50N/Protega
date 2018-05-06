using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Protocol
{
    class pPing : InterfaceRunTimeTasks
    {
        public string AdditionalMessage;

        public pPing(ref networkServer.networkClientInterface Client, Protocol prot)
        {
            this.Client = Client;
            this.prot = prot;
            this.Session = prot.GetUserID();
        }

        public bool Initialize()
        {
            if (prot.HasValues())
            {
                int Parameter;
                if (Int32.TryParse(prot.GetValues()[0].ToString(), out Parameter))
                {
                    switch (Parameter)
                    {
                        case 1:
                            AdditionalMessage = ";123";
                            return true;
                        default:
                            break;
                    }
                }
            }
            return false;
        }
    }
}
