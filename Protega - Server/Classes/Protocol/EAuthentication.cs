﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Protocol
{
    class EAuthentication
    {
        public networkServer.networkClientInterface Client;
        public string ApplicationHash;
        public string architecture;
        public string language;
        public double version;
        Protocol prot;

        public EAuthentication(ref networkServer.networkClientInterface Client, Protocol protocol)
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
        
        public int TimePassedSecs()
        {
            return (int)Math.Round(prot.TimeNeededSecs().TotalSeconds);
        }
    }
}
