using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Protega___Server.Classes;


namespace Protega___Server.Classes
{
    public static class CCstDatabase
    {
        public static DBEngine DatabaseEngine;

        #region Stored Procedures

        #region Player
        public const string SP_Player_GetByName = "Player_GetByName";
        #endregion

        #endregion
    }
}
