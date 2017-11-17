using System;

namespace Protega___Server.Classes
{
    public static class CCstDatabase
    {
        public static DBEngine DatabaseEngine;

        #region Stored Procedures

        #region Player
        public const string SP_Player_GetByName = "Player_GetByName";
        #endregion

        #region LoggerType
        public const string SP_LoggerType_GetList = "LoggerType_GetList";
        #endregion

        #endregion
    }
}
