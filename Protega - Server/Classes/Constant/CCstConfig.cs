using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes
{
    public class CCstData
    {
        #region Settings
        public string EncryptionKey = "1234567890123456";
        public string EncryptionIV = "bbbbbbbbbbbbbbbb";

        public int SessionIDLength = 10;
        public int PingTimer = 9999000;

        //1 = User output, 2 = User output with more details, 3 = Debug infos
        public int LogLevel=3;
        #endregion


        #region Manager Classes
        public Entity.EApplication Application;
        public DBEngine DatabaseEngine;
        public Support.logWriter Logger;
        #endregion

        #region Constructor
        public CCstData(Entity.EApplication _Application, DBEngine _DatabaseEngine, Support.logWriter _Logger)
        {
            Application = _Application;
            DatabaseEngine = _DatabaseEngine;
            Logger = _Logger;
            Instances.Add(this);
        }
        #endregion

        #region Instance Management
        private static List<CCstData> Instances = new List<CCstData>();

        public static CCstData GetInstance(Entity.EApplication _Application)
        {
            foreach (CCstData item in Instances)
            {
                if (item.Application == _Application)
                    return item;
            }
            return null;
        }
        public static CCstData GetInstance(int _ID)
        {
            foreach (CCstData item in Instances)
            {
                if (item.Application.ID == _ID)
                    return item;
            }
            return null;
        }
        public static CCstData GetInstance(string _Hash)
        {
            foreach (CCstData item in Instances)
            {
                if (item.Application.Hash == _Hash)
                    return item;
            }
            return null;
        }
        public static CCstData GetInstanceByName(string _Name)
        {
            foreach (CCstData item in Instances)
            {
                if (item.Application.Name == _Name)
                    return item;
            }
            return null;
        }

        public static bool InstanceExists(string _HashID)
        {
            foreach (CCstData item in Instances)
            {
                if (item.Application.Hash == _HashID)
                    return true;
            }
            return false;
        }

        public static bool InstanceClose(int _ID)
        {
            foreach (CCstData item in Instances)
            {
                if (item.Application.ID == _ID)
                {
                    Instances.Remove(item);
                    return true;
                }
            }
            return false;
        }
        #endregion


    }
}
