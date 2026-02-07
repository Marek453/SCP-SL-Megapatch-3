using System;
using System.Net;

namespace Mono.Nat.Upnp
{
	internal class CreatePortMappingResponseMessage : MessageBase
	{
		public CreatePortMappingResponseMessage()
			: base(null)
		{
		}

		public override WebRequest Encode(out byte[] body)
		{
			throw new NotImplementedException();
		}
	}
}
