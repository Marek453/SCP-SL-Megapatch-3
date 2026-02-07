using System;
using System.Net;

namespace Mono.Nat.Upnp
{
	internal class PortMapAsyncResult : AsyncResult
	{
		private WebRequest request;

		private MessageBase savedMessage;

		internal WebRequest Request
		{
			get
			{
				return request;
			}
			set
			{
				request = value;
			}
		}

		internal MessageBase SavedMessage
		{
			get
			{
				return savedMessage;
			}
			set
			{
				savedMessage = value;
			}
		}

		protected PortMapAsyncResult(WebRequest request, AsyncCallback callback, object asyncState)
			: base(callback, asyncState)
		{
			this.request = request;
		}

		internal static PortMapAsyncResult Create(MessageBase message, WebRequest request, AsyncCallback storedCallback, object asyncState)
		{
			if (message is GetGenericPortMappingEntry)
			{
				return new GetAllMappingsAsyncResult(request, storedCallback, asyncState);
			}
			if (message is GetSpecificPortMappingEntryMessage)
			{
				GetSpecificPortMappingEntryMessage getSpecificPortMappingEntryMessage = (GetSpecificPortMappingEntryMessage)message;
				GetAllMappingsAsyncResult getAllMappingsAsyncResult = new GetAllMappingsAsyncResult(request, storedCallback, asyncState);
				getAllMappingsAsyncResult.SpecificMapping = new Mapping(getSpecificPortMappingEntryMessage.protocol, 0, getSpecificPortMappingEntryMessage.externalPort, 0);
				return getAllMappingsAsyncResult;
			}
			return new PortMapAsyncResult(request, storedCallback, asyncState);
		}
	}
}
