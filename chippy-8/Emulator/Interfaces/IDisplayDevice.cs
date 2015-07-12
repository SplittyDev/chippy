using System;

namespace chippy8
{
	public interface IDisplayDevice : IComponent
	{
		byte this [ushort i] { get; set; }
		void Clear ();
		bool CheckPixel (ushort pos);
		void SetPixel (ushort pos);
		void Update ();
		void Draw ();
	}
}

