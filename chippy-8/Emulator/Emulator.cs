using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace chippy8
{
	public class Emulator
	{
		static object syncLock = new object ();
		static Emulator instance;

		public static Emulator Instance {
			get {
				if (instance == null)
					lock (syncLock)
						if (instance == null)
							instance = new Emulator ();
				return instance;
			}
		}

		public IMemory Memory;
		public ICpu Cpu;
		public IScreen Screen;
		public IHardwareRegister DelayTimer;
		public IHardwareRegister SoundTimer;

		bool stop;
		bool running;
		bool initialized;

		Emulator () {
			DelayTimer = new ManagedDelayTimer ();
			SoundTimer = new ManagedSoundTimer ();
		}

		public void Load (byte[] rom) {
			if (Memory == null)
				throw new Exception ("Please connect a IMemory instance!");
			Memory.Load (rom);
		}

		public byte[] DumpRam () {
			if (Memory == null)
				throw new Exception ("Please connect a IMemory instance!");
			return Memory.Dump ();
		}

		public byte[] DumpRom () {
			if (Memory == null)
				throw new Exception ("Please connect a IMemory instance!");
			return Memory.DumpRom ();
		}

		public void SoftReset () {
			Console.WriteLine ("Performing soft-reset...");
			Stop ();
			Memory.Clear ();
			Cpu.ClearRegisters ();
			Cpu.ClearFlags ();
			Console.WriteLine ("Done.");
		}

		public void HardReset () {
			Console.WriteLine ("Performing hard-reset...");
			instance = null;
			Console.WriteLine ("Done. Please refresh your reference to the Emulator singleton.");
		}

		public void RunTask () {
			Task.Factory.StartNew (Run);
		}

		public void InitRun (bool clear_memory = true, bool set_run_flag = true) {
			stop = false;
			running = false;

			// Null-checking
			if (Memory == null)
				throw new Exception ("Please connect a IMemory instance!");
			if (Cpu == null)
				throw new Exception ("Please connect a ICPU instance!");
			if (Screen == null)
				throw new Exception ("Please connect a IDisplay instance!");

			// Initialization
			if (!initialized) {
				Cpu.Init ();
				if (clear_memory)
					Memory.Init ();
				Screen.Init ();
				DelayTimer.Init ();
				SoundTimer.Init ();
				initialized = true;
			}

			if (set_run_flag)
				running = true;
		}

		public void ReinitRun (bool clear_memory = true) {
			initialized = false;
			InitRun (clear_memory);
			running = false;
		}

		public void Run () {
			InitRun ();
			while (!stop) {
				Cpu.RunCycle ();
			}

			running = false;
		}

		public void RunCycle () {
			InitRun ();
			Cpu.RunCycle ();
			running = false;
		}

		public void BlindRunCycle () {
			running = true;
			Cpu.RunCycle ();
			running = false;
		}

		public void Stop () {
			if (!running)
				return;
			stop = true;
			var success = false;
			Task.Factory.StartNew (() => {
				while (running)
					;
				success = true;
			}).Wait (2000);
			if (!success) {
				Console.WriteLine ("Could not stop the CPU!");
				Console.WriteLine ("Performing hard reset...");
				HardReset ();
			} else
				Console.WriteLine ("CPU stopped.");
		}
	}
}

