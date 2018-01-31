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
        static ApplicationAdapter _instance = null;
        object _adapter;

        // method definitions (i.e. API declaration)
        MethodInfo _KickUser;
        MethodInfo _BanUser;

        public static ApplicationAdapter getInstance()
        {
            if (_instance != null) return _instance;

            return new ApplicationAdapter();
        }

        private ApplicationAdapter()
        {
            String path = Directory.GetCurrentDirectory() + "\\demoapplicationadapter.dll";
            Assembly lib = Assembly.LoadFile(path);
            Type type = lib.GetType("Protega___Server.Classes.ApplicationAdapter.ApplicationAdapter");
            _adapter = Activator.CreateInstance(type);
            _KickUser = type.GetMethod("KickUser");
            _BanUser = type.GetMethod("BanUser");
        }

        public Boolean KickUser()
        {
           return (Boolean) _KickUser.Invoke(_adapter, null);
        }

        public Boolean BanUser()
        {
            return (Boolean) _BanUser.Invoke(_adapter, null);
        }

    }
}
