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
    }
}
