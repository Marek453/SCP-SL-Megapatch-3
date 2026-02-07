using System.Net;
using System.Text;
using System.Xml;

namespace Mono.Nat.Upnp
{
	internal class GetSpecificPortMappingEntryMessage : MessageBase
	{
		internal Protocol protocol;

		internal int externalPort;

		public GetSpecificPortMappingEntryMessage(Protocol protocol, int externalPort, UpnpNatDevice device)
			: base(device)
		{
			this.protocol = protocol;
			this.externalPort = externalPort;
		}

		public override WebRequest Encode(out byte[] body)
		{
			StringBuilder stringBuilder = new StringBuilder(64);
			XmlWriter xmlWriter = MessageBase.CreateWriter(stringBuilder);
			MessageBase.WriteFullElement(xmlWriter, "NewRemoteHost", string.Empty);
			MessageBase.WriteFullElement(xmlWriter, "NewExternalPort", externalPort.ToString());
			MessageBase.WriteFullElement(xmlWriter, "NewProtocol", (protocol != 0) ? "UDP" : "TCP");
			xmlWriter.Flush();
			return CreateRequest("GetSpecificPortMappingEntry", stringBuilder.ToString(), out body);
		}
	}
}
