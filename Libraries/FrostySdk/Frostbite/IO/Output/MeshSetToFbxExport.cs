using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FrostbiteSdk;
using FrostbiteSdk.FbxExporter;
using FrostySdk;
using FrostySdk.Frosty;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
//using SharpDX;

namespace FrostySdk.Frostbite.IO.Output
{
	public class MeshSetToFbxExport
	{
		private int totalExportCount;

		private int currentProgress;

		private int boneCount;

		private bool flattenHierarchy;

		private FbxGeometryConverter geomConverter;

		private int lodIndex;

		public void Export(AssetManager assetManager, dynamic meshAsset, string filename, string fbxVersion, string units, bool flattenHierarchy, string skeleton, string fileType, params MeshSet[] meshSets)
		{
			if (assetManager == null)
			{
				throw new ArgumentNullException("assetManager");
			}
			if (meshAsset == null)
			{
				throw new ArgumentNullException("meshAsset");
			}
			if (filename == null)
			{
				throw new ArgumentNullException("filename");
			}
			if (fbxVersion == null)
			{
				throw new ArgumentNullException("fbxVersion");
			}
			if (units == null)
			{
				throw new ArgumentNullException("units");
			}
			if (fileType == null)
			{
				throw new ArgumentNullException("fileType");
			}
			this.flattenHierarchy = flattenHierarchy;
			using FbxManager fbxManager = new FbxManager();
			FbxIOSettings fbxIOSettings = new FbxIOSettings(fbxManager, "IOSRoot");
			fbxIOSettings.SetBoolProp("Export|AdvOptGrp|Fbx|Material", pValue: false);
			fbxIOSettings.SetBoolProp("Export|AdvOptGrp|Fbx|Texture", pValue: false);
			fbxIOSettings.SetBoolProp("Export|AdvOptGrp|Fbx|Global_Settings", pValue: true);
			fbxIOSettings.SetBoolProp("Export|AdvOptGrp|Fbx|Shape", pValue: true);
			fbxManager.SetIOSettings(fbxIOSettings);
			FbxScene fbxScene = new FbxScene(fbxManager, "")
			{
				SceneInfo = new FbxDocumentInfo(fbxManager, "SceneInfo")
				{
					Title = "FBX Exporter",
					Subject = "Export FBX meshes from",
					OriginalApplicationVendor = string.Empty,
					OriginalApplicationName = string.Empty,
					OriginalApplicationVersion = string.Empty,
					LastSavedApplicationVendor = string.Empty,
					LastSavedApplicationName = string.Empty,
					LastSavedApplicationVersion = string.Empty,
				}
			};
			geomConverter = new FbxGeometryConverter(fbxManager);
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
			MeshSet[] array = meshSets;
			for (int k = 0; k < array.Length; k++)
			{
				foreach (MeshSetLod lod in array[k].Lods)
				{
					foreach (MeshSetSection section in lod.Sections)
					{
						totalExportCount += ((section.Name != "") ? 1 : 0);
					}
				}
			}
			List<FbxNode> boneNodes = new List<FbxNode>();
			if (meshSets[0].Lods[0].Type == MeshType.MeshType_Skinned && !string.IsNullOrEmpty(skeleton))
			{
				FbxNode pNode = this.FBXCreateSkeleton(assetManager, fbxScene, meshAsset, skeleton, ref boneNodes);
				fbxScene.RootNode.AddChild(pNode);
			}
			currentProgress++;
			array = meshSets;
			foreach (MeshSet meshSet in array)
			{
				for (int i = 0; i < meshSet.Lods.Count; i++)
				{
					FBXCreateMesh(assetManager, fbxScene, meshSet.Lods[i], boneNodes);
				}
			}
			using FbxExporter fbxExporter = new FbxExporter(fbxManager, "");
			fbxExporter.SetFileExportVersion("FBX" + fbxVersion + "00");
			int writerFormatCount = fbxManager.IOPluginRegistry.WriterFormatCount;
			for (int j = 0; j < writerFormatCount; j++)
			{
				if (fbxManager.IOPluginRegistry.GetWriterFormatDescription(j).Contains(fileType))
				{
					if (fbxExporter.Initialize(filename, j, fbxIOSettings))
					{
						fbxExporter.Export(fbxScene);
					}
					break;
				}
			}
			geomConverter.Dispose();
		}

		private FbxNode FBXCreateCompositeSkeleton(FbxScene scene, List<LinearTransform> partTransforms, ref List<FbxNode> boneNodes)
		{
			int num = 0;
			FbxSkeleton fbxSkeleton = new FbxSkeleton(scene, "ROOT");
			fbxSkeleton.SetSkeletonType(FbxSkeleton.EType.eRoot);
			fbxSkeleton.Size = 1.0;
			FbxNode fbxNode = new FbxNode(scene, "ROOT");
			fbxNode.SetNodeAttribute(fbxSkeleton);
			Matrix4x4 identity = Matrix4x4.Identity;
			Vector3 translationVector = identity.Translation;
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
				fbxNode2.LclTranslation = new Vector3(translationVector.X, translationVector.Y, translationVector.Z);
				fbxNode2.LclRotation = new Vector3(val.X, val.Y, val.Z);
				boneNodes.Add(fbxNode2);
				fbxNode.AddChild(fbxNode2);
				num++;
			}
			return fbxNode;
		}

		private FbxNode FBXCreateSkeleton(AssetManager assetManager, FbxScene scene, dynamic meshAsset, string skeleton, ref List<FbxNode> boneNodes)
		{
			dynamic rootObject = assetManager.GetEbx(assetManager.GetEbxEntry(skeleton)).RootObject;
			boneCount = rootObject.BoneNames.Count;
			for (int i = 0; i < boneCount; i++)
			{
				dynamic val = rootObject.LocalPose;
				Matrix4x4 j = new Matrix4x4(val[i].right.x, val[i].right.y, val[i].right.z, 0f, val[i].up.x, val[i].up.y, val[i].up.z, 0f, val[i].forward.x, val[i].forward.y, val[i].forward.z, 0f, val[i].trans.x, val[i].trans.y, val[i].trans.z, 1f);
				Vector3 translationVector = j.Translation;
				Vector3 val2 = SharpDXUtils.ExtractEulerAngles(j);
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

		private void FBXCreateMesh(AssetManager assetManager, FbxScene scene, MeshSetLod lod, List<FbxNode> boneNodes)
		{
			if (assetManager == null)
			{
				throw new ArgumentNullException("assetManager");
			}
			if (scene == null)
			{
				throw new ArgumentNullException("scene");
			}
			if (lod == null)
			{
				throw new ArgumentNullException("lod");
			}
			if (boneNodes == null)
			{
				throw new ArgumentNullException("boneNodes");
			}
			int indexSize = lod.IndexUnitSize / 8;
			FbxNode fbxNode = (flattenHierarchy ? scene.RootNode : new FbxNode(scene, lod.String03));
			foreach (MeshSetSection section in lod.Sections)
			{
				if (section.Name == "")
				{
					continue;
				}
				NativeReader reader = new NativeReader((lod.ChunkId != Guid.Empty) ? assetManager.GetChunk(assetManager.GetChunkEntry(lod.ChunkId)) : new MemoryStream(lod.InlineData));
				FbxNode fbxNode2 = FBXExportSubObject(scene, section, lod.VertexBufferSize, indexSize, reader);
				if (flattenHierarchy)
				{
					fbxNode2.Name = lod.String03 + ":" + section.Name;
				}
				fbxNode.AddChild(fbxNode2);
				if ((lod.Type != MeshType.MeshType_Skinned && lod.Type != MeshType.MeshType_Composite) || boneNodes.Count == 0)
				{
					continue;
				}
				List<ushort> list = section.BoneList;
				if (lod.Type == MeshType.MeshType_Composite && lod.PartTransforms.Count != 0)
				{
					list = new List<ushort>();
					for (ushort num = 0; num < lod.PartTransforms.Count; num = (ushort)(num + 1))
					{
						list.Add(num);
					}
				}
				list.Clear();
				for (ushort num2 = 0; num2 < boneNodes.Count; num2 = (ushort)(num2 + 1))
				{
					list.Add(num2);
				}
				FBXCreateSkin(scene, section, fbxNode2, boneNodes, list, lod.Type, reader);
				FBXCreateBindPose(scene, section, fbxNode2);
			}
			if (!flattenHierarchy)
			{
				scene.RootNode.AddChild(fbxNode);
			}
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
				FbxPose fbxPose = new FbxPose(scene, section.Name)
				{
					IsBindPose = true
				};
				for (int l = 0; l < list.Count; l++)
				{
					FbxNode fbxNode = list[l];
					FbxMatrix matrix = new FbxMatrix(fbxNode.EvaluateGlobalTransform());
					fbxPose.Add(fbxNode, matrix);
				}
				scene.AddPose(fbxPose);
			}
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
									array[3] = reader.ReadUInt16LittleEndian();
									array[2] = reader.ReadUInt16LittleEndian();
									array[1] = reader.ReadUInt16LittleEndian();
									array[0] = reader.ReadUInt16LittleEndian();
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
									array[7] = reader.ReadUInt16LittleEndian();
									array[6] = reader.ReadUInt16LittleEndian();
									array[5] = reader.ReadUInt16LittleEndian();
									array[4] = reader.ReadUInt16LittleEndian();
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
						num3 = boneList[num3];
						if (((uint)num3 & 0x8000u) != 0)
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
			int num8 = 0;
			int num9 = 0;
			bool flag = false;
			bool flag2 = false;
			Vector3 val = default(Vector3);
			Vector3 val2 = default(Vector3);
			Vector3 val3 = default(Vector3);
			List<float> list = new List<float>();
			List<object> list2 = new List<object>();
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
					int num10 = 0;
					GeometryDeclarationDesc.Element[] elements = section.GeometryDeclDesc[0].Elements;
					for (int k = 0; k < elements.Length; k++)
					{
						GeometryDeclarationDesc.Element element = elements[k];
						if (element.Usage == VertexElementUsage.Unknown)
						{
							continue;
						}
						if (num10 >= num9 && num10 < num9 + stream.VertexStride)
						{
							if (element.Usage == VertexElementUsage.Pos)
							{
								double* ptr = (double*)(void*)controlPoints;
								if (element.Format == VertexElementFormat.Float3)
								{
									ptr[j * 4] = reader.ReadSingleLittleEndian();
									ptr[j * 4 + 1] = reader.ReadSingleLittleEndian();
									ptr[j * 4 + 2] = reader.ReadSingleLittleEndian();
								}
								else if (element.Format == VertexElementFormat.Half3 || element.Format == VertexElementFormat.Half4)
								{
									ptr[j * 4] = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
									ptr[j * 4 + 1] = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
									ptr[j * 4 + 2] = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
									if (element.Format == VertexElementFormat.Half4)
									{
										reader.ReadUInt16LittleEndian();
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
										val2.X = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
										val2.Y = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
										val2.Z = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
										list.Add(HalfUtils.Unpack(reader.ReadUInt16LittleEndian()));
									}
									else
									{
										val2.X = reader.ReadSingleLittleEndian();
										val2.Y = reader.ReadSingleLittleEndian();
										val2.Z = reader.ReadSingleLittleEndian();
										list.Add(reader.ReadSingleLittleEndian());
									}
								}
								else if (element.Format == VertexElementFormat.Float)
								{
									list.Add(reader.ReadSingleLittleEndian());
								}
								else
								{
									list.Add(HalfUtils.Unpack(reader.ReadUInt16LittleEndian()));
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
								val.X = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								val.Y = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								val.Z = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								fbxLayerElementNormal.DirectArray.Add(val.X, val.Y, val.Z);
								val3.Y = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
							}
							else if (element.Usage == VertexElementUsage.Binormal)
							{
								if (fbxLayerElementBinormal == null)
								{
									fbxLayerElementBinormal = new FbxLayerElementBinormal(fbxMesh, "");
									fbxLayerElementBinormal.MappingMode = EMappingMode.eByControlPoint;
									fbxLayerElementBinormal.ReferenceMode = EReferenceMode.eDirect;
								}
								val3.X = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								val3.Y = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								val3.Z = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								fbxLayerElementBinormal.DirectArray.Add(val3.X, val3.Y, val3.Z);
								reader.ReadUInt16LittleEndian();
							}
							else if (element.Usage == VertexElementUsage.Tangent)
							{
								if (fbxLayerElementTangent == null)
								{
									fbxLayerElementTangent = new FbxLayerElementTangent(fbxMesh, "");
									fbxLayerElementTangent.MappingMode = EMappingMode.eByControlPoint;
									fbxLayerElementTangent.ReferenceMode = EReferenceMode.eDirect;
								}
								val2.X = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								val2.Y = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								val2.Z = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								fbxLayerElementTangent.DirectArray.Add(val2.X, val2.Y, val2.Z);
								val3.Z = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
							}
							else if (element.Usage == VertexElementUsage.TangentSpace)
							{
								if (fbxLayerElementTangent == null)
								{
									fbxLayerElementNormal = new FbxLayerElementNormal(fbxMesh, "");
									fbxLayerElementNormal.MappingMode = EMappingMode.eByControlPoint;
									fbxLayerElementNormal.ReferenceMode = EReferenceMode.eDirect;
									fbxLayerElementTangent = new FbxLayerElementTangent(fbxMesh, "");
									fbxLayerElementTangent.MappingMode = EMappingMode.eByControlPoint;
									fbxLayerElementTangent.ReferenceMode = EReferenceMode.eDirect;
									fbxLayerElementBinormal = new FbxLayerElementBinormal(fbxMesh, "");
									fbxLayerElementBinormal.MappingMode = EMappingMode.eByControlPoint;
									fbxLayerElementBinormal.ReferenceMode = EReferenceMode.eDirect;
								}
								if (element.Format == VertexElementFormat.UByte4N)
								{
									list2.Add(new Vector4((float)(int)reader.ReadByte() / 255f, (float)(int)reader.ReadByte() / 255f, (float)(int)reader.ReadByte() / 255f, (float)(int)reader.ReadByte() / 255f));
								}
								else if (element.Format == VertexElementFormat.UShort4N)
								{
									list2.Add(new Vector4((float)(int)reader.ReadUInt16LittleEndian() / 65535f, (float)(int)reader.ReadUInt16LittleEndian() / 65535f, (float)(int)reader.ReadUInt16LittleEndian() / 65535f, (float)(int)reader.ReadUInt16LittleEndian() / 65535f));
								}
								else if (element.Format == VertexElementFormat.UInt)
								{
									list2.Add(reader.ReadUInt32LittleEndian());
								}
								flag2 = true;
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
								float num11;
								float num12;
								if (element.Format == VertexElementFormat.Float2)
								{
									num11 = reader.ReadSingleLittleEndian();
									num12 = 1f - reader.ReadSingleLittleEndian();
								}
								else
								{
									num11 = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
									num12 = 1f - HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
								}
								array2[dictionary[element.Usage]].DirectArray.Add(num11, num12);
							}
							else if ((int)element.Usage >= 30 && (int)element.Usage <= 31)
							{
								if (!dictionary2.ContainsKey(element.Usage))
								{
									array[num8] = new FbxLayerElementVertexColor(fbxMesh, "VColor" + (num8 + 1));
									array[num8].MappingMode = EMappingMode.eByControlPoint;
									array[num8].ReferenceMode = EReferenceMode.eDirect;
									dictionary2.Add(element.Usage, num8);
									num8++;
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
								if (element.Format == VertexElementFormat.Half2 || element.Format == VertexElementFormat.Float2 || element.Format == VertexElementFormat.Short2N)
								{
									if (!dictionary.ContainsKey(element.Usage))
									{
										array2[num] = new FbxLayerElementUV(fbxMesh, pName);
										array2[num].MappingMode = EMappingMode.eByControlPoint;
										array2[num].ReferenceMode = EReferenceMode.eDirect;
										dictionary.Add(element.Usage, num);
										num++;
									}
									float num13 = 0f;
									float num14 = 0f;
									if (element.Format == VertexElementFormat.Half2)
									{
										num13 = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
										num14 = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
									}
									else if (element.Format == VertexElementFormat.Float2)
									{
										num13 = reader.ReadSingleLittleEndian();
										num14 = reader.ReadSingleLittleEndian();
									}
									else
									{
										num13 = reader.ReadSingleLittleEndian() / 32767f * 0.5f + 0.5f;
										num14 = reader.ReadSingleLittleEndian() / 32767f * 0.5f + 0.5f;
									}
									array2[dictionary[element.Usage]].DirectArray.Add(num13, num14);
								}
								else if (element.Format == VertexElementFormat.Half4 || element.Format == VertexElementFormat.Float4)
								{
									if (!dictionary2.ContainsKey(element.Usage))
									{
										array[num8] = new FbxLayerElementVertexColor(fbxMesh, pName);
										array[num8].MappingMode = EMappingMode.eByControlPoint;
										array[num8].ReferenceMode = EReferenceMode.eDirect;
										dictionary2.Add(element.Usage, num8);
										num8++;
									}
									float num15 = 0f;
									float num2 = 0f;
									float num3 = 0f;
									float num4 = 0f;
									if (element.Format == VertexElementFormat.Half4)
									{
										num15 = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
										num2 = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
										num3 = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
										num4 = HalfUtils.Unpack(reader.ReadUInt16LittleEndian());
									}
									else
									{
										num15 = reader.ReadSingleLittleEndian();
										num2 = reader.ReadSingleLittleEndian();
										num3 = reader.ReadSingleLittleEndian();
										num4 = reader.ReadSingleLittleEndian();
									}
									array[dictionary2[element.Usage]].DirectArray.Add(num15, num2, num3, num4);
								}
								else if (element.Format == VertexElementFormat.Float)
								{
									if (!dictionary2.ContainsKey(element.Usage))
									{
										array[num8] = new FbxLayerElementVertexColor(fbxMesh, pName);
										array[num8].MappingMode = EMappingMode.eByControlPoint;
										array[num8].ReferenceMode = EReferenceMode.eDirect;
										dictionary2.Add(element.Usage, num8);
										num8++;
									}
									float num5 = reader.ReadSingleLittleEndian();
									array[dictionary2[element.Usage]].DirectArray.Add(num5, 0.0, 0.0);
								}
								else if (element.Format == VertexElementFormat.Int)
								{
									if (!dictionary2.ContainsKey(element.Usage))
									{
										array[num8] = new FbxLayerElementVertexColor(fbxMesh, pName);
										array[num8].MappingMode = EMappingMode.eByControlPoint;
										array[num8].ReferenceMode = EReferenceMode.eDirect;
										dictionary2.Add(element.Usage, num8);
										num8++;
									}
									int num7 = reader.ReadInt32LittleEndian();
									array[dictionary2[element.Usage]].DirectArray.Add((float)num7 / 255f, 0.0, 0.0);
								}
								else
								{
									reader.Position += element.Size;
								}
							}
						}
						num10 += element.Size;
					}
				}
				num9 += stream.VertexStride;
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
					int index = (int)((indexSize == 2) ? reader.ReadUInt16LittleEndian() : reader.ReadUInt32LittleEndian());
					fbxMesh.AddPolygon(index);
				}
				fbxMesh.EndPolygon();
			}
			for (int num6 = 0; num6 < 15; num6++)
			{
				FbxLayer layer = fbxMesh.GetLayer(num6);
				if (layer == null)
				{
					fbxMesh.CreateLayer();
					layer = fbxMesh.GetLayer(num6);
				}
				if (num6 == 0)
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
				if (array[num6] != null)
				{
					layer.SetVertexColors(array[num6]);
				}
				if (array2[num6] != null)
				{
					layer.SetUVs(array2[num6]);
				}
			}
			fbxMesh.BuildMeshEdgeArray();
			geomConverter.ComputeEdgeSmoothingFromNormals(fbxMesh);
			fbxNode.SetNodeAttribute(fbxMesh);
			return fbxNode;
		}

		public MeshSet LoadMeshSet(AssetManager assetManager, object rootObject)
		{
			MeshSet meshSet = new MeshSet(assetManager.GetRes(assetManager.GetResEntry(((dynamic)rootObject).Name)));
			return meshSet;
		}
		public MeshSet LoadMeshSet(EbxAssetEntry ebx)
		{
			EbxAsset ebxAsset = AssetManager.Instance.GetEbx(ebx);
			return LoadMeshSet(AssetManager.Instance, ebxAsset.RootObject);
		}
	}
}
