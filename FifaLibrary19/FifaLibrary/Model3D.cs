using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FifaLibrary
{
	public class Model3D
	{
		private string m_TextureFileName;

		private Bitmap m_TextureBitmap;

		private int m_NVertex;

		private int m_NOriginalVertex;

		private int m_NIndex;

		private int m_NOriginalIndex;

		private bool m_IsTriangleList;

		private CustomVertex.PositionNormalTextured[] m_Vertex;

		private short[] m_Index;

		private short[] m_IndexStream;

		private int m_NFaces;

		public string TextureFileName
		{
			get
			{
				return m_TextureFileName;
			}
			set
			{
				m_TextureFileName = value;
			}
		}

		public Bitmap TextureBitmap
		{
			get
			{
				return m_TextureBitmap;
			}
			set
			{
				m_TextureBitmap = value;
			}
		}

		public int NVertex => m_NVertex;

		public int NIndex => m_NIndex;

		public CustomVertex.PositionNormalTextured[] Vertex => m_Vertex;

		public short[] Index => m_Index;

		public int NFaces => m_NFaces;

		public Model3D()
		{
		}

		public Model3D(Rx3IndexArray indexArray, Rx3VertexArray vertexArray)
		{
			Initialize(indexArray, vertexArray);
		}

		public Model3D(Rx3IndexArray indexArray, Rx3VertexArray vertexArray, Bitmap textureBitmap)
		{
			Initialize(indexArray, vertexArray);
			m_TextureBitmap = textureBitmap;
		}

		public void Initialize(Rx3IndexArray indexArray, Rx3VertexArray vertexArray)
		{
			SetVertexArray(vertexArray);
			SetIndexArray(indexArray);
			ComputeNormals();
		}

		private void SetIndexArray(Rx3IndexArray indexArray)
		{
			m_NIndex = indexArray.NIndex;
			m_NOriginalIndex = m_NIndex;
			m_IndexStream = indexArray.IndexStream;
			m_NFaces = indexArray.nFaces;
			m_IsTriangleList = indexArray.IsTriangleList;
			if (!m_IsTriangleList)
			{
				m_Index = new short[m_NIndex];
				for (int i = 0; i < m_NIndex; i += 3)
				{
					m_Index[i] = m_IndexStream[i];
					m_Index[i + 1] = m_IndexStream[i + 1];
					m_Index[i + 2] = m_IndexStream[i + 2];
				}
				return;
			}
			m_Index = new short[m_NFaces * 3];
			int num = 0;
			int num2 = (Rx3IndexArray.TriangleListType == Rx3IndexArray.ETriangleListType.InvertOdd) ? 1 : 0;
			for (int j = 0; j < m_NIndex - 2; j++)
			{
				short num3 = m_IndexStream[j];
				short num4 = m_IndexStream[j + 1];
				short num5 = m_IndexStream[j + 2];
				if (num3 > m_NVertex || num4 > m_NVertex || num5 > m_NVertex || num3 < 0 || num4 < 0 || num5 < 0)
				{
					break;
				}
				if (num3 != num4 && num4 != num5 && num3 != num5)
				{
					if ((j & 1) == num2)
					{
						m_Index[num++] = num3;
						m_Index[num++] = num4;
						m_Index[num++] = num5;
					}
					else
					{
						m_Index[num++] = num3;
						m_Index[num++] = num5;
						m_Index[num++] = num4;
					}
				}
			}
			m_NIndex = m_NFaces * 3;
		}

		private void SetVertexArray(Rx3VertexArray vertexArray)
		{
			m_NVertex = vertexArray.nVertex;
			m_NOriginalVertex = m_NVertex;
			m_Vertex = new CustomVertex.PositionNormalTextured[m_NVertex];
			for (int i = 0; i < m_NVertex; i++)
			{
				m_Vertex[i].X = vertexArray.Vertexes[i].X;
				m_Vertex[i].Y = vertexArray.Vertexes[i].Y;
				m_Vertex[i].Z = vertexArray.Vertexes[i].Z;
				m_Vertex[i].Tu = vertexArray.Vertexes[i].U;
				m_Vertex[i].Tv = vertexArray.Vertexes[i].V;
			}
		}

		public Model3D Clone()
		{
			Model3D obj = (Model3D)MemberwiseClone();
			obj.m_Index = (short[])m_Index.Clone();
			obj.m_IndexStream = (short[])m_IndexStream.Clone();
			obj.m_Vertex = (CustomVertex.PositionNormalTextured[])m_Vertex.Clone();
			return obj;
		}

		private void ComputeNormals()
		{
			Vector3[] array = new Vector3[m_NVertex];
			int[] array2 = new int[m_NVertex];
			for (int i = 0; i < m_NFaces; i++)
			{
				int num = m_Index[i * 3];
				int num2 = m_Index[i * 3 + 1];
				int num3 = m_Index[i * 3 + 2];
				Vector3 right = new Vector3(m_Vertex[num].X, m_Vertex[num].Y, m_Vertex[num].Z);
				Vector3 left = new Vector3(m_Vertex[num2].X, m_Vertex[num2].Y, m_Vertex[num2].Z);
				Vector3 left2 = new Vector3(m_Vertex[num3].X, m_Vertex[num3].Y, m_Vertex[num3].Z);
				Vector3 right3 = Vector3.Normalize(Vector3.Cross(right: Vector3.Subtract(left, right), left: Vector3.Subtract(left2, right)));
				array[num] = Vector3.Add(array[num], right3);
				array[num2] = Vector3.Add(array[num2], right3);
				array[num3] = Vector3.Add(array[num3], right3);
				array2[num]++;
				array2[num2]++;
				array2[num3]++;
			}
			for (int j = 0; j < m_NVertex; j++)
			{
				Vector3 vector = Vector3.Scale(array[j], 1f / (float)array2[j]);
				m_Vertex[j].Nx = vector.X;
				m_Vertex[j].Ny = vector.Y;
				m_Vertex[j].Nz = vector.Z;
			}
		}

		public bool CanMerge(Model3D model)
		{
			if (model == null)
			{
				return true;
			}
			if (m_NOriginalVertex < model.NVertex)
			{
				return false;
			}
			if (m_NOriginalIndex < model.NIndex)
			{
				return false;
			}
			return true;
		}

		public static bool CanMorphing(Model3D model1, Model3D model2)
		{
			if (model1 == null || model2 == null)
			{
				return false;
			}
			if (model1.NVertex != model2.NVertex)
			{
				return false;
			}
			return true;
		}

		public bool CanMorphing(Model3D model2)
		{
			return CanMorphing(this, model2);
		}

		public bool Merge(Model3D model)
		{
			if (!CanMerge(model))
			{
				return false;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (model != null)
			{
				num = model.NVertex;
				num2 = model.NIndex;
				num3 = model.NFaces;
			}
			for (int i = num; i < m_NVertex; i++)
			{
				m_Vertex[i].X = 0f;
				m_Vertex[i].Y = 0f;
				m_Vertex[i].Z = 0f;
				m_Vertex[i].Nx = 0f;
				m_Vertex[i].Ny = 0f;
				m_Vertex[i].Nz = 0f;
				m_Vertex[i].Tu = 0f;
				m_Vertex[i].Tv = 0f;
			}
			m_NVertex = num;
			for (int j = 0; j < m_NVertex; j++)
			{
				m_Vertex[j].X = model.Vertex[j].X;
				m_Vertex[j].Y = model.Vertex[j].Y;
				m_Vertex[j].Z = model.Vertex[j].Z;
				m_Vertex[j].Nx = model.Vertex[j].Nx;
				m_Vertex[j].Ny = model.Vertex[j].Ny;
				m_Vertex[j].Nz = model.Vertex[j].Nz;
				m_Vertex[j].Tu = model.Vertex[j].Tu;
				m_Vertex[j].Tv = model.Vertex[j].Tv;
			}
			for (int k = num2; k < m_NIndex; k++)
			{
				m_IndexStream[k] = 0;
			}
			m_NIndex = num2;
			for (int l = 0; l < m_NIndex; l++)
			{
				m_IndexStream[l] = model.m_IndexStream[l];
			}
			if (num3 < m_NFaces)
			{
				for (int m = num3 * 3; m < m_NFaces * 3; m++)
				{
					m_Index[m] = 0;
				}
			}
			else
			{
				Array.Resize(ref m_Index, num3 * 3);
			}
			m_NFaces = num3;
			for (int n = 0; n < m_NFaces * 3; n++)
			{
				m_Index[n] = model.m_Index[n];
			}
			return true;
		}

		public bool Morphing(Model3D model, float percent)
		{
			if (!CanMorphing(model))
			{
				return false;
			}
			float num = 1f - percent;
			m_NVertex = model.NVertex;
			for (int i = 0; i < m_NVertex; i++)
			{
				m_Vertex[i].X = m_Vertex[i].X * num + model.Vertex[i].X * percent;
				m_Vertex[i].Y = m_Vertex[i].Y * num + model.Vertex[i].Y * percent;
				m_Vertex[i].Z = m_Vertex[i].Z * num + model.Vertex[i].Z * percent;
				m_Vertex[i].Tu = m_Vertex[i].Tu * num + model.Vertex[i].Tu * percent;
				m_Vertex[i].Tv = m_Vertex[i].Tv * num + model.Vertex[i].Tv * percent;
			}
			return true;
		}

		public bool Morphing(Model3D model1, Model3D model2, float percent)
		{
			if (!CanMorphing(model1, model2))
			{
				return false;
			}
			if (!CanMorphing(model1))
			{
				return false;
			}
			float num = 1f - percent;
			int nVertex = m_NVertex;
			if (model1.NVertex < nVertex)
			{
				nVertex = model1.NVertex;
			}
			if (model2.NVertex < nVertex)
			{
				nVertex = model2.NVertex;
			}
			m_NVertex = nVertex;
			for (int i = 0; i < nVertex; i++)
			{
				m_Vertex[i].X = model1.m_Vertex[i].X * num + model2.Vertex[i].X * percent;
				m_Vertex[i].Y = model1.m_Vertex[i].Y * num + model2.Vertex[i].Y * percent;
				m_Vertex[i].Z = model1.m_Vertex[i].Z * num + model2.Vertex[i].Z * percent;
				m_Vertex[i].Tu = model1.m_Vertex[i].Tu * num + model2.Vertex[i].Tu * percent;
				m_Vertex[i].Tv = model1.m_Vertex[i].Tv * num + model2.Vertex[i].Tv * percent;
			}
			return true;
		}

		public bool MorphingThroughUV(Model3D model1, Model3D model2, float percent)
		{
			return MorphingThroughUV(model1, model2, percent, null);
		}

		public bool MorphingThroughUV(Model3D model1, Model3D model2, float percent, Bitmap mask)
		{
			float num = 1f - percent;
			float num2 = percent;
			if (model1.NVertex >= 3157 && model2.NVertex >= 3157 && NVertex >= 3157)
			{
				for (int i = 0; i < NVertex; i++)
				{
					if (mask != null)
					{
						int x = (int)(Vertex[i].Tu * (float)mask.Width);
						int y = (int)(Vertex[i].Tv * (float)mask.Height);
						num2 = (float)(int)mask.GetPixel(x, y).R * percent / 255f;
						num = 1f - num2;
					}
					float num3 = float.MaxValue;
					int num4 = -1;
					for (int j = 0; j < model1.NVertex; j++)
					{
						float num5 = (Vertex[i].Tu - model1.Vertex[j].Tu) * (Vertex[i].Tu - model1.Vertex[j].Tu) + (Vertex[i].Tv - model1.Vertex[j].Tv) * (Vertex[i].Tv - model1.Vertex[j].Tv);
						if (num5 < num3)
						{
							num3 = num5;
							num4 = j;
						}
					}
					num3 = float.MaxValue;
					int num6 = -1;
					for (int k = 0; k < model2.NVertex; k++)
					{
						float num7 = (Vertex[i].Tu - model2.Vertex[k].Tu) * (Vertex[i].Tu - model2.Vertex[k].Tu) + (Vertex[i].Tv - model2.Vertex[k].Tv) * (Vertex[i].Tv - model2.Vertex[k].Tv);
						if (num7 < num3)
						{
							num3 = num7;
							num6 = k;
						}
					}
					if (num4 != -1 && num6 != -1)
					{
						Vertex[i].X = model1.Vertex[num4].X * num + model2.Vertex[num6].X * num2;
						Vertex[i].Y = model1.Vertex[num4].Y * num + model2.Vertex[num6].Y * num2;
						Vertex[i].Z = model1.Vertex[num4].Z * num + model2.Vertex[num6].Z * num2;
					}
				}
			}
			else
			{
				if (model1.NVertex != 132 || model2.NVertex != 132 || NVertex != 132)
				{
					return false;
				}
				for (int l = 0; l < NVertex; l++)
				{
					float num3 = float.MaxValue;
					int num4 = -1;
					for (int m = 0; m < model1.NVertex; m++)
					{
						if (Vertex[l].X * model1.Vertex[m].X > 0f)
						{
							float num8 = (Vertex[l].Tu - model1.Vertex[m].Tu) * (Vertex[l].Tu - model1.Vertex[m].Tu) + (Vertex[l].Tv - model1.Vertex[m].Tv) * (Vertex[l].Tv - model1.Vertex[m].Tv);
							if (num8 < num3)
							{
								num3 = num8;
								num4 = m;
							}
						}
					}
					num3 = float.MaxValue;
					int num6 = -1;
					for (int n = 0; n < model2.NVertex; n++)
					{
						if (Vertex[l].X * model2.Vertex[n].X > 0f)
						{
							float num9 = (Vertex[l].Tu - model2.Vertex[n].Tu) * (Vertex[l].Tu - model2.Vertex[n].Tu) + (Vertex[l].Tv - model2.Vertex[n].Tv) * (Vertex[l].Tv - model2.Vertex[n].Tv);
							if (num9 < num3)
							{
								num3 = num9;
								num6 = n;
							}
						}
					}
					if (num4 != -1 && num6 != -1)
					{
						Vertex[l].X = model1.Vertex[num4].X * num + model2.Vertex[num6].X * num2;
						Vertex[l].Y = model1.Vertex[num4].Y * num + model2.Vertex[num6].Y * num2;
						Vertex[l].Z = model1.Vertex[num4].Z * num + model2.Vertex[num6].Z * num2;
					}
				}
			}
			return true;
		}

		public void MoveForward()
		{
			for (int i = 0; i < m_NVertex; i++)
			{
				m_Vertex[i].Z += 0.1f;
			}
		}

		public void MoveBack()
		{
			for (int i = 0; i < m_NVertex; i++)
			{
				m_Vertex[i].Z -= 0.1f;
			}
		}

		public void MoveUp()
		{
			for (int i = 0; i < m_NVertex; i++)
			{
				m_Vertex[i].Y += 0.1f;
			}
		}

		public void MoveDown()
		{
			for (int i = 0; i < m_NVertex; i++)
			{
				m_Vertex[i].Y -= 0.1f;
			}
		}

		public void MoveLeft()
		{
			for (int i = 0; i < m_NVertex; i++)
			{
				m_Vertex[i].X += 0.1f;
			}
		}

		public void MoveRight()
		{
			for (int i = 0; i < m_NVertex; i++)
			{
				m_Vertex[i].X -= 0.1f;
			}
		}

		public void MakeCloser()
		{
			for (int i = 0; i < m_NVertex; i++)
			{
				if (m_Vertex[i].X > 0f)
				{
					m_Vertex[i].X -= 0.1f;
				}
				else if (m_Vertex[i].X < 0f)
				{
					m_Vertex[i].X += 0.1f;
				}
			}
		}

		public void MakeWider()
		{
			for (int i = 0; i < m_NVertex; i++)
			{
				if (m_Vertex[i].X > 0f)
				{
					m_Vertex[i].X += 0.1f;
				}
				else if (m_Vertex[i].X < 0f)
				{
					m_Vertex[i].X -= 0.1f;
				}
			}
		}

		public void SaveXFile(string fileName)
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
			StreamWriter streamWriter = new StreamWriter(fileStream);
			SaveXFile(streamWriter);
			streamWriter.Close();
			fileStream.Close();
		}

		public void SaveXFile(StreamWriter w)
		{
			if (m_NVertex > 0)
			{
				w.WriteLine("xof 0303txt 0032");
				w.WriteLine("Mesh {");
				w.WriteLine(" {0};", m_NVertex);
				for (int i = 0; i < m_NVertex - 1; i++)
				{
					w.WriteLine(" {0:F6};{1:F6};{2:F6};,", m_Vertex[i].X, m_Vertex[i].Y, m_Vertex[i].Z);
				}
				w.WriteLine(" {0:F6};{1:F6};{2:F6};;", m_Vertex[m_NVertex - 1].X, m_Vertex[m_NVertex - 1].Y, m_Vertex[m_NVertex - 1].Z);
				w.WriteLine(" {0};", m_NFaces);
				for (int j = 0; j < m_NFaces - 1; j++)
				{
					w.WriteLine(" 3;{0},{1},{2};,", m_Index[j * 3], m_Index[j * 3 + 1], m_Index[j * 3 + 2]);
				}
				w.WriteLine(" 3;{0},{1},{2};;", m_Index[(m_NFaces - 1) * 3], m_Index[(m_NFaces - 1) * 3 + 1], m_Index[(m_NFaces - 1) * 3 + 2]);
				w.WriteLine(" MeshNormals {");
				w.WriteLine("  {0};", m_NVertex);
				for (int k = 0; k < m_NVertex - 1; k++)
				{
					w.WriteLine("  {0:F6};{1:F6};{2:F6};,", m_Vertex[k].Nx, m_Vertex[k].Ny, m_Vertex[k].Nz);
				}
				w.WriteLine("  {0:F6};{1:F6};{2:F6};;", m_Vertex[m_NVertex - 1].Nx, m_Vertex[m_NVertex - 1].Ny, m_Vertex[m_NVertex - 1].Nz);
				w.WriteLine(" {0};", m_NFaces);
				for (int l = 0; l < m_NFaces - 1; l++)
				{
					w.WriteLine("  3;{0},{1},{2};,", m_Index[l * 3], m_Index[l * 3 + 1], m_Index[l * 3 + 2]);
				}
				w.WriteLine("  3;{0},{1},{2};;", m_Index[(m_NFaces - 1) * 3], m_Index[(m_NFaces - 1) * 3 + 1], m_Index[(m_NFaces - 1) * 3 + 2]);
				w.WriteLine(" }");
				w.WriteLine(" MeshTextureCoords {");
				w.WriteLine("  {0};", m_NVertex);
				for (int m = 0; m < m_NVertex - 1; m++)
				{
					w.WriteLine("  {0:F6};{1:F6};,", m_Vertex[m].Tu, m_Vertex[m].Tv);
				}
				w.WriteLine("  {0:F6};{1:F6};;", m_Vertex[m_NVertex - 1].Tu, m_Vertex[m_NVertex - 1].Tv);
				w.WriteLine(" }");
				w.WriteLine(" MeshMaterialList {");
				w.WriteLine("  1;");
				w.WriteLine("  {0};", m_NFaces);
				for (int n = 0; n < m_NFaces - 1; n++)
				{
					w.WriteLine("  0,");
				}
				w.WriteLine("  0;");
				w.WriteLine("  Material {");
				w.WriteLine("   1.000000;1.000000;1.000000;0.000000;;");
				w.WriteLine("   0.000000;");
				w.WriteLine("   0.000000;0.000000;0.000000;;");
				w.WriteLine("   0.000000;0.000000;0.000000;;");
				w.WriteLine("   TextureFilename {");
				w.WriteLine("   \"" + m_TextureFileName + "\";");
				w.WriteLine("   }");
				w.WriteLine("  }");
				w.WriteLine(" }");
				w.WriteLine("}");
				m_TextureBitmap.Save(m_TextureFileName, ImageFormat.Png);
			}
		}

		public void OffsetVertex(int offsetX, int offsetY, int offsetZ)
		{
			if (m_Vertex != null && m_Vertex.Length >= m_NOriginalVertex)
			{
				for (int i = 0; i < m_NOriginalVertex; i++)
				{
					m_Vertex[i].X -= offsetX;
					m_Vertex[i].Y -= offsetY;
					m_Vertex[i].Z -= offsetZ;
				}
			}
		}

		public void Morphing(int[] points, Vector3 symmetryPoint, Vector3 delta)
		{
			foreach (int num in points)
			{
				if (num >= 0 && num < NVertex)
				{
					if (Vertex[num].X < symmetryPoint.X)
					{
						Vertex[num].X -= delta.X;
					}
					else
					{
						Vertex[num].X += delta.X;
					}
					if (Vertex[num].Y < symmetryPoint.Y)
					{
						Vertex[num].Y -= delta.Y;
					}
					else
					{
						Vertex[num].Y += delta.Y;
					}
					if (Vertex[num].Z < symmetryPoint.Z)
					{
						Vertex[num].Z -= delta.Z;
					}
					else
					{
						Vertex[num].Z += delta.Z;
					}
				}
			}
		}
	}
}
