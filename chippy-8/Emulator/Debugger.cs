using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace chippy8
{
	public class Debugger
	{
		static object syncLock = new object ();
		static Debugger instance;

		bool running;
		bool halted;
		bool initialized;
		double frequency;
		int count;

		public static Debugger Instance {
			get {
				if (instance == null)
					lock (syncLock)
						if (instance == null)
							instance = new Debugger ();
				return instance;
			}
		}

		public double Frequency {
			get { return frequency; }
			set {
				bool continue_after = !!running;
				if (running) {
					continue_after = true;
					Pause ();
				}
				frequency = value;
				if (continue_after)
					Continue ();
			}
		}

		Debugger () {
			Frequency = 4000;
		}

		void Initialize () {
			if (initialized)
				return;
			try {
				Emulator.Instance.InitRun (clear_memory: false, set_run_flag: false);
				initialized = true;
			} catch {
				initialized = false;
			}
		}

		public void Pause () {
			Initialize ();
			if (!initialized)
				return;
			var wait = running;
			running = false;
			if (wait)
				while (!halted);
		}

		public void Continue () {
			Initialize ();
			if (!initialized)
				return;
			if (!running)
				InternalRun ();
		}

		public void Stop () {
			Initialize ();
			if (!initialized)
				return;
			Pause ();
			Emulator.Instance.ReinitRun (clear_memory: false);
			Emulator.Instance.Screen.Draw ();
		}

		public void Step () {
			Initialize ();
			if (!initialized)
				return;
			Emulator.Instance.BlindRunCycle ();
		}

		public void Restart () {
			Initialize ();
			if (!initialized)
				return;
			Pause ();
			Emulator.Instance.ReinitRun (clear_memory: false);
			InternalRun ();
		}

		public void Run () {
			Initialize ();
			if (!initialized)
				return;
			if (!running)
				InternalRun ();
		}

		void InternalRun () {
			halted = false;
			running = true;
			Task.Factory.StartNew (() => {
				bool stopped = false;
				Emulator.Instance.InitRun ();

				Stopwatch sw = Stopwatch.StartNew ();
				while (running) {
					if (frequency == 0 || (double)((double)sw.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond) > (1000d / frequency)) {
						if (stopped && running) {
							stopped = false;
							Emulator.Instance.InitRun ();
						}
						if (running) {
							Emulator.Instance.BlindRunCycle ();
						} else
							stopped = true;
						sw.Restart ();
					}
				}

				halted = true;
			});
		}
	}
}

