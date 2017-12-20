using System;
using CustomSocket;
using System.Net;
using System.Threading;
using System.Net.Sockets;

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

            if (parameter.Equals(CALL_REQUEST))
            {
                Policy.userAuthentication(messageParameters.getFirstParameter());               
                userAddress_1 = Directory.getTranslatedAddress(messageParameters.getFirstParameter());
                userAddress_2 = Directory.getTranslatedAddress(messageParameters.getSecondParameter());
                capacity = messageParameters.getCapacity();
                SendingManager.Init(Config.getIntegerProperty("sendPortToNCC"));
                SendingManager.SendMessage(CALL_COORDINATION, userAddress_1, userAddress_2, capacity);
            }
            else if (parameter.Equals(CALL_COORDINATION))
            {
                Console.WriteLine("eloszka");
                SendingManager.Init(Config.getIntegerProperty("sendPortToCPCC"));
                SendingManager.SendMessage(CALL_INDICATION, messageParameters.getFirstParameter(), messageParameters.getSecondParameter(), messageParameters.getCapacity());
            }
            else if (parameter.Equals(CALL_CONFIRMED_CPCC))
            {
                Console.WriteLine("kokoszka");
                SendingManager.Init(Config.getIntegerProperty("sendPortToNCC"));
                SendingManager.SendMessage(CALL_CONFIRMED_NCC, messageParameters.getFirstParameter(), messageParameters.getSecondParameter(), messageParameters.getCapacity());
            }
            else if (parameter.Equals(CALL_CONFIRMED_NCC))
            {
                Console.WriteLine("ConnectionRequestToCC");
                SendingManager.Init(Config.getIntegerProperty("sendPortToCC"));
                SendingManager.SendMessage(CONNECTION_REQUEST, messageParameters.getFirstParameter(), messageParameters.getSecondParameter(), messageParameters.getCapacity());

            }
        }
    }
}
