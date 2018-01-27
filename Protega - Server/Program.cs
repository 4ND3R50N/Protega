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
            ControllerCore Controller = new ControllerCore("Test", 10000, ';', 'a', "asdf", "mssql", "62.138.6.50", 1433, "sa", "h4TqSDs762eqbEyw", "Protega", String.Format(@"{0}/Log.txt", Directory.GetCurrentDirectory()),3);
            Controller.Start();
            Console.ReadLine();
        }
    }
}
