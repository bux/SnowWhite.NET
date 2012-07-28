using System;
using System.Diagnostics;
using System.Windows.Forms;
using ZeroconfService;

namespace SnowWhite.NET
{
    public class Bonjour
    {
        private const string DOMAIN = "local";
        private const string TYPE = "_airplay._tcp";
        private const int PORT = 7000;
        private readonly string m_name = String.Format("{0} - {1}", SystemInformation.ComputerName, "SnowWhite");
        private NetService m_publishService;

        private bool m_publishing;

        private Server m_theServer;


        public Bonjour()
        {
            if (bonjourIsInstalled())
            {
                //start the TCP Server
                m_theServer = new Server(PORT);


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