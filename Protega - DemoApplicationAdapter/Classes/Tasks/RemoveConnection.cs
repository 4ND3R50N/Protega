using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Protega.ApplicationAdapter.Classes.Tasks
{
    class RemoveConnection:_InterfaceTask
    {
        public RemoveConnection(IPAddress IP, DateTime TimeStamp, string Username = null) : base(IP, TimeStamp, Username)
        {

        }
    }
}
