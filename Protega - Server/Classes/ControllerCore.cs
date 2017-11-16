using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Support;
using System.Net;
using System.Net.Sockets;
using Protega___Server.Classes.Protocol;

namespace Protega___Server.Classes.Core
{
    public class ControllerCore
    {

        //Variablen
        networkServer TcpServer;
        List<networkServer.networkClientInterface> ActiveConnections;
        
        private string sAesKey;
        private char   cProtocolDelimiter;
        private char   cDataDelimiter;

        //Konstruktor
        public ControllerCore(short _iPort, char _cProtocolDelimiter, char _cDataDelimiter, string _sAesKey, string _sDatabaseDriver,
            string _sDBHostIp, short _sDBPort, string _sDBUser, string _sDBPass, string _sDBDefaultDB, string _sLogPath)
        {
            //Logging initialisations
            CCstLogging.Logger = new logWriter(_sLogPath);
            CCstLogging.Logger.writeInLog(true, "Logging class initialized!");
            //Database Initialisations
            if(_sDatabaseDriver == "mysql")
            {
               // CCstDatabase.DatabaseEngine = new DBMysqlDataManager(_sDBHostIp,_sDBUser,_sDBPass,_sDBPort,_sDBDefaultDB);

            }else if(_sDatabaseDriver == "mssql")
            {
                CCstDatabase.DatabaseEngine = new DBMssqlDataManager(_sDBHostIp, _sDBUser, _sDBPass, _sDBPort, _sDBDefaultDB);
            }
            //Database test
            if (CCstDatabase.DatabaseEngine.testDBConnection())
            {
                CCstLogging.Logger.writeInLog(true, "Database test successfull!");
            }else
            {
                CCstLogging.Logger.writeInLog(true, "ERROR: Database test was not successfull!");
                return;
            }

            //Network Initialisations
            ActiveConnections = new List<networkServer.networkClientInterface>();
            sAesKey = _sAesKey;
            this.cProtocolDelimiter = _cProtocolDelimiter;
            this.cDataDelimiter = _cDataDelimiter;
            TcpServer = new networkServer(NetworkProtocol, _sAesKey, IPAddress.Any, _iPort, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            CCstLogging.Logger.writeInLog(true, "TCP Server ready for start!");

            SPlayer.GetByName("Semado123", CCstDatabase.DatabaseEngine);

            //TESTCASE
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            //Auth
            //  networkProtocol("#104;Anderson2;Lars;Pickelin;miau1234;l.pickelin@web.de", ref dummy);
            //  networkProtocol("#102;Anderson2;miau1x234", ref dummy);
            //Content
            //Get all rooms
            //  networkProtocol("#201", ref dummy);
            //Get all rooms of a specific user
            //  NetworkProtocol("#211;18", ref dummy);
            //Add new or update room
            //NetworkProtocol("#203;5;1;Avelinas Test raum;Hallo Welt;1;http://www.AvelinaLerntArrays.net", ref dummy);
            //Get all workouts of room id 2
            //  NetworkProtocol("#205;Hadd e", ref dummy);
            //Get Levels of workout with id 1
            //  NetworkProtocol("#207;1", ref dummy);
            //Get all excercises of workout 1
            //  NetworkProtocol("#209;1", ref dummy);
            //Delete room
            //NetworkProtocol("#213;114", ref dummy);

        }

        public void Start()
        {
            if(TcpServer.startListening())
            {
                CCstLogging.Logger.writeInLog(true, "Server has been started successfully!");
            }
            else
            {
                CCstLogging.Logger.writeInLog(true, "ERROR: The server was not able to start!");
            }
           
        }

        private void NetworkProtocol(string message)
        {
            ProtocolController contr = new ProtocolController(this);

            contr.RecievedProtocol(message);
        }


        #region Support functions
        private List<string> GetProtocolData(string message)
        {
            return message.Split(cProtocolDelimiter).ToList();
        }

        private string GetProtocolShortcut(string message)
        {
            return message.Split(cProtocolDelimiter)[0];
        }
        private string GetProtocolMessage(string message)
        {
            try
            {
                return message.Substring(GetProtocolShortcut(message).Length + 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "-";
            }
            
        }
        #endregion
        
    }
}
