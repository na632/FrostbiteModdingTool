using FMT.FileTools;
using FrostySdk.FrostySdk.IO;
using FrostySdk.IO;
using System.Collections.Generic;
using System.Text;

namespace FrostySdk.Resources
{
    public class SvgPath
	{
		private List<Vec2> points = new List<Vec2>();

		private byte closed;

		private Vec2 minBoundingBox;

		private Vec2 maxBoundingBox;

		public bool Closed
		{
			get
			{
				return closed != 0;
			}
			set
			{
				closed = (byte)(value ? 1 : 0);
			}
		}

		public Vec2 MinBoundingBox => minBoundingBox;

		public Vec2 MaxBoundingBox => maxBoundingBox;

		public List<Vec2> Points => points;

		public SvgPath()
		{
			minBoundingBox = default(Vec2);
			maxBoundingBox = default(Vec2);
			minBoundingBox.x = float.MaxValue;
			minBoundingBox.y = float.MaxValue;
			maxBoundingBox.x = float.MinValue;
			maxBoundingBox.y = float.MinValue;
		}

		internal SvgPath(NativeReader reader)
		{
			int num = reader.ReadInt();
			for (int i = 0; i < num; i++)
			{
				points.Add(reader.ReadVec2());
			}
			closed = reader.ReadByte();
			minBoundingBox = reader.ReadVec2();
			maxBoundingBox = reader.ReadVec2();
		}

		public void AddPoint(Vec2 point)
		{
			points.Add(point);
			if (point.x < minBoundingBox.x)
			{
				minBoundingBox.x = point.x;
			}
			if (point.y < minBoundingBox.y)
			{
				minBoundingBox.y = point.y;
			}
			if (point.x > maxBoundingBox.x)
			{
				maxBoundingBox.x = point.x;
			}
			if (point.y > maxBoundingBox.y)
			{
				maxBoundingBox.y = point.y;
			}
		}

		public string ToDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Closed: " + closed);
			stringBuilder.AppendLine("BBox Min: (" + minBoundingBox.x + ", " + minBoundingBox.y + ")");
			stringBuilder.AppendLine("BBox Max: (" + maxBoundingBox.x + ", " + maxBoundingBox.y + ")");
			stringBuilder.AppendLine("Num Points: " + points.Count);
			stringBuilder.AppendLine();
			int num = 0;
			for (int i = 0; i < points.Count; i++)
			{
				stringBuilder.AppendLine("Point #" + num++ + ": (" + points[i].x + ", " + points[i].y + ")");
			}
			return stringBuilder.ToString();
		}

		internal void Write(NativeWriter writer)
		{
			writer.Write(points.Count);
			foreach (Vec2 point in points)
			{
				writer.Write(point.x);
				writer.Write(point.y);
			}
			writer.Write(closed);
			writer.Write(minBoundingBox.x);
			writer.Write(minBoundingBox.y);
			writer.Write(maxBoundingBox.x);
			writer.Write(maxBoundingBox.y);
		}
	}
}
