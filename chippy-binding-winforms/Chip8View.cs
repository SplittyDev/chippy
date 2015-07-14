using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace chippy.bindings.winforms
{
	[DesignerCategory ("Chippy")]
	public class Chip8View : UserControl
	{
		Keyboard keypad;
		ScreenControl screen;

		public Color BACKGROUND_GREEN = Color.FromArgb (67, 107, 67);
		public Color FOREGROUND_GRAY = Color.FromArgb (200, 200, 200);

		public Chip8View () {
			SetStyle (ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle (ControlStyles.ResizeRedraw, true);
			SetStyle (ControlStyles.SupportsTransparentBackColor, true);
			SetStyle (ControlStyles.UserPaint, true);
			keypad = new Keyboard ();
			screen = new ScreenControl {
				BackColor = BACKGROUND_GREEN,
				ForeColor = FOREGROUND_GRAY,
			};
			Emulator.Instance
				.Connect <ManagedCPU> ()
				.Connect <ManagedMemory> ()
				.Connect (keypad)
				.Connect (screen);
		}

		protected override void OnResize (EventArgs e)
		{
			if (this.Width % 64 != 0)
				this.Width -= this.Width % 64;
			if (this.Height % 32 != 0)
				this.Height -= this.Height % 32;
			screen.Width = this.Width;
			screen.Height = this.Height;
			base.OnResize (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			screen.AttachTo (this);
			keypad.AttachTo (this.FindForm ());
			base.OnHandleCreated (e);
		}
	}
}

