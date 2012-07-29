using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SnowWhite.NET
{
    internal class MessageHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="tcpClient"></param>
        public void ReadClientStream(NetworkStream clientStream, TcpClient tcpClient)
        {
            Debug.WriteLine(String.Format("{0} hit \"{1}\"",
                                          Thread.CurrentThread.Name, "ReadClientStream"));

            if (tcpClient.Connected && clientStream.CanRead)
            {
                var rawData = new List<byte>();
                var readBuffer = new byte[1024];

                var myMessage = new StringBuilder();

                int numberOfBytesRead = 0;

                do
                {
                    try
                    {
                        numberOfBytesRead = clientStream.Read(readBuffer, 0, readBuffer.Length);
                        myMessage.Append(Encoding.ASCII.GetString(readBuffer, 0, numberOfBytesRead));
                        rawData.AddRange(readBuffer.Take(numberOfBytesRead));

                        Thread.Sleep(10);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error while reading the Stream" + ex.Message);
                    }
                } while (tcpClient.Connected && clientStream.DataAvailable); //make sure the client is still connected before checking for DataAvailable because the socket could get closed halfway through a read

                // again we need to make sure that the client is connected
                // because the socket could get closed halfway through a read
                // Don't care for a closed socket, because we can't send any replies
                if (tcpClient.Connected)
                {
                    var message = myMessage.ToString();
                    Debug.WriteLine(String.Format("{0} recived message: {1}", Thread.CurrentThread.Name, message));

                    //clients IpAddress
                    var IpAddress = GetClientIPAddress(tcpClient);


                    // as the tcp socket is a persitant connection we might recieve more than one message
                    // so the string has more than one http verbs (GET, POST, etc...)
                    var regX = new Regex("^HTTP|^GET [.]*|^POST [.]*|^PUT [.]*", RegexOptions.Multiline);
                    var matches = regX.Matches(message);

                    // so each new verb is a new request
                    // split the message
                    var requests = new List<String>();
                    for (int i = 0; i < matches.Count; i++)
                    {
                        // all elements except the last one
                        if (i + 1 < matches.Count)
                        {
                            // substring beginning from the index of match to the index of the next match
                            requests.Add(message.Substring(matches[i].Index, matches[i + 1].Index - matches[i].Index));
                        }
                        else
                        {
                            // handle the last match different because there's no next match
                            requests.Add(message.Substring(matches[i].Index));
                        }
                    }

                    foreach (var request in requests)
                    {
                        // grab the request and handle it
                        HandleRequest(request);
                        
                        // raise event
                        //ClientConnected(this, request);
                    }

                }

            }
        }




        /// <summary>
        /// Handles all the requests
        /// </summary>
        /// <param name="request"></param>
        private void HandleRequest(string request)
        {

        }



        private string GetClientIPAddress(TcpClient tcpClient)
        {
            var temp = tcpClient.Client.RemoteEndPoint.ToString().Split(':');
            return temp[0];
        }




    }
}