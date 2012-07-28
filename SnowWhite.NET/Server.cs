using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SnowWhite.NET
{
    public class Server
    {
        private Dictionary<int, TcpClient> m_dicCurrentConnections;
        private Thread m_listenerThread;
        private int m_port;
        private TcpListener m_tcpListener;
        private NetworkStream m_twoWayStream;


        public Server(int port)
        {
            // in case port is missing
            if (port <=0)
            {
                port = 7000;
            }

            m_port = port;

            m_tcpListener = new TcpListener(IPAddress.Any, m_port);

            this.m_listenerThread = new Thread(new ThreadStart(ListenForClients));
            m_listenerThread.IsBackground = true;

            m_dicCurrentConnections = new Dictionary<int, TcpClient>();


        }

    }
}