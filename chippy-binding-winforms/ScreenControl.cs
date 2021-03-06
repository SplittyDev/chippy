﻿using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using chippy;

namespace chippy.bindings.winforms
{
	public class ScreenControl : Control, IDisplayDevice
	{
		Bitmap bmp;
		VirtualScreen screen;
		Control ctrl;
		bool updating;

		public ScreenControl () {
			Width = 64;
			Height = 32;
			SetStyle (ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle (ControlStyles.ResizeRedraw, true);
			SetStyle (ControlStyles.SupportsTransparentBackColor, true);
			SetStyle (ControlStyles.UserPaint, true);
		}

		public void AttachTo (Control frm) {
			frm.Invoke (new MethodInvoker (() => {
				this.ctrl = frm;
				this.ctrl.Controls.Add (this);
			}));
		}

		public void Detach () {
			this.ctrl.Invoke (new MethodInvoker (() => {
				this.ctrl.Controls.Remove (this);
			}));
		}

		public string Identifier { get; } = "WinForms display control";
		public byte this [int i] {
			get { return screen [i]; }
			set { screen [i] = value; }
		}

		public void PreInit () {
			screen = new VirtualScreen ();
			screen.PreInit ();
			bmp = new Bitmap (Width, Height);
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

		public bool CheckPixel (int pos) {
			return screen.CheckPixel (pos);
		}

		public void SetPixel (int pos) {
			screen.SetPixel (pos);
		}

		public void Draw () {
			this.Invalidate ();
		}

		public bool ShouldRedraw () {
			return screen.ShouldRedraw ();
		}

		void InternalDraw () {
			if (updating)
				return;
			if (screen.ShouldRedraw ()) {
				updating = true;
				for (var y = 0; y < 32; y++)
					for (var x = 0; x < 64; x++)
						bmp.SetPixel (x, y, this.BackColor);
				for (var y = 0; y < 32; y++)
					for (var x = 0; x < 64; x++)
						if (screen.CheckPixel ((ushort)(x + y * 64)))
							bmp.SetPixel (x, y, this.ForeColor);
				screen.Draw ();
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
		}
	}
}

