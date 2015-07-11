using System;

namespace chippy8
{
	public interface ICpu : IComponent
	{
		int Cycles { get; }
		void RunCycle ();
		void ClearRegisters ();
		void ClearFlags ();
	}
}

