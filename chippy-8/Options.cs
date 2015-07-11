using System;
using Codeaddicts.libArgument.Attributes;

namespace chippy8
{
	public class Options
	{
		[Argument ("i", "input")]
		[Docs ("Input file")]
		public string input;

		[Switch ("h", "help")]
		[Docs ("Show this help")]
		public bool help;
	}
}

