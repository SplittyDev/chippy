using System;

namespace chippy8
{
	public class ManagedSoundTimer : IHardwareRegister
	{
		byte val;

		public string Identifier { get; } = "Sound timer";

		public void PreInit () {
		}

		public void Init () {
		}
	}
}

