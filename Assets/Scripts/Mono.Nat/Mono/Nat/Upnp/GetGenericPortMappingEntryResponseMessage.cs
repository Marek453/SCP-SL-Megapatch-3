using System;
using System.Net;
using System.Xml;

namespace Mono.Nat.Upnp
{
	internal class GetGenericPortMappingEntryResponseMessage : MessageBase
	{
		private string remoteHost;

		private int externalPort;

		private Protocol protocol;

		private int internalPort;

		private string internalClient;

		private bool enabled;

		private string portMappingDescription;

		private int leaseDuration;

		public string RemoteHost
		{
			get
			{
				return remoteHost;
			}
		}

		public int ExternalPort
		{
			get
			{
				return externalPort;
			}
		}

		public Protocol Protocol
		{
			get
			{
				return protocol;
			}
		}

		public int InternalPort
		{
			get
			{
				return internalPort;
			}
		}

		public string InternalClient
		{
			get
			{
				return internalClient;
			}
		}

		public bool Enabled
		{
			get
			{
				return enabled;
			}
		}

		public string PortMappingDescription
		{
			get
			{
				return portMappingDescription;
			}
		}

		public int LeaseDuration
		{
			get
			{
				return leaseDuration;
			}
		}

		public GetGenericPortMappingEntryResponseMessage(XmlNode data, bool genericMapping)
			: base(null)
		{
			remoteHost = ((!genericMapping) ? string.Empty : data["NewRemoteHost"].InnerText);
			externalPort = ((!genericMapping) ? (-1) : Convert.ToInt32(data["NewExternalPort"].InnerText));
			if (genericMapping)
			{
				protocol = ((!data["NewProtocol"].InnerText.Equals("TCP", StringComparison.InvariantCultureIgnoreCase)) ? Protocol.Udp : Protocol.Tcp);
			}
			else
			{
				protocol = Protocol.Udp;
			}
			internalPort = Convert.ToInt32(data["NewInternalPort"].InnerText);
			internalClient = data["NewInternalClient"].InnerText;
			enabled = ((data["NewEnabled"].InnerText == "1") ? true : false);
			portMappingDescription = data["NewPortMappingDescription"].InnerText;
			leaseDuration = Convert.ToInt32(data["NewLeaseDuration"].InnerText);
		}

		public override WebRequest Encode(out byte[] body)
		{
			throw new NotImplementedException();
		}
	}
}
