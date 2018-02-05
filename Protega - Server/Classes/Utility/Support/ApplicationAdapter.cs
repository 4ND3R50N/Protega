using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Support;

namespace Protega___Server.Classes.Utility.Support
{
    public class ApplicationAdapter
    {
        object _adapter;
        Entity.EApplication Application;

        // method definitions (i.e. API declaration)
        MethodInfo _KickUser;
        MethodInfo _BanUser;
        MethodInfo _AllowUser;
        MethodInfo _PrepareServer;

        
        public delegate void Testing(string Test);
        public event Testing TestIt;
        logWriter.WriteLog LogError;
        
        

        public ApplicationAdapter(string DllPath, Entity.EApplication Application)
        {
            this.Application = Application;
            Assembly lib = Assembly.LoadFile(DllPath);
            // maybe change this so that the class name can be loaded dyamically, e.g. from config file?
            Type type = lib.GetType("Protega.ApplicationAdapter.ApplicationAdapter");
            string Test = lib.FullName;

            TestIt += ApplicationAdapter_TestIt;
            _adapter = Activator.CreateInstance(type, new object[2] {"", 1});
            _KickUser = type.GetMethod("KickUser");
            _BanUser = type.GetMethod("BanUser");
            _AllowUser = type.GetMethod("AllowUser");
            _PrepareServer = type.GetMethod("PrepareServer");

            PrepareServer(null, null, null, 0, null, null);
            //Delegate Handler = Delegate.CreateDelegate(test2.EventHandlerType, TestIt);
        }


        string TestSomething = "Not successful";
        private void ApplicationAdapter_TestIt(string something)
        {
            TestSomething = something;
            Console.WriteLine(something);
        }

        public bool PrepareServer(string ServerIP, string LoginName, string LoginPass, int LoginPort, List<int> BlockedPorts, string DefaultCommand)
        {
            return (bool)_PrepareServer.Invoke(_adapter, new object[7] { ServerIP, LoginName, LoginPass, LoginPort, BlockedPorts, DefaultCommand, CCstData.GetInstance(Application).Logger.writeLog });
        }

        public bool AllowUser(string IP, string UserName)
        {
            return (bool)_AllowUser.Invoke(_adapter, new string[2] { IP, UserName });
        }

        public void TestMethod()
        { }

        public bool KickUser(string IP, string UserName)
        {
           return (bool) _KickUser.Invoke(_adapter, new string[2] { IP, UserName } );
        }

        public bool BanUser(string IP, string UserName, DateTime BanTime)
        {
            return (bool)_BanUser.Invoke(_adapter, new object[3] { IP, UserName, BanTime });
        }

    }
}
