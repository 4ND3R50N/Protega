using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Protega___Server.Classes.Protocol
{
    class pHackDetectionHeuristic:InterfaceRunTimeTasks
    {
        public Entity.EHackHeuristic hackData;
        string ProcessName = null;
        string MD5Value = null;

        public pHackDetectionHeuristic(ref networkServer.networkClientInterface Client, Protocol prot)
        {
            hackData = new Entity.EHackHeuristic();
            this.Client = Client;
            this.prot = prot;
        }

        public bool Initialize(out int ErrorCode)
        {
            ErrorCode = 0;
            ArrayList Objects = prot.GetValues();
            if (Objects.Count != 2)
            {
                //Log error - protocol size not as expected
                ErrorCode = 1;
                return false;
            }

            //The section ID defines which hack detection method triggered
            int SectionID;
            if (!Int32.TryParse(Objects[0].ToString(), out SectionID))
            {
                ErrorCode = 2;
                return false;
            }
            
            //The section ID defines which value is sent
            switch (SectionID)
            {
                case 1:
                    hackData.ProcessName = Convert.ToString(Objects[1]);
                    break;
                case 2:
                    hackData.MD5Value = Convert.ToString(Objects[1]);
                    break;
                default:
                    break;
            }
            hackData.ApplicationID = Client.User.Application.ID;
            hackData.User = Client.User;

            return true;
        }
    }
}
