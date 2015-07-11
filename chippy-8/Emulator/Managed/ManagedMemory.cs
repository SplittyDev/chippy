using System;

namespace chippy8
{
	public class ManagedMemory : IComponent, IMemory
	{
		byte[] mem;

		public string Identifier { get; } = "Memory";

		public byte this [ushort i] {
			get { return mem [i]; }
			private set { mem [i] = value; }
		}

		public void PreInit () {
			this.mem = new byte[4096];
		}

		public void Init () {
			this.Clear ();
		}

		public void Load (byte[] rom) {
			Clear ();
			Array.Copy (rom, 0, mem, 0x200, rom.Length <= 3584 ? rom.Length : 3584);
		}

		public void Clear () {
			mem.Initialize ();
			Console.WriteLine ("Zero-filled memory.");
		}

		public byte[] Dump () {
			return mem;
		}

		public byte[] DumpRom () {
			var tmp = new byte[3584];
			Array.Copy (mem, 0x200, tmp, 0, 3584);
			return tmp;
		}

		public byte Read8 (ushort addr) {
			return this [addr];
		}

		public ushort Read16 (ushort addr) {
			return (ushort)(mem[addr] << 8 | mem[(ushort)(addr + 1)]);
		}

		public void Write8 (ushort addr, byte val) {
			this [addr] = val;
		}

		public void Write16 (ushort addr, ushort val) {
			this [addr] = (byte)((val >> 8) & 0xFF);
			this [(ushort)(addr + 1)] = (byte)(val & 0xFF);
		}

		static byte[] font = new byte[80] { 
			0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
			0x20, 0x60, 0x20, 0x20, 0x70, // 1
			0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
			0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
			0x90, 0x90, 0xF0, 0x10, 0x10, // 4
			0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
			0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
			0xF0, 0x10, 0x20, 0x40, 0x40, // 7
			0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
			0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
			0xF0, 0x90, 0xF0, 0x90, 0x90, // A
			0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
			0xF0, 0x80, 0x80, 0x80, 0xF0, // C
			0xE0, 0x90, 0x90, 0x90, 0xE0, // D
			0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
			0xF0, 0x80, 0xF0, 0x80, 0x80  // F
		};
	}
}

