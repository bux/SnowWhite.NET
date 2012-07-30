using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnowWhite.NET
{
    class AirPlayHandler
    {

        /*
         *  TCP ports 7000, 47000, 7100, 49228, 50259 and UDP 62572, 54780 
         */

        private readonly Bonjour m_Bonjour;
        private readonly TcpServer m_MainTcpServer;
        private readonly TcpServer m_MirrorTcpServer;

        public const int MAIN_PORT = 7000;
        public const int MIRROR_PORT = 7100;
        public const int NTP_PORT = 7010;

        public AirPlayHandler()
        {
            m_Bonjour = new Bonjour();

            m_MainTcpServer = new TcpServer(MAIN_PORT);
            m_MirrorTcpServer = new TcpServer(MIRROR_PORT);
        }

        public void StartBonjour()
        {
            if (m_Bonjour != null) m_Bonjour.StartPublishing();
        }

        public void StartServers()
        {
            if (m_MainTcpServer != null) m_MainTcpServer.StartServer();
            if (m_MirrorTcpServer != null) m_MirrorTcpServer.StartServer();
        }
    }
}
