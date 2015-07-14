using System;

namespace chippy
{
	public class VirtualKeypad : IInputDevice
	{
		byte[] keys;
		short buf;

		public string Identifier { get; } = "Virtual keypad";

		public void PreInit () {
			keys = new byte[16];
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
			SetKey (key);
		}

		public void SetKey (ushort key) {
			buf = (short)key;
			keys [key] = 1;
		}

		public void UnsetKey (ushort key) {
			keys [key] = 0;
		}

		public bool CheckKey (ushort pos) {
			return keys [pos] == 1;
		}

		public bool KeyAvailable () {
			return buf >= 0;
		}
	}
}

