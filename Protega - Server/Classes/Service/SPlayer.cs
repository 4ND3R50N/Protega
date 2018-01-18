using System;
using Protega___Server.Classes.Entity;
using Protega___Server.Classes.Data;

namespace Protega___Server.Classes
{
    static class SPlayer
    {
        //public static EPlayer GetByName(string Name)
        //{
        //    return DPlayer.GetByName(Name);
        //}

        public static EPlayer Authenticate(string ComputerID, string ApplicationHash, string Architecture, string Language, string Ip)
        {
            return DPlayer.Authenticate(new EPlayer() { ID = ComputerID, Application = (new EApplication() { Hash = ApplicationHash }), OperatingSystem = Architecture, Language = Language, IP = Ip });
        }
    }
}
