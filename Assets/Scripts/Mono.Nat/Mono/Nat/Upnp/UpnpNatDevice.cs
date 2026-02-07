#define TRACE
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

namespace Mono.Nat.Upnp
{
	public sealed class UpnpNatDevice : AbstractNatDevice, IEquatable<UpnpNatDevice>
	{
		private EndPoint hostEndPoint;

		private IPAddress localAddress;

		private string serviceDescriptionUrl;

		private string controlUrl;

		private string serviceType;

		private NatDeviceCallback callback;

		internal EndPoint HostEndPoint
		{
			get
			{
				return hostEndPoint;
			}
		}

		internal string ServiceDescriptionUrl
		{
			get
			{
				return serviceDescriptionUrl;
			}
		}

		internal string ControlUrl
		{
			get
			{
				return controlUrl;
			}
		}

		internal string ServiceType
		{
			get
			{
				return serviceType;
			}
		}

		internal UpnpNatDevice(IPAddress localAddress, string deviceDetails, string serviceType)
		{
			base.LastSeen = DateTime.Now;
			this.localAddress = localAddress;
			string text = deviceDetails.Substring(deviceDetails.IndexOf("Location", StringComparison.InvariantCultureIgnoreCase) + 9).Split('\r')[0];
			this.serviceType = serviceType;
			text = text.Trim();
			if (text.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
			{
				NatUtility.Log("Found device at: {0}", text);
				text = text.Substring(7);
				string text2 = text.Remove(text.IndexOf('/'));
				if (text2.IndexOf(':') > 0)
				{
					hostEndPoint = new IPEndPoint(IPAddress.Parse(text2.Remove(text2.IndexOf(':'))), Convert.ToUInt16(text2.Substring(text2.IndexOf(':') + 1), CultureInfo.InvariantCulture));
				}
				else
				{
					hostEndPoint = new IPEndPoint(IPAddress.Parse(text2), 80);
				}
				NatUtility.Log("Parsed device as: {0}", hostEndPoint.ToString());
				serviceDescriptionUrl = text.Substring(text.IndexOf('/'));
			}
			else
			{
				Trace.WriteLine("Couldn't decode address. Please send following string to the developer: ");
				Trace.WriteLine(deviceDetails);
			}
		}

		public override IAsyncResult BeginGetExternalIP(AsyncCallback callback, object asyncState)
		{
			GetExternalIPAddressMessage message = new GetExternalIPAddressMessage(this);
			return BeginMessageInternal(message, callback, asyncState, EndGetExternalIPInternal);
		}

		public override IAsyncResult BeginCreatePortMap(Mapping mapping, AsyncCallback callback, object asyncState)
		{
			CreatePortMappingMessage message = new CreatePortMappingMessage(mapping, localAddress, this);
			return BeginMessageInternal(message, callback, mapping, EndCreatePortMapInternal);
		}

		public override IAsyncResult BeginDeletePortMap(Mapping mapping, AsyncCallback callback, object asyncState)
		{
			DeletePortMappingMessage message = new DeletePortMappingMessage(mapping, this);
			return BeginMessageInternal(message, callback, asyncState, EndDeletePortMapInternal);
		}

		public override IAsyncResult BeginGetAllMappings(AsyncCallback callback, object asyncState)
		{
			GetGenericPortMappingEntry message = new GetGenericPortMappingEntry(0, this);
			return BeginMessageInternal(message, callback, asyncState, EndGetAllMappingsInternal);
		}

		public override IAsyncResult BeginGetSpecificMapping(Protocol protocol, int port, AsyncCallback callback, object asyncState)
		{
			GetSpecificPortMappingEntryMessage message = new GetSpecificPortMappingEntryMessage(protocol, port, this);
			return BeginMessageInternal(message, callback, asyncState, EndGetSpecificMappingInternal);
		}

		public override void EndCreatePortMap(IAsyncResult result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			PortMapAsyncResult portMapAsyncResult = result as PortMapAsyncResult;
			if (portMapAsyncResult == null)
			{
				throw new ArgumentException("Invalid AsyncResult", "result");
			}
			if (!result.IsCompleted)
			{
				result.AsyncWaitHandle.WaitOne();
			}
			if (portMapAsyncResult.SavedMessage is ErrorMessage)
			{
				ErrorMessage errorMessage = portMapAsyncResult.SavedMessage as ErrorMessage;
				throw new MappingException(errorMessage.ErrorCode, errorMessage.Description);
			}
		}

		public override void EndDeletePortMap(IAsyncResult result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			PortMapAsyncResult portMapAsyncResult = result as PortMapAsyncResult;
			if (portMapAsyncResult == null)
			{
				throw new ArgumentException("Invalid AsyncResult", "result");
			}
			if (!portMapAsyncResult.IsCompleted)
			{
				portMapAsyncResult.AsyncWaitHandle.WaitOne();
			}
			if (portMapAsyncResult.SavedMessage is ErrorMessage)
			{
				ErrorMessage errorMessage = portMapAsyncResult.SavedMessage as ErrorMessage;
				throw new MappingException(errorMessage.ErrorCode, errorMessage.Description);
			}
		}

		public override Mapping[] EndGetAllMappings(IAsyncResult result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			GetAllMappingsAsyncResult getAllMappingsAsyncResult = result as GetAllMappingsAsyncResult;
			if (getAllMappingsAsyncResult == null)
			{
				throw new ArgumentException("Invalid AsyncResult", "result");
			}
			if (!getAllMappingsAsyncResult.IsCompleted)
			{
				getAllMappingsAsyncResult.AsyncWaitHandle.WaitOne();
			}
			if (getAllMappingsAsyncResult.SavedMessage is ErrorMessage)
			{
				ErrorMessage errorMessage = getAllMappingsAsyncResult.SavedMessage as ErrorMessage;
				if (errorMessage.ErrorCode != 713)
				{
					throw new MappingException(errorMessage.ErrorCode, errorMessage.Description);
				}
			}
			return getAllMappingsAsyncResult.Mappings.ToArray();
		}

		public override IPAddress EndGetExternalIP(IAsyncResult result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			PortMapAsyncResult portMapAsyncResult = result as PortMapAsyncResult;
			if (portMapAsyncResult == null)
			{
				throw new ArgumentException("Invalid AsyncResult", "result");
			}
			if (!result.IsCompleted)
			{
				result.AsyncWaitHandle.WaitOne();
			}
			if (portMapAsyncResult.SavedMessage is ErrorMessage)
			{
				ErrorMessage errorMessage = portMapAsyncResult.SavedMessage as ErrorMessage;
				throw new MappingException(errorMessage.ErrorCode, errorMessage.Description);
			}
			return ((GetExternalIPAddressResponseMessage)portMapAsyncResult.SavedMessage).ExternalIPAddress;
		}

		public override Mapping EndGetSpecificMapping(IAsyncResult result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			GetAllMappingsAsyncResult getAllMappingsAsyncResult = result as GetAllMappingsAsyncResult;
			if (getAllMappingsAsyncResult == null)
			{
				throw new ArgumentException("Invalid AsyncResult", "result");
			}
			if (!getAllMappingsAsyncResult.IsCompleted)
			{
				getAllMappingsAsyncResult.AsyncWaitHandle.WaitOne();
			}
			if (getAllMappingsAsyncResult.SavedMessage is ErrorMessage)
			{
				ErrorMessage errorMessage = getAllMappingsAsyncResult.SavedMessage as ErrorMessage;
				if (errorMessage.ErrorCode != 714)
				{
					throw new MappingException(errorMessage.ErrorCode, errorMessage.Description);
				}
			}
			if (getAllMappingsAsyncResult.Mappings.Count == 0)
			{
				return new Mapping(Protocol.Tcp, -1, -1);
			}
			return getAllMappingsAsyncResult.Mappings[0];
		}

		public override bool Equals(object obj)
		{
			UpnpNatDevice upnpNatDevice = obj as UpnpNatDevice;
			return upnpNatDevice != null && Equals(upnpNatDevice);
		}

		public bool Equals(UpnpNatDevice other)
		{
			return other != null && hostEndPoint.Equals(other.hostEndPoint) && serviceDescriptionUrl == other.serviceDescriptionUrl;
		}

		public override int GetHashCode()
		{
			return hostEndPoint.GetHashCode() ^ controlUrl.GetHashCode() ^ serviceDescriptionUrl.GetHashCode();
		}

		private IAsyncResult BeginMessageInternal(MessageBase message, AsyncCallback storedCallback, object asyncState, AsyncCallback callback)
		{
			byte[] body;
			WebRequest request = message.Encode(out body);
			PortMapAsyncResult mappingResult = PortMapAsyncResult.Create(message, request, storedCallback, asyncState);
			if (body.Length > 0)
			{
				request.ContentLength = body.Length;
				request.BeginGetRequestStream(delegate(IAsyncResult result)
				{
					try
					{
						Stream stream = request.EndGetRequestStream(result);
						stream.Write(body, 0, body.Length);
						request.BeginGetResponse(callback, mappingResult);
					}
					catch (Exception ex)
					{
						mappingResult.Complete(ex);
					}
				}, null);
			}
			else
			{
				request.BeginGetResponse(callback, mappingResult);
			}
			return mappingResult;
		}

		private void CompleteMessage(IAsyncResult result)
		{
			PortMapAsyncResult portMapAsyncResult = result.AsyncState as PortMapAsyncResult;
			portMapAsyncResult.CompletedSynchronously = result.CompletedSynchronously;
			portMapAsyncResult.Complete();
		}

		private MessageBase DecodeMessageFromResponse(Stream s, long length)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			int i = 0;
			byte[] array = new byte[10240];
			if (length != -1)
			{
				for (; i < length; i += num)
				{
					num = s.Read(array, 0, array.Length);
					stringBuilder.Append(Encoding.UTF8.GetString(array, 0, num));
				}
			}
			else
			{
				while ((num = s.Read(array, 0, array.Length)) != 0)
				{
					stringBuilder.Append(Encoding.UTF8.GetString(array, 0, num));
				}
			}
			return MessageBase.Decode(this, stringBuilder.ToString());
		}

		private void EndCreatePortMapInternal(IAsyncResult result)
		{
			EndMessageInternal(result);
			CompleteMessage(result);
		}

		private void EndMessageInternal(IAsyncResult result)
		{
			HttpWebResponse httpWebResponse = null;
			PortMapAsyncResult portMapAsyncResult = result.AsyncState as PortMapAsyncResult;
			try
			{
				try
				{
					httpWebResponse = (HttpWebResponse)portMapAsyncResult.Request.EndGetResponse(result);
				}
				catch (WebException ex)
				{
					httpWebResponse = ex.Response as HttpWebResponse;
					if (httpWebResponse == null)
					{
						portMapAsyncResult.SavedMessage = new ErrorMessage((int)ex.Status, ex.Message);
					}
				}
				if (httpWebResponse != null)
				{
					portMapAsyncResult.SavedMessage = DecodeMessageFromResponse(httpWebResponse.GetResponseStream(), httpWebResponse.ContentLength);
				}
			}
			finally
			{
				if (httpWebResponse != null)
				{
					httpWebResponse.Close();
				}
			}
		}

		private void EndDeletePortMapInternal(IAsyncResult result)
		{
			EndMessageInternal(result);
			CompleteMessage(result);
		}

		private void EndGetAllMappingsInternal(IAsyncResult result)
		{
			EndMessageInternal(result);
			GetAllMappingsAsyncResult getAllMappingsAsyncResult = result.AsyncState as GetAllMappingsAsyncResult;
			GetGenericPortMappingEntryResponseMessage getGenericPortMappingEntryResponseMessage = getAllMappingsAsyncResult.SavedMessage as GetGenericPortMappingEntryResponseMessage;
			if (getGenericPortMappingEntryResponseMessage != null)
			{
				Mapping mapping = new Mapping(getGenericPortMappingEntryResponseMessage.Protocol, getGenericPortMappingEntryResponseMessage.InternalPort, getGenericPortMappingEntryResponseMessage.ExternalPort, getGenericPortMappingEntryResponseMessage.LeaseDuration);
				mapping.Description = getGenericPortMappingEntryResponseMessage.PortMappingDescription;
				getAllMappingsAsyncResult.Mappings.Add(mapping);
				GetGenericPortMappingEntry getGenericPortMappingEntry = new GetGenericPortMappingEntry(getAllMappingsAsyncResult.Mappings.Count, this);
				byte[] body;
				WebRequest webRequest = getGenericPortMappingEntry.Encode(out body);
				if (body.Length > 0)
				{
					webRequest.ContentLength = body.Length;
					webRequest.GetRequestStream().Write(body, 0, body.Length);
				}
				getAllMappingsAsyncResult.Request = webRequest;
				webRequest.BeginGetResponse(EndGetAllMappingsInternal, getAllMappingsAsyncResult);
			}
			else
			{
				CompleteMessage(result);
			}
		}

		private void EndGetExternalIPInternal(IAsyncResult result)
		{
			EndMessageInternal(result);
			CompleteMessage(result);
		}

		private void EndGetSpecificMappingInternal(IAsyncResult result)
		{
			EndMessageInternal(result);
			GetAllMappingsAsyncResult getAllMappingsAsyncResult = result.AsyncState as GetAllMappingsAsyncResult;
			GetGenericPortMappingEntryResponseMessage getGenericPortMappingEntryResponseMessage = getAllMappingsAsyncResult.SavedMessage as GetGenericPortMappingEntryResponseMessage;
			if (getGenericPortMappingEntryResponseMessage != null)
			{
				Mapping mapping = new Mapping(getAllMappingsAsyncResult.SpecificMapping.Protocol, getGenericPortMappingEntryResponseMessage.InternalPort, getAllMappingsAsyncResult.SpecificMapping.PublicPort, getGenericPortMappingEntryResponseMessage.LeaseDuration);
				mapping.Description = getAllMappingsAsyncResult.SpecificMapping.Description;
				getAllMappingsAsyncResult.Mappings.Add(mapping);
			}
			CompleteMessage(result);
		}

		internal void GetServicesList(NatDeviceCallback callback)
		{
			this.callback = callback;
			byte[] body;
			WebRequest webRequest = new GetServicesMessage(serviceDescriptionUrl, hostEndPoint).Encode(out body);
			if (body.Length > 0)
			{
				NatUtility.Log("Error: Services Message contained a body");
			}
			webRequest.BeginGetResponse(ServicesReceived, webRequest);
		}

		private void ServicesReceived(IAsyncResult result)
		{
			HttpWebResponse httpWebResponse = null;
			try
			{
				int num = 0;
				int num2 = 0;
				byte[] array = new byte[10240];
				StringBuilder stringBuilder = new StringBuilder();
				XmlDocument xmlDocument = new XmlDocument();
				HttpWebRequest httpWebRequest = result.AsyncState as HttpWebRequest;
				httpWebResponse = httpWebRequest.EndGetResponse(result) as HttpWebResponse;
				Stream responseStream = httpWebResponse.GetResponseStream();
				if (httpWebResponse.StatusCode != HttpStatusCode.OK)
				{
					NatUtility.Log("{0}: Couldn't get services list: {1}", HostEndPoint, httpWebResponse.StatusCode);
					return;
				}
				while (true)
				{
					num2 = responseStream.Read(array, 0, array.Length);
					stringBuilder.Append(Encoding.UTF8.GetString(array, 0, num2));
					try
					{
						xmlDocument.LoadXml(stringBuilder.ToString());
						httpWebResponse.Close();
					}
					catch (XmlException)
					{
						if (num++ > 50)
						{
							httpWebResponse.Close();
							return;
						}
						NatUtility.Log("{0}: Couldn't parse services list", HostEndPoint);
						Thread.Sleep(10);
						continue;
					}
					break;
				}
				NatUtility.Log("{0}: Parsed services list", HostEndPoint);
				XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManager.AddNamespace("ns", "urn:schemas-upnp-org:device-1-0");
				XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//*/ns:serviceList", xmlNamespaceManager);
				IEnumerator enumerator = xmlNodeList.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						XmlNode xmlNode = (XmlNode)enumerator.Current;
						IEnumerator enumerator2 = xmlNode.ChildNodes.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								XmlNode xmlNode2 = (XmlNode)enumerator2.Current;
								string innerText = xmlNode2["serviceType"].InnerText;
								NatUtility.Log("{0}: Found service: {1}", HostEndPoint, innerText);
								StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;
								if (!innerText.Equals(serviceType, comparisonType))
								{
									continue;
								}
								controlUrl = xmlNode2["controlURL"].InnerText;
								NatUtility.Log("{0}: Found upnp service at: {1}", HostEndPoint, controlUrl);
								try
								{
									Uri uri = new Uri(controlUrl);
									if (uri.IsAbsoluteUri)
									{
										EndPoint endPoint = hostEndPoint;
										hostEndPoint = new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);
										NatUtility.Log("{0}: Absolute URI detected. Host address is now: {1}", endPoint, HostEndPoint);
										controlUrl = controlUrl.Substring(uri.GetLeftPart(UriPartial.Authority).Length);
										NatUtility.Log("{0}: New control url: {1}", HostEndPoint, controlUrl);
									}
								}
								catch
								{
									NatUtility.Log("{0}: Assuming control Uri is relative: {1}", HostEndPoint, controlUrl);
								}
								NatUtility.Log("{0}: Handshake Complete", HostEndPoint);
								callback(this);
								return;
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = enumerator2 as IDisposable) != null)
							{
								disposable.Dispose();
							}
						}
					}
				}
				finally
				{
					IDisposable disposable2;
					if ((disposable2 = enumerator as IDisposable) != null)
					{
						disposable2.Dispose();
					}
				}
			}
			catch (WebException ex2)
			{
				NatUtility.Log("{0}: Device denied the connection attempt: {1}", HostEndPoint, ex2);
			}
			finally
			{
				if (httpWebResponse != null)
				{
					httpWebResponse.Close();
				}
			}
		}

		public override string ToString()
		{
			return string.Format("UpnpNatDevice - EndPoint: {0}, External IP: {1}, Control Url: {2}, Service Description Url: {3}, Service Type: {4}, Last Seen: {5}", hostEndPoint, "Manually Check", controlUrl, serviceDescriptionUrl, serviceType, base.LastSeen);
		}
	}
}
