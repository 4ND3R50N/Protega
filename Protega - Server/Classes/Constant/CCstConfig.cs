using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes
{
    public static class CCstConfig
    {
        public const string EncryptionKey = "1234567890123456";
        public const string EncryptionIV = "bbbbbbbbbbbbbbbb";

        public const int ApplicationID = 1;

        public const int SessionIDLength = 10;
        public const int PingTimer = 9999000;
    }
}
