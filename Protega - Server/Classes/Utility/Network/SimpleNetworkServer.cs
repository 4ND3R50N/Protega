/**
 * WhiteCode
 *
 * An self made server socket system to send and get packets from a connected client
 *
 * @author		Anderson from WhiteCode
 * @copyright	Copyright (c) 2016
 * @link		http://white-code.org
 * @since		Version 2.1
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Renci.SshNet;
using System.Collections;
using System.Collections.Generic;

namespace Protega___Server
{
    public class networkServer:IDisposable
    {
        //Variables
        //--Public
        public delegate void protocolFunction(ref networkClientInterface NetworkClient, string prot);
        public delegate void _AuthenticateClient(networkClientInterface Client);
        //--Private
        private IPEndPoint serverEndPoint;
        private Socket serverSocket;
        private event protocolFunction protAnalyseFunction;
        private string network_AKey;
        int ApplicationID;

        //Constructor
        public networkServer(protocolFunction protAnalyseFunction, string network_AKey, int ApplicationID)
        { 
            this.network_AKey = network_AKey;
            this.protAnalyseFunction = protAnalyseFunction;
            this.ApplicationID = ApplicationID;
        }

        public networkServer(protocolFunction protAnalyseFunction, string network_AKey, int ApplicationID, IPAddress ip, short port, 
            AddressFamily familyType, SocketType socketType, ProtocolType protocolType)
        {
            this.network_AKey = network_AKey;
            this.protAnalyseFunction = protAnalyseFunction;
            this.ApplicationID = ApplicationID;
            serverEndPoint = new IPEndPoint(IPAddress.Any, port);
            serverSocket = new Socket(familyType, socketType, protocolType);
            serverSocket.Blocking = false;
        }



        //Functions
        public void setSocketEndPoint(IPAddress ip, short port, AddressFamily familyType, SocketType socketType, ProtocolType protocolType)
        {
            serverEndPoint = new IPEndPoint(IPAddress.Any, port);
            serverSocket = new Socket(familyType, socketType, protocolType);
            serverSocket.Blocking = false;
        }

        public bool startListening()
        {
            try
            {
                serverSocket.Bind(serverEndPoint);
                serverSocket.Listen((int)SocketOptionName.MaxConnections);
                //for (int i = 0; i < 1000; i++)
                    serverSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback), serverSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine("Starting Error: " + e.Message);
                return false;
            }
            return true;
        }

        private void AcceptCallback(IAsyncResult result)
        {
            Protega___Server.Classes.CCstData.GetInstance(ApplicationID).Logger.writeInLog(5, Support.LogCategory.OK, Support.LoggerType.SERVER, "Protocol accepted");
            //Wenns klappt, using drum!
            networkClientInterface connection = new networkClientInterface((Socket)result.AsyncState, result);
            
            try
            {
                // Start Receive
                connection.networkSocket.BeginReceive(connection.buffer, 0,
                    connection.buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), connection);
                // Start new Accept
                //serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),
                //    result.AsyncState);
                //for (int i = 0; i < 1000; i++)
                serverSocket.BeginAccept(
                    new AsyncCallback(AcceptCallback), serverSocket);

            }
            catch (SocketException e)
            {
                closeConnection(connection);
            }
            catch (Exception e)
            {
                Console.WriteLine("DEBUG: " + e.ToString());
                closeConnection(connection);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            Classes.CCstData.GetInstance(ApplicationID).Logger.writeInLog(4, Support.LogCategory.OK, Support.LoggerType.SERVER, "Protocol received");
            networkClientInterface connection = (networkClientInterface)result.AsyncState;
            try
            {
                //bytesread = count of bytes
                int bytesRead = connection.networkSocket.EndReceive(result);
                if (0 != bytesRead)
                {
                    protAnalyseFunction(ref connection, Encoding.Default.GetString(connection.buffer, 0, bytesRead));
                    //connection.networkSocket.BeginReceive(connection.buffer, 0,
                    //  connection.buffer.Length, SocketFlags.None,
                    //  new AsyncCallback(ReceiveCallback), connection);
                }
                else
                {
                    closeConnection(connection);
                }
            }
            catch (SocketException)
            {
                closeConnection(connection);
                try
                {
                    connection.Dispose();

                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not dispose connection! Error: " + e.Message);
                }
            }
            catch (Exception e)
            {
                closeConnection(connection);
                try
                {
                    connection.Dispose();
                }
                catch (Exception f)
                {
                    Console.WriteLine("Could not dispose connection 2! Error: " + f.Message);
                }
            }
        }

        public void sendMessage(string message, networkClientInterface client)
        {
            try
            {
                byte[] bytes = Encoding.Default.GetBytes(message);
                client.networkSocket.Send(bytes, bytes.Length,
                                SocketFlags.None);
                Classes.CCstData.GetInstance(client.User.Application.ID).Logger.writeInLog(4, Support.LogCategory.OK, Support.LoggerType.SERVER, String.Format("Protocol sending succeeded. Protocol: {0}, Session: {1}, HardwareID: {2}", message, client.SessionID, client.User.ID));
            }
            catch (Exception e)
            {
                Classes.CCstData.GetInstance(client.User.Application.ID).Logger.writeInLog(4, Support.LogCategory.ERROR, Support.LoggerType.SERVER, String.Format("Protocol sending failed. Protocol: {0}. Error {1}", message, e.Message));
                closeConnection(client);
            }
        }

        public void closeConnection(networkClientInterface client)
        {
            client.networkSocket.Close();
        }

        public void closeServer()
        {
            serverSocket.Close();
        }

        public void Dispose()
        {
            serverSocket.Close();
            //serverSocket.Dispose();
        }

        //Class model -> Client -> MUST be edited for each implementation 
        public class networkClientInterface : IDisposable
        {
            //Technical API
            public Socket networkSocket;
            public byte[] buffer;
            //Protes Values
            public Classes.Entity.EPlayer User;
            public string SessionID;
            public DateTime ConnectedTime;
            List<IPAddress> listIPs = new List<IPAddress>();
            public DateTime _LastPing = DateTime.MinValue;

            private IPAddress _LatestIP;
            public bool KickTriggered = false;

            public IPAddress IP
            {
                get { return _LatestIP; }
            }
            

            public delegate void KickUser(networkClientInterface Client);
            event KickUser Kick;
            System.Timers.Timer tmrPing;
            
            public void Dispose()
            {
                if (networkSocket != null)
                {
                    try
                    {
                        networkSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception)
                    {
                    }
                    networkSocket.Close();
                    networkSocket.Dispose();
                }

                if (tmrPing != null)
                {
                    tmrPing.Enabled = false;
                    tmrPing.Dispose();
                }
            }

            public void CheckIP(int ApplicationID)
            {
                IPAddress CurrentIP = null;

                //Try to get the current IP address
                try
                {
                    if (networkSocket.Connected)
                        CurrentIP = (networkSocket.RemoteEndPoint as IPEndPoint).Address;
                }
                catch (Exception e)
                {
                    if (ApplicationID != 0)
                        Classes.CCstData.GetInstance(User.Application.ID).Logger.writeInLog(2, Support.LogCategory.ERROR, Support.LoggerType.CLIENT, "Could not get IP.User: " + User.ID + ", Session: " + SessionID + ", Error: " + e.Message);
                }
                
                //If CurrentIP could not be fetched, log and ignore it
                if (CurrentIP != null)
                {
                    if(_LatestIP==null)
                    {
                        _LatestIP = CurrentIP;
                        return;
                    }

                    if (CurrentIP.ToString() == _LatestIP.ToString())
                        return;
                    
                    //Update LatestIP if IP has changed
                    _LatestIP = CurrentIP;
                    listIPs.Add(CurrentIP);

                    //Log that this case is possible
                    if (listIPs.Count > 1 && ApplicationID != 0)
                    {
                        string FormerIPs = "";
                        listIPs.ForEach(IP => FormerIPs += IP + " - ");
                        Classes.CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, Support.LogCategory.ERROR, Support.LoggerType.CLIENT, String.Format("Client IP Changed! CurrentIP: {0}. Former IPs: {1}", CurrentIP, FormerIPs));
                    }
                }
                else
                {
                    if (ApplicationID != 0)
                    {
                        Classes.CCstData.GetInstance(ApplicationID).Logger.writeInLog(2, Support.LogCategory.ERROR, Support.LoggerType.CLIENT, String.Format("Cannot get current IP! User: {0}. Session: {1}", User.ID, SessionID));
                    }
                }
            }

            public networkClientInterface()
            {
            }
            public void SetPingTimer(int Interval, KickUser _Kick)
            {
                this.Kick = _Kick;
                tmrPing = new System.Timers.Timer();
                tmrPing.Elapsed += TmrPing_Elapsed;
                ConnectedTime = DateTime.Now;
                tmrPing.Interval = Interval;
                tmrPing.Start();
                
            }

            private void TmrPing_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                //tmrPing.Stop();
                //Kick - Timer elapsed
                Kick(this);

                Classes.CCstData.GetInstance(this.User.Application.ID).Logger.writeInLog(4, Support.LogCategory.OK, Support.LoggerType.SERVER, String.Format("User timeout! Session: {0}, HardwareID: {1}", this.SessionID, this.User.ID));
                //this.Dispose();
                //User kicked
            }
            

            public void ResetPingTimer()
            {
                tmrPing.Stop();
                tmrPing.Start();
                _LastPing = DateTime.Now;
                CheckIP(User.Application.ID);
            }

            public networkClientInterface(Socket connection, IAsyncResult result)
            {
                AddressFamily test= connection.AddressFamily;
                try
                {
                    networkSocket = connection.EndAccept(result);
                }
                catch (Exception)
                {
                    Protega___Server.Classes.CCstData.GetInstance(User.Application != null ? User.Application.ID : 1).Logger.writeInLog(2, Support.LogCategory.CRITICAL, Support.LoggerType.SERVER, "Endaccept error 1");
                    return;
                }
                
                networkSocket.Blocking = false;
                buffer = new byte[1024];
            }
        }
    }
}
