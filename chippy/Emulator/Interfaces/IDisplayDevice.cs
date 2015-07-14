using System;

namespace chippy
{
	public interface IDisplayDevice : IComponent
	{
		byte this [int i] { get; set; }
		void Clear ();
		bool CheckPixel (int pos);
		void SetPixel (int pos);
		void Update ();
		void Draw ();
		bool ShouldRedraw ();
	}
}

