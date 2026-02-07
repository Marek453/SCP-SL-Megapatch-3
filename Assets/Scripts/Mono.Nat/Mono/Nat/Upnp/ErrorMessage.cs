using System;
using System.Net;

namespace Mono.Nat.Upnp
{
	internal class ErrorMessage : MessageBase
	{
		private string description;

		private int errorCode;

		public string Description
		{
			get
			{
				return description;
			}
		}

		public int ErrorCode
		{
			get
			{
				return errorCode;
			}
		}

		public ErrorMessage(int errorCode, string description)
			: base(null)
		{
			this.description = description;
			this.errorCode = errorCode;
		}

		public override WebRequest Encode(out byte[] body)
		{
			throw new NotImplementedException();
		}
	}
}
