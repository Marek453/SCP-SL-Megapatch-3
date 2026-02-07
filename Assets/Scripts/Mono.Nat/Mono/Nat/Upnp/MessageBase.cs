using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;

namespace Mono.Nat.Upnp
{
	internal abstract class MessageBase
	{
		internal static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

		protected UpnpNatDevice device;

		protected MessageBase(UpnpNatDevice device)
		{
			this.device = device;
		}

		protected WebRequest CreateRequest(string upnpMethod, string methodParameters, out byte[] body)
		{
			string text = "http://" + device.HostEndPoint.ToString() + device.ControlUrl;
			NatUtility.Log("Initiating request to: {0}", text);
			Uri requestUri = new Uri(text);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
			httpWebRequest.KeepAlive = false;
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "text/xml; charset=\"utf-8\"";
			httpWebRequest.Headers.Add("SOAPACTION", "\"" + device.ServiceType + "#" + upnpMethod + "\"");
			string s = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body><u:" + upnpMethod + " xmlns:u=\"" + device.ServiceType + "\">" + methodParameters + "</u:" + upnpMethod + "></s:Body></s:Envelope>\r\n\r\n";
			body = Encoding.UTF8.GetBytes(s);
			return httpWebRequest;
		}

		public static MessageBase Decode(UpnpNatDevice device, string message)
		{
			XmlNode xmlNode = null;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(message);
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("errorNs", "urn:schemas-upnp-org:control-1-0");
			xmlNamespaceManager.AddNamespace("responseNs", device.ServiceType);
			if ((xmlNode = xmlDocument.SelectSingleNode("//errorNs:UPnPError", xmlNamespaceManager)) != null)
			{
				return new ErrorMessage(Convert.ToInt32(xmlNode["errorCode"].InnerText, CultureInfo.InvariantCulture), xmlNode["errorDescription"].InnerText);
			}
			if ((xmlNode = xmlDocument.SelectSingleNode("//responseNs:AddPortMappingResponse", xmlNamespaceManager)) != null)
			{
				return new CreatePortMappingResponseMessage();
			}
			if ((xmlNode = xmlDocument.SelectSingleNode("//responseNs:DeletePortMappingResponse", xmlNamespaceManager)) != null)
			{
				return new DeletePortMapResponseMessage();
			}
			if ((xmlNode = xmlDocument.SelectSingleNode("//responseNs:GetExternalIPAddressResponse", xmlNamespaceManager)) != null)
			{
				return new GetExternalIPAddressResponseMessage(xmlNode["NewExternalIPAddress"].InnerText);
			}
			if ((xmlNode = xmlDocument.SelectSingleNode("//responseNs:GetGenericPortMappingEntryResponse", xmlNamespaceManager)) != null)
			{
				return new GetGenericPortMappingEntryResponseMessage(xmlNode, true);
			}
			if ((xmlNode = xmlDocument.SelectSingleNode("//responseNs:GetSpecificPortMappingEntryResponse", xmlNamespaceManager)) != null)
			{
				return new GetGenericPortMappingEntryResponseMessage(xmlNode, false);
			}
			NatUtility.Log("Unknown message returned. Please send me back the following XML:");
			NatUtility.Log(message);
			return null;
		}

		public abstract WebRequest Encode(out byte[] body);

		internal static void WriteFullElement(XmlWriter writer, string element, string value)
		{
			writer.WriteStartElement(element);
			writer.WriteString(value);
			writer.WriteEndElement();
		}

		internal static XmlWriter CreateWriter(StringBuilder sb)
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
			return XmlWriter.Create(sb, xmlWriterSettings);
		}
	}
}
