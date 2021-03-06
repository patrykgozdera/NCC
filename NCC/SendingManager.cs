﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomSocket;
using System.Net;

namespace NCC
{
    class SendingManager
    {      
        private static CSocket csocket; 
        private static  MessageParameters message;

        public static void Init(int port)
        {
            string ipAdd = Config.getProperty("IPaddress");
            IPAddress ipAddress = IPAddress.Parse(ipAdd);
            csocket = new CSocket(ipAddress, port, CSocket.CONNECT_FUNCTION);            
        }

        public static void SendMessage(string messageName, string param1, string param2, int capacity)
        {
            message = new MessageParameters(param1, param2, capacity);
            csocket.SendObject(messageName, message);
        }

        public static void SendObject(object toSend)
        {
            csocket.SendObject(String.Empty, toSend);
        }
    }
}
