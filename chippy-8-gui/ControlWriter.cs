using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace chippy8gui
{
	public class ControlWriter : TextWriter
	{
		string linebuf;
		TextBox tb;

		public override Encoding Encoding { get; } = Encoding.ASCII;

		public ControlWriter (TextBox tb) {
			this.tb = tb;
		}

		public override void Write (char value) {
			linebuf += value;
			Task.Factory.StartNew (() => {
				while (!tb.IsHandleCreated) {
				}
				tb.Invoke (new MethodInvoker (() => {
					tb.AppendText (linebuf);
					linebuf = "";
					tb.Update ();
				}));
			});
		}

		public override void Write (string value) {
		}
	}
}

