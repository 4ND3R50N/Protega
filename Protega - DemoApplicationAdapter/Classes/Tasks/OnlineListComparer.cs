using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Protega.ApplicationAdapter.Classes.Database.Service;
using Protega.ApplicationAdapter.Classes.Database.Entity;

namespace Protega.ApplicationAdapter.Classes.Tasks
{
    class OnlineListComparer
    {
        List<EPlayer> OnlinePlayers;

        public OnlineListComparer()
        {
            OnlinePlayers = new List<EPlayer>();
        }

        void GetOnlinePlayers()
        {
            List<EPlayer> OnlinePlayersNow = SPlayer.GetOnlineIst();
            foreach (EPlayer item in OnlinePlayersNow)
            {
                if(!OnlinePlayers.Contains(item))
                {
                    NewPlayer(item);
                }
            }
            OnlinePlayers = OnlinePlayersNow;
        }

        void NewPlayer(EPlayer Player)
        {
            
        }

    }
}
