using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Protega___Server.Classes.Protocol
{
    class pAuthentication:InterfaceLoginLogout
    {
        public string ApplicationHash;
        public string architecture;
        public string language;
        public double version;
        public IPAddress IPtoGame;
        public Protocol prot;

        public pAuthentication(ref networkServer.networkClientInterface Client, Protocol protocol)
        {
            this.Client = Client;
            this.prot = protocol;
        }
        public bool Initialize()
        {
            if (this.Client == null)
                throw new Exception("Client is null!");
            
            ArrayList ProtValues = prot.GetValues();
            if (ProtValues == null)
                throw new Exception("Could not fetch protocol values!");
            if (ProtValues.Count != 5)
                throw new Exception("Parameter Length incorrect, having " + ProtValues.Count + ", expecting 5");

            this.ApplicationHash = ProtValues[1].ToString();
            this.architecture = ProtValues[2].ToString();
            this.language = ProtValues[3].ToString();
            if (!Double.TryParse(ProtValues[0].ToString(), out this.version))
                throw new Exception("Incorrect version format: " + ProtValues[0].ToString());

            if (!IPAddress.TryParse(ProtValues[4].ToString(), out this.IPtoGame))
                throw new Exception("Incorrect IP format: " + ProtValues[4].ToString());

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
