using System;
using System.Collections;
using Support;

namespace Protega___Server.Classes.Protocol
{
    class Protocol
    {
        public int key;
        public string UserID;
        private string ReceivedString;
        private ArrayList values = new ArrayList();
        
        public Protocol(String protocol)
        {
            ReceivedString = protocol;
            Object[] elements = null;
            // split the protocol at the delimiter ; to get the parts of the protocol
            // we do need some check if the protocol has the right syntax, what happens if we get traffic and we can decrypt it and there is no ;?
            // we can't split it so we will get errors by trying to get the second element
            if (protocol.Contains(";"))
            {
                elements = protocol.Split(';');
            }
            // the key is always saved at the first entry
            // QUESTION: What happens if this is not an integer? does the whole server crash?
            // I know that the possibility is very low that this happens but if someone is sending us traffic, we decrypt it and give it to the protocol controler, this protocol will be broken
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
        public string GetOriginalString()
        {
            return ReceivedString != null ? ReceivedString : "";
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
