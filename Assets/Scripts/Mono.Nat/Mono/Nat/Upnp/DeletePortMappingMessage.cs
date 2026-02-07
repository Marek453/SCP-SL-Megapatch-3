using System.Net;
using System.Text;
using System.Xml;

namespace Mono.Nat.Upnp
{
	internal class DeletePortMappingMessage : MessageBase
	{
		private Mapping mapping;

		public DeletePortMappingMessage(Mapping mapping, UpnpNatDevice device)
			: base(device)
		{
			this.mapping = mapping;
		}

		public override WebRequest Encode(out byte[] body)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			XmlWriter xmlWriter = MessageBase.CreateWriter(stringBuilder);
			MessageBase.WriteFullElement(xmlWriter, "NewRemoteHost", string.Empty);
			MessageBase.WriteFullElement(xmlWriter, "NewExternalPort", mapping.PublicPort.ToString(MessageBase.Culture));
			MessageBase.WriteFullElement(xmlWriter, "NewProtocol", (mapping.Protocol != 0) ? "UDP" : "TCP");
			xmlWriter.Flush();
			return CreateRequest("DeletePortMapping", stringBuilder.ToString(), out body);
		}
	}
}
