using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using hqx;

namespace chippy8
{
	public class WinFormsDisplay : Control, IDisplayDevice
	{
		Bitmap bmp;
		VirtualScreen screen;
		Form frm;
		bool updating;

		public WinFormsDisplay () {
			Width = 64;
			Height = 32;
			bmp = new Bitmap (Width, Height);
			SetStyle (ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle (ControlStyles.ResizeRedraw, true);
			SetStyle (ControlStyles.SupportsTransparentBackColor, true);
			SetStyle (ControlStyles.UserPaint, true);
			BackColor = Color.Transparent;
		}

		public void AttachTo (Form frm) {
			frm.Invoke (new MethodInvoker (() => {
				this.frm = frm;
				this.frm.Controls.Add (this);
			}));
		}

		public void Detach () {
			this.frm.Invoke (new MethodInvoker (() => {
				this.frm.Controls.Remove (this);
			}));
		}

		public string Identifier { get; } = "WinForms display control";
		public byte this [ushort i] {
			get { return screen [i]; }
			set { screen [i] = value; }
		}

		public void PreInit () {
			screen = new VirtualScreen ();
			screen.PreInit ();
		}

		public void Init () {
			screen.Init ();
		}

		public void Clear () {
			screen.Clear ();
		}

		void IDisplayDevice.Update () {
			screen.Update ();
		}

		public bool CheckPixel (ushort pos) {
			return screen.CheckPixel (pos);
		}

		public void SetPixel (ushort pos) {
			screen.SetPixel (pos);
		}

		public void Draw () {
			this.Invalidate ();
		}

		void InternalDraw () {
			if (updating)
				return;
			if (screen.draw) {
				updating = true;
				for (var y = 0; y < 32; y++)
					for (var x = 0; x < 64; x++)
						bmp.SetPixel (x, y, Color.Black);
				for (var y = 0; y < 32; y++)
					for (var x = 0; x < 64; x++)
						if (screen.CheckPixel ((ushort)(x + y * 64)))
							bmp.SetPixel (x, y, Color.White);
				screen.draw = false;
				updating = false;
			}
		}

		protected override void OnPaint (PaintEventArgs e) {
			InternalDraw ();
			e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			if (bmp != null)
				e.Graphics.DrawImage (bmp, e.ClipRectangle);
			e.Graphics.DrawString (string.Format ("Cycle {0}", Emulator.Instance.Cpu.Cycles), SystemFonts.DefaultFont, Brushes.Gray, new Point (3, 3));
			base.OnPaint (e);
		}
	}
}

