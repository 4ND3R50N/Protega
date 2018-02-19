using System;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes
{
    public static class SLoggerData
    {
        public static ELoggerData Insert(int _ApplicationID, Support.LogCategory _Category, Support.LoggerType _Type, int _Importance, string _Message)
        {
            return DLoggerData.Insert(new ELoggerData() { ApplicationID = _ApplicationID, Category = _Category, Importance = _Importance, Message = _Message, Type = _Type });
        }
    }
}
