using System;

namespace Mono.Nat
{
	public class DeviceEventArgs : EventArgs
	{
		private INatDevice device;

		public INatDevice Device
		{
			get
			{
				return device;
			}
		}

		public DeviceEventArgs(INatDevice device)
		{
			this.device = device;
		}
	}
}
