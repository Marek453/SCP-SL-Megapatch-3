using System;

namespace Mono.Nat
{
	public class Mapping
	{
		private string description;

		private DateTime expiration;

		private int lifetime;

		private int privatePort;

		private Protocol protocol;

		private int publicPort;

		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				description = value;
			}
		}

		public Protocol Protocol
		{
			get
			{
				return protocol;
			}
			internal set
			{
				protocol = value;
			}
		}

		public int PrivatePort
		{
			get
			{
				return privatePort;
			}
			internal set
			{
				privatePort = value;
			}
		}

		public int PublicPort
		{
			get
			{
				return publicPort;
			}
			internal set
			{
				publicPort = value;
			}
		}

		public int Lifetime
		{
			get
			{
				return lifetime;
			}
			internal set
			{
				lifetime = value;
			}
		}

		public DateTime Expiration
		{
			get
			{
				return expiration;
			}
			internal set
			{
				expiration = value;
			}
		}

		public Mapping(Protocol protocol, int privatePort, int publicPort)
			: this(protocol, privatePort, publicPort, 0)
		{
		}

		public Mapping(Protocol protocol, int privatePort, int publicPort, int lifetime)
		{
			this.protocol = protocol;
			this.privatePort = privatePort;
			this.publicPort = publicPort;
			this.lifetime = lifetime;
			switch (lifetime)
			{
			case int.MaxValue:
				expiration = DateTime.MaxValue;
				break;
			case 0:
				expiration = DateTime.Now;
				break;
			default:
				expiration = DateTime.Now.AddSeconds(lifetime);
				break;
			}
		}

		public bool IsExpired()
		{
			return expiration < DateTime.Now;
		}

		public override bool Equals(object obj)
		{
			Mapping mapping = obj as Mapping;
			return mapping != null && protocol == mapping.protocol && privatePort == mapping.privatePort && publicPort == mapping.publicPort;
		}

		public override int GetHashCode()
		{
			return protocol.GetHashCode() ^ privatePort.GetHashCode() ^ publicPort.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("Protocol: {0}, Public Port: {1}, Private Port: {2}, Description: {3}, Expiration: {4}, Lifetime: {5}", protocol, publicPort, privatePort, description, expiration, lifetime);
		}
	}
}
