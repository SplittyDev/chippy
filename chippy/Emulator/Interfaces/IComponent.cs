using System;

namespace chippy
{
	public interface IComponent
	{
		string Identifier { get; }
		void PreInit ();
		void Init ();
	}
}

