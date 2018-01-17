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
    public class UnitTest1
    {
        networkServer.networkClientInterface Client = new networkServer.networkClientInterface();
        ControllerCore Core;

       public UnitTest1()
        {
            Core = new ControllerCore(10000, ';', 'a', "asdf", "mssql", "62.138.6.50", 1433, "sa", "h4TqSDs762eqbEyw", "Protega", String.Format(@"{0}/Test.txt", Directory.GetCurrentDirectory()));
            Client.SessionID = "123";
            Client.User.ID = "1234";
            Client.User.ApplicationName = "Test";
            Core.ActiveConnections.Add(Client);
        }
        
        [TestMethod]
        public void DatabaseConnection()
        {
            Assert.AreEqual(CCstDatabase.DatabaseEngine.testDBConnection(), true);
        }

        [TestMethod]
        public void Authentification()
        {
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            Assert.AreEqual(Core.ProtocolController.ReceivedProtocol(dummy, "500;98765;Test;Windoofs 7;Deutsch;1"), true);
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
            string HardwareID = "3213";
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            Core.ProtocolController.ReceivedProtocol(dummy, String.Format("500;{0};Test;Windoofs 7;Deutsch;1",HardwareID));
            string SessionID="";
            foreach (var item in Core.ActiveConnections)
            {
                if (item.User.ID == HardwareID
                    && item.User.ApplicationName == "Test")
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
    }
}
