using System;

namespace chippy8
{
	public interface IComponent
	{
		string Identifier { get; }
		void PreInit ();
		void Init ();
	}
}

