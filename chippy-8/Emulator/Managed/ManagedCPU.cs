using System;
using System.Threading;

namespace chippy8
{
	public class ManagedCPU : ICpu
	{
		public int Cycles { get; private set; }

		byte[] V; // Registers
		short I; // Index register
		short PC; // Program counter
		short SB; // Stack base address
		byte SP; // Stack pointer
		byte DT; // Delay timer
		byte ST; // Sound timer
		Random rng;

		short prevPC;
		int loopcount;
		bool endless_loop_msg;
		bool invalid_mem_region_msg;

		public string Identifier { get; } = "CPU";

		public void PreInit () {
			V = new byte[16];
			rng = new Random ();
		}

		public CpuSnapshot Snapshot () {
			return new CpuSnapshot (V, I, PC, SP);
		}

		public void Init () {
			Array.Clear (V, 0, V.Length);
			endless_loop_msg = false;
			invalid_mem_region_msg = false;
			loopcount = 0;
			prevPC = 0;
			Cycles = 0;
			PC = 0x200;
			SB = 0xEA0;
			SP = 0;
			DT = 0;
			ST = 0;
			I = 0;
		}

		public void RunCycle () {

			// Invalid memory region
			if (PC + 0x200 >= 4096) {
				if (!invalid_mem_region_msg) {
					Console.WriteLine ("Invalid memory region.");
					invalid_mem_region_msg = true;
				}
				return;
			}

			// Endless loop
			else if (loopcount > 100) {
				if (!endless_loop_msg) {
					Console.WriteLine ("Endless loop detected.");
					endless_loop_msg = true;
				}
				return;
			}

			ushort instr = (ushort)Emulator.Instance.Memory.Read16 (PC);
			short nnn = (short)(instr & 0x0FFF);
			byte op = (byte)((instr & 0xF000) >> 12);
			byte x = (byte)((instr & 0x0F00) >> 8);
			byte y = (byte)((instr & 0x00F0) >> 4);
			byte n = (byte)(instr & 0x000F);
			byte nn = (byte)(instr & 0x00FF);

			prevPC = PC;
			PC += 2;

			switch (op) {
			// 0x0NNN
			// 0x00E0
			// 0x00EE
			case 0x0:
				// Clears the Emulator.Instance.Screen
				switch (nn) {
				case 0x00:
					break;
				case 0xE0:
					Emulator.Instance.Screen.Clear ();
					Emulator.Instance.Screen.Update ();
					break;
				case 0xEE:
					PC = Emulator.Instance.Memory.Read16 (SB + SP);
					SP -= 2;
					break;
				default:
					Console.WriteLine ("Invalid opcode: 0x{0:X4}", op);
					break;
				}
				break;
			// 0x1NNN
			case 0x1:
				// Jumps to address NNN
				PC = nnn;
				break;
			// 0x2NNN
			case 0x2:
				// Calls subroutine at NNN
				SP += 2;
				Emulator.Instance.Memory.Write16 (SB + SP, PC);
				PC = nnn;
				break;
			// 0x3XNN
			case 0x3:
				// Skips the next instruction if V[X] equals NN
				if (V [x] == nn)
					PC += 2;
				break;
			// 0x4XNN
			case 0x4:
				// Skips the next instruction if V[X] does not equal NN
				if (V [x] != nn)
					PC += 2;
				break;
			// 0x5XY0
			case 0x5:
				// Skips the next instruction if V[X] equals V[Y]
				if (V [x] == V [y])
					PC += 2;
				break;
			// 0x6XNN
			case 0x6:
				// Sets V[X] to NN
				V [x] = nn;
				break;
			// 0x7XNN
			case 0x7:
				// Adds NN to V[X]
				V [x] += nn;
				break;
			// 0x8XY_
			case 0x8:
				switch (n) {
				// 0x8XY0
				case 0x0:
					// Sets V[X] to the value of V[Y]
					V [x] = V [y];
					break;
				// 0x8XY1
				case 0x1:
					// Sets V[X] to V[X] or V[Y]
					V [x] |= V [y];
					break;
				// 0x8XY2
				case 0x2:
					// Sets V[X] to V[X] and V[Y]
					V [x] &= V [y];
					break;
				// 0x8XY3
				case 0x3:
					// Sets V[X] to V[X] xor V[Y]
					V [x] ^= V [y];
					break;
				// 0x8XY4
				case 0x4:
					// Adds V[Y] to V[X]
					// V[0xF] is set to 1 when there is a carry,
					// and to 0 when there is not
					V [0xF] = 0;
					if ((V[x] + V[y]) > 0xFF)
						V [0xF] = 1;
					V [x] = (byte)((V [x] + V [y]) & 0xFF);
					break;
				// 0x8XY5
				case 0x5:
					// V[Y] is subtracted from V[X]
					// V[0xF] is set to 0 when there is a borrow,
					// and 1 when there is not
					V [0xF] = 0;
					if (V [x] > V [y])
						V [0xF] = 1;
					V [x] = (byte)((V [x] - V [y]) & 0xFF);
					break;
				// 0x8XY6
				case 0x6:
					// Shifts V[X] right by one
					// V[0xF] is set to the value of the least
					// significant bit of V[X] before the shift
					V [0xF] = (byte)(V[x] & 1);
					V [x] >>= 1;
					break;
				// 0x8XY7
				case 0x7:
					// Sets V[X] to V[Y] minus V[X]
					// V[0xF] is set to 0 when there is a borrow,
					// and 1 when there is not
					V [0xF] = 0;
					if (V [y] > V [x])
						V [0xF] = 1;
					V [x] = (byte)((V [y] - V [x]) & 0xFF);
					break;
				// 0x8XYE
				case 0xE:
					// Shifts V[X] left by one
					// V[0xF] is set to the value of the most
					// significant bit of V[X] before the shift
					V [0xF] = (byte)((V [x] & 128) >> 7);
					V [x] <<= 1;
					break;
				default:
					Console.WriteLine ("Invalid opcode: 0x{0:X4}", op);
					break;
				}
				break;
			// 0x9XY0
			case 0x9:
				// Skips the next instruction if V[X] does not equal V[Y]
				if (V [x] != V [y])
					PC += 2;
				break;
			// 0xANNN
			case 0xA:
				// Sets I to the address NNN
				I = nnn;
				break;
			// 0xBNNN
			case 0xB:
				// Jumps to the address NNN plus V[0x0]
				PC = (short)(nnn + V [0x0]);
				break;
			// 0xCXNN
			case 0xC:
				// Sets V[X] to a random number, masked by NN
				V [x] = (byte)(rng.Next (0, 0xFF) & nn);
				break;
			// 0xDXYN
			case 0xD:
				// Sprites stored in Emulator.Instance.Memoryory at location in
				// index register I, maximum 8 bits wide
				// Wraps around the Emulator.Instance.Screen
				// When drawn, clears a pixel and register VF is set to 1; otherwise it is zero
				// All drawing is XOR drawing (i.e. it toggles the Emulator.Instance.Screen pixels)
				V[0xF] = 0;
				byte px, py = 0;
				for (int yy = 0; yy < n; yy++) {
					py = (byte)((V[y] + yy) % 32);
					if (py < 0 || py > 32)
						continue;
					for (int xx = 0; xx < 8; xx++) {
						px = (byte)((V[x] + xx) % 64);
						if (px < 0 || px > 64)
							continue;
						byte color = (byte)((Emulator.Instance.Memory [I + yy] >> (7 - xx)) & 1);
						byte col = Emulator.Instance.Screen [px + py * 64];
						if (color > 0 && col > 0)
							V[0xF] = 1;
						color ^= col;
						Emulator.Instance.Screen [px + py * 64] = color;
					}
				}
				Emulator.Instance.Screen.Update ();
				break;
			// 0xEX__
			case 0xE:
				switch (nn) {
				// 0xEX9E
				case 0x9E:
					// Skips the next instruction if the key stored in V[X] is pressed
					if (Emulator.Instance.Keypad.CheckKey (V [x]))
						PC += 2;
					break;
				// 0xEXA1
				case 0xA1:
					// Skips the next instruction if the key stored in V[X] isn't pressed
					if (!Emulator.Instance.Keypad.CheckKey (V [x]))
						PC += 2;
					break;
				default:
					Console.WriteLine ("Invalid opcode: 0x{0:X4}", op);
					break;
				}
				break;
			// 0xF__
			case 0xF:
				switch (nn) {
				case 0x07:
					// Sets V[X] to the value of the delay timer
					V [x] = DT;
					break;
				case 0x0A:
					// A key press is awaited, and then stored in V[X]
					var key = Emulator.Instance.Keypad.Await ();
					V [x] = (byte)key;
					break;
				case 0x15:
					// Sets the delay timer to V[X]
					DT = V [x];
					break;
				case 0x18:
					// Sets the sound timer to V[X]
					ST = V [x];
					break;
				case 0x1E:
					// Adds V[X] to I
					I += V [x];
					break;
				case 0x29:
					// Sets I to the location of the sprite for the character in V[X]
					// Characters 0-F (in hexadecimal) are represented by a 4x5 font
					I = (short)(V [x] * 5);
					break;
				case 0x33:
					// Stores the Binary-coded decimal representation of V[X],
					// with the most significant of three digits at the address in I,
					// the middle digit at I plus 1, and the least significant digit at I plus 2.
					// (In other words, take the decimal representation of VX,
					// place the hundreds digit in Emulator.Instance.Memoryory at location in I,
					// the tens digit at location I+1, and the ones digit at location I+2.)
					Emulator.Instance.Memory[I] = (byte)(V [x] / 100);
					Emulator.Instance.Memory[I + 1] = (byte)((V [x] / 10) % 10);
					Emulator.Instance.Memory[I + 2] = (byte)((V [x] / 100) % 10);
					break;
				case 0x55:
					// Stores V[0] to V[X] in Emulator.Instance.Memoryory starting at address I
					for (var i = 0; i <= x; i++)
						Emulator.Instance.Memory.Write8 (I + i, V [i]);
					break;
				case 0x65:
					// Fills V[0] to V[X] with values from Emulator.Instance.Memoryory starting at address I
					for (var i = 0; i <= x; i++)
						V [i] = Emulator.Instance.Memory [I + i];
					break;
				default:
					Console.WriteLine ("Invalid opcode: 0x{0:X4}", op);
					break;
				}
				break;
			default:
				Console.WriteLine ("Invalid opcode: 0x{0:X4}", op);
				break;
			}

			if (prevPC == PC)
				loopcount++;

			if (DT > 0)
				DT--;

			if (ST > 0) {
				ST--;
				Console.Beep ();
			}

			++Cycles;
			Emulator.Instance.Screen.Draw ();
		}

		public void ClearRegisters () {
			V.Initialize ();
			Console.WriteLine ("Zero-filled registers.");
		}

		public void ClearFlags () {
			Console.WriteLine ("Cleared flags.");
		}
	}
}

