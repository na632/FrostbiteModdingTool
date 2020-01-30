namespace FifaLibrary
{
	public class Vector3x1
	{
		private float x;

		private float y;

		private float z;

		public float X
		{
			get
			{
				return x;
			}
			set
			{
				x = value;
			}
		}

		public float Y
		{
			get
			{
				return y;
			}
			set
			{
				y = value;
			}
		}

		public float Z
		{
			get
			{
				return z;
			}
			set
			{
				z = value;
			}
		}

		public Vector3x1(float c0, float c1, float c2)
		{
			x = c0;
			y = c1;
			z = c2;
		}

		public Vector3x1(int c0, int c1, int c2)
		{
			x = c0;
			y = c1;
			z = c2;
		}

		public Vector3x1()
		{
			x = 0f;
			y = 0f;
			z = 0f;
		}
	}
}
