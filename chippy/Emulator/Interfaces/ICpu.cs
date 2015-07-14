using System;

namespace chippy
{
	public interface ICpu : IComponent
	{
		int Cycles { get; }
		void RunCycle ();
		void ClearRegisters ();
		void ClearFlags ();
		CpuSnapshot Snapshot ();
	}
}

