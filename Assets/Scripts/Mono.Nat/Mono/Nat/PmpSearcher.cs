using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Mono.Nat.Pmp;

namespace Mono.Nat
{
	internal class PmpSearcher : ISearcher
	{
		private static PmpSearcher instance;

		public static List<UdpClient> sockets;

		private static Dictionary<UdpClient, List<IPEndPoint>> gatewayLists;

		private int timeout;

		private DateTime nextSearch;

		private EventHandler<DeviceEventArgs> m_DeviceFound;

		private EventHandler<DeviceEventArgs> m_DeviceLost;

		public static PmpSearcher Instance
		{
			get
			{
				return instance;
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

		private PmpSearcher()
		{
			timeout = 250;
		}

		private static void CreateSocketsAndAddGateways()
		{
			sockets = new List<UdpClient>();
			gatewayLists = new Dictionary<UdpClient, List<IPEndPoint>>();
			try
			{
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface networkInterface in allNetworkInterfaces)
				{
					IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
					List<IPEndPoint> list = new List<IPEndPoint>();
					foreach (GatewayIPAddressInformation gatewayAddress in iPProperties.GatewayAddresses)
					{
						if (gatewayAddress.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							list.Add(new IPEndPoint(gatewayAddress.Address, 5351));
						}
					}
					if (list.Count <= 0)
					{
						continue;
					}
					foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
					{
						if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							UdpClient udpClient;
							try
							{
								udpClient = new UdpClient(new IPEndPoint(unicastAddress.Address, 0));
							}
							catch (SocketException)
							{
								continue;
							}
							gatewayLists.Add(udpClient, list);
							sockets.Add(udpClient);
						}
					}
				}
			}
			catch (Exception)
			{
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
			nextSearch = DateTime.Now.AddMilliseconds(timeout);
			timeout *= 2;
			if (timeout == 128000)
			{
				timeout = 250;
				nextSearch = DateTime.Now.AddMinutes(10.0);
				return;
			}
			byte[] array = new byte[2];
			foreach (IPEndPoint item in gatewayLists[client])
			{
				client.Send(array, array.Length, item);
			}
		}

		private bool IsSearchAddress(IPAddress address)
		{
			foreach (List<IPEndPoint> value in gatewayLists.Values)
			{
				foreach (IPEndPoint item in value)
				{
					if (item.Address.Equals(address))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void Handle(IPAddress localAddress, byte[] response, IPEndPoint endpoint)
		{
			if (IsSearchAddress(endpoint.Address) && response.Length == 12 && response[0] == 0 && response[1] == 128)
			{
				int num = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, 2));
				if (num != 0)
				{
					NatUtility.Log("Non zero error: {0}", num);
				}
				IPAddress publicAddress = new IPAddress(new byte[4]
				{
					response[8],
					response[9],
					response[10],
					response[11]
				});
				nextSearch = DateTime.Now.AddMinutes(5.0);
				timeout = 250;
				OnDeviceFound(new DeviceEventArgs(new PmpNatDevice(endpoint.Address, publicAddress)));
			}
		}

		private void OnDeviceFound(DeviceEventArgs args)
		{
			//i/f (this.DeviceFound != null)
			//{
			//	this.DeviceFound(this, args);
			//}
		}

		static PmpSearcher()
		{
			instance = new PmpSearcher();
			CreateSocketsAndAddGateways();
		}
	}
}
