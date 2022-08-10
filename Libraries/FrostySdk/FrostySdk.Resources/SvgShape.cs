using FrostySdk.IO;
using System.Collections.Generic;
using System.Text;

namespace FrostySdk.Resources
{
	public class SvgShape
	{
		private bool stroke;

		private uint strokeColor;

		private bool fill;

		private uint fillColor;

		private float opacity;

		private float thickness;

		private byte[] unknownBytes;

		private byte visible;

		private Vec2 minBoundingBox;

		private Vec2 maxBoundingBox;

		private List<SvgPath> paths = new List<SvgPath>();

		public bool Stroke
		{
			get
			{
				return stroke;
			}
			set
			{
				stroke = value;
			}
		}

		public uint StrokeColor
		{
			get
			{
				return strokeColor;
			}
			set
			{
				strokeColor = value;
			}
		}

		public bool Fill
		{
			get
			{
				return fill;
			}
			set
			{
				fill = value;
			}
		}

		public uint FillColor
		{
			get
			{
				return fillColor;
			}
			set
			{
				fillColor = value;
			}
		}

		public float Opacity
		{
			get
			{
				return opacity;
			}
			set
			{
				opacity = value;
			}
		}

		public float Thickness
		{
			get
			{
				return thickness;
			}
			set
			{
				thickness = value;
			}
		}

		public byte Visible
		{
			get
			{
				return visible;
			}
			set
			{
				visible = value;
			}
		}

		public IEnumerable<SvgPath> Paths
		{
			get
			{
				for (int i = 0; i < paths.Count; i++)
				{
					yield return paths[i];
				}
			}
		}

		public SvgShape()
		{
			opacity = 1f;
			thickness = 1f;
			unknownBytes = new byte[40];
			visible = 1;
			minBoundingBox = default(Vec2);
			maxBoundingBox = default(Vec2);
			minBoundingBox.x = float.MaxValue;
			minBoundingBox.y = float.MaxValue;
			maxBoundingBox.x = float.MinValue;
			maxBoundingBox.y = float.MinValue;
		}

		internal SvgShape(NativeReader reader)
		{
			fill = reader.ReadBoolean();
			if (fill)
			{
				fillColor = reader.ReadUInt();
			}
			stroke = reader.ReadBoolean();
			if (stroke)
			{
				strokeColor = reader.ReadUInt();
			}
			opacity = reader.ReadFloat();
			thickness = reader.ReadFloat();
			unknownBytes = reader.ReadBytes(40);
			visible = reader.ReadByte();
			minBoundingBox = reader.ReadVec2();
			maxBoundingBox = reader.ReadVec2();
			int num = reader.ReadInt();
			for (int i = 0; i < num; i++)
			{
				paths.Add(new SvgPath(reader));
			}
		}

		public void AddPaths(IEnumerable<SvgPath> inPaths)
		{
			paths.AddRange(inPaths);
			foreach (SvgPath inPath in inPaths)
			{
				if (inPath.MinBoundingBox.x < minBoundingBox.x)
				{
					minBoundingBox.x = inPath.MinBoundingBox.x;
				}
				if (inPath.MinBoundingBox.y < minBoundingBox.y)
				{
					minBoundingBox.y = inPath.MinBoundingBox.y;
				}
				if (inPath.MaxBoundingBox.x > maxBoundingBox.x)
				{
					maxBoundingBox.x = inPath.MaxBoundingBox.x;
				}
				if (inPath.MaxBoundingBox.y > maxBoundingBox.y)
				{
					maxBoundingBox.y = inPath.MaxBoundingBox.y;
				}
			}
		}

		public string ToDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Stroke: " + stroke.ToString());
			stringBuilder.AppendLine("Stroke Color: " + strokeColor);
			stringBuilder.AppendLine("Fill: " + fill.ToString());
			stringBuilder.AppendLine("Fill Color: " + fillColor);
			stringBuilder.AppendLine("Opacity: " + opacity);
			stringBuilder.AppendLine("Thickness: " + thickness);
			stringBuilder.AppendLine("Visible: " + visible);
			stringBuilder.AppendLine("BBox Min: (" + minBoundingBox.x + ", " + minBoundingBox.y + ")");
			stringBuilder.AppendLine("BBox Max: (" + maxBoundingBox.x + ", " + maxBoundingBox.y + ")");
			for (int i = 0; i < 5; i++)
			{
				stringBuilder.Append("Unknown: ");
				for (int j = 0; j < 8; j++)
				{
					stringBuilder.Append(unknownBytes[i * 8 + j].ToString("X2") + " ");
				}
				stringBuilder.Append("\n");
			}
			stringBuilder.AppendLine("Num Paths: " + paths.Count);
			int num = 0;
			foreach (SvgPath path in paths)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Path #" + num++);
				stringBuilder.Append(path.ToDebugString());
			}
			return stringBuilder.ToString();
		}

		internal void Write(NativeWriter writer)
		{
			writer.Write(fill);
			if (fill)
			{
				writer.Write(fillColor);
			}
			writer.Write(stroke);
			if (stroke)
			{
				writer.Write(strokeColor);
			}
			writer.Write(opacity);
			writer.Write(thickness);
			writer.Write(unknownBytes);
			writer.Write(visible);
			writer.Write(minBoundingBox.x);
			writer.Write(minBoundingBox.y);
			writer.Write(maxBoundingBox.x);
			writer.Write(maxBoundingBox.y);
			writer.Write(paths.Count);
			foreach (SvgPath path in paths)
			{
				path.Write(writer);
			}
		}
	}
}
