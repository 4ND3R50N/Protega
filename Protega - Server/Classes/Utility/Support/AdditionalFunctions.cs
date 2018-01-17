using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes
{
    static class AdditionalFunctions
    {
        static public string GenerateSessionID(int Length)
        {
            Random ran = new Random();
            string SessionID = "";
            string Symbols = "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ0123456789";
            for (int i = 0; i < Length; i++)
            {
                SessionID += Symbols[ran.Next(0, Symbols.Length)];
            }
            return SessionID;
        }
    }
}
