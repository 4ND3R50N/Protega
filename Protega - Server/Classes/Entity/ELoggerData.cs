using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Entity
{
    public class ELoggerData
    {
        string _ID;
        Support.LogCategory _Category;
        Support.LoggerType _Type;
        int _Importance;
        string _Message;
        int _ApplicationID;

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public Support.LogCategory Category
        {
            get { return _Category; }
            set { _Category = value; }
        }

        public Support.LoggerType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        public int Importance
        {
            get { return _Importance; }
            set { _Importance = value; }
        }

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
        
        public int ApplicationID
        {
            get { return _ApplicationID; }
            set { _ApplicationID = value; }
        }

    }
}
