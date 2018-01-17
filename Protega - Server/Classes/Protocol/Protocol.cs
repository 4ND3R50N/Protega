using System;
using System.Collections;

namespace Protega___Server.Classes.Protocol
{
    class Protocol
    {
        public int key;
        public string UserID;
        private ArrayList values = new ArrayList();
        
        public Protocol(String protocol)
        {
            Object[] elements = null;
            // split the protocol at the delimiter ; to get the parts of the protocol
            if (protocol.Contains(";"))
            {
                elements = protocol.Split(';');
            }
            // the key is always saved at the first entry
            key =  Convert.ToInt32(elements[0]);
            UserID = elements[1].ToString();

            // if the protocol has not only the key, save the values.
            if (elements.Length > 1)
            {
                for (int i = 2; i < elements.Length; i++)
                {
                    values.Add(elements[i]);
                }
            }
            // otherwise values stays an empty arraylist
        }
        public int GetKey()
        {
            return key;
        }
        public string GetUserID()
        {
            return UserID;
        }
        public ArrayList GetValues()
        {
            return values;
        }
        public bool HasValues()
        {
            return values.Count > 0;
        }
    }
}
