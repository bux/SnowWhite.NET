using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace SnowWhite.NET
{
    public class Utils
    {
        public static string m_MacAddress;


        /// <summary>
        /// Returns the first found Mac Address 
        /// could be optimized (me thinks)
        /// </summary>
        /// <returns></returns>
        public static string GetMacAddress()
        {
            if (string.IsNullOrEmpty(m_MacAddress))
            {
                string macAddr = (from nif in NetworkInterface.GetAllNetworkInterfaces()
                                  where nif.OperationalStatus == OperationalStatus.Up
                                  select nif.GetPhysicalAddress().ToString()).FirstOrDefault();

                if (string.IsNullOrEmpty(macAddr))
                {
                    macAddr = "11:22:33:44:55:66";
                }

                m_MacAddress = macAddr;
            }

            return m_MacAddress;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static string BuildPropertyListXML(Dictionary<string, KeyValuePair<string, string>> properties)
        {
            var messageBuilder = new StringBuilder();

            messageBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n");
            messageBuilder.Append("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n");
            messageBuilder.Append("<plist version=\"1.0\">\r\n");
            messageBuilder.Append("<dict>\r\n");

            foreach (var property in properties)
            {
                // <key>hello</key>
                messageBuilder.Append(String.Format("<key>{0}</key>\r\n", property.Key));

                if (property.Value.Value == "boolean")
                {
                    // <true\>
                    messageBuilder.Append(String.Format("<{0}/>\r\n", property.Value.Key));
                }
                else
                {
                    //<string>world</string>
                    messageBuilder.Append(String.Format("<{1}>{0}</{1}>\r\n", property.Value.Key, property.Value.Value));   
                }
            }

            messageBuilder.Append("</dict>\r\n");
            messageBuilder.Append("</plist>\r\n");

            return messageBuilder.ToString();
        }
    }
}