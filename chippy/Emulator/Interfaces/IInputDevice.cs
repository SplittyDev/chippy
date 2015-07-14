using System;

namespace chippy
{
	public interface IInputDevice : IComponent
	{
		ushort Await ();
		void Send (ushort pos);
		bool CheckKey (ushort pos);
		bool KeyAvailable ();
	}
}

