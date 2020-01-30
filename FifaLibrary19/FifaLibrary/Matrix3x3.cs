namespace FifaLibrary
{
	public class Matrix3x3
	{
		private float a;

		private float b;

		private float c;

		private float d;

		private float e;

		private float f;

		private float g;

		private float h;

		private float i;

		private float m_Determinant;

		public float Determinant => m_Determinant;

		public Matrix3x3(float c00, float c01, float c02, float c10, float c11, float c12, float c20, float c21, float c22)
		{
			a = c00;
			b = c01;
			c = c02;
			d = c10;
			e = c11;
			f = c12;
			g = c20;
			h = c21;
			i = c22;
		}

		public Matrix3x3(int c00, int c01, int c02, int c10, int c11, int c12, int c20, int c21, int c22)
		{
			a = c00;
			b = c01;
			c = c02;
			d = c10;
			e = c11;
			f = c12;
			g = c20;
			h = c21;
			i = c22;
		}

		public float ComputeDeterminant()
		{
			m_Determinant = a * (e * i - f * h) - b * (i * d - f * g) + c * (d * h - e * g);
			return m_Determinant;
		}

		public Matrix3x3 Invert()
		{
			ComputeDeterminant();
			if (m_Determinant != 0f)
			{
				float c = (e * i - f * h) / m_Determinant;
				float c2 = (f * g - d * i) / m_Determinant;
				float c3 = (d * h - e * g) / m_Determinant;
				float c4 = (this.c * h - b * i) / m_Determinant;
				float c5 = (a * i - this.c * g) / m_Determinant;
				float c6 = (b * g - a * h) / m_Determinant;
				float c7 = (b * f - this.c * e) / m_Determinant;
				float c8 = (this.c * d - a * f) / m_Determinant;
				float c9 = (a * e - b * d) / m_Determinant;
				return new Matrix3x3(c, c2, c3, c4, c5, c6, c7, c8, c9);
			}
			return null;
		}

		public Vector3x1 PostMultiply(Vector3x1 v)
		{
			float c = a * v.X + b * v.Y + this.c * v.Z;
			float c2 = d * v.X + e * v.Y + f * v.Z;
			float c3 = g * v.X + h * v.Y + i * v.Z;
			return new Vector3x1(c, c2, c3);
		}

		public Vector3x1 PreMultiply(Vector3x1 v)
		{
			float c = a * v.X + d * v.Y + g * v.Z;
			float c2 = b * v.X + e * v.Y + h * v.Z;
			float c3 = this.c * v.X + f * v.Y + i * v.Z;
			return new Vector3x1(c, c2, c3);
		}
	}
}
