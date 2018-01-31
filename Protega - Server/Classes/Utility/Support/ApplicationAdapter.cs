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

        // method definitions (i.e. API declaration)
        MethodInfo _KickUser;
        MethodInfo _BanUser;
        MethodInfo _AllowUser;
        MethodInfo _PrepareServer;


        public ApplicationAdapter(string DllPath)
        {
            Assembly lib = Assembly.LoadFile(DllPath);
            Type type = lib.GetType("Protega.ApplicationAdapter.ApplicationAdapter");
            _adapter = Activator.CreateInstance(type);
            _KickUser = type.GetMethod("KickUser");
            _BanUser = type.GetMethod("BanUser");
            _AllowUser = type.GetMethod("AllowUser");
            _PrepareServer = type.GetMethod("PrepareServer");
        }

        public bool PrepareServer(string ServerIP, string LoginName, string LoginPass, int LoginPort, List<int> BlockedPorts, string DefaultCommand)
        {
            return (bool)_PrepareServer.Invoke(_adapter, new object[6] { ServerIP, LoginName, LoginPass, LoginPort, BlockedPorts, DefaultCommand });
        }

        public bool AllowUser(string IP, string UserName)
        {
            return (bool)_AllowUser.Invoke(_adapter, new string[2] { IP, UserName });
        }

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
