using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Support;
using System.Net;
using System.Net.Sockets;
using Protega___Server.Classes.Protocol;

namespace Protega___Server.Classes
{
    public static class CCstDatabase
    {
        #region Stored Procedures

        #region Application
        public const string SP_Application_GetByName = "Application_GetByName";
        #endregion

        #region User
        public const string SP_User_GetByName = "Player_GetByName";
        public const string SP_User_Authenticate = "User_Authenticate";
        #endregion

        #region LoggerType
        public const string SP_LoggerData_Insert = "Log_Insert";
        #endregion

        #region Hack Detection
        public const string SP_HackDetection_Insert_Heuristic = "HackDetection_Insert_Heuristic";
        public const string SP_HackDetection_Insert_VirtualMemory = "HackDetection_Insert_VirtualMemory";
        public const string SP_HackDetection_Insert_File = "HackDetection_Insert_File";
        #endregion

        #endregion
    }
}
