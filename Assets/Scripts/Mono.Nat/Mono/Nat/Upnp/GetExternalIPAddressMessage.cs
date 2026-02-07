using System.Net;

namespace Mono.Nat.Upnp
{
	internal class GetExternalIPAddressMessage : MessageBase
	{
		public GetExternalIPAddressMessage(UpnpNatDevice device)
			: base(device)
		{
		}

		public override WebRequest Encode(out byte[] body)
		{
			return CreateRequest("GetExternalIPAddress", string.Empty, out body);
		}
	}
}
