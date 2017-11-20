using System;
using System.Collections;

namespace Protega___Server.Classes.Protocol
{
    class Protocol
    {
        public int key;
        private ArrayList values = new ArrayList();

        public Protocol(String protocol)
        {
            Object[] elements;
            if (protocol.Contains(";"))
                elements = protocol.Split(';');
            //this.key = (int)elements[0];
            //foreach(var o in elements)
            //{
            //    values.Add(o);
            //}
        }
        public int GetKey()
        {
            return key;
        }
        public ArrayList GetValues()
        {
            return values;
        }
    }
}
