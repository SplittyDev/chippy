﻿using System;

namespace chippy
{
	public class VirtualScreen : IDisplayDevice
	{
		public const byte WIDTH = 64;
		public const byte HEIGHT = 32;

		internal byte[] vmem;
		internal bool draw;

		public virtual string Identifier { get; } = "Virtual screen";

		public byte this [int i] {
			get { return vmem [i]; }
			set { vmem [i] = value; }
		}

		public virtual void PreInit () {
			vmem = new byte [WIDTH * HEIGHT];
			draw = false;
		}

		public virtual void Init () {
			Clear ();
			// Draw to make sure that the screen really gets cleared
			draw = true;
		}

		public virtual void Clear () {
			Array.Clear (vmem, 0, vmem.Length);
			Console.WriteLine ("Cleared screen.");
		}

		public virtual bool CheckPixel (int pos) {
			return vmem [pos] == 1;
		}

		public virtual void SetPixel (int pos) {
			vmem [pos] ^= 1;
		}

		public virtual void Update () {
			// Set draw flag
			draw = true;
		}

		public virtual void Draw () {
			// Reset draw flag
			draw = false;
		}

		public bool ShouldRedraw () {
			return draw;
		}
	}
}

