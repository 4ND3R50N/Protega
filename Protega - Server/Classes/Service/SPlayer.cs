using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes
{
    static class SPlayer
    {
        public static EPlayer GetByName(string Name, DBEngine dBEngine)
        {
            return DPlayer.GetByName(Name, dBEngine);
        }
    }
}
