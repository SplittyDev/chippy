using System;
using System.Windows.Forms;

namespace chippy
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			using (var frm = new MainForm ())
				Application.Run (frm);
		}
	}
}
