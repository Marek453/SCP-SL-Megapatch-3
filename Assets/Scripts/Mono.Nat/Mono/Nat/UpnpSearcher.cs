#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Mono.Nat.Upnp;

namespace Mono.Nat
{
	internal class UpnpSearcher : ISearcher
	{
		internal const string WanIPUrn = "urn:schemas-upnp-org:service:WANIPConnection:1";

		private const int SearchPeriod = 300;

		private static UpnpSearcher instance = new UpnpSearcher();

		public static List<UdpClient> sockets = CreateSockets();

		private List<INatDevice> devices;

		private Dictionary<IPAddress, DateTime> lastFetched;

		private DateTime nextSearch;

		private IPEndPoint searchEndpoint;

		private EventHandler<DeviceEventArgs> m_DeviceFound;

		private EventHandler<DeviceEventArgs> m_DeviceLost;

		public static UpnpSearcher Instance
		{
			get
			{
				return instance;
			}
		}

		public IPEndPoint SearchEndpoint
		{
			get
			{
				return searchEndpoint;
			}
		}

		public DateTime NextSearch
		{
			get
			{
				return nextSearch;
			}
		}

		public event EventHandler<DeviceEventArgs> DeviceFound
		{
			add
			{
				EventHandler<DeviceEventArgs> eventHandler = this.m_DeviceFound;
				EventHandler<DeviceEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref this.m_DeviceFound, (EventHandler<DeviceEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<DeviceEventArgs> eventHandler = this.m_DeviceFound;
				EventHandler<DeviceEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref this.m_DeviceFound, (EventHandler<DeviceEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
		}

		public event EventHandler<DeviceEventArgs> DeviceLost
		{
			add
			{
				EventHandler<DeviceEventArgs> eventHandler = this.m_DeviceLost;
				EventHandler<DeviceEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref this.m_DeviceLost, (EventHandler<DeviceEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<DeviceEventArgs> eventHandler = this.m_DeviceLost;
				EventHandler<DeviceEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref this.m_DeviceLost, (EventHandler<DeviceEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
		}

		private UpnpSearcher()
		{
			devices = new List<INatDevice>();
			lastFetched = new Dictionary<IPAddress, DateTime>();
			searchEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
		}

		private static List<UdpClient> CreateSockets()
		{
			List<UdpClient> list = new List<UdpClient>();
			try
			{
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface networkInterface in allNetworkInterfaces)
				{
					foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
					{
						if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							try
							{
								list.Add(new UdpClient(new IPEndPoint(unicastAddress.Address, 0)));
							}
							catch
							{
							}
						}
					}
				}
				return list;
			}
			catch (Exception)
			{
				list.Add(new UdpClient(0));
				return list;
			}
		}

		public void Search()
		{
			foreach (UdpClient socket in sockets)
			{
				try
				{
					Search(socket);
				}
				catch
				{
				}
			}
		}

		private void Search(UdpClient client)
		{
			nextSearch = DateTime.Now.AddSeconds(300.0);
			byte[] array = DiscoverDeviceMessage.Encode();
			for (int i = 0; i < 3; i++)
			{
				client.Send(array, array.Length, searchEndpoint);
			}
		}

		public void Handle(IPAddress localAddress, byte[] response, IPEndPoint endpoint)
		{
			string text = null;
			try
			{
				text = Encoding.UTF8.GetString(response);
				if (NatUtility.Verbose)
				{
					NatUtility.Log("UPnP Response: {0}", text);
				}
				string format = "UPnP Response: Router advertised a '{0}' service";
				StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;
				if (text.IndexOf("urn:schemas-upnp-org:service:WANIPConnection:", comparisonType) != -1)
				{
					NatUtility.Log(format, "urn:schemas-upnp-org:service:WANIPConnection:");
				}
				else if (text.IndexOf("urn:schemas-upnp-org:device:InternetGatewayDevice:", comparisonType) != -1)
				{
					NatUtility.Log(format, "urn:schemas-upnp-org:device:InternetGatewayDevice:");
				}
				else
				{
					if (text.IndexOf("urn:schemas-upnp-org:service:WANPPPConnection:", comparisonType) == -1)
					{
						return;
					}
					NatUtility.Log(format, "urn:schemas-upnp-org:service:WANPPPConnection:");
				}
				UpnpNatDevice upnpNatDevice = new UpnpNatDevice(localAddress, text, "urn:schemas-upnp-org:service:WANIPConnection:1");
				if (devices.Contains(upnpNatDevice))
				{
					devices[devices.IndexOf(upnpNatDevice)].LastSeen = DateTime.Now;
					return;
				}
				if (lastFetched.ContainsKey(endpoint.Address))
				{
					DateTime dateTime = lastFetched[endpoint.Address];
					if (DateTime.Now - dateTime < TimeSpan.FromSeconds(20.0))
					{
						return;
					}
				}
				lastFetched[endpoint.Address] = DateTime.Now;
				NatUtility.Log("Fetching service list: {0}", upnpNatDevice.HostEndPoint);
				upnpNatDevice.GetServicesList(DeviceSetupComplete);
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Unhandled exception when trying to decode a device's response Send me the following data: ");
				Trace.WriteLine("ErrorMessage:");
				Trace.WriteLine(ex.Message);
				Trace.WriteLine("Data string:");
				Trace.WriteLine(text);
			}
		}

		private void DeviceSetupComplete(INatDevice device)
		{
			lock (devices)
			{
				if (devices.Contains(device))
				{
					return;
				}
				devices.Add(device);
			}
			OnDeviceFound(new DeviceEventArgs(device));
		}

		private void OnDeviceFound(DeviceEventArgs args)
		{
			//if (this.DeviceFound != null)
			//{
			//	this.DeviceFound(this, args);
			//}
		}
	}
}
