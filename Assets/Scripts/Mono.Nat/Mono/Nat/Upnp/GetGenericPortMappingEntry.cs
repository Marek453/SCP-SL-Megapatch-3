using System.Net;
using System.Text;
using System.Xml;

namespace Mono.Nat.Upnp
{
	internal class GetGenericPortMappingEntry : MessageBase
	{
		private int index;

		public GetGenericPortMappingEntry(int index, UpnpNatDevice device)
			: base(device)
		{
			this.index = index;
		}

		public override WebRequest Encode(out byte[] body)
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			XmlWriter xmlWriter = MessageBase.CreateWriter(stringBuilder);
			MessageBase.WriteFullElement(xmlWriter, "NewPortMappingIndex", index.ToString());
			xmlWriter.Flush();
			return CreateRequest("GetGenericPortMappingEntry", stringBuilder.ToString(), out body);
		}
	}
}
