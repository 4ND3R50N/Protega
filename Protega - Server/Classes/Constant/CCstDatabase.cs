using System;

namespace Protega___Server.Classes
{
    public static class CCstDatabase
    {
        public static DBEngine DatabaseEngine;

        #region Stored Procedures

        #region User
        public const string SP_User_GetByName = "Player_GetByName";
        public const string SP_User_Authenticate = "User_Authenticate";
        #endregion

        #region LoggerType
        public const string SP_LoggerType_GetList = "LoggerType_GetList";
        #endregion

        #region Hack Detection
        public const string SP_HackDetection_Insert_Heuristic = "HackDetection_Insert_Heuristic";
        public const string SP_HackDetection_Insert_VirtualMemory = "HackDetection_Insert_VirtualMemory";
        #endregion

        #endregion
    }
}
