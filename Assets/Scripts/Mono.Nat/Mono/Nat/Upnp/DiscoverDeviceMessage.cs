using System.Text;

namespace Mono.Nat.Upnp
{
	internal static class DiscoverDeviceMessage
	{
		public static byte[] Encode()
		{
			string s = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 3\r\nST: ssdp:all\r\n\r\n";
			return Encoding.ASCII.GetBytes(s);
		}
	}
}
