using System;

namespace chippy8
{
	public class VirtualKeypad : IInputDevice
	{
		byte[] keys;
		short buf;

		public string Identifier { get; } = "Virtual keypad";

		public void PreInit () {
			keys = new byte[16];
			buf = -1;
		}

		public void Init () {
			Array.Clear (keys, 0, keys.Length);
			buf = -1;
		}

		public ushort Await () {
			if (buf == -1) {
				Console.WriteLine ("No key available!");
				return 0;
			}
			var tmp = buf;
			buf = -1;
			return (ushort)tmp;
		}

		public void Send (ushort key) {
			buf = (short)key;
		}

		public bool CheckKey (ushort pos) {
			return keys [pos] != 0;
		}

		public bool KeyAvailable () {
			return buf >= 0;
		}
	}
}

