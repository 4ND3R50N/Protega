using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Protega.ApplicationAdapter.Classes
{
    public class SshConnectionManager
    {
        List<SshConnection> listSshConnections;
        string LinuxIP, LinuxLoginName, LinuxPassword;
        int LinuxPort;
        int SshInstancesToKeep;
        System.Timers.Timer tmrCleanup;
        Support.logWriter.WriteLog LogFunction;

        private readonly object LockList = new object();

        public SshConnectionManager(string LinuxIP, int LinuxPort, string LinuxLoginName, string LinuxPassword, int SshInstancesToKeep, Support.logWriter.WriteLog LogFunction)
        {
            this.LogFunction = LogFunction;
            this.LinuxIP = LinuxIP;
            this.LinuxPort = LinuxPort;
            this.LinuxLoginName = LinuxLoginName;
            this.LinuxPassword = LinuxPassword;
            this.SshInstancesToKeep = SshInstancesToKeep;
            listSshConnections = new List<SshConnection>();

            tmrCleanup = new System.Timers.Timer(10000);
            tmrCleanup.Elapsed += TmrCleanup_Elapsed;
            tmrCleanup.Start();
        }

        public int CreateInstances(int Amount)
        {
            //Create new instances in the list
            for (int i = 0; i < Amount; i++)
            {
                DateTime timestamp = DateTime.Now;
                SshConnection Client = new SshConnection(LinuxIP, LinuxPort, LinuxLoginName, LinuxPassword);
                if (Client.Initialize())
                {
                    lock (LockList)
                    {
                        listSshConnections.Add(Client);
                    }
                    LogFunction(2, Support.LogCategory.OK, Support.LoggerType.GAMEDLL, "DLL: Ssh Instance number " + listSshConnections.Count.ToString() + " created! (" + (Math.Round((DateTime.Now - timestamp).TotalMilliseconds).ToString() + "ms" + ")"));
                }
                else
                    LogFunction(2, Support.LogCategory.OK, Support.LoggerType.GAMEDLL, "DLL: Creating new Ssh instance failed! Remaining " + listSshConnections.Count.ToString());
            }
            return listSshConnections.Count;
        }

        public SshConnection GetAvailableSshClient()
        {
            foreach (var item in listSshConnections)
            {
                //if(item.reLoadThis)
                //{
                    
                //    if (CreateInstances(1) != 1)
                //        return null;
                //    else
                //        return GetAvailableSshClient();
                //}
                if (item.isAvailable)
                {
                    item.isAvailable = false;
                    item.LastUsed = DateTime.Now;
                    return item;
                }
            }
            if (CreateInstances(1) == 1)
                return GetAvailableSshClient();
            else
                return null;
        }

        private void TmrCleanup_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Clean connections if they are not needed
            tmrCleanup.Stop();
            if (listSshConnections.Count > SshInstancesToKeep)
            {
                foreach (var item in listSshConnections)
                {
                    if ((DateTime.Now - item.LastUsed).TotalMinutes > 10)
                    {
                        lock (LockList)
                        {
                            listSshConnections.Remove(item);
                        }
                            item.Dispose();
                        LogFunction(2, Support.LogCategory.OK, Support.LoggerType.GAMEDLL, "DLL: Ssh Instance disposed. Remaining: " + listSshConnections.Count.ToString());
                        break;
                    }
                }
            }
            tmrCleanup.Start();
        }
    }

    [Serializable]
    public class SshConnection:IDisposable
    {
        public SshClient Client;
        public bool isAvailable;
        public bool reLoadThis = false;
        public DateTime LastUsed;

        public SshConnection(string LinuxIP, int LinuxPort, string LinuxLoginName, string LinuxPassword)
        {
            Client = new SshClient(LinuxIP, LinuxPort, LinuxLoginName, LinuxPassword);
            isAvailable = false;
        }

        public void Dispose()
        {
            if (Client.IsConnected)
                Client.Disconnect();
            Client.Dispose();
        }

        public bool Initialize()
        {
            try
            {
                Client.Connect();
                isAvailable = true;
                LastUsed = DateTime.Now;
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not create sshClient! Error " + e.Message);
                return false;
            }
            return Client.IsConnected;
        }
    }
}
