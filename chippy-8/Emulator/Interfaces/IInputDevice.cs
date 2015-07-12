using System;

namespace chippy8
{
	public interface IInputDevice : IComponent
	{
		ushort Await ();
		void Send (ushort pos);
	}
}

