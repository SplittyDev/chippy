using System;

namespace chippy8
{
	public class ManagedDelayTimer : IHardwareRegister
	{
		byte val;

		public string Identifier { get; } = "Delay timer";

		public void PreInit () {
		}

		public void Init () {
		}
	}
}

