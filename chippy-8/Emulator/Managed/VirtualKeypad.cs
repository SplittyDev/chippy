using System;

namespace chippy8
{
	public class VirtualKeypad : IInputDevice
	{
		short buf;

		public string Identifier { get; } = "Virtual keypad";

		public void PreInit () {
			buf = -1;
		}

		public void Init () {
			buf = -1;
		}

		public ushort Await () {
			while (buf < 0);
			var tmp = buf;
			buf = -1;
			return (ushort)tmp;
		}

		public void Send (ushort key) {
			buf = (short)key;
		}
	}
}

