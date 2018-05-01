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
        char Delimiter;
        DateTime TimeStampReceived;
        
        public Protocol(string protocol, char Delimiter)
        {
            ReceivedString = protocol;
            this.Delimiter = Delimiter;
            TimeStampReceived = DateTime.Now;
        }
        
        public bool Split()
        {
            string[] elements = null;

            //Every protocol must have at least 2 parameters (Protocol ID & Session ID), seperated by the delimiter
            //If this is not the case, the protocol is not sent by one of our clients
            if (!ReceivedString.Contains(Delimiter.ToString()) || (elements = ReceivedString.Split(Delimiter)).Length < 2)
                return false;

            //Make sure that Index 0 is an Integer
            if (!Int32.TryParse(elements[0].ToString(), out key))
                return false;
            UserID = elements[1].ToString();


            // if the protocol has not only the key, save the values.
            for (int i = 2; i < elements.Length; i++)
            {
                values.Add(elements[i]);
            }

            return true;
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

        public TimeSpan TimeNeededSecs()
        {
            return (DateTime.Now - TimeStampReceived);
        }
    }
}
