using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace chippy8
{
	public class WinFormsKeyboard : VirtualKeypad
	{
		Form frm;

		List<char> keymap = new List<char> {
			'x', '1', '2', '3',
			'q', 'w', 'e', 'a',
			's', 'd', 'z', 'c',
			'4', 'r', 'f', 'v'
		};

		public WinFormsKeyboard () {
		}

		public void AttachTo (Form frm) {
			this.frm = frm;
			this.frm.KeyPreview = true;
			this.frm.KeyDown += Frm_KeyDown;
			this.frm.KeyUp += Frm_KeyUp;
		}

		void Frm_KeyDown (object sender, KeyEventArgs e) {
			if (e.Control || e.Alt || e.Shift)
				return;
			if (keymap.Contains (char.ToLowerInvariant ((char)e.KeyValue)))
				this.SetKey ((ushort)keymap.IndexOf (char.ToLowerInvariant ((char)e.KeyValue)));
			e.Handled = false;
		}

		void Frm_KeyUp (object sender, KeyEventArgs e) {
			if (e.Control || e.Alt || e.Shift)
				return;
			if (keymap.Contains (char.ToLowerInvariant ((char)e.KeyValue)))
				this.UnsetKey ((ushort)keymap.IndexOf (char.ToLowerInvariant ((char)e.KeyValue)));
			e.Handled = false;
		}
	}
}

