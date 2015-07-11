using System;
using System.Windows.Forms;
using System.Drawing;
using hqx;

namespace chippy8
{
	public class WinFormsDisplay : ManagedScreen
	{
		public Panel pnl;
		Bitmap bmp;

		public override void PreInit () {
			pnl = new Panel {
				Width = ManagedScreen.WIDTH,
				Height = ManagedScreen.HEIGHT,
				Dock = DockStyle.Fill,
				BackColor = Color.Transparent,
			};
			base.PreInit ();
		}

		public override void Init () {
			base.Init ();
		}

		public override void Draw () {
			if (draw) {
				if (bmp != null)
					bmp.Dispose ();
				bmp = new Bitmap (WIDTH, HEIGHT);
				for (var y = 0; y < HEIGHT; y++)
					for (var x = 0; x < WIDTH; x++)
						bmp.SetPixel (x, y, Color.Transparent);
				for (var y = 0; y < HEIGHT; y++)
					for (var x = 0; x < WIDTH; x++)
						if (CheckPixel ((ushort)(x + y * WIDTH))) {
							bmp.SetPixel (x / WIDTH, y % WIDTH, Color.Black);
						}
				pnl.BackgroundImage = HqxSharp.Scale2 (HqxSharp.Scale4 (bmp));
				pnl.BackgroundImageLayout = ImageLayout.Center;
				base.Draw ();
			}
		}
	}
}

