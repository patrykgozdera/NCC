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
        public const string CALL_TEARDOWN = "callTeardown";
        public const string CALL_TEARDOWN_NCC = "callTeardownNCC";
        public const string CALL_TEARDOWN_CPCC = "callTeardownCPCC";
        public const string CALL_TEARDOWN_NCC_2 = "callTeardownNCC_2";
        public const string CALL_COORDINATION = "callCoordination";
        public const string CALL_INDICATION = "callIndication";
        public const string CALL_CONFIRMED_CPCC = "callConfirmedCPCC";
        public const string CALL_CONFIRMED_NCC = "callConfirmedNCC";
        public const String CALL_REJECTED_CPCC = "callRejectedCPCC";
        public const String ACK_FUNCTION = "ackFunction";
        public const String NACK_FUNCTION = "nackFunction";

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
            Object receivedObject = received.Item2;
            string firstParam = null;
            string secondParam = null;

            if (receivedObject is MessageParameters)
            {
                messageParameters = (MessageParameters)received.Item2;
                firstParam = messageParameters.getFirstParameter();
                secondParam = messageParameters.getSecondParameter();
                capacity = messageParameters.getCapacity();
            }

            if (parameter.Equals(CALL_REQUEST))
            {
                LogClass.Log("Received CALL REQUEST from " + firstParam);
                try
                {
                    Policy.userAuthentication(firstParam);
                    userAddress_1 = Directory.getTranslatedAddress(firstParam);
                    LogClass.Log("Translated " + firstParam + " to " + userAddress_1);
                    userAddress_2 = Directory.getTranslatedAddress(secondParam);
                    if (userAddress_2 != null)
                        LogClass.Log("Translated " + secondParam + " to " + userAddress_2);

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
                catch (NullReferenceException)
                {
                    LogClass.Log("Cannot translate " + secondParam);
                    LogClass.Log("Cannot set up the connection");
                    SendingManager.Init(Config.getIntegerProperty("sendPortToCPCC"));
                    LogClass.Log("Sending CALL REJECTED to CPCC" + Environment.NewLine);
                    SendingManager.SendMessage(CALL_REJECTED_CPCC, "1", "1", 0);
                }
            }

            else if (parameter.Equals(CALL_TEARDOWN))
            {
                LogClass.Log("Received CALL TEARDOWN from " + firstParam);
                try
                {
                    Policy.userAuthentication(firstParam);
                    userAddress_1 = Directory.getTranslatedAddress(firstParam);
                    LogClass.Log("Translated " + firstParam + " to " + userAddress_1);
                    userAddress_2 = Directory.getTranslatedAddress(secondParam);
                    if (userAddress_2 != null)
                        LogClass.Log("Translated " + secondParam + " to " + userAddress_2);

                    if (userAddress_2.Contains("10.1"))
                    {
                        SendingManager.Init(Config.getIntegerProperty("sendPortToCC"));
                        LogClass.Log("Sending CALL TEARDOWN to CC" + Environment.NewLine);
                        SendingManager.SendObject(new Dictionary<string, string>());
                        SendingManager.SendMessage(CALL_TEARDOWN, userAddress_1, userAddress_2, capacity);

                    }
                    else if (userAddress_2.Contains("10.2"))
                    {
                        SendingManager.Init(Config.getIntegerProperty("sendPortToNCC"));
                        LogClass.Log("Sending CALL TEARDOWN to NCC_2" + Environment.NewLine);
                        SendingManager.SendMessage(CALL_TEARDOWN_NCC, userAddress_1, userAddress_2, capacity);
                    }

                }
                catch (NullReferenceException)
                {
                    LogClass.Log("Cannot translate " + secondParam);
                    LogClass.Log("Cannot set up the connection");
                    SendingManager.Init(Config.getIntegerProperty("sendPortToCPCC"));
                    LogClass.Log("Sending CALL REJECTED to CPCC" + Environment.NewLine);
                    SendingManager.SendMessage(CALL_REJECTED_CPCC, "1", "1", 0);
                }
            }

            else if (parameter.Equals(CALL_TEARDOWN_NCC))
            {
                LogClass.Log("Received CALL TEARDOWN from NCC_1");
                SendingManager.Init(Config.getIntegerProperty("sendPortToCPCC"));
                LogClass.Log("Sending CALL TEARDOWN to " + secondParam + Environment.NewLine);
                SendingManager.SendMessage(CALL_TEARDOWN_CPCC, firstParam, secondParam, capacity);
            }

            else if (parameter.Equals(CALL_TEARDOWN_CPCC))
            {
                LogClass.Log(secondParam + " accepted the dealocation");
                SendingManager.Init(Config.getIntegerProperty("sendPortToNCC"));
                LogClass.Log("Sending CALL TEARDOWN to NCC_1" + Environment.NewLine);
                SendingManager.SendMessage(CALL_TEARDOWN_NCC_2, userAddress_1, userAddress_2, capacity);
            }

            else if (parameter.Equals(CALL_TEARDOWN_NCC_2))
            {
                LogClass.Log("Received DEALLOCATION CONFIRMATION from NCC_2");
                SendingManager.Init(Config.getIntegerProperty("sendPortToCC"));
                LogClass.Log("Sending CALL TEARDOWN to CC" + Environment.NewLine);
                SendingManager.SendObject(new Dictionary<string, string>());
                SendingManager.SendMessage(CALL_TEARDOWN, userAddress_1, userAddress_2, capacity);
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

            else if (parameter.Equals(ACK_FUNCTION))
            {
                LogClass.Log("Connection set properly");
                SendingManager.Init(Config.getIntegerProperty("sendPortToCPCC"));
                LogClass.Log("Sending CALL CONFIRMED to CPCC" + Environment.NewLine);
                SendingManager.SendMessage(CALL_CONFIRMED_CPCC, "1", "1", 0);
            }

            else if (parameter.Equals(NACK_FUNCTION))
            {
                LogClass.Log("Cannot set up the connection");
                SendingManager.Init(Config.getIntegerProperty("sendPortToCPCC"));
                LogClass.Log("Sending CALL REJECTED to CPCC" + Environment.NewLine);
                SendingManager.SendMessage(CALL_REJECTED_CPCC, "1", "1", 0);
            }
        }
    }
}
