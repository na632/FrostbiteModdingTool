using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace FrostbiteModdingUI.Fbx
{
	internal class FBXExporter
	{
		private int totalExportCount;

		private int currentProgress;

		private int boneCount;


		public void ExportFBX(MeshSet meshSet, dynamic meshAsset, string filename, string fbxVersion, string units, string skeleton, string fileType)
		{
			using (FbxManager fbxManager = new FbxManager())
			{
				FbxIOSettings fbxIOSettings = new FbxIOSettings(fbxManager, "IOSRoot");
				fbxIOSettings.SetBoolProp("Export|AdvOptGrp|Fbx|Material", pValue: false);
				fbxIOSettings.SetBoolProp("Export|AdvOptGrp|Fbx|Texture", pValue: false);
				fbxIOSettings.SetBoolProp("Export|AdvOptGrp|Fbx|Global_Settings", pValue: true);
				fbxIOSettings.SetBoolProp("Export|AdvOptGrp|Fbx|Shape", pValue: true);
				fbxManager.SetIOSettings(fbxIOSettings);
				FbxScene fbxScene = new FbxScene(fbxManager, "");
				FbxDocumentInfo fbxDocumentInfo = new FbxDocumentInfo(fbxManager, "SceneInfo");
				fbxDocumentInfo.Title = "FBX Exporter";
				fbxDocumentInfo.Subject = "Export FBX meshes";
				fbxDocumentInfo.OriginalApplicationVendor = "";
				fbxDocumentInfo.OriginalApplicationName = "";
				fbxDocumentInfo.OriginalApplicationVersion = string.Empty;
				fbxDocumentInfo.LastSavedApplicationVendor = string.Empty;
				fbxDocumentInfo.LastSavedApplicationName = string.Empty;
				fbxDocumentInfo.LastSavedApplicationVersion = string.Empty;
				fbxScene.SceneInfo = fbxDocumentInfo;
				switch (units)
				{
					case "Millimeters":
						fbxScene.GlobalSettings.SetSystemUnit(FbxSystemUnit.Millimeters);
						break;
					case "Centimeters":
						fbxScene.GlobalSettings.SetSystemUnit(FbxSystemUnit.Centimeters);
						break;
					case "Meters":
						fbxScene.GlobalSettings.SetSystemUnit(FbxSystemUnit.Meters);
						break;
					case "Kilometers":
						fbxScene.GlobalSettings.SetSystemUnit(FbxSystemUnit.Kilometers);
						break;
				}
				totalExportCount++;
				foreach (MeshSetLod lod in meshSet.Lods)
				{
					foreach (MeshSetSection section in lod.Sections)
					{
						totalExportCount += ((section.Name != "") ? 1 : 0);
					}
				}
				List<FbxNode> boneNodes = new List<FbxNode>();
				if (meshSet.Lods[0].Type == MeshType.MeshType_Skinned && skeleton != "")
				{
					FbxNode pNode = this.FBXCreateSkeleton(fbxScene, meshAsset, skeleton, ref boneNodes);
					fbxScene.RootNode.AddChild(pNode);
				}
				else if (meshSet.Lods[0].Type == MeshType.MeshType_Composite)
				{
					FbxNode pNode2 = FBXCreateCompositeSkeleton(fbxScene, meshSet.Lods[0].PartTransforms, ref boneNodes);
					fbxScene.RootNode.AddChild(pNode2);
				}
				currentProgress++;
				foreach (MeshSetLod lod2 in meshSet.Lods)
				{
					FBXCreateMesh(fbxScene, lod2, boneNodes);
				}
				if (meshSet.Type == MeshType.MeshType_Composite)
				{
					MeshSetLod meshSetLod = meshSet.Lods[0];
					for (int i = 0; i < meshSetLod.PartTransforms.Count; i++)
					{
						LinearTransform linearTransform = meshSetLod.PartTransforms[i];
						FbxNode fbxNode = boneNodes[i];
						Matrix m = SharpDXUtils.FromLinearTransform(linearTransform);
						Vector3 translationVector = m.TranslationVector;
						Vector3 val = SharpDXUtils.ExtractEulerAngles(m);
						fbxNode.LclTranslation = new Vector3(translationVector.X, translationVector.Y, translationVector.Z);
						fbxNode.LclRotation = new Vector3(val.X, val.Y, val.Z);
					}
				}
				using (FbxExporter fbxExporter = new FbxExporter(fbxManager, ""))
				{
					fbxExporter.SetFileExportVersion("FBX" + fbxVersion + "00");
					int writerFormatCount = fbxManager.IOPluginRegistry.WriterFormatCount;
					int num = 0;
					while (true)
					{
						if (num >= writerFormatCount)
						{
							return;
						}
						if (fbxManager.IOPluginRegistry.GetWriterFormatDescription(num).Contains(fileType))
						{
							break;
						}
						num++;
					}
					if (fbxExporter.Initialize(filename, num, fbxIOSettings))
					{
						fbxExporter.Export(fbxScene);
					}
				}
			}
		}

		private FbxNode FBXCreateCompositeSkeleton(FbxScene scene, List<LinearTransform> partTransforms, ref List<FbxNode> boneNodes)
		{
			int num = 0;
			FbxSkeleton fbxSkeleton = new FbxSkeleton(scene, "ROOT");
			fbxSkeleton.SetSkeletonType(FbxSkeleton.EType.eRoot);
			fbxSkeleton.Size = 1.0;
			FbxNode fbxNode = new FbxNode(scene, "ROOT");
			fbxNode.SetNodeAttribute(fbxSkeleton);
			Matrix identity = Matrix.Identity;
			var identitySV = identity.ScaleVector;
			Vector3 translationVector = identity.TranslationVector;
			Vector3 val = SharpDXUtils.ExtractEulerAngles(identity);
			fbxNode.LclTranslation = new Vector3(translationVector.X, translationVector.Y, translationVector.Z);
			fbxNode.LclRotation = new Vector3(val.X, val.Y, val.Z);
			foreach (LinearTransform partTransform in partTransforms)
			{
				_ = partTransform;
				string pName = "PART_" + num;
				fbxSkeleton = new FbxSkeleton(scene, pName);
				fbxSkeleton.SetSkeletonType(FbxSkeleton.EType.eLimbNode);
				fbxSkeleton.Size = 1.0;
				FbxNode fbxNode2 = new FbxNode(scene, pName);
				fbxNode2.SetNodeAttribute(fbxSkeleton);
				identity = Matrix.Identity;
				identitySV = identity.ScaleVector;
				translationVector = identity.TranslationVector;
				val = SharpDXUtils.ExtractEulerAngles(identity);
				fbxNode2.LclTranslation = new Vector3(translationVector.X, translationVector.Y, translationVector.Z);
				fbxNode2.LclRotation = new Vector3(val.X, val.Y, val.Z);
				boneNodes.Add(fbxNode2);
				fbxNode.AddChild(fbxNode2);
				num++;
			}
			return fbxNode;
		}

		private FbxNode FBXCreateSkeleton(FbxScene scene, dynamic meshAsset, string skeleton, ref List<FbxNode> boneNodes)
		{
			dynamic rootObject = AssetManager.Instance.GetEbx(AssetManager.Instance.GetEbxEntry(skeleton)).RootObject;
			boneCount = rootObject.BoneNames.Count;
			for (int i = 0; i < boneCount; i++)
			{
				dynamic val = rootObject.LocalPose;
				Matrix m = new Matrix(val[i].right.x, val[i].right.y, val[i].right.z, 0f, val[i].up.x, val[i].up.y, val[i].up.z, 0f, val[i].forward.x, val[i].forward.y, val[i].forward.z, 0f, val[i].trans.x, val[i].trans.y, val[i].trans.z, 1f);
				var sv = m.ScaleVector;
				Vector3 translationVector = m.TranslationVector;
				Vector3 val2 = SharpDXUtils.ExtractEulerAngles(m);
				FbxSkeleton fbxSkeleton = new FbxSkeleton(scene, rootObject.BoneNames[i]);
				fbxSkeleton.SetSkeletonType((i != 0) ? FbxSkeleton.EType.eLimbNode : FbxSkeleton.EType.eRoot);
				fbxSkeleton.Size = 1.0;
				FbxNode fbxNode = new FbxNode(scene, rootObject.BoneNames[i]);
				fbxNode.SetNodeAttribute(fbxSkeleton);
				fbxNode.LclTranslation = new Vector3(translationVector.X, translationVector.Y, translationVector.Z);
				fbxNode.LclRotation = new Vector3(val2.X, val2.Y, val2.Z);
				if (i != 0)
				{
					int index = rootObject.Hierarchy[i];
					boneNodes[index].AddChild(fbxNode);
				}
				boneNodes.Add(fbxNode);
			}
			return boneNodes[0];
		}

		private void FBXCreateMesh(FbxScene scene, MeshSetLod lod, List<FbxNode> boneNodes)
		{
			int indexSize = lod.IndexUnitSize / 8;
			FbxNode fbxNode = new FbxNode(scene, lod.String03);
			foreach (MeshSetSection section in lod.Sections)
			{
				if (!(section.Name == ""))
				{
					using (NativeReader reader = new NativeReader((lod.ChunkId != Guid.Empty) ? AssetManager.Instance.GetChunk(AssetManager.Instance.GetChunkEntry(lod.ChunkId)) : new MemoryStream(lod.InlineData)))
					{
						FbxNode fbxNode2 = FBXExportSubObject(scene, section, lod.VertexBufferSize, indexSize, reader);
						fbxNode.AddChild(fbxNode2);
						if ((lod.Type == MeshType.MeshType_Skinned || lod.Type == MeshType.MeshType_Composite) && boneNodes.Count > 0)
						{
							List<ushort> list = section.BoneList;
							if (lod.Type == MeshType.MeshType_Composite && lod.PartTransforms.Count != 0)
							{
								list = new List<ushort>();
								for (ushort num = 0; num < lod.PartTransforms.Count; num = (ushort)(num + 1))
								{
									list.Add(num);
								}
							}
							if (ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20180628 || ProfilesLibrary.DataVersion == 20190911)
							{
								list.Clear();
								for (ushort num2 = 0; num2 < boneNodes.Count; num2 = (ushort)(num2 + 1))
								{
									list.Add(num2);
								}
							}
							FBXCreateSkin(scene, section, fbxNode2, boneNodes, list, lod.Type, reader);
							FBXCreateBindPose(scene, section, fbxNode2);
						}
					}
				}
			}
			scene.RootNode.AddChild(fbxNode);
		}

		private void FBXCreateSkin(FbxScene scene, MeshSetSection section, FbxNode actor, List<FbxNode> boneNodes, List<ushort> boneList, MeshType meshType, NativeReader reader)
		{
			FbxMesh fbxMesh = new FbxMesh(actor.GetNodeAttribute(FbxNodeAttribute.EType.eMesh));
			FbxSkin fbxSkin = new FbxSkin(scene, "");
			List<FbxCluster> list = new List<FbxCluster>();
			for (int i = 0; i < section.VertexCount; i++)
			{
				ushort[] array = new ushort[8];
				float[] array2 = new float[8];
				int num = 0;
				GeometryDeclarationDesc.Stream[] streams = section.GeometryDeclDesc[0].Streams;
				for (int j = 0; j < streams.Length; j++)
				{
					GeometryDeclarationDesc.Stream stream = streams[j];
					if (stream.VertexStride == 0)
					{
						continue;
					}
					int num2 = 0;
					GeometryDeclarationDesc.Element[] elements = section.GeometryDeclDesc[0].Elements;
					for (int k = 0; k < elements.Length; k++)
					{
						GeometryDeclarationDesc.Element element = elements[k];
						if (num2 >= num && num2 < num + stream.VertexStride && (element.Usage == VertexElementUsage.BoneIndices || element.Usage == VertexElementUsage.BoneIndices2 || element.Usage == VertexElementUsage.BoneWeights || element.Usage == VertexElementUsage.BoneWeights2))
						{
							reader.Position = section.VertexOffset + num * section.VertexCount + i * stream.VertexStride + (num2 - num);
							if (element.Usage == VertexElementUsage.BoneIndices)
							{
								if (element.Format == VertexElementFormat.Byte4 || element.Format == VertexElementFormat.Byte4N || element.Format == VertexElementFormat.UByte4 || element.Format == VertexElementFormat.UByte4N)
								{
									array[3] = reader.ReadByte();
									array[2] = reader.ReadByte();
									array[1] = reader.ReadByte();
									array[0] = reader.ReadByte();
								}
								else if (element.Format == VertexElementFormat.UShort4 || element.Format == VertexElementFormat.UShort4N)
								{
									array[3] = reader.ReadUShort();
									array[2] = reader.ReadUShort();
									array[1] = reader.ReadUShort();
									array[0] = reader.ReadUShort();
								}
							}
							else if (element.Usage == VertexElementUsage.BoneIndices2)
							{
								if (element.Format == VertexElementFormat.Byte4 || element.Format == VertexElementFormat.Byte4N || element.Format == VertexElementFormat.UByte4 || element.Format == VertexElementFormat.UByte4N)
								{
									array[7] = reader.ReadByte();
									array[6] = reader.ReadByte();
									array[5] = reader.ReadByte();
									array[4] = reader.ReadByte();
								}
								else if (element.Format == VertexElementFormat.UShort4 || element.Format == VertexElementFormat.UShort4N)
								{
									array[7] = reader.ReadUShort();
									array[6] = reader.ReadUShort();
									array[5] = reader.ReadUShort();
									array[4] = reader.ReadUShort();
								}
							}
							else if (element.Usage == VertexElementUsage.BoneWeights)
							{
								array2[3] = (float)(int)reader.ReadByte() / 255f;
								array2[2] = (float)(int)reader.ReadByte() / 255f;
								array2[1] = (float)(int)reader.ReadByte() / 255f;
								array2[0] = (float)(int)reader.ReadByte() / 255f;
							}
							else if (element.Usage == VertexElementUsage.BoneWeights2)
							{
								array2[7] = (float)(int)reader.ReadByte() / 255f;
								array2[6] = (float)(int)reader.ReadByte() / 255f;
								array2[5] = (float)(int)reader.ReadByte() / 255f;
								array2[4] = (float)(int)reader.ReadByte() / 255f;
							}
						}
						num2 += element.Size;
					}
					num += stream.VertexStride;
				}
				if (meshType == MeshType.MeshType_Composite)
				{
					array2[0] = 1f;
				}
				for (int l = 0; l < 8; l++)
				{
					if (array2[l] > 0f)
					{
						int num3 = array[l];
						if (ProfilesLibrary.DataVersion != 20180628 && ProfilesLibrary.DataVersion != 20171117 && ProfilesLibrary.DataVersion != 20190905)
						{
							num3 = boneList[num3];
						}
						if ((num3 & 0x8000) != 0)
						{
							num3 = num3 - 32768 + boneCount;
						}
						while (num3 >= list.Count)
						{
							list.Add(null);
						}
						if (list[num3] == null)
						{
							list[num3] = new FbxCluster(scene, "");
							list[num3].SetLink(boneNodes[num3]);
							list[num3].SetLinkMode(FbxCluster.ELinkMode.eTotalOne);
							FbxAMatrix transformLinkMatrix = boneNodes[num3].EvaluateGlobalTransform();
							list[num3].SetTransformLinkMatrix(transformLinkMatrix);
							fbxSkin.AddCluster(list[num3]);
						}
						list[num3].AddControlPointIndex(i, array2[l]);
					}
				}
			}
			fbxMesh.AddDeformer(fbxSkin);
		}

		private void FBXCreateBindPose(FbxScene scene, MeshSetSection section, FbxNode actor)
		{
			List<FbxNode> list = new List<FbxNode>();
			FbxMesh fbxMesh = new FbxMesh(actor.GetNodeAttribute(FbxNodeAttribute.EType.eMesh));
			int deformerCount = fbxMesh.GetDeformerCount(FbxDeformer.EDeformerType.eSkin);
			int num = 0;
			for (int i = 0; i < deformerCount; i++)
			{
				FbxSkin fbxSkin = new FbxSkin(fbxMesh.GetDeformer(i, FbxDeformer.EDeformerType.eSkin));
				num += fbxSkin.ClusterCount;
			}
			if (num > 0)
			{
				for (int j = 0; j < deformerCount; j++)
				{
					FbxSkin fbxSkin2 = new FbxSkin(fbxMesh.GetDeformer(j, FbxDeformer.EDeformerType.eSkin));
					for (int k = 0; k < fbxSkin2.ClusterCount; k++)
					{
						FbxNode link = fbxSkin2.GetCluster(k).GetLink();
						FBXAddNodeRecursively(list, link);
					}
				}
				list.Add(actor);
			}
			if (list.Count > 0)
			{
				FbxPose fbxPose = new FbxPose(scene, section.Name);
				fbxPose.IsBindPose = true;
				for (int l = 0; l < list.Count; l++)
				{
					FbxNode fbxNode = list[l];
					FbxMatrix matrix = new FbxMatrix(fbxNode.EvaluateGlobalTransform());
					fbxPose.Add(fbxNode, matrix);
				}
				scene.AddPose(fbxPose);
			}
		}

		private unsafe FbxNode FBXExportSubObject(FbxScene scene, MeshSetSection section, long indicesOffset, int indexSize, NativeReader reader)
		{
			FbxNode fbxNode = new FbxNode(scene, section.Name);
			FbxMesh fbxMesh = new FbxMesh(scene, section.Name);
			FbxLayerElementNormal fbxLayerElementNormal = null;
			FbxLayerElementTangent fbxLayerElementTangent = null;
			FbxLayerElementBinormal fbxLayerElementBinormal = null;
			FbxLayerElementVertexColor[] array = new FbxLayerElementVertexColor[15];
			FbxLayerElementUV[] array2 = new FbxLayerElementUV[15];
			Dictionary<VertexElementUsage, int> dictionary = new Dictionary<VertexElementUsage, int>();
			Dictionary<VertexElementUsage, int> dictionary2 = new Dictionary<VertexElementUsage, int>();
			reader.Position = section.VertexOffset;
			fbxMesh.InitControlPoints((int)section.VertexCount);
			IntPtr controlPoints = fbxMesh.GetControlPoints();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			bool flag = false;
			Vector3 val = default(Vector3);
			Vector3 val2 = default(Vector3);
			Vector3 val3 = default(Vector3);
			List<float> list = new List<float>();
			GeometryDeclarationDesc.Stream[] streams = section.GeometryDeclDesc[0].Streams;
			for (int i = 0; i < streams.Length; i++)
			{
				GeometryDeclarationDesc.Stream stream = streams[i];
				if (stream.VertexStride == 0)
				{
					continue;
				}
				for (int j = 0; j < section.VertexCount; j++)
				{
					int num4 = 0;
					GeometryDeclarationDesc.Element[] elements = section.GeometryDeclDesc[0].Elements;
					for (int k = 0; k < elements.Length; k++)
					{
						GeometryDeclarationDesc.Element element = elements[k];
						if (element.Usage == VertexElementUsage.Unknown)
						{
							continue;
						}
						if (num4 >= num3 && num4 < num3 + stream.VertexStride)
						{
							if (element.Usage == VertexElementUsage.Pos)
							{
								double* ptr = (double*)(void*)controlPoints;
								if (element.Format == VertexElementFormat.Float3)
								{
									ptr[j * 4] = reader.ReadFloat();
									ptr[j * 4 + 1] = reader.ReadFloat();
									ptr[j * 4 + 2] = reader.ReadFloat();
								}
								else if (element.Format == VertexElementFormat.Half3 || element.Format == VertexElementFormat.Half4)
								{
									ptr[j * 4] = HalfUtils.Unpack(reader.ReadUShort());
									ptr[j * 4 + 1] = HalfUtils.Unpack(reader.ReadUShort());
									ptr[j * 4 + 2] = HalfUtils.Unpack(reader.ReadUShort());
									if (element.Format == VertexElementFormat.Half4)
									{
										reader.ReadUShort();
									}
								}
							}
							else if (element.Usage == VertexElementUsage.BinormalSign)
							{
								if (element.Format == VertexElementFormat.Half4 || element.Format == VertexElementFormat.Float4)
								{
									if (fbxLayerElementTangent == null)
									{
										fbxLayerElementTangent = new FbxLayerElementTangent(fbxMesh, "");
										fbxLayerElementTangent.MappingMode = EMappingMode.eByControlPoint;
										fbxLayerElementTangent.ReferenceMode = EReferenceMode.eDirect;
									}
									if (element.Format == VertexElementFormat.Half4)
									{
										val2.X = HalfUtils.Unpack(reader.ReadUShort());
										val2.Y = HalfUtils.Unpack(reader.ReadUShort());
										val2.Z = HalfUtils.Unpack(reader.ReadUShort());
										list.Add(HalfUtils.Unpack(reader.ReadUShort()));
									}
									else
									{
										val2.X = reader.ReadFloat();
										val2.Y = reader.ReadFloat();
										val2.Z = reader.ReadFloat();
										list.Add(reader.ReadFloat());
									}
								}
								else if (element.Format == VertexElementFormat.Float)
								{
									list.Add(reader.ReadFloat());
								}
								else
								{
									list.Add(HalfUtils.Unpack(reader.ReadUShort()));
								}
								flag = true;
							}
							else if (element.Usage == VertexElementUsage.Normal)
							{
								if (fbxLayerElementNormal == null)
								{
									fbxLayerElementNormal = new FbxLayerElementNormal(fbxMesh, "");
									fbxLayerElementNormal.MappingMode = EMappingMode.eByControlPoint;
									fbxLayerElementNormal.ReferenceMode = EReferenceMode.eDirect;
								}
								val.X = HalfUtils.Unpack(reader.ReadUShort());
								val.Y = HalfUtils.Unpack(reader.ReadUShort());
								val.Z = HalfUtils.Unpack(reader.ReadUShort());
								fbxLayerElementNormal.DirectArray.Add(val.X, val.Y, val.Z);
								val3.Y = HalfUtils.Unpack(reader.ReadUShort());
							}
							else if (element.Usage == VertexElementUsage.Binormal)
							{
								if (fbxLayerElementBinormal == null)
								{
									fbxLayerElementBinormal = new FbxLayerElementBinormal(fbxMesh, "");
									fbxLayerElementBinormal.MappingMode = EMappingMode.eByControlPoint;
									fbxLayerElementBinormal.ReferenceMode = EReferenceMode.eDirect;
								}
								val3.X = HalfUtils.Unpack(reader.ReadUShort());
								val3.Y = HalfUtils.Unpack(reader.ReadUShort());
								val3.Z = HalfUtils.Unpack(reader.ReadUShort());
								fbxLayerElementBinormal.DirectArray.Add(val3.X, val3.Y, val3.Z);
								reader.ReadUShort();
							}
							else if (element.Usage == VertexElementUsage.Tangent)
							{
								if (fbxLayerElementTangent == null)
								{
									fbxLayerElementTangent = new FbxLayerElementTangent(fbxMesh, "");
									fbxLayerElementTangent.MappingMode = EMappingMode.eByControlPoint;
									fbxLayerElementTangent.ReferenceMode = EReferenceMode.eDirect;
								}
								val2.X = HalfUtils.Unpack(reader.ReadUShort());
								val2.Y = HalfUtils.Unpack(reader.ReadUShort());
								val2.Z = HalfUtils.Unpack(reader.ReadUShort());
								fbxLayerElementTangent.DirectArray.Add(val2.X, val2.Y, val2.Z);
								val3.Z = HalfUtils.Unpack(reader.ReadUShort());
							}
							else if ((int)element.Usage >= 33 && (int)element.Usage <= 40)
							{
								if (!dictionary.ContainsKey(element.Usage))
								{
									array2[num] = new FbxLayerElementUV(fbxMesh, "UVChannel" + num);
									array2[num].MappingMode = EMappingMode.eByControlPoint;
									array2[num].ReferenceMode = EReferenceMode.eDirect;
									dictionary.Add(element.Usage, num);
									num++;
								}
								float num5;
								float num6;
								if (element.Format == VertexElementFormat.Float2)
								{
									num5 = reader.ReadFloat();
									num6 = 1f - reader.ReadFloat();
								}
								else
								{
									num5 = HalfUtils.Unpack(reader.ReadUShort());
									num6 = 1f - HalfUtils.Unpack(reader.ReadUShort());
								}
								array2[dictionary[element.Usage]].DirectArray.Add(num5, num6);
							}
							else if ((int)element.Usage >= 30 && (int)element.Usage <= 31)
							{
								if (!dictionary2.ContainsKey(element.Usage))
								{
									array[num2] = new FbxLayerElementVertexColor(fbxMesh, "VColor" + (num2 + 1));
									array[num2].MappingMode = EMappingMode.eByControlPoint;
									array[num2].ReferenceMode = EReferenceMode.eDirect;
									dictionary2.Add(element.Usage, num2);
									num2++;
								}
								array[dictionary2[element.Usage]].DirectArray.Add((double)(int)reader.ReadByte() / 255.0, (double)(int)reader.ReadByte() / 255.0, (double)(int)reader.ReadByte() / 255.0, (double)(int)reader.ReadByte() / 255.0);
							}
							else if (element.Usage == VertexElementUsage.RadiosityTexCoord)
							{
								reader.Position += element.Size;
							}
							else if (element.Usage == VertexElementUsage.DisplacementMapTexCoord)
							{
								reader.Position += element.Size;
							}
							else
							{
								VertexElementUsage usage = element.Usage;
								string pName = usage.ToString();
								if (element.Format == VertexElementFormat.Half2 || element.Format == VertexElementFormat.Float2)
								{
									if (!dictionary.ContainsKey(element.Usage))
									{
										array2[num] = new FbxLayerElementUV(fbxMesh, pName);
										array2[num].MappingMode = EMappingMode.eByControlPoint;
										array2[num].ReferenceMode = EReferenceMode.eDirect;
										dictionary.Add(element.Usage, num);
										num++;
									}
									float num7 = 0f;
									float num8 = 0f;
									if (element.Format == VertexElementFormat.Half2)
									{
										num7 = HalfUtils.Unpack(reader.ReadUShort());
										num8 = HalfUtils.Unpack(reader.ReadUShort());
									}
									else
									{
										num7 = reader.ReadFloat();
										num8 = reader.ReadFloat();
									}
									array2[dictionary[element.Usage]].DirectArray.Add(num7, num8);
								}
								else if (element.Format == VertexElementFormat.Half4 || element.Format == VertexElementFormat.Float4)
								{
									if (!dictionary2.ContainsKey(element.Usage))
									{
										array[num2] = new FbxLayerElementVertexColor(fbxMesh, pName);
										array[num2].MappingMode = EMappingMode.eByControlPoint;
										array[num2].ReferenceMode = EReferenceMode.eDirect;
										dictionary2.Add(element.Usage, num2);
										num2++;
									}
									float num9 = 0f;
									float num10 = 0f;
									float num11 = 0f;
									float num12 = 0f;
									if (element.Format == VertexElementFormat.Half4)
									{
										num9 = HalfUtils.Unpack(reader.ReadUShort());
										num10 = HalfUtils.Unpack(reader.ReadUShort());
										num11 = HalfUtils.Unpack(reader.ReadUShort());
										num12 = HalfUtils.Unpack(reader.ReadUShort());
									}
									else
									{
										num9 = reader.ReadFloat();
										num10 = reader.ReadFloat();
										num11 = reader.ReadFloat();
										num12 = reader.ReadFloat();
									}
									array[dictionary2[element.Usage]].DirectArray.Add(num9, num10, num11, num12);
								}
								else
								{
									reader.Position += element.Size;
								}
							}
						}
						num4 += element.Size;
					}
				}
				num3 += stream.VertexStride;
			}
			if (flag)
			{
				fbxLayerElementBinormal = new FbxLayerElementBinormal(fbxMesh, "");
				fbxLayerElementBinormal.MappingMode = EMappingMode.eByControlPoint;
				fbxLayerElementBinormal.ReferenceMode = EReferenceMode.eDirect;
				for (int l = 0; l < section.VertexCount; l++)
				{
					fbxLayerElementNormal.DirectArray.GetAt(l, out Vector4 outValue);
					fbxLayerElementTangent.DirectArray.GetAt(l, out Vector4 outValue2);
					val = new Vector3(outValue.X, outValue.Y, outValue.Z);
					val2 = new Vector3(outValue2.X, outValue2.Y, outValue2.Z);
					val3 = Vector3.Cross(val2, val) * list[l];
					fbxLayerElementBinormal.DirectArray.Add(val3.X, val3.Y, val3.Z);
				}
			}
			reader.Position = indicesOffset + section.StartIndex * indexSize;
			for (int m = 0; m < section.PrimitiveCount; m++)
			{
				fbxMesh.BeginPolygon(0);
				for (int n = 0; n < 3; n++)
				{
					int index = (int)((indexSize == 2) ? reader.ReadUShort() : reader.ReadUInt());
					fbxMesh.AddPolygon(index);
				}
				fbxMesh.EndPolygon();
			}
			for (int num13 = 0; num13 < 15; num13++)
			{
				FbxLayer layer = fbxMesh.GetLayer(num13);
				if (layer == null)
				{
					fbxMesh.CreateLayer();
					layer = fbxMesh.GetLayer(num13);
				}
				if (num13 == 0)
				{
					if (fbxLayerElementNormal != null)
					{
						layer.SetNormals(fbxLayerElementNormal);
					}
					if (fbxLayerElementTangent != null)
					{
						layer.SetTangents(fbxLayerElementTangent);
					}
					if (fbxLayerElementBinormal != null)
					{
						layer.SetBinormals(fbxLayerElementBinormal);
					}
				}
				if (array[num13] != null)
				{
					layer.SetVertexColors(array[num13]);
				}
				if (array2[num13] != null)
				{
					layer.SetUVs(array2[num13]);
				}
			}
			fbxNode.SetNodeAttribute(fbxMesh);
			return fbxNode;
		}

		private void FBXAddNodeRecursively(List<FbxNode> nodes, FbxNode node)
		{
			if (node != null)
			{
				FBXAddNodeRecursively(nodes, node.GetParent());
				if (!nodes.Contains(node))
				{
					nodes.Add(node);
				}
			}
		}
	}

	internal class FbxExporter : FbxObject
	{
		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxExporter@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
		private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Initialize@FbxExporter@fbxsdk@@UEAA_NPEBDHPEAVFbxIOSettings@2@@Z")]
		private static extern bool InitializeInternal(IntPtr InHandle, [MarshalAs(UnmanagedType.LPStr)] string pFileName, int pFileFormat, IntPtr pIOSettings);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Export@FbxExporter@fbxsdk@@QEAA_NPEAVFbxDocument@2@_N@Z")]
		private static extern bool ExportInternal(IntPtr InHandle, IntPtr pDocument, bool pNonBlocking);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetFileExportVersion@FbxExporter@fbxsdk@@QEAA_NVFbxString@2@W4ERenamingMode@FbxSceneRenamer@2@@Z")]
		private static extern bool SetFileExportVersionInternal(IntPtr InHandle, IntPtr pVersion, int pRenamingMode);

		public FbxExporter(FbxManager Manager, string Name)
		{
			pHandle = CreateFromManager(Manager.Handle, Name);
		}

		public bool Initialize(string pFileName, int pFileFormat = -1, FbxIOSettings pIOSettings = null)
		{
			IntPtr pIOSettings2 = (pIOSettings != null) ? pIOSettings.Handle : IntPtr.Zero;
			return InitializeInternal(pHandle, pFileName, pFileFormat, pIOSettings2);
		}

		public bool Export(FbxDocument pDocument, bool pNonBlocking = false)
		{
			return ExportInternal(pHandle, pDocument.Handle, pNonBlocking);
		}

		public bool SetFileExportVersion(string pVersion)
		{
			IntPtr pVersion2 = FbxString.Construct(pVersion);
			return SetFileExportVersionInternal(pHandle, pVersion2, 0);
		}
	}

}
