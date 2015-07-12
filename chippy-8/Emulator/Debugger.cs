﻿using System;
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

		delegate void MMTimerProc (UInt32 timerid, UInt32 msg, IntPtr user, UInt32 dw1, UInt32 dw2);
		bool running;
		bool halted;
		double frequency = 0;

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
				double frequency = 1000d / value;
				if (continue_after)
					Continue ();
			}
		}

		[DllImport ("winmm.dll")]
		static extern uint timeSetEvent (
			UInt32 uDelay,
			UInt32 uResolution,
			[MarshalAs (UnmanagedType.FunctionPtr)]
			MMTimerProc lpTimeProc,  
			UInt32 dwUser,      
			Int32 fuEvent      
		);

		[DllImport ("winmm.dll")]
		static extern uint timeKillEvent (uint uTimerID);

		[DllImport ("kernel32.dll")]
		static extern bool QueryPerformanceCounter (out long PerformanceCount);

		[DllImport ("kernel32.dll")]
		static extern bool QueryPerformanceFrequency (out long Frequency);

		Debugger () {
			Frequency = 60; // 60hz
		}

		public void Pause () {
			var wait = running;
			running = false;
			if (wait) {
				while (!halted) {
				}
			}
		}

		public void Continue () {
			running = true;
		}

		public void Stop () {
			Pause ();
			Emulator.Instance.ReinitRun ();
		}

		public void Step () {
			Emulator.Instance.RunCycle ();
		}

		public void Restart () {
			Stop ();
			InternalRun ();
		}

		public void Run () {
			Stop ();
			InternalRun ();
		}

		void InternalRun () {
			halted = false;
			running = true;
			Stopwatch watch = new Stopwatch ();
			Task.Factory.StartNew (() => {
				bool stopped = false;
				Emulator.Instance.InitRun ();
				while (running) {
					if (stopped && running) {
						stopped = false;
						Emulator.Instance.InitRun ();
					}
					if (running) {
						Emulator.Instance.BlindRunCycle ();
					} else
						stopped = true;
					Thread.Sleep (1);
				}
				halted = true;
			});
		}
	}
}

