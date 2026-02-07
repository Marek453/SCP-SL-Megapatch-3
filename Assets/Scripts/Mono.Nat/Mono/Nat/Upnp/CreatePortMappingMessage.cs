using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;

namespace Mono.Nat.Upnp
{
	internal class CreatePortMappingMessage : MessageBase
	{
		private IPAddress localIpAddress;

		private Mapping mapping;

		public CreatePortMappingMessage(Mapping mapping, IPAddress localIpAddress, UpnpNatDevice device)
			: base(device)
		{
			this.mapping = mapping;
			this.localIpAddress = localIpAddress;
		}

		public override WebRequest Encode(out byte[] body)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			StringBuilder stringBuilder = new StringBuilder(256);
			XmlWriter xmlWriter = MessageBase.CreateWriter(stringBuilder);
			MessageBase.WriteFullElement(xmlWriter, "NewRemoteHost", string.Empty);
			MessageBase.WriteFullElement(xmlWriter, "NewExternalPort", mapping.PublicPort.ToString(invariantCulture));
			MessageBase.WriteFullElement(xmlWriter, "NewProtocol", (mapping.Protocol != 0) ? "UDP" : "TCP");
			MessageBase.WriteFullElement(xmlWriter, "NewInternalPort", mapping.PrivatePort.ToString(invariantCulture));
			MessageBase.WriteFullElement(xmlWriter, "NewInternalClient", localIpAddress.ToString());
			MessageBase.WriteFullElement(xmlWriter, "NewEnabled", "1");
			MessageBase.WriteFullElement(xmlWriter, "NewPortMappingDescription", (!string.IsNullOrEmpty(mapping.Description)) ? mapping.Description : "Mono.Nat");
			MessageBase.WriteFullElement(xmlWriter, "NewLeaseDuration", mapping.Lifetime.ToString());
			xmlWriter.Flush();
			return CreateRequest("AddPortMapping", stringBuilder.ToString(), out body);
		}
	}
}
