using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protega.ApplicationAdapter.Classes.Database.Data;
using Protega.ApplicationAdapter.Classes.Database.Entity;

namespace Protega.ApplicationAdapter.Classes.Database.Service
{
    static class SPlayer
    {
        public static List<EPlayer> GetOnlineIst()
        {
            return DPlayer.GetOnlineList();
        }
    }
}
