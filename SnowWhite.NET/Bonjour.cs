using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using ZeroconfService;

namespace SnowWhite.NET
{
    public class Bonjour
    {
        private const string DOMAIN = "local";
        private const string TYPE = "_airplay._tcp";
        private const int PORT = 9001;
        private readonly string m_name = String.Format("{0} - {1}", SystemInformation.ComputerName, "SnowWhite");
        private readonly Server m_theServer;
        private NetService m_publishService;

        private bool m_publishing;


        public Bonjour()
        {
            if (bonjourIsInstalled())
            {
                //start the TCP Server
                m_theServer = new Server(PORT);

                m_theServer.StartServer();


                PublishTheService();
            }
        }


        private bool bonjourIsInstalled()
        {
            Version bonjourVersion;

            const string errorMessage =
                "Bonjour couldn't be found. Download and install it from:\nhttp://support.apple.com/kb/DL999";

            try
            {
                bonjourVersion = DNSService.DaemonVersion;
                Debug.WriteLine(String.Format("Bonjour Version: {0}", DNSService.DaemonVersion));
            }
            catch (Exception ex)
            {
                string message;
                if (ex is DNSServiceException)
                {
                    message = errorMessage;
                }
                else
                {
                    message = ex.Message;
                }

                Debug.WriteLine(message);

                return false;
            }

            if (bonjourVersion == null || bonjourVersion.MajorRevision == 0)
            {
                Debug.WriteLine(errorMessage);
                return false;
            }

            return true;
        }


        private void PublishTheService()
        {
            m_publishService = new NetService(DOMAIN, TYPE, m_name, PORT);


            string macAddr = Utils.GetMacAddress();
            
            // AirPlay now shows everywhere :) not only in "Photos.app" and "Videos.app"
            var dicTXTRecord = new Dictionary<string, string>();
            dicTXTRecord.Add("model", "AppleTV2,1");

            dicTXTRecord.Add("deviceid", "58:55:CA:06:BD:9E");
            //dicTXTRecord.Add("deviceid", macAddr);

            // Bit field -> http://nto.github.com/AirPlay.html#servicediscovery-airplayservice
            dicTXTRecord.Add("features", "0x39f7");

            dicTXTRecord.Add("protovers", "1.0");
            dicTXTRecord.Add("srcvers", "101.10");

            // set to 1 to enable
            dicTXTRecord.Add("pw", "0");
            m_publishService.TXTRecordData = NetService.DataFromTXTRecordDictionary(dicTXTRecord);


            //add delegates for success/false
            m_publishService.DidPublishService += publishService_DidPublishService;
            m_publishService.DidNotPublishService += publishService_DidNotPublishService;

            m_publishService.Publish();
        }

        private void publishService_DidNotPublishService(NetService service, DNSServiceException exception)
        {
        }

        private void publishService_DidPublishService(NetService service)
        {
            m_publishing = true;
        }
    }
}