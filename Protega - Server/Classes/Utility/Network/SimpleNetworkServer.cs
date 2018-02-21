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
        public delegate void protocolFunction(networkClientInterface NetworkClient, string prot);
        public delegate void _AuthenticateClient(networkClientInterface Client);
        //--Private
        private IPEndPoint serverEndPoint;
        private Socket serverSocket;
        private event protocolFunction protAnalyseFunction;
        private string network_AKey;

        //Constructor
        public networkServer(protocolFunction protAnalyseFunction, string network_AKey)
        { 
            this.network_AKey = network_AKey;
            this.protAnalyseFunction = protAnalyseFunction;
        }

        public networkServer(protocolFunction protAnalyseFunction, string network_AKey, IPAddress ip, short port, 
            AddressFamily familyType, SocketType socketType, ProtocolType protocolType)
        {
            this.network_AKey = network_AKey;
            this.protAnalyseFunction = protAnalyseFunction;
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
                for (int i = 0; i < 1000; i++)
                    serverSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback), serverSocket);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        private void AcceptCallback(IAsyncResult result)
        {
            networkClientInterface connection = new networkClientInterface((Socket)result.AsyncState, result);
            try
            {
                // Start Receive
                connection.networkSocket.BeginReceive(connection.buffer, 0,
                    connection.buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), connection);
                // Start new Accept
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),
                    result.AsyncState);
                //for (int i = 0; i < 1000; i++)
                serverSocket.BeginAccept(
                    new AsyncCallback(AcceptCallback), serverSocket);

            }
            catch (SocketException)
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
            networkClientInterface connection = (networkClientInterface)result.AsyncState;
            try
            {
                //bytesread = count of bytes
                int bytesRead = connection.networkSocket.EndReceive(result);
                if (0 != bytesRead)
                {
                    protAnalyseFunction(connection, Encoding.Default.GetString(connection.buffer, 0, bytesRead));
                    connection.networkSocket.BeginReceive(connection.buffer, 0,
                      connection.buffer.Length, SocketFlags.None,
                      new AsyncCallback(ReceiveCallback), connection);
                }
                else closeConnection(connection);
            }
            catch (SocketException)
            {
                closeConnection(connection);
            }
            catch (Exception e)
            {
                closeConnection(connection);
            }
        }

        public void sendMessage(string message, networkClientInterface client)
        {
            try
            {
                byte[] bytes = Encoding.Default.GetBytes(message);
                client.networkSocket.Send(bytes, bytes.Length,
                                SocketFlags.None);
                Classes.CCstData.GetInstance(client.User.Application.ID).Logger.writeInLog(3, Support.LogCategory.OK, Support.LoggerType.SERVER, String.Format("Protocol sending succeeded. Protocol: {0}, Session: {1}, HardwareID: {2}", message, client.SessionID, client.User.ID));
                //Classes.CCstData.GetInstance(client.User.Application.ID).Logger.writeInLog(3, Support.LogCategory.OK, String.Format("Protocol sent. Protocol: {0}, Session: {1}, HardwareID: {2}", message, client.SessionID, client.User.ID));
            }
            catch (Exception e)
            {
                Classes.CCstData.GetInstance(client.User.Application.ID).Logger.writeInLog(3, Support.LogCategory.ERROR, Support.LoggerType.SERVER, String.Format("Protocol sending failed. Protocol: {0}, Session: {1}, HardwareID: {2}. Error {3}", message, client.SessionID, client.User.ID, e.Message));
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
        public class networkClientInterface:IDisposable
        {
            //Technical API
            public Socket networkSocket;
            public byte[] buffer;
            //Protes Values
            public Classes.Entity.EPlayer User;
            public string SessionID;
            public DateTime ConnectedTime;
            public IPAddress IP;
            public SshClient unixSshConnectorAccept;
            public int Counter = 0;

            public delegate void KickUser(networkClientInterface Client);
            event KickUser Kick;
            System.Timers.Timer tmrPing;
            
            public void Dispose()
            {
                if (unixSshConnectorAccept != null)
                    unixSshConnectorAccept.Dispose();
                if (tmrPing != null)
                    tmrPing.Dispose();
            }

            public networkClientInterface()
            {
            }
            public void SetPingTimer(int Interval, string LinuxIP, string User, string Pass, int Port, KickUser _Kick, string _IP=null)
            {
                //IPAddress.TryParse(_IP, out IP);
                IP = (networkSocket.RemoteEndPoint as IPEndPoint).Address;
                unixSshConnectorAccept = new SshClient(LinuxIP, Port, User, Pass);
                this.Kick = _Kick;

                tmrPing = new System.Timers.Timer();
                tmrPing.Elapsed += TmrPing_Elapsed;
                ConnectedTime = DateTime.Now;
                tmrPing.Interval = Interval;
                tmrPing.Start();
            }

            private void TmrPing_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                //Kick - Timer elapsed
                Kick(this);

                Classes.CCstData.GetInstance(this.User.Application.ID).Logger.writeInLog(3, Support.LogCategory.OK, Support.LoggerType.SERVER, String.Format("User timeout! Session: {0}, HardwareID: {1}", this.SessionID, this.User.ID));
                this.Dispose();
                //User kicked
            }
            

            public void ResetPingTimer()
            {
                tmrPing.Stop();
                tmrPing.Start();
            }

            public networkClientInterface(Socket connection, IAsyncResult result)
            {
                AddressFamily test= connection.AddressFamily;
                networkSocket = connection.EndAccept(result);
                
                networkSocket.Blocking = false;
                buffer = new byte[1024];
            }
        }
    }
}
