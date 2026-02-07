using System;
using System.Collections.Generic;
using System.Net;

namespace Mono.Nat.Upnp
{
	internal class GetAllMappingsAsyncResult : PortMapAsyncResult
	{
		private List<Mapping> mappings;

		private Mapping specificMapping;

		public List<Mapping> Mappings
		{
			get
			{
				return mappings;
			}
		}

		public Mapping SpecificMapping
		{
			get
			{
				return specificMapping;
			}
			set
			{
				specificMapping = value;
			}
		}

		public GetAllMappingsAsyncResult(WebRequest request, AsyncCallback callback, object asyncState)
			: base(request, callback, asyncState)
		{
			mappings = new List<Mapping>();
		}
	}
}
