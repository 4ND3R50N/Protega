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
            //HandleInOutTasks(Tasks, sshClient);
            Thread thread = new Thread(() => HandleInOutTasks(Tasks, sshClient));
            thread.Start();
        }

        readonly object _lockSshClient = new object();
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


            lock (_lockSshClient)
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
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            using (SshCommand Result = sshClient.Client.RunCommand(Task.LinuxQuery))
                            {
                                if (Result.Error.Length > 0)
                                {
                                    if (!Result.Error.Contains("does a matching rule exist in that chain?"))
                                    {
                                        LogFunction(2, LogCategory.ERROR, Support.LoggerType.GAMEDLL, "IPTable " + Task.GetType().Name.ToString() + " failed! Session ID: " + Task.IP + ", Error: " + Result.Error + ", Result " + Result.Result + ", Query: " + Task.LinuxQuery);
                                        if (i == 2)
                                        {
                                            dllFeedback(Task.Username, Task.IP, Task.Task, Protega___Server.Classes.Utility.ApplicationAdapter.Result.FAIL, Task.TimeStamp);
                                            //ReAddItems(new List<_InterfaceTask>() { Task });
                                            break;
                                        }
                                        Thread.Sleep(200);
                                        continue;
                                    }
                                    else
                                    {
                                        //Successful - just rule didnt exist
                                        LogFunction(3, LogCategory.OK, Support.LoggerType.GAMEDLL, "IPTable " + Task.GetType().Name.ToString() + " ok! Session ID: " + Task.IP + ", Error: " + Result.Error + ", Result " + Result.Result + ", Query: " + Task.LinuxQuery);
                                        dllFeedback(Task.Username, Task.IP, Task.Task, Protega___Server.Classes.Utility.ApplicationAdapter.Result.SUCCESS, Task.TimeStamp);
                                        i = 3;
                                        break;
                                    }
                                }
                                else
                                {
                                    //sshClient.reLoadThis = true;
                                    LogFunction(4, LogCategory.OK, Support.LoggerType.GAMEDLL, "IPTable " + Task.GetType().Name.ToString() + " success! Session ID: " + Task.IP + ", Error: " + Result.Error + ", Result " + Result.Result + ", Query: " + Task.LinuxQuery);
                                    dllFeedback(Task.Username, Task.IP, Task.Task, Protega___Server.Classes.Utility.ApplicationAdapter.Result.SUCCESS, Task.TimeStamp);
                                    break;
                                }
                            }
                            i = 3;
                            break;
                        }
                        catch (Exception e)
                        {
                            LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "Ssh execution error. " + Task.Username + " (" + Task.IP + "), Attempt " + i.ToString() + ": " + e.Message);
                            if (i == 2)
                            {
                                LogFunction(2, LogCategory.ERROR, LoggerType.GAMEDLL, "Ssh critical error (Task readded to queue). " + Task.Username + " (" + Task.IP + "), Attempt " + i.ToString() + ": " + e.Message);
                                ReAddItems(new List<_InterfaceTask>() { Task });
                                break;
                            }
                            Thread.Sleep(200);
                        }
                        finally
                        {
                            
                        }
                    }
                }
                sshClient.isAvailable = true;
            }
        }
    }
}
