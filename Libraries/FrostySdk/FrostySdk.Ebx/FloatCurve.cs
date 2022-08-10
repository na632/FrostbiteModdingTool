using FrostySdk.Ebx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.Ebx
{
	public class FloatCurve
	{
		public CString __Id
		{
			get;set;
		}

		public List<FloatCurvePoint> Points { get; set; }

		public float MaxX { get; set; }

		public float MinX { get; set; }

		
	}
}
