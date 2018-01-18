using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes
{
    public class CCstData
    {
        private static List<CCstData> ListeTest = new List<CCstData>();
        
        public DBEngine DatabaseEngine;
        public Support.logWriter Logger;

        public const string EncryptionKey = "1234567890123456";
        public const string EncryptionIV = "bbbbbbbbbbbbbbbb";

        public const int SessionIDLength = 10;
        public const int PingTimer = 9999000;

        public Classes.Entity.EApplication Application;

        #region Constructor
        public CCstData(Entity.EApplication _Application, DBEngine _DatabaseEngine, Support.logWriter _Logger)
        {
            Application = _Application;
            DatabaseEngine = _DatabaseEngine;
            Logger = _Logger;
            ListeTest.Add(this);
        }
        #endregion

        #region Instance Management
        public static CCstData GetInstance(Entity.EApplication _Application)
        {
            foreach (CCstData item in ListeTest)
            {
                if (item.Application == _Application)
                    return item;
            }
            return null;
        }
        public static CCstData GetInstance(int _ID)
        {
            foreach (CCstData item in ListeTest)
            {
                if (item.Application.ID == _ID)
                    return item;
            }
            return null;
        }
        public static CCstData GetInstanceByName(string _Name)
        {
            foreach (CCstData item in ListeTest)
            {
                if (item.Application.Name == _Name)
                    return item;
            }
            return null;
        }
        public static CCstData GetInstance(string _Hash)
        {
            foreach (CCstData item in ListeTest)
            {
                if (item.Application.Hash == _Hash)
                    return item;
            }
            return null;
        }

        public static bool InstanceExists(string _HashID)
        {
            foreach (CCstData item in ListeTest)
            {
                if (item.Application.Hash == _HashID)
                    return true;
            }
            return false;
        }

        public static bool InstanceClose(int _ID)
        {
            foreach (CCstData item in ListeTest)
            {
                if (item.Application.ID == _ID)
                {
                    ListeTest.Remove(item);
                    return true;
                }
            }
            return false;
        }
        #endregion


    }
}
