using System;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes.Service
{
    class SHackHeuristic
    {
        public static bool Insert(string _HardwareID, string _ProcessName, string _WindowName, string _ClassName, string _MD5Value)
        {
            return DHackHeuristic.Insert(new EHackHeuristic() { ProcessName = _ProcessName, WindowName = _WindowName, ClassName = _ClassName, MD5Value = _MD5Value, User = new EPlayer() { ID = _HardwareID } });
        }
    }
}
