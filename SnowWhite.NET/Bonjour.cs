using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZeroconfService;


namespace SnowWhite.NET
{
    internal class Bonjour
    {
        private const string DOMAIN = "local";
        private const string TYPE = "_airplay._tcp";
        private string m_Name = String.Format("{0} - {1}", SystemInformation.ComputerName, "SnowWhite");
        private const int PORT = 7000;

        private bool m_Publishing;

        private NetService m_PublishService;
        private Server m_Server;


        public Bonjour()
        {
            try
            {
                if (bonjourIsInstalled())
                {
                    publishTheService();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        private bool bonjourIsInstalled()
        {
            Version bonjourVersion;

            const string errorMessage =
                "Bonjour couldn't be found. Download and install it from:\nhttp://support.apple.com/kb/DL999";

            try
            {
                bonjourVersion = NetService.DaemonVersion;
                Debug.WriteLine(String.Format("Bonjour Version: {0}", NetService.DaemonVersion));
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


        private void publishTheService()
        {
            m_PublishService = new NetService(DOMAIN,TYPE,m_Name,PORT);

            //add delegates for success/false
           // m_PublishService.DidPublishService += publishService_DidPublishService;
          //  m_PublishService.DidNotPublishService += publishService_DidNotPublishService;

            m_PublishService.Publish();


        }

    }
}