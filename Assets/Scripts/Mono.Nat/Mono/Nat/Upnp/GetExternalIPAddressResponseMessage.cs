using System;
using System.Net;

namespace Mono.Nat.Upnp
{
	internal class GetExternalIPAddressResponseMessage : MessageBase
	{
		private IPAddress externalIPAddress;

		public IPAddress ExternalIPAddress
		{
			get
			{
				return externalIPAddress;
			}
		}

		public GetExternalIPAddressResponseMessage(string ip)
			: base(null)
		{
			externalIPAddress = IPAddress.Parse(ip);
		}

		public override WebRequest Encode(out byte[] body)
		{
			throw new NotImplementedException();
		}
	}
}
