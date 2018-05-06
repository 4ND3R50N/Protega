using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Protega.ApplicationAdapter.Classes.Tasks
{
    class InsertConnection:_InterfaceTask
    {

        public InsertConnection(IPAddress IP, DateTime TimeStamp, string Username = null) : base(IP, TimeStamp, Username)
        {

        }

    }
}
