using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Support;
using System.Net;
using System.Net.Sockets;
using Protega___Server.Classes.Protocol;

namespace Protega.ApplicationAdapter.Classes.Database
{
    public static class CCstDatabase
    {
        public static Utility.DBEngine DatabaseEngine;

        #region Stored Procedures

        #region CheckLogins
        public const string OnlinePlayers_GetList = "select CharacterIdx, Name, A.LastIp, C.[Login] as 'isOnline'"
                                                  + "from Server01.dbo.cabal_character_table C"
                                                  + "join Account.dbo.cabal_auth_table A on C.CharacterIdx between A.UserNum*8 and A.UserNum*8+7"
                                                  + "where C.[Login]= 1";
        #endregion

        #endregion
    }
}
