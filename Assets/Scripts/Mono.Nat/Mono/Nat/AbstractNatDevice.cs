using System;
using System.Net;

namespace Mono.Nat
{
	public abstract class AbstractNatDevice : INatDevice
	{
		private DateTime lastSeen;

		public DateTime LastSeen
		{
			get
			{
				return lastSeen;
			}
			set
			{
				lastSeen = value;
			}
		}

		public virtual void CreatePortMap(Mapping mapping)
		{
			IAsyncResult result = BeginCreatePortMap(mapping, null, null);
			EndCreatePortMap(result);
		}

		public virtual void DeletePortMap(Mapping mapping)
		{
			IAsyncResult result = BeginDeletePortMap(mapping, null, mapping);
			EndDeletePortMap(result);
		}

		public virtual Mapping[] GetAllMappings()
		{
			IAsyncResult result = BeginGetAllMappings(null, null);
			return EndGetAllMappings(result);
		}

		public virtual IPAddress GetExternalIP()
		{
			IAsyncResult result = BeginGetExternalIP(null, null);
			return EndGetExternalIP(result);
		}

		public virtual Mapping GetSpecificMapping(Protocol protocol, int port)
		{
			IAsyncResult result = BeginGetSpecificMapping(protocol, port, null, null);
			return EndGetSpecificMapping(result);
		}

		public abstract IAsyncResult BeginCreatePortMap(Mapping mapping, AsyncCallback callback, object asyncState);

		public abstract IAsyncResult BeginDeletePortMap(Mapping mapping, AsyncCallback callback, object asyncState);

		public abstract IAsyncResult BeginGetAllMappings(AsyncCallback callback, object asyncState);

		public abstract IAsyncResult BeginGetExternalIP(AsyncCallback callback, object asyncState);

		public abstract IAsyncResult BeginGetSpecificMapping(Protocol protocol, int externalPort, AsyncCallback callback, object asyncState);

		public abstract void EndCreatePortMap(IAsyncResult result);

		public abstract void EndDeletePortMap(IAsyncResult result);

		public abstract Mapping[] EndGetAllMappings(IAsyncResult result);

		public abstract IPAddress EndGetExternalIP(IAsyncResult result);

		public abstract Mapping EndGetSpecificMapping(IAsyncResult result);
	}
}
