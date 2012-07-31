using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TimeSync;

namespace SnowWhite.NET
{
    public class TcpServer
    {
        private readonly Dictionary<int, TcpClient> m_dicCurrentConnections;
        private readonly Thread m_listenerThread;
        private readonly int m_port;
        private readonly TcpListener m_tcpListener;
        private bool m_CloseAfterClientConnect;


        public TcpServer(int port)
        {
            // in case port is missing
            if (port <= 0)
            {
                port = 9001;
            }

            m_port = port;

            // new tcplistner that accepts connections from all network interfaces
            m_tcpListener = new TcpListener(IPAddress.Any, m_port);

            // listener runs in its own thread
            m_listenerThread = new Thread(ListenForTcpConnections);
            m_listenerThread.IsBackground = true;

            // holds the active connections
            m_dicCurrentConnections = new Dictionary<int, TcpClient>();
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public void StartServer(bool CloseAfterClientConnect)
        {
            m_CloseAfterClientConnect = CloseAfterClientConnect;
            Debug.WriteLine("Starting Server");
            m_listenerThread.Start();
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void StopServer()
        {
            Debug.WriteLine("Stopping server");
            foreach (TcpClient tcpClient in m_dicCurrentConnections.Values)
            {
                if (tcpClient.Connected)
                {
                    tcpClient.Close();
                }
            }
            m_tcpListener.Stop();
        }


        private void ListenForTcpConnections()
        {
            try
            {
                m_tcpListener.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't start the TcpListener: " + ex.Message);
                StopServer();
            }

            Debug.WriteLine("TcpListener started - Waiting for connections");


            if (m_CloseAfterClientConnect ==true)
            {
                
                 m_tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, m_tcpListener);

            }
            else
            {

                    m_tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, m_tcpListener);

                

            }
          

//Debug.WriteLine("Listener Closed: " + m_tcpListener.LocalEndpoint.ToString() );

        }

        /// <summary>
        /// A client iDevice has connected
        /// </summary>
        /// <param name="ar">Status of async operation</param>
        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            try
            {
                // get the current TcpListener
                var currentTcpListener = (TcpListener) ar.AsyncState;

                // get the currentClient
                TcpClient tcpClient = currentTcpListener.EndAcceptTcpClient(ar);

                Debug.WriteLine("Client connected on port: " + tcpClient.Client.RemoteEndPoint);

                // don't stop to listen
                m_tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, m_tcpListener);

                // Parameterized
                var clientThread = new Thread(HandleClientCommunication);

                clientThread.IsBackground = true;
                clientThread.Start(tcpClient);

                m_dicCurrentConnections.Add(clientThread.ManagedThreadId, tcpClient);

                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in \"AcceptTcpClientCallback\":  " + ex.Message);
            }
        }



        private void StartNTP(object s, EventArgs e)
        {

            TcpClient t;

            if ( m_dicCurrentConnections.ContainsKey( Thread.CurrentThread.ManagedThreadId) )
            {
                 m_dicCurrentConnections.TryGetValue(Thread.CurrentThread.ManagedThreadId,out t);
                string ip = t.Client.RemoteEndPoint.ToString().Split(":".ToCharArray()[0])[0];



                NTPClient ntpClient = new NTPClient(ip, AirPlayHandler.NTP_PORT);

                ntpClient.Connect(false);


            }



        }

      

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentClient">Needs to be object -> parameterized thread</param>
        private void HandleClientCommunication(object currentClient)
        {   TcpClient m_tcpClient;
            // cast currentClient
            m_tcpClient = (TcpClient)currentClient;

            Thread.CurrentThread.Name = m_tcpClient.Client.RemoteEndPoint.ToString();

            NetworkStream clientStream = m_tcpClient.GetStream();

            


            var handler = new MessageHandler();

            handler.m_StartNTP += this.StartNTP;

            // Now that we have the stream
            // we can close the connection

            if (m_CloseAfterClientConnect == true )

            {

                handler.ReadClientStream(clientStream, m_tcpClient);

                if (m_tcpClient.Connected)
                {
                    m_tcpClient.Close();
                }

                m_dicCurrentConnections.Remove(Thread.CurrentThread.ManagedThreadId); 


            }
            else
            {

                handler.ReadClientStreamEndless(clientStream, m_tcpClient);


            }
           
        }


    }
}