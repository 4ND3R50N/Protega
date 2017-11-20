using System;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes
{
    static class LoggerType
    {
        public static ECollectionLoggerType GetList()
        {
            return DLoggerType.GetList();
        }
    }
}
