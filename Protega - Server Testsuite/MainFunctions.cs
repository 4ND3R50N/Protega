﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Protega___Server;
using Protega___Server.Classes;
using Protega___Server.Classes.Protocol;
using Protega___Server.Classes.Core;
using Protega___Server.Classes.Utility;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Linq;

namespace Protega___Server_Testsuite
{
    [TestClass]
    public class MainFunctions
    {
        networkServer.networkClientInterface Client = new networkServer.networkClientInterface();
        
        ControllerCore Core;


        static void Main(string[] args)
        {
            IPAddress address = IPAddress.Parse("12.3.0.42");
            byte[] t = GetMacAddress(address);
            string mac = string.Join(":", (from z in t select z.ToString("X2")).ToArray());
            Console.WriteLine(mac);
            Console.ReadLine();
        }

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(uint destIP, uint srcIP, byte[] macAddress, ref uint macAddressLength);

        public static byte[] GetMacAddress(IPAddress address)
        {
            byte[] mac = new byte[6];
            uint len = (uint)mac.Length;
            byte[] addressBytes = address.GetAddressBytes();
            uint dest = ((uint)addressBytes[3] << 24)
              + ((uint)addressBytes[2] << 16)
              + ((uint)addressBytes[1] << 8)
              + ((uint)addressBytes[0]);
            if (SendARP(dest, 0, mac, ref len) != 0)
            {
                throw new Exception("The ARP request failed.");
            }
            return mac;
        }

        public MainFunctions()
        {
            IPAddress address = IPAddress.Parse("12.3.0.42");
            byte[] t = GetMacAddress(address);
            string mac = string.Join(":", (from z in t select z.ToString("X2")).ToArray());

            //Core = new ControllerCore("Test", 102, 10000, ';', 'a', "asdf", "mssql", "62.138.6.50", 1433, "sa", "xCod3zero", "Protega", String.Format(@"{0}/Test.txt", Directory.GetCurrentDirectory()), 3,null,null);
            Client.User = new Protega___Server.Classes.Entity.EPlayer();
            Client.SessionID = "123";
            Client.User.ID = "1234";
            Client.User.Application.ID = 1;
            Core.ActiveConnections.Add(Client);
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
        }

        [TestMethod]
        public void DatabaseConnection()
        {
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
            ////CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test Database connection started!");
            //Assert.AreEqual(CCstData.GetInstanceByName("Test").DatabaseEngine.testDBConnection(), true);
            ////CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test Database connection finished!");
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
        }

        [TestMethod]
        public void Authentification()
        {
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
            ////CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test Authentification started!");
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            Assert.AreEqual(Core.ProtocolController.ReceivedProtocol(dummy, String.Format("500;12312315;{0};1;Windoofs 7;Deutsch",Core.Application.Hash)), true);
            ////CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test Authentification finished!");
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
        }

        [TestMethod]
        public void HackDetectionHeuristic()
        {
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
            //CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test HackDetection Heuristic started!");
            Assert.AreEqual(Core.ProtocolController.ReceivedProtocol(Client, "701;123;Process;Window;Class;MD5"), true);
            //CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test HackDetection nHeuristic finished!");
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
        }

        [TestMethod]
        public void HackDetectionVirtual()
        {
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
            //CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test HackDetection Virtual started!");
            Assert.AreEqual(Core.ProtocolController.ReceivedProtocol(Client, "702;123;Base;OfS;DetectedV;DefaultV"), true);
            //CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test HackDetection Virtual finished!");
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
        }

        [TestMethod]
        public void AuthHackError()
        {
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
            //CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test AuthHackError started!");
            Random Ran = new Random();
            string HardwareID = Ran.Next(1, 50000).ToString();
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            Core.ProtocolController.ReceivedProtocol(dummy, String.Format("500;{0};{1};Windoofs 7;Deutsch;1",HardwareID, Core.Application.Hash));
            string SessionID="";
            foreach (var item in Core.ActiveConnections)
            {
                if (item.User.ID == HardwareID
                    && item.User.Application.ID == 1)
                {
                    SessionID = item.SessionID;
                    dummy = item;
                }
            } 
            bool Test= Core.ProtocolController.ReceivedProtocol(Client, String.Format("701;{0};Process;Window;Class;MD5", SessionID));
            Test = Core.ProtocolController.ReceivedProtocol(Client, String.Format("701;{0};Process;Window;Class;MD5", SessionID));
            Test = Core.ProtocolController.ReceivedProtocol(Client, String.Format("701;{0};Process;Window;Class;MD5", SessionID));
            Test = Core.ProtocolController.ReceivedProtocol(Client, String.Format("701;{0};Process;Window;Class;MD5", SessionID));
            Test = Core.ProtocolController.ReceivedProtocol(Client, String.Format("701;{0};Process;Window;Class;MD5", SessionID));
            Test = Core.ProtocolController.ReceivedProtocol(Client, String.Format("701;{0};Process;Window;Class;MD5", SessionID));
            Core.ActiveConnections.Remove(dummy);
            Assert.AreEqual(Core.ProtocolController.ReceivedProtocol(dummy, String.Format("500;{0};{1};Windoofs 7;Deutsch;1", HardwareID, Core.Application.Hash)), false);

            //CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test AuthHackError finished!");
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
        }

        [TestMethod]
        public void SendMessage()
        {
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            string Test = "Test\0\0";
            string Test2 = Test.TrimEnd('\0');
            if (Test.EndsWith("\0"))
            {
                Test2 = Test.Substring(0, Test.Length - 1);
            }
            //Core.TcpServer.sendMessage("Test", Client);
        }

        void SendMessage(string network_AKey, IPAddress ip, short port, AddressFamily familyType, SocketType socketType, ProtocolType protocolType)
        {

        }

        /*[TestMethod]
        public void InstanceManagement()
        {
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
            //CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test Instance Management started!");
            string HardwareID = "12312315";

            ControllerCore Core2;// = new ControllerCore("Test2",102, 10000, ';', 'a', "asdf", "mssql", "62.138.6.50", 1433, "sa", "h4TqSDs762eqbEyw", "Protega", String.Format(@"{0}/Test.txt", Directory.GetCurrentDirectory()),3,null,null);
            networkServer.networkClientInterface Client2 = new networkServer.networkClientInterface();
            Client2.SessionID = "123";
            Client2.User.ID = "1234";
            Client2.User.Application.ID = 1;
            //Core2.ActiveConnections.Add(Client2);

            //Authentificate in Core 2

            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            Assert.AreEqual//(Core2.ProtocolController.ReceivedProtocol(dummy, String.Format("500;{0};{1};Windoofs 7;Deutsch;1", HardwareID, Core2.Application.Hash)), true);

            //Get Session ID of dummy
            string SessionID = "";
            foreach (var item in Core2.ActiveConnections)
            {
                if (item.User.ID == HardwareID
                    && item.User.Application.ID == 2)
                {
                    SessionID = item.SessionID;
                    dummy = item;
                }
            }

            //Ping to Core 2
            Assert.AreEqual(Core2.ProtocolController.ReceivedProtocol(new networkServer.networkClientInterface(), String.Format("600;{0}", SessionID)), true);

            //CCstData.GetInstance(Core.Application).Logger.writeInLog(3, Support.LogCategory.OK, "Test Instance Management finished!");
            //CCstData.GetInstance(Core.Application).Logger.Seperate();
        }*/
        [TestMethod]
        public void testApplicationAdapter()
        {
            ApplicationAdapter dummy = new ApplicationAdapter(Path.Combine(Environment.CurrentDirectory, "DLL", "Cabal.dll"), Core.Application);
            //dummy.PrepareServer("", "", "", 0, null, "");
            //Assert.IsTrue(dummy.BanUser());
            //Assert.IsTrue(dummy.KickUser());           

            //CCstData config = //CCstData.GetInstance(Core.Application);
            //ApplicationAdapter dummy = new ApplicationAdapter(Path.Combine(Environment.CurrentDirectory, "Cabal.dll"));

            //Assert.IsTrue(dummy.PrepareServer("1.1.1.1", "hugo", "wurschd", 4711, null, "whoami", config));
            //Assert.IsTrue(dummy.AllowUser("IP", "UserName"));
            //Assert.IsTrue(dummy.BanUser("IP", "UserName", DateTime.Now));
        }

        [TestMethod]
        public void DatabaseLogging()
        {
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    CCstData.GetInstance(Core.Application.ID).Logger.writeInLog(1, Support.LogCategory.ERROR, Support.LoggerType.SERVER, "Test" + i.ToString());
                    CCstData.GetInstance(Core.Application.ID).Logger.writeInLog(1, Support.LogCategory.CRITICAL, Support.LoggerType.SERVER, "Test" + i.ToString());
                    CCstData.GetInstance(Core.Application.ID).Logger.writeInLog(1, Support.LogCategory.OK, Support.LoggerType.SERVER, "Test" + i.ToString());
                }
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CreateConfigIni()
        {
            string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "config.ini");

            string ApplicationName = "CodeZero";
            int InputPort = 13010;
            char ProtocolDelimiter = ';';
            string EncryptionKey = "1234567890123456";
            string EncryptionIV = "bbbbbbbbbbbbbbbb";
            int PingTimer = 20000;
            int SessionLength = 10;
            string DatabaseDriver = "mssql";
            string DatabaseIP = "62.138.6.50";
            int DatabasePort = 1433;
            string DatabaseLoginName = "sa";
            string DatabasePassword = "xCod3zero";
            string DatabaseDefault = "Protega";
            string LogFile = Path.Combine(Directory.GetCurrentDirectory(), "Log.txt");
            int LogLevel = 3;

            //CABAL DEFAULT!
            string LinuxIP= "167.88.15.106";
            string LinuxLoginName= "root";
            string LinuxPassword = "Wn51b453gpEdZTB5Bl";
            int LinuxPort=22;
            System.Collections.Generic.List<int> Ports = new System.Collections.Generic.List<int>() { 12001, 12002, 12003 };
            string sPorts = "";
            Ports.ForEach(Port => sPorts += Port.ToString() + ";");
            sPorts = sPorts.TrimEnd(';');
            string IniText = String.Format("[{0}]", ApplicationName);


            if (File.Exists(ConfigPath))
                File.Delete(ConfigPath);
            FileStream fs = File.Create(ConfigPath);
            fs.Close();

            //File.WriteAllText(ConfigPath, "[Data]\r\nName=\r\nPassword=\r\nAccess=");
            File.WriteAllText(ConfigPath, IniText);
            Support.iniManager iniEngine = new Support.iniManager(ConfigPath);

            iniEngine.IniWriteValue(ApplicationName, "InputPort", InputPort.ToString());
            iniEngine.IniWriteValue(ApplicationName, "ProtocolDelimiter", ProtocolDelimiter.ToString());
            iniEngine.IniWriteValue(ApplicationName, "EncryptionKey", EncryptionKey.ToString());
            iniEngine.IniWriteValue(ApplicationName, "EncryptionIV", EncryptionIV.ToString());
            iniEngine.IniWriteValue(ApplicationName, "PingTimer", PingTimer.ToString());
            iniEngine.IniWriteValue(ApplicationName, "SessionLength", SessionLength.ToString());
            iniEngine.IniWriteValue(ApplicationName, "DatabaseDriver", DatabaseDriver.ToString());
            iniEngine.IniWriteValue(ApplicationName, "DatabaseIP", DatabaseIP.ToString());
            iniEngine.IniWriteValue(ApplicationName, "DatabasePort", DatabasePort.ToString());
            iniEngine.IniWriteValue(ApplicationName, "DatabaseLoginName", DatabaseLoginName.ToString());
            iniEngine.IniWriteValue(ApplicationName, "DatabasePassword", DatabasePassword.ToString());
            iniEngine.IniWriteValue(ApplicationName, "DatabaseDefault", DatabaseDefault.ToString());
            iniEngine.IniWriteValue(ApplicationName, "LogFile", LogFile.ToString());
            iniEngine.IniWriteValue(ApplicationName, "LogLevel", LogLevel.ToString());
            iniEngine.IniWriteValue(ApplicationName, "LinuxIP", LinuxIP.ToString());
            iniEngine.IniWriteValue(ApplicationName, "LinuxLoginName", LinuxLoginName.ToString());
            iniEngine.IniWriteValue(ApplicationName, "LinuxPassword", LinuxPassword.ToString());
            iniEngine.IniWriteValue(ApplicationName, "LinuxPort", LinuxPort.ToString());
            iniEngine.IniWriteValue(ApplicationName, "Ports", sPorts.ToString());
        }
    }
}
