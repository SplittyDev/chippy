using System;
using System.Threading;

namespace chippy8
{
	public class ManagedCPU : ICpu
	{
		public int Cycles { get; private set; }

		byte[] V; // Registers
		ushort I; // Index register
		ushort PC; // Program counter
		ushort SP; // Stack pointer
		ushort SB; // Stack base address
		Random rng;

		IMemory mem;
		IScreen screen;

		public string Identifier { get; } = "CPU";

		public void PreInit () {
			V = new byte[16];
			rng = new Random ();

		}

		public void Init () {
			Array.Clear (V, 0, V.Length);
			mem = Emulator.Instance.Memory;
			screen = Emulator.Instance.Screen;
			Cycles = 0;
			PC = 0x200;
			SB = 0xEA0;
		}

		public void RunCycle () {
			ushort op, addr, x, y;
			byte n, nn, nibble;

			op = mem.Read16 (PC);
			nibble = (byte)((op & 0xF000) >> 12);
			PC += 2;

			switch (nibble) {
			// 0x0NNN
			// 0x00E0
			// 0x00EE
			case 0x0:
				// Clears the screen
				if (op == 0x00E0)
					screen.Clear ();
				// Returns form subroutine
				else if (op == 0x00EE) {
					SP -= 2;
					PC = mem.Read16 ((ushort)(SB + (SP * 2)));
				}
				break;
			// 0x1NNN
			case 0x1:
				// Jumps to address NNN
				addr = (ushort)(op & 0x0FFF);
				PC = addr;
				break;
			// 0x2NNN
			case 0x2:
				// Calls subroutine at NNN
				addr = (ushort)(op & 0x0FFF);
				mem.Write16 (PC, (ushort)(SB + (SP * 2)));
				SP += 2;
				PC = addr;
				break;
			// 0x3XNN
			case 0x3:
				// Skips the next instruction if V[X] equals NN
				x = (ushort)((op & 0x0F00) >> 8);
				nn = (byte)(op & 0x00FF);
				if (V [x] == nn)
					PC += 2;
				break;
			// 0x4XNN
			case 0x4:
				// Skips the next instruction if V[X] does not equal NN
				x = (ushort)((op & 0x0F00) >> 8);
				nn = (byte)(op & 0x00FF);
				if (V [x] != nn)
					PC += 2;
				break;
			// 0x5XY0
			case 0x5:
				// Skips the next instruction if V[X] equals V[Y]
				x = (ushort)((op & 0x0F00) >> 8);
				y = (ushort)((op & 0x00F0) >> 4);
				if (V [x] == V [y])
					PC += 2;
				break;
			// 0x6XNN
			case 0x6:
				// Sets V[X] to NN
				x = (ushort)((op & 0x0F00) >> 8);
				nn = (byte)(op & 0x00FF);
				V [x] = nn;
				break;
			// 0x7XNN
			case 0x7:
				// Adds NN to V[X]
				x = (ushort)((op & 0x0F00) >> 8);
				nn = (byte)(op & 0x00FF);
				V [x] += nn;
				break;
			// 0x8XY_
			case 0x8:
				x = (ushort)((op & 0x0F00) >> 8);
				y = (ushort)((op & 0x00F0) >> 4);
				n = (byte)(op & 0x000F);
				switch (n) {
				// 0x8XY0
				case 0x0:
					// Sets V[X] to the value of V[Y]
					V [x] = V [y];
					break;
				// 0x8XY1
				case 0x1:
					// Sets V[X] to V[X] or V[Y]
					V [x] = (byte)(V [x] | V [y]);
					break;
				// 0x8XY2
				case 0x2:
					// Sets V[X] to V[X] and V[Y]
					V [x] = (byte)(V [x] & V [y]);
					break;
				// 0x8XY3
				case 0x3:
					// Sets V[X] to V[X] xor V[Y]
					V [x] = (byte)(V [x] ^ V [y]);
					break;
				// 0x8XY4
				case 0x4:
					// Adds V[Y] to V[X]
					// V[0xF] is set to 1 when there is a carry,
					// and to 0 when there is not
					V [0xF] = 0;
					if ((V[x] + V[y]) > 255)
						V [0xF] = 1;
					V [x] += V [y];
					break;
				// 0x8XY5
				case 0x5:
					// V[Y] is subtracted from V[X]
					// V[0xF] is set to 0 when there is a borrow,
					// and 1 when there is not
					V [0xF] = 1;
					if (V [x] < V [y])
						V [0xF] = 0;
					V [x] -= V [y];
					break;
				// 0x8XY6
				case 0x6:
					// Shifts V[X] right by one
					// V[0xF] is set to the value of the least
					// significant bit of V[X] before the shift
					V [0xF] = (byte)(V[x] & 0xFF);
					V [x] >>= 1;
					break;
				// 0x8XY7
				case 0x7:
					// Sets V[X] to V[Y] minus V[X]
					// V[0xF] is set to 0 when there is a borrow,
					// and 1 when there is not
					V [0xF] = 1;
					if (V [x] < (V [y] - V [x]))
						V [0xF] = 0;
					V [x] = (byte)(V [y] - V [x]);
					break;
				// 0x8XYE
				case 0xE:
					// Shifts V[X] left by one
					// V[0xF] is set to the value of the most
					// significant bit of V[X] before the shift
					V [0xF] = (byte)(V [x] & (1 << 7));
					V [x] <<= 1;
					break;
				default:
					Console.WriteLine ("Invalid opcode: 0x8{0:X}{1:X}{2:X}", x, y, n);
					break;
				}
				break;
			// 0x9XY0
			case 0x9:
				// Skips the next instruction if V[X] does not equal V[Y]
				x = (ushort)((op & 0x0F00) >> 8);
				y = (ushort)((op & 0x00F0) >> 4);
				if (V [x] != V [y])
					PC += 2;
				break;
			// 0xANNN
			case 0xA:
				// Sets I to the address NNN
				addr = (ushort)(op & 0x0FFF);
				I = addr;
				break;
			// 0xBNNN
			case 0xB:
				// Jumps to the address NNN plus V[0x0]
				addr = (ushort)(op & 0x0FFF);
				PC = (ushort)(addr + V [0x0]);
				break;
			// 0xCXNN
			case 0xC:
				// Sets V[X] to a random number, masked by NN
				x = (ushort)((op & 0x0F00) >> 8);
				nn = (byte)(op & 0x00FF);
				V [x] = (byte)(rng.Next (0, 0xFF) & nn);
				break;
			// 0xDXYN
			case 0xD:
				// Sprites stored in memory at location in
				// index register I, maximum 8 bits wide
				// Wraps around the screen
				// When drawn, clears a pixel and register VF is set to 1; otherwise it is zero
				// All drawing is XOR drawing (i.e. it toggles the screen pixels)
				x = (ushort)((op & 0x0F00) >> 8);
				y = (ushort)((op & 0x00F0) >> 4);
				n = (byte)(op & 0x000F);

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
						byte color = (byte)((mem [(ushort)(I + yy)] >> (7 - xx)) & 1);
						byte col = screen [(ushort)(px + py * 64)];
						if (color > 0 && col > 0)
							V[0xF] = 1;
						color ^= col;
						screen[(ushort)(px + py * 64)] = color;
					}
				}
				screen.Update ();
				break;
			default:
				Console.WriteLine ("Invalid opcode: 0x{0:X4}", nibble);
				break;
			}

			++Cycles;
			screen.Draw ();
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

