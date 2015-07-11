using System;

namespace chippy8
{
	public class ManagedScreen : IScreen
	{
		public const byte WIDTH = 64;
		public const byte HEIGHT = 32;

		byte[] vmem;
		protected bool draw;

		public virtual string Identifier { get; } = "Virtual screen";

		public virtual void PreInit () {
			vmem = new byte[WIDTH * HEIGHT];
			draw = false;
		}

		public virtual void Init () {
			Clear ();
		}

		public virtual void Clear () {
			vmem.Initialize ();
			Console.WriteLine ("Cleared screen.");
		}

		public virtual bool CheckPixel (ushort pos) {
			return vmem [pos] == 1;
		}

		public virtual void SetPixel (ushort pos) {
			vmem [pos] ^= 1;
		}

		public virtual void Update () {
			draw = true;
		}

		public virtual void Draw () {
			draw = false;
		}
	}
}

