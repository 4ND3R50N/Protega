﻿using System;
using System.Collections;

namespace Protega___Server.Classes.Protocol
{
    class Protocol
    {
        public int key;
        public string ComputerID;
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
            ComputerID = elements[1].ToString();

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
        public string GetComputerID()
        {
            return ComputerID;
        }
        public ArrayList GetValues()
        {
            return values;
        }
    }
}
