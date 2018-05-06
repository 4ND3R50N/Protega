using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Support;
using System.Net;

namespace Protega___Server.Classes.Utility
{
    public class ApplicationAdapter
    {
        public enum Task { InsertConnection, RemoveConnection, PrepareServer }
        public enum Result { SUCCESS, FAIL }
        public delegate void DllFeedback(string Username, IPAddress IP, Task task, Result result, DateTime TimeStamp);
        object _adapter;
        Entity.EApplication Application;

        // method definitions (i.e. API declaration)
        MethodInfo _KickUser;
        MethodInfo _BanUser;
        MethodInfo _AllowUser;
        MethodInfo _PrepareServer;
        AppDomain appDomain;

        public bool ConstructorSuccessful = false;

        public ApplicationAdapter(string DllPath, Entity.EApplication Application)
        {
            this.Application = Application;
            

            Assembly lib = Assembly.LoadFile(DllPath);

            // maybe change this so that the class name can be loaded dyamically, e.g. from config file?
            Type type = lib.GetType("Protega.ApplicationAdapter.ApplicationAdapter");
            _adapter = Activator.CreateInstance(type, new object[0]);
            _KickUser = type.GetMethod("KickUser");
            _BanUser = type.GetMethod("BanUser");
            _AllowUser = type.GetMethod("AllowUser");
            _PrepareServer = type.GetMethod("PrepareServer");
            ConstructorSuccessful = true;
        }

        public bool PrepareServer(string ConfigPath, DllFeedback dllFeedback)
        {
            return (bool)_PrepareServer.Invoke(_adapter, new object[4] {ConfigPath, Application.Name, CCstData.GetInstance(Application).Logger.writeLog, dllFeedback });
        }

        public bool AllowUser(IPAddress IP, string UserName, DateTime TimeStamp)
        {
            return (bool)_AllowUser.Invoke(_adapter, new object[3] { IP, UserName, TimeStamp });
        }

        public bool KickUser(IPAddress IP, string UserName, DateTime TimeStamp)
        {
            return (bool)_KickUser.Invoke(_adapter, new object[3] { IP, UserName, TimeStamp });
        }

        public bool BanUser(IPAddress IP, string UserName, DateTime BanTime)
        {
            return (bool)_BanUser.Invoke(_adapter, new object[3] { IP, UserName, BanTime });
        }

    }

    public class Proxy : MarshalByRefObject
    {
        public Assembly GetAssembly(string assemblyPath)
        {
            try
            {
                return Assembly.LoadFile(assemblyPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + e.Message);
                return null;
                // throw new InvalidOperationException(ex);
            }
        }
    }
}
