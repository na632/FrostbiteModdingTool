using FMT.FileTools;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrostySdk.Resources
{
    public class SvgImage
    {
        private float width;

        private float height;

        private List<SvgShape> shapes = new List<SvgShape>();

        public float Width => width;

        public float Height => height;

        public IEnumerable<SvgShape> Shapes
        {
            get
            {
                for (int i = 0; i < shapes.Count; i++)
                {
                    yield return shapes[i];
                }
            }
        }

        public SvgImage(float inWidth, float inHeight)
        {
            width = inWidth;
            height = inHeight;
        }

        public SvgImage(Stream stream)
        {
            using (NativeReader nativeReader = new NativeReader(stream))
            {
                width = nativeReader.ReadFloat();
                height = nativeReader.ReadFloat();
                int num = nativeReader.ReadInt();
                for (int i = 0; i < num; i++)
                {
                    shapes.Add(new SvgShape(nativeReader));
                }
            }
        }

        public void ClearShapes()
        {
            shapes.Clear();
        }

        public void AddShape(SvgShape shape)
        {
            shapes.Add(shape);
        }

        public string ToDebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Width: " + width.ToString());
            stringBuilder.AppendLine("Height: " + height.ToString());
            stringBuilder.AppendLine("Num Shapes: " + shapes.Count);
            int num = 0;
            foreach (SvgShape shape in shapes)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Shape #" + num++);
                stringBuilder.Append(shape.ToDebugString());
            }
            return stringBuilder.ToString();
        }

        public byte[] ToBytes()
        {
            using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
            {
                nativeWriter.Write(width);
                nativeWriter.Write(height);
                nativeWriter.Write(shapes.Count);
                foreach (SvgShape shape in shapes)
                {
                    shape.Write(nativeWriter);
                }
                return ((MemoryStream)nativeWriter.BaseStream).ToArray();
            }
        }
    }
}
