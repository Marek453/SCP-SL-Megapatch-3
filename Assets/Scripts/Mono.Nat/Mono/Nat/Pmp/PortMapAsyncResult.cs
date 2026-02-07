using System;

namespace Mono.Nat.Pmp
{
	internal class PortMapAsyncResult : AsyncResult
	{
		private Mapping mapping;

		internal Mapping Mapping
		{
			get
			{
				return mapping;
			}
		}

		internal PortMapAsyncResult(Mapping mapping, AsyncCallback callback, object asyncState)
			: base(callback, asyncState)
		{
			this.mapping = mapping;
		}

		internal PortMapAsyncResult(Protocol protocol, int port, int lifetime, AsyncCallback callback, object asyncState)
			: base(callback, asyncState)
		{
			mapping = new Mapping(protocol, port, port, lifetime);
		}
	}
}
