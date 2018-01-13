using System;
using CustomSocket;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace NCC
{
    class NccServer
    {
        public const string CALL_REQUEST = "callRequest";
        public const string CONNECTION_REQUEST = "connectionRequestFromNCC";
        public const string CALL_TEARDOWN = "callTerdown";
        public const string CALL_COORDINATION = "callCoordination";
        public const string CALL_INDICATION = "callIndication";
        public const string CALL_CONFIRMED_CPCC = "callConfirmedCPCC";
        public const string CALL_CONFIRMED_NCC = "callConfirmedNCC";

        private static string userAddress_1;
        private static string userAddress_2;
        private static int capacity;

        private static CSocket csocket;
        private static MessageParameters messageParameters;

        public static void init()
        {
            initListeningCustomSocket();
            listenForConnections();
        }

        public static void initListeningCustomSocket()
        {
            string ipAdd = Config.getProperty("IPaddress");
            IPAddress ipAddress = IPAddress.Parse(ipAdd);
            int port = Config.getIntegerProperty("listenPort");
            csocket = new CSocket(ipAddress, port, CSocket.LISTENING_FUNCTION);
        }

        public static void listenForConnections()
        {
            initListenThread();
        }

        private static Thread initListenThread()
        {
            var t = new Thread(() => realStart());
            t.IsBackground = true;
            t.Start();
            return t;
        }

        private static void realStart()
        {
            csocket.Listen();
            while (true)
            {
                CSocket connected = csocket.Accept();
                waitForInputFromSocketInAnotherThread(connected);
            }

        }

        private static void waitForInputFromSocketInAnotherThread(CSocket connected)
        {
            var t = new Thread(() => waitForInput(connected));
            t.Start();
        }

        private static void waitForInput(CSocket socket)
        {
            Tuple<String, Object> received = socket.ReceiveObject();
            String parameter = received.Item1;
            messageParameters = (MessageParameters)received.Item2;
            string firstParam = messageParameters.getFirstParameter();
            string secondParam = messageParameters.getSecondParameter();
            capacity = messageParameters.getCapacity();

            if (parameter.Equals(CALL_REQUEST))
            {
                LogClass.Log("Received CALL REQUEST from " + firstParam);
                Policy.userAuthentication(firstParam);
                userAddress_1 = Directory.getTranslatedAddress(firstParam);
                userAddress_2 = Directory.getTranslatedAddress(secondParam);
                if (userAddress_2.Contains("10.1"))
                {
                    SendingManager.Init(Config.getIntegerProperty("sendPortToCC"));
                    LogClass.Log("Sending CONNECTION REQUEST to CC" + Environment.NewLine);
                    SendingManager.SendObject(new Dictionary<string, string>());
                    SendingManager.SendMessage(CONNECTION_REQUEST, userAddress_1, userAddress_2, capacity);

                }
                else if (userAddress_2.Contains("10.2"))
                {
                    SendingManager.Init(Config.getIntegerProperty("sendPortToNCC"));
                    LogClass.Log("Sending CALL COORDINATION to NCC_2" + Environment.NewLine);
                    SendingManager.SendMessage(CALL_COORDINATION, userAddress_1, userAddress_2, capacity);
                }
            }
            else if (parameter.Equals(CALL_COORDINATION))
            {
                LogClass.Log("Received CALL COORDINATION from NCC_1");
                SendingManager.Init(Config.getIntegerProperty("sendPortToCPCC"));
                LogClass.Log("Sending CALL INDICATION to " + secondParam + Environment.NewLine);
                SendingManager.SendMessage(CALL_INDICATION, firstParam, secondParam, capacity);
            }
            else if (parameter.Equals(CALL_CONFIRMED_CPCC))
            {
                LogClass.Log("Received CALL CONFIRMED from " + secondParam);
                SendingManager.Init(Config.getIntegerProperty("sendPortToNCC"));
                LogClass.Log("Sending CALL CONFIRMED to NCC_1" + Environment.NewLine);
                SendingManager.SendMessage(CALL_CONFIRMED_NCC, firstParam, secondParam, capacity);
            }
            else if (parameter.Equals(CALL_CONFIRMED_NCC))
            {
                LogClass.Log("Received CALL CONFIRMED from NCC_2");
                SendingManager.Init(Config.getIntegerProperty("sendPortToCC"));
                LogClass.Log("Sending CONNECTION REQUEST to CC" + Environment.NewLine);
                SendingManager.SendObject(new Dictionary<string, string>());
                SendingManager.SendMessage(CONNECTION_REQUEST, firstParam, secondParam, capacity);

            }
        }
    }
}
