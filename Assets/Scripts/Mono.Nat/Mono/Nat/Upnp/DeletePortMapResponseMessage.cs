using System;
using System.Net;

namespace Mono.Nat.Upnp
{
	internal class DeletePortMapResponseMessage : MessageBase
	{
		public DeletePortMapResponseMessage()
			: base(null)
		{
		}

		public override WebRequest Encode(out byte[] body)
		{
			throw new NotSupportedException();
		}
	}
}
