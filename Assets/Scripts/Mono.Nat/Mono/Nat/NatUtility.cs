using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Mono.Nat
{
	public static class NatUtility
	{
		private static ManualResetEvent searching;

		private static TextWriter logger;

		private static List<ISearcher> controllers;

		private static bool verbose;

		private static EventHandler<DeviceEventArgs> m_DeviceFound;

		private static EventHandler<DeviceEventArgs> m_DeviceLost;

		private static EventHandler<UnhandledExceptionEventArgs> m_UnhandledException;

		public static TextWriter Logger
		{
			get
			{
				return logger;
			}
			set
			{
				logger = value;
			}
		}

		public static bool Verbose
		{
			get
			{
				return verbose;
			}
			set
			{
				verbose = value;
			}
		}

		public static event EventHandler<DeviceEventArgs> DeviceFound
		{
			add
			{
				EventHandler<DeviceEventArgs> eventHandler = NatUtility.m_DeviceFound;
				EventHandler<DeviceEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref NatUtility.m_DeviceFound, (EventHandler<DeviceEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<DeviceEventArgs> eventHandler = NatUtility.m_DeviceFound;
				EventHandler<DeviceEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref NatUtility.m_DeviceFound, (EventHandler<DeviceEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
		}

		public static event EventHandler<DeviceEventArgs> DeviceLost
		{
			add
			{
				EventHandler<DeviceEventArgs> eventHandler = NatUtility.m_DeviceLost;
				EventHandler<DeviceEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref NatUtility.m_DeviceLost, (EventHandler<DeviceEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<DeviceEventArgs> eventHandler = NatUtility.m_DeviceLost;
				EventHandler<DeviceEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref NatUtility.m_DeviceLost, (EventHandler<DeviceEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
		}

		public static event EventHandler<UnhandledExceptionEventArgs> UnhandledException
		{
			add
			{
				EventHandler<UnhandledExceptionEventArgs> eventHandler = NatUtility.m_UnhandledException;
				EventHandler<UnhandledExceptionEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref NatUtility.m_UnhandledException, (EventHandler<UnhandledExceptionEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<UnhandledExceptionEventArgs> eventHandler = NatUtility.m_UnhandledException;
				EventHandler<UnhandledExceptionEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref NatUtility.m_UnhandledException, (EventHandler<UnhandledExceptionEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
		}

		internal static void Log(string format, params object[] args)
		{
			TextWriter textWriter = Logger;
			if (textWriter != null)
			{
				textWriter.WriteLine(format, args);
			}
		}

		private static void SearchAndListen()
		{
			while (true)
			{
				searching.WaitOne();
				try
				{
					Receive(UpnpSearcher.Instance, UpnpSearcher.sockets);
					Receive(PmpSearcher.Instance, PmpSearcher.sockets);
					foreach (ISearcher controller in controllers)
					{
						if (controller.NextSearch < DateTime.Now)
						{
							Log("Searching for: {0}", controller.GetType().Name);
							controller.Search();
						}
					}
				}
				catch (Exception exception)
				{
				//	if (NatUtility.UnhandledException != null)
					//{
					//	NatUtility.UnhandledException(typeof(NatUtility), new UnhandledExceptionEventArgs(exception, false));
					//}
				}
				Thread.Sleep(10);
			}
		}

		private static void Receive(ISearcher searcher, List<UdpClient> clients)
		{
			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 5351);
			foreach (UdpClient client in clients)
			{
				if (client.Available > 0)
				{
					IPAddress address = ((IPEndPoint)client.Client.LocalEndPoint).Address;
					byte[] response = client.Receive(ref remoteEP);
					searcher.Handle(address, response, remoteEP);
				}
			}
		}

		public static void StartDiscovery()
		{
			searching.Set();
		}

		public static void StopDiscovery()
		{
			searching.Reset();
		}

		[Obsolete("This method serves no purpose and shouldn't be used")]
		public static IPAddress[] GetLocalAddresses(bool includeIPv6)
		{
			List<IPAddress> list = new List<IPAddress>();
			IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress[] addressList = hostEntry.AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork || (includeIPv6 && iPAddress.AddressFamily == AddressFamily.InterNetworkV6))
				{
					list.Add(iPAddress);
				}
			}
			return list.ToArray();
		}

		public static bool IsPrivateAddressSpace(IPAddress address)
		{
			byte[] addressBytes = address.GetAddressBytes();
			switch (addressBytes[0])
			{
			case 10:
				return true;
			case 172:
				return (addressBytes[1] & 0x10) != 0;
			case 192:
				return addressBytes[1] == 168;
			default:
				return false;
			}
		}

		static NatUtility()
		{
			searching = new ManualResetEvent(false);
			controllers = new List<ISearcher>();
			controllers.Add(UpnpSearcher.Instance);
			controllers.Add(PmpSearcher.Instance);
			foreach (ISearcher controller in controllers)
			{
				controller.DeviceFound += delegate(object sender, DeviceEventArgs args)
				{
					//if (NatUtility.DeviceFound != null)
					//{
				//		NatUtility.DeviceFound(sender, args);
					//}
				};
				controller.DeviceLost += delegate(object sender, DeviceEventArgs args)
				{
					//if (NatUtility.DeviceLost != null)
					//{
					//	NatUtility.DeviceLost(sender, args);
					//}
				};
			}
			Thread thread = new Thread((ThreadStart)delegate
			{
				SearchAndListen();
			});
			thread.IsBackground = true;
			thread.Start();
		}
	}
}
