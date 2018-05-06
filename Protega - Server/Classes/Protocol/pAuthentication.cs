using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Protocol
{
    class pAuthentication:InterfaceLoginLogout
    {
        public string ApplicationHash;
        public string architecture;
        public string language;
        public double version;
        protected Protocol prot;

        public pAuthentication(ref networkServer.networkClientInterface Client, Protocol protocol)
        {
            this.Client = Client;
            this.prot = protocol;
        }
        public bool Initialize()
        {
            if (this.Client == null)
                return false;
            
            ArrayList ProtValues = prot.GetValues();
            if (ProtValues == null || ProtValues.Count != 4)
                return false;

            this.ApplicationHash = ProtValues[1].ToString();
            this.architecture = ProtValues[2].ToString();
            this.language = ProtValues[3].ToString();
            if (!Double.TryParse(ProtValues[0].ToString(), out this.version))
                return false;
            
            return true;
        }

        public string HardwareID
        {
            get { return prot.GetUserID(); }
        }

        public string TimePassed()
        {
            return prot.TimePassedMs();
        }
        public DateTime TimeStampStart()
        {
            return prot.TimeStampStart();
        }
        
    }
}
