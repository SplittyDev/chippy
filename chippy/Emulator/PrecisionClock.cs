using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace chippy
{
	/// <summary>
	/// Precision clock.
	/// </summary>
	public class PrecisionClock
	{
		int frequency, cycles;
		long timer, lastTime;
		double delta, ticksPerCycle;
		bool running;
		Task worker;
		CancellationTokenSource tksrc;
		CancellationToken tk;

		public delegate void TickEventHandler (int cycles);
		public event TickEventHandler OnTick;
		public event TickEventHandler OnSecondPassed;

		/// <summary>
		/// Initializes a new instance of the <see cref="chippy.PrecisionClock"/> class.
		/// </summary>
		/// <param name="frequency">Frequency.</param>
		public PrecisionClock (int frequency) {
			
			this.frequency = frequency;
		}

		/// <summary>
		/// Resets the clock.
		/// </summary>
		public void Reset () {
			
			lastTime = Stopwatch.GetTimestamp ();
			timer = lastTime;
			ticksPerCycle = Stopwatch.Frequency / frequency;
		}

		/// <summary>
		/// Starts the clock.
		/// </summary>
		public Task Start () {

			if (running)
				return null;

			running = true;

			tksrc = new CancellationTokenSource ();
			tk = tksrc.Token;

			worker = Task.Factory.StartNew (() => {

				tk.ThrowIfCancellationRequested ();

				Reset ();
				long now;

				while (true) {

					if (tk.IsCancellationRequested)
						tk.ThrowIfCancellationRequested ();
					
					now = Stopwatch.GetTimestamp ();
					delta += (now - lastTime) / ticksPerCycle;
					lastTime = now;

					while (delta >= 1) {
						delta--;
						cycles++;
						OnTick (cycles);
					}

					if (Stopwatch.GetTimestamp () - timer > Stopwatch.Frequency) {
						timer += Stopwatch.Frequency;
						OnSecondPassed (cycles);
						cycles = 0;
					}
				}
			}, tksrc.Token);

			return worker;
		}

		/// <summary>
		/// Stops the clock.
		/// </summary>
		public void Stop () {
			
			if (!running || tksrc == null)
				return;

			tksrc.Cancel ();
			try {
				worker.Wait ();
			} catch (AggregateException) {
			} finally {
				tksrc.Dispose ();
				running = false;
			}
		}
	}
}

