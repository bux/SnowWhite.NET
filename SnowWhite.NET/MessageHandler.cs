using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SnowWhite.NET
{
    public class MessageHandler
    {

        private NetworkStream m_twoWayStream;

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
                        string request;
                        // all elements except the last one
                        if (i + 1 < matches.Count)
                        {
                            // substring beginning from the index of match to the index of the next match
                            request=message.Substring(matches[i].Index, matches[i + 1].Index - matches[i].Index);
                        }
                        else
                        {
                            // handle the last match different because there's no next match
                            request=message.Substring(matches[i].Index);
                        }
                        
                        requests.Add(request.Trim());

                    }

                    foreach (var request in requests)
                    {
                        // grab the request and handle it
                        HandleRequest(request, clientStream, rawData);
                        
                        // todo: raise event
                        //ClientConnected(this, request);
                    }

                }

            }
        }


        /// <summary>
        /// Handles all the requests
        /// </summary>
        /// <param name="request"></param>
        /// <param name="clientStream"> </param>
        /// <param name="rawData"> </param>
        private void HandleRequest(string request, NetworkStream clientStream, List<byte> rawData)
        {

            Debug.WriteLine(request);

            // This is the first message the Apple device will send
            // http://nto.github.com/AirPlay.html#servicediscovery-airplayservice
            if (request.StartsWith("POST /reverse HTTP/1.1")) 
            {
                // We store this stream so we can use it later for replies
                // Needs to be this stream because the Apple device would refuse a new instance
                m_twoWayStream = clientStream;

                /* the repsonse 
                 * (\r\n -> newline) 
                 * {0:R} RFC1123 compliant date format -> Thu, 23 Feb 2012 17:33:41 GMT
                */
                var response = "HTTP/1.1 101 Switching Protocols\r\n" +
                                  "Date: " + String.Format("{0:R}", DateTime.Now) + "\r\n" +
                                  "Upgrade: PTTH/1.0\r\n" +
                                  "Connection: Upgrade\r\n" +
                                  "\r\n";

                sendResponse(response, clientStream);
                return;
            }

            // Fetch general informations about the AirPlay server. These informations are returned as an XML property list
            // http://nto.github.com/AirPlay.html#video-httprequests
            if (request.StartsWith("GET /server-info HTTP/1.1"))
            {

                string macAddr = Utils.GetMacAddress();

                var properties = new Dictionary<string, KeyValuePair<string, string>>();
                properties.Add("deviceid", new KeyValuePair<string, string>(macAddr, "string"));
                properties.Add("features", new KeyValuePair<string, string>("14839", "integer"));
                properties.Add("model", new KeyValuePair<string, string>("SnowWhite1,0", "string"));
                properties.Add("protovers", new KeyValuePair<string, string>("1.0", "string"));
                properties.Add("srcvers", new KeyValuePair<string, string>("120.2", "string"));

                //var propertyList = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                //                   "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n" +
                //                   "<plist version=\"1.0\">\r\n" +
                //                   "<dict>\r\n" +
                //                   "<key>deviceid</key>\r\n" +
                //                   "<string>" + macAddr + "</string>\r\n" +
                //                   "<key>features</key>\r\n" +
                //                   "<integer>14839</integer>\r\n" +
                //                   "<key>model</key>\r\n" +
                //                   "<string>SnowWhite1,0</string>\r\n" +
                //                   "<key>protovers</key>\r\n" +
                //                   "<string>1.0</string>\r\n" +
                //                   "<key>srcvers</key>\r\n" +
                //                   "<string>120.2</string>\r\n" +
                //                   "<\\dict>\r\n" +
                //                   "<\\plist>\r\n";

                var propertyList = Utils.BuildPropertyListXML(properties);


                var response = "HTTP/1.1 200 OK\r\n" +
                               "Date: " + String.Format("{0:R}", DateTime.Now) + "\r\n" +
                               "Content-Type: text/x-apple-plist+xml\r\n" +
                               "Content-Length: " + (propertyList.Length + 1).ToString() + "\r\n\r\n" +
                               propertyList;

                sendResponse(response, clientStream);
                return;
            }

            // Retrieve information about the server capabilities. The server sends an XML property list
            // http://nto.github.com/AirPlay.html#screenmirroring
            if (request.StartsWith("GET /stream.xml HTTP/1.1"))
            {
                var properties = new Dictionary<string, KeyValuePair<string, string>>();
                properties.Add("height", new KeyValuePair<string, string>("720", "integer"));
                properties.Add("overscanned", new KeyValuePair<string, string>("true", "boolean"));
                properties.Add("refreshRate", new KeyValuePair<string, string>("0.016666666666666666", "real"));
                properties.Add("version", new KeyValuePair<string, string>("130.14", "string"));
                properties.Add("width", new KeyValuePair<string, string>("1280", "integer"));

                var propertyList = Utils.BuildPropertyListXML(properties);


                var response = "HTTP/1.1 200 OK\r\n" +
                               "Date: " + String.Format("{0:R}", DateTime.Now) + "\r\n" +
                               "Content-Type: text/x-apple-plist+xml\r\n" +
                               "Content-Length: " + (propertyList.Length + 1).ToString() + "\r\n\r\n" +
                               propertyList;

                sendResponse(response, clientStream);
            }

        }

        /// <summary>
        /// Sends the response to the connected client
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="response"></param>
        private void sendResponse(string response, NetworkStream clientStream)
        {
            var buffer = new ASCIIEncoding().GetBytes(response);

            try
            {
                Debug.WriteLine(response);
                clientStream.Write(buffer, 0, buffer.Length);
                clientStream.Flush();
                
                // todo: raise event
                // responseSent(this, response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while sending the response: " + ex.Message);
            }

        }


        private string GetClientIPAddress(TcpClient tcpClient)
        {
            var temp = tcpClient.Client.RemoteEndPoint.ToString().Split(':');
            return temp[0];
        }




    }
}