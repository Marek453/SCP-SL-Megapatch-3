using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Mono.Nat.Pmp
{
	internal sealed class PmpNatDevice : AbstractNatDevice, IEquatable<PmpNatDevice>
	{
		private class CreatePortMapAsyncState
		{
			internal byte[] Buffer;

			internal ManualResetEvent ResetEvent = new ManualResetEvent(false);

			internal Mapping Mapping;

			internal bool Success;
		}

		private class CreatePortMapListenState
		{
			internal volatile bool Success;

			internal Mapping Mapping;

			internal UdpClient UdpClient;

			internal ManualResetEvent UdpClientReady;

			internal CreatePortMapListenState(CreatePortMapAsyncState state, UdpClient client)
			{
				Mapping = state.Mapping;
				UdpClient = client;
				UdpClientReady = new ManualResetEvent(false);
			}
		}

		private AsyncResult externalIpResult;

		private bool pendingOp;

		private IPAddress localAddress;

		private IPAddress publicAddress;

		internal IPAddress LocalAddress
		{
			get
			{
				return localAddress;
			}
		}

		internal PmpNatDevice(IPAddress localAddress, IPAddress publicAddress)
		{
			this.localAddress = localAddress;
			this.publicAddress = publicAddress;
		}

		public override IPAddress GetExternalIP()
		{
			return publicAddress;
		}

		public override IAsyncResult BeginCreatePortMap(Mapping mapping, AsyncCallback callback, object asyncState)
		{
			PortMapAsyncResult pmar = new PortMapAsyncResult(mapping.Protocol, mapping.PublicPort, 3600, callback, asyncState);
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					CreatePortMap(pmar.Mapping, true);
					pmar.Complete();
				}
				catch (Exception ex)
				{
					pmar.Complete(ex);
				}
			});
			return pmar;
		}

		public override IAsyncResult BeginDeletePortMap(Mapping mapping, AsyncCallback callback, object asyncState)
		{
			PortMapAsyncResult pmar = new PortMapAsyncResult(mapping, callback, asyncState);
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					CreatePortMap(pmar.Mapping, false);
					pmar.Complete();
				}
				catch (Exception ex)
				{
					pmar.Complete(ex);
				}
			});
			return pmar;
		}

		public override void EndCreatePortMap(IAsyncResult result)
		{
			PortMapAsyncResult portMapAsyncResult = result as PortMapAsyncResult;
			portMapAsyncResult.AsyncWaitHandle.WaitOne();
		}

		public override void EndDeletePortMap(IAsyncResult result)
		{
			PortMapAsyncResult portMapAsyncResult = result as PortMapAsyncResult;
			portMapAsyncResult.AsyncWaitHandle.WaitOne();
		}

		public override IAsyncResult BeginGetAllMappings(AsyncCallback callback, object asyncState)
		{
			throw new NotSupportedException();
		}

		public override IAsyncResult BeginGetExternalIP(AsyncCallback callback, object asyncState)
		{
			StartOp(ref externalIpResult, callback, asyncState);
			AsyncResult asyncResult = externalIpResult;
			asyncResult.Complete();
			return asyncResult;
		}

		public override IAsyncResult BeginGetSpecificMapping(Protocol protocol, int port, AsyncCallback callback, object asyncState)
		{
			throw new NotSupportedException();
		}

		public override Mapping[] EndGetAllMappings(IAsyncResult result)
		{
			throw new NotSupportedException();
		}

		public override IPAddress EndGetExternalIP(IAsyncResult result)
		{
			EndOp(result, ref externalIpResult);
			return publicAddress;
		}

		private void StartOp(ref AsyncResult result, AsyncCallback callback, object asyncState)
		{
			if (pendingOp)
			{
				throw new InvalidOperationException("Can only have one simultaenous async operation");
			}
			pendingOp = true;
			result = new AsyncResult(callback, asyncState);
		}

		private void EndOp(IAsyncResult supplied, ref AsyncResult actual)
		{
			if (supplied == null)
			{
				throw new ArgumentNullException("result");
			}
			if (supplied != actual)
			{
				throw new ArgumentException("Supplied IAsyncResult does not match the stored result");
			}
			if (!supplied.IsCompleted)
			{
				supplied.AsyncWaitHandle.WaitOne();
			}
			if (actual.StoredException != null)
			{
				throw actual.StoredException;
			}
			pendingOp = false;
			actual = null;
		}

		public override Mapping EndGetSpecificMapping(IAsyncResult result)
		{
			throw new NotSupportedException();
		}

		public override bool Equals(object obj)
		{
			PmpNatDevice pmpNatDevice = obj as PmpNatDevice;
			return pmpNatDevice != null && Equals(pmpNatDevice);
		}

		public override int GetHashCode()
		{
			return publicAddress.GetHashCode();
		}

		public bool Equals(PmpNatDevice other)
		{
			return other != null && publicAddress.Equals(other.publicAddress);
		}

		private Mapping CreatePortMap(Mapping mapping, bool create)
		{
			List<byte> list = new List<byte>();
			list.Add(0);
			list.Add((byte)((mapping.Protocol != 0) ? 1 : 2));
			list.Add(0);
			list.Add(0);
			list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mapping.PrivatePort)));
			list.AddRange(BitConverter.GetBytes((short)(create ? IPAddress.HostToNetworkOrder((short)mapping.PublicPort) : 0)));
			list.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(mapping.Lifetime)));
			CreatePortMapAsyncState createPortMapAsyncState = new CreatePortMapAsyncState();
			createPortMapAsyncState.Buffer = list.ToArray();
			createPortMapAsyncState.Mapping = mapping;
			ThreadPool.QueueUserWorkItem(CreatePortMapAsync, createPortMapAsyncState);
			WaitHandle.WaitAll(new WaitHandle[1] { createPortMapAsyncState.ResetEvent });
			if (!createPortMapAsyncState.Success)
			{
				string arg = ((!create) ? "delete" : "create");
				throw new MappingException(string.Format("Failed to {0} portmap (protocol={1}, private port={2}", arg, mapping.Protocol, mapping.PrivatePort));
			}
			return createPortMapAsyncState.Mapping;
		}

		private void CreatePortMapAsync(object obj)
		{
			CreatePortMapAsyncState createPortMapAsyncState = obj as CreatePortMapAsyncState;
			UdpClient udpClient = new UdpClient();
			CreatePortMapListenState createPortMapListenState = new CreatePortMapListenState(createPortMapAsyncState, udpClient);
			int num = 0;
			int num2 = 250;
			ThreadPool.QueueUserWorkItem(CreatePortMapListen, createPortMapListenState);
			while (num < 9 && !createPortMapListenState.Success)
			{
				udpClient.Send(createPortMapAsyncState.Buffer, createPortMapAsyncState.Buffer.Length, new IPEndPoint(localAddress, 5351));
				createPortMapListenState.UdpClientReady.Set();
				num++;
				num2 *= 2;
				Thread.Sleep(num2);
			}
			createPortMapAsyncState.Success = createPortMapListenState.Success;
			udpClient.Close();
			createPortMapAsyncState.ResetEvent.Set();
		}

		private void CreatePortMapListen(object obj)
		{
			CreatePortMapListenState createPortMapListenState = obj as CreatePortMapListenState;
			UdpClient udpClient = createPortMapListenState.UdpClient;
			createPortMapListenState.UdpClientReady.WaitOne();
			IPEndPoint remoteEP = new IPEndPoint(localAddress, 5351);
			while (!createPortMapListenState.Success)
			{
				byte[] array;
				try
				{
					array = udpClient.Receive(ref remoteEP);
				}
				catch (SocketException)
				{
					createPortMapListenState.Success = false;
					break;
				}
				if (array.Length >= 16 && array[0] == 0)
				{
					byte b = (byte)(array[1] & 0x7Fu);
					Protocol protocol = Protocol.Tcp;
					if (b == 1)
					{
						protocol = Protocol.Udp;
					}
					short num = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(array, 2));
					uint num2 = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(array, 4));
					int num3 = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(array, 8));
					int publicPort = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(array, 10));
					uint num4 = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(array, 12));
					if (num != 0)
					{
						createPortMapListenState.Success = false;
						break;
					}
					if (num4 == 0)
					{
						createPortMapListenState.Success = true;
						createPortMapListenState.Mapping = null;
						break;
					}
					Mapping mapping = createPortMapListenState.Mapping;
					mapping.PublicPort = publicPort;
					mapping.Protocol = protocol;
					mapping.Expiration = DateTime.Now.AddSeconds(num4);
					createPortMapListenState.Success = true;
				}
			}
		}

		public override string ToString()
		{
			return string.Format("PmpNatDevice - Local Address: {0}, Public IP: {1}, Last Seen: {2}", localAddress, publicAddress, base.LastSeen);
		}
	}
}
