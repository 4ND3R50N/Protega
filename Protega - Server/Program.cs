using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protega___Server.Classes.Core;
using System.IO;

namespace Protega___Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ControllerCore Controller = new ControllerCore(1, ';', 'a', "asdf", "mssql", "217.23.14.23", 1433, "sa", "S3mad0123", "Network", String.Format(@"{0}/Test.txt", Directory.GetCurrentDirectory()));
            Controller.Start();
            Console.ReadLine();
        }
    }
}
