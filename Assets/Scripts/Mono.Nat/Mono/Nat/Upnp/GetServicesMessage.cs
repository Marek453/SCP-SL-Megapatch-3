#define TRACE
using System.Diagnostics;
using System.Net;

namespace Mono.Nat.Upnp
{
	internal class GetServicesMessage : MessageBase
	{
		private string servicesDescriptionUrl;

		private EndPoint hostAddress;

		public GetServicesMessage(string description, EndPoint hostAddress)
			: base(null)
		{
			if (string.IsNullOrEmpty(description))
			{
				Trace.WriteLine("Description is null");
			}
			if (hostAddress == null)
			{
				Trace.WriteLine("hostaddress is null");
			}
			servicesDescriptionUrl = description;
			this.hostAddress = hostAddress;
		}

		public override WebRequest Encode(out byte[] body)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://" + hostAddress.ToString() + servicesDescriptionUrl);
			httpWebRequest.Headers.Add("ACCEPT-LANGUAGE", "en");
			httpWebRequest.Method = "GET";
			body = new byte[0];
			return httpWebRequest;
		}
	}
}
