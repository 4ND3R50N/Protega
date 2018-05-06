using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Protega___Server.Classes.Utility;
using Support;
using Protega.ApplicationAdapter.Classes.Tasks;
using System.Threading;

namespace Protega.ApplicationAdapter.Classes.Utility
{
    public class LinuxInterface
    {
        Support.logWriter.WriteLog LogFunction;
        Protega___Server.Classes.Utility.ApplicationAdapter.DllFeedback dllFeedback;
        string LinuxIP, LinuxLoginName, LinuxPassword;
        int LinuxPort;

        public delegate void ReaddItems(List<_InterfaceTask> Tasks);
        ReaddItems ReAddItems;

        public LinuxInterface(logWriter.WriteLog LogFunction, Protega___Server.Classes.Utility.ApplicationAdapter.DllFeedback dllFeedback, ReaddItems ReAddItems)
        {
            this.LogFunction = LogFunction;
            this.dllFeedback = dllFeedback;
            this.ReAddItems = ReAddItems;

        }

        public void DoTasks(List<_InterfaceTask> Tasks, SshConnection sshClient)
        {
            Thread thread = new Thread(() => HandleInOutTasks(Tasks, sshClient));
            thread.Start();
        }

        readonly object _locklogin = new object();
        public void HandleInOutTasks(List<_InterfaceTask> Tasks, SshConnection sshClient)
        {
            //if (!LinuxInterface.Client.IsConnected)
            //    if(!LinuxInterface.Reconnect())
            //    {
            //        LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "AllowTask: LinuxInterface reconnect not possible!");
            //        return;
            //    }


            LogFunction(4, LogCategory.OK, LoggerType.GAMEDLL, "IPTable new InOut Thread initiated!");

            //string Query = "";
            //foreach (_InterfaceTask Task in Tasks)
            //{
            //    Query += Task.LinuxQuery + " && ";
            //}
            //Query = Query.TrimEnd(' ').TrimEnd('&');


            lock (_locklogin)
            {
                try
                {
                    //Try to connect
                    if (!sshClient.Client.IsConnected)
                        sshClient.Client.Connect();
                    if (!sshClient.Client.IsConnected)
                        throw new Exception("Not connected!");
                }
                catch (Exception e)
                {
                    //Log error
                    LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "Error connecting to Linux: " + e.Message);

                    //Enqueue tasks to main queue again
                    ReAddItems(Tasks);
                    return;
                }


                foreach (_InterfaceTask Task in Tasks)
                {
                    using (SshCommand Result = sshClient.Client.RunCommand(Task.LinuxQuery))
                    {
                        sshClient.isAvailable = true;
                        if (Result.Error.Length > 0)
                        {
                            if (!Result.Error.Contains("does a matching rule exist in that chain?"))
                            {
                                LogFunction(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, "IPTable " + Task.GetType().ToString() + " failed! Session ID: " + Task.IP + ", Error: " + Result.Error);
                                dllFeedback(Task.Username, Task.IP, Task.Task, Protega___Server.Classes.Utility.ApplicationAdapter.Result.FAIL, Task.TimeStamp);
                                continue;
                            }
                        }
                        else
                            dllFeedback(Task.Username, Task.IP, Task.Task, Protega___Server.Classes.Utility.ApplicationAdapter.Result.SUCCESS, Task.TimeStamp);
                    }
                }
            }
        }
    }
}
