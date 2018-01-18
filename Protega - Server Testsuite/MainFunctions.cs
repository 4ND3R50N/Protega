using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Protega___Server;
using Protega___Server.Classes;
using Protega___Server.Classes.Protocol;
using Protega___Server.Classes.Core;

namespace Protega___Server_Testsuite
{
    [TestClass]
    public class MainFunctions
    {
        networkServer.networkClientInterface Client = new networkServer.networkClientInterface();
        ControllerCore Core;

       public MainFunctions()
        {
            Core = new ControllerCore("Test",10000, ';', 'a', "asdf", "mssql", "62.138.6.50", 1433, "sa", "h4TqSDs762eqbEyw", "Protega", String.Format(@"{0}/Test.txt", Directory.GetCurrentDirectory()));
            Client.SessionID = "123";
            Client.User.ID = "1234";
            Client.User.Application.ID = 1;
            Core.ActiveConnections.Add(Client);
        }

        [TestMethod]
        public void DatabaseConnection()
        {
            Assert.AreEqual(CCstData.GetInstanceByName("Test").DatabaseEngine.testDBConnection(), true);
        }

        [TestMethod]
        public void Authentification()
        {
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            Assert.AreEqual(Core.ProtocolController.ReceivedProtocol(dummy, String.Format("500;12312315;{0};Windoofs 7;Deutsch;1",Core.Application.Hash)), true);
        }

        [TestMethod]
        public void HackDetectionHeuristic()
        {
            Assert.AreEqual(Core.ProtocolController.ReceivedProtocol(Client, "701;123;Process;Window;Class;MD5"), true);
        }

        [TestMethod]
        public void HackDetectionVirtual()
        {
            Assert.AreEqual(Core.ProtocolController.ReceivedProtocol(Client, "702;123;Base;OfS;DetectedV;DefaultV"), true);
        }

        [TestMethod]
        public void AuthHackError()
        {
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
            Assert.AreEqual(Core.ProtocolController.ReceivedProtocol(dummy, String.Format("500;{0};Test;Windoofs 7;Deutsch;1", HardwareID)), false);


        }


        [TestMethod]
        public void InstanceManagement()
        {
            string HardwareID = "12312315";

            ControllerCore Core2 = new ControllerCore("Test2", 10000, ';', 'a', "asdf", "mssql", "62.138.6.50", 1433, "sa", "h4TqSDs762eqbEyw", "Protega", String.Format(@"{0}/Test.txt", Directory.GetCurrentDirectory()));
            networkServer.networkClientInterface Client2 = new networkServer.networkClientInterface();
            Client2.SessionID = "123";
            Client2.User.ID = "1234";
            Client2.User.Application.ID = 1;
            Core2.ActiveConnections.Add(Client2);

            //Authentificate in Core 2

            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            Assert.AreEqual(Core2.ProtocolController.ReceivedProtocol(dummy, String.Format("500;{0};{1};Windoofs 7;Deutsch;1", HardwareID, Core2.Application.Hash)), true);

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
            
        }
    }
}
