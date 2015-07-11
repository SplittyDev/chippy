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
		RichTextBox rtb;

		public override Encoding Encoding { get; } = Encoding.ASCII;

		public ControlWriter (RichTextBox rtb) {
			this.rtb = rtb;
		}

		public override void Write (char value) {
			if (value != '\n')
				linebuf += value;
			else
				Task.Factory.StartNew (() => {
					while (!rtb.IsHandleCreated) {
					}
					rtb.Invoke (new MethodInvoker (() => {
						rtb.Text += linebuf;
						linebuf = "";
						FixNewlines ();
					}));
				});
		}

		public override void Write (string value) {
		}

		void FixNewlines () {
			rtb.Text = rtb.Text.Replace ("\n\n", "\n");
			Scroll ();
		}

		void Scroll () {
			rtb.SelectionStart = rtb.Text.Length;
			rtb.ScrollToCaret ();
		}
	}
}

