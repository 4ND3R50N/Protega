using System;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes
{
    static class SPlayer
    {
        public static EPlayer GetByName(string Name)
        {
            return DPlayer.GetByName(Name);
        }

        public static EPlayer Authenticate(string ComputerID, string Architecture, string Language, string Ip)
        {
            return DPlayer.Authenticate(new EPlayer() { ID = ComputerID, OperatingSystem = Architecture, Language = Language, IP = Ip });
        }
    }
}
