using System;

namespace chippy8
{
	public interface IScreen : IComponent
	{
		void Clear ();
		bool CheckPixel (ushort pos);
		void SetPixel (ushort pos);
		void Update ();
		void Draw ();
	}
}

