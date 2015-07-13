using System;
using System.Collections.Generic;

namespace chippy8
{
	public interface IMemory : IComponent
	{
		byte this [int i] { get; set; }
		void Clear ();
		void Load (byte[] rom);
		byte[] Dump ();
		byte[] DumpRom ();
		void Write8 (int addr, byte val);
		byte Read8 (int addr);
		void Write16 (int addr, short val);
		short Read16 (int addr);
	}
}

