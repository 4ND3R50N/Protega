using System;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes
{
    static class SHackVirtual
    {
        public static bool Insert(string _HardwareID, int _ApplicationID, string _BaseAddress, string _Offset, string _DetectedValue, string _DefaultValue)
        {
            return DHackVirtual.Insert(new EHackVirtual() { ApplicationID = _ApplicationID, BaseAddress = _BaseAddress, Offset = _Offset, DetectedValue = _DetectedValue, DefaultValue = _DefaultValue, User = new EPlayer() { ID = _HardwareID } });
        }
    }
}
