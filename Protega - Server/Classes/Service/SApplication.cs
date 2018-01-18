using System;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes
{
    static class SApplication
    {
        public static EApplication GetByName(string _Name, DBEngine _DatabaseEngine)
        {
            return DApplication.GetByName(new EApplication() { Name = _Name }, _DatabaseEngine);
        }
    }
}
