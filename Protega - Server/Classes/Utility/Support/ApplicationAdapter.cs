using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

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
        
        public ApplicationAdapter(string DllPath, Entity.EApplication Application)
        {
            this.Application = Application;
            Assembly lib = Assembly.LoadFile(DllPath);
            Type type = lib.GetType("Protega.ApplicationAdapter.ApplicationAdapter");
            string Test = lib.FullName;
            TestIt += ApplicationAdapter_TestIt;
            _adapter = Activator.CreateInstance(type, new object[2] {"", 1});
            _KickUser = type.GetMethod("KickUser");
            _BanUser = type.GetMethod("BanUser");
            _AllowUser = type.GetMethod("AllowUser");
            _PrepareServer = type.GetMethod("PrepareServer");
            
            EventInfo test2 = type.GetEvent("TestingEvent");
            Type test2Type = test2.EventHandlerType;

            //TestIt = type.GetEvent("");

            MethodInfo myFunction = typeof(ApplicationAdapter).GetMethod("ApplicationAdapter_TestIt", BindingFlags.NonPublic | BindingFlags.Instance);
            Delegate delegateHandler = Delegate.CreateDelegate(test2Type, this, myFunction);
            test2.AddEventHandler(this, delegateHandler);

            MethodInfo TestEvent = type.GetMethod("TestEvent");
            TestEvent.Invoke(_adapter, new object[1] { "" });


            MethodInfo CTest = test2.GetRaiseMethod();
            CTest.CreateDelegate(test2.EventHandlerType.GetType(), TestIt);
            MethodInfo SetEvent = type.GetMethod("SetEvent");

            SetEvent.Invoke(_adapter, new object[1] {  TestIt });
            
            test2.AddEventHandler(_adapter, TestIt);
            MethodInfo test5= test2.AddMethod;
            EventInfo[] test3 = type.GetEvents();
            MethodInfo[] test4 = type.GetMethods();


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
            return (bool)_PrepareServer.Invoke(_adapter, new object[7] { ServerIP, LoginName, LoginPass, LoginPort, BlockedPorts, DefaultCommand, TestIt});
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
