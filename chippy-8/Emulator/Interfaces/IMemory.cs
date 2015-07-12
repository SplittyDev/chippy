using System;
using System.Collections.Generic;

namespace chippy8
{
	public interface IMemory : IComponent
	{
		byte this [ushort i] { get; set; }
		void Clear ();
		void Load (byte[] rom);
		byte[] Dump ();
		byte[] DumpRom ();
		void Write8 (ushort addr, byte val);
		byte Read8 (ushort addr);
		void Write16 (ushort addr, ushort val);
		ushort Read16 (ushort addr);
	}
}

