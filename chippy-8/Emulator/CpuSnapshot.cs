using System;

namespace chippy8
{
	public class CpuSnapshot
	{
		public byte V0, V1, V2, V3, V4, V5, V6, V7;
		public byte V8, V9, VA, VB, VC, VD, VE;
		public byte Carry;
		public short I;
		public short PC;
		public short SP;

		public CpuSnapshot (byte[] V, short I, short PC, short SP) {
			V0 = V [0x0];
			V1 = V [0x1];
			V2 = V [0x2];
			V3 = V [0x3];
			V4 = V [0x4];
			V5 = V [0x5];
			V6 = V [0x6];
			V7 = V [0x7];
			V8 = V [0x8];
			V9 = V [0x9];
			VA = V [0xA];
			VB = V [0xB];
			VC = V [0xC];
			VD = V [0xD];
			VE = V [0xE];
			Carry = V [0xF];
			this.I = I;
			this.PC = PC;
			this.SP = SP;
		}
	}
}

