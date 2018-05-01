using System;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes
{
    static class SHackFile
    {
        public static bool Insert(string _HardwareID, int _ApplicationID, int _CaseID, string _Content)
        {
            return DHackFile.Insert(new EHackFile() { ApplicationID = _ApplicationID, CaseID = _CaseID, Content = _Content, User = new EPlayer() { ID = _HardwareID } });
        }
    }
}
