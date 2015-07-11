using System;
using System.Windows.Forms;
using System.Drawing;
using hqx;

namespace chippy8
{
	public class WinFormsDisplay : Control, IScreen
	{
		Bitmap bmp;
		ManagedScreen screen;

		public WinFormsDisplay () {
			Width = 64;
			Height = 32;
			Dock = DockStyle.Fill;
			bmp = new Bitmap (Width, Height);
		}

		public string Identifier { get; } = "WinForms display control";
		public byte this [ushort i] {
			get { return screen [i]; }
			set { screen [i] = value; }
		}

		public void PreInit () {
			screen = new ManagedScreen ();
			screen.PreInit ();
		}

		public void Init () {
			screen.Init ();
		}

		public void Clear () {
			screen.Clear ();
		}

		void IScreen.Update () {
			screen.Update ();
		}

		public bool CheckPixel (ushort pos) {
			return screen.CheckPixel (pos);
		}

		public void SetPixel (ushort pos) {
			screen.SetPixel (pos);
		}

		public void Draw () {
			if (screen.draw) {
				// Fill with black
				for (var y = 0; y < 32; y++)
					for (var x = 0; x < 64; x++)
						bmp.SetPixel (x, y, Color.Transparent);
				var scaleY = this.Height / 32;
				var scaleX = this.Width / 64;
				for (var y = 0; y < 32; y++)
					for (var x = 0; x < 64; x++)
						if (screen.CheckPixel ((ushort)(x + y * 64)))
							bmp.SetPixel (x, y, Color.Black);
				screen.Draw ();
				this.Invalidate ();
			}
		}

		protected override void OnPaint (PaintEventArgs e) {
			e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			if (bmp != null)
				e.Graphics.DrawImage (bmp, e.ClipRectangle);
			base.OnPaint (e);
		}
	}
}

