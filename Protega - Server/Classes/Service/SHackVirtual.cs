using System;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes
{
    class SHackVirtual
    {
        public static bool Insert(string _HardwareID, string _ApplicationName, string _BaseAddress, string _Offset, string _DetectedValue, string _DefaultValue)
        {
            return DHackVirtual.Insert(new EHackVirtual() { ApplicationName = _ApplicationName, BaseAddress = _BaseAddress, Offset = _Offset, DetectedValue = _DetectedValue, DefaultValue = _DefaultValue, User = new EPlayer() { ID = _HardwareID } });
        }
    }
}
