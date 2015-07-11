﻿using System;
using System.IO;

namespace chippy8
{
	public static class EmulatorExtensions
	{
		public static Emulator Connect<TComponent> (this Emulator emulator)
			where TComponent : IComponent, new() {
			var com = new TComponent ();
			com.PreInit ();
			if (com is IMemory)
				emulator.Memory = com as IMemory;
			else if (com is ICpu)
				emulator.Cpu = com as ICpu;
			else if (com is IScreen)
				emulator.Screen = com as IScreen;
			else
				throw new Exception ("Unsupported component.");
			Console.Out.WriteLine ("Connected {0}", com.Identifier);
			return emulator;
		}

		public static Emulator LoadFile (this Emulator emulator, string path) {
			byte[] rom;
			using (var fs = new FileStream (path, FileMode.Open))
			using (var reader = new BinaryReader (fs)) {
				rom = reader.ReadBytes (4096);
			}
			emulator.Memory.Load (rom);
			return emulator;
		}
	}
}

