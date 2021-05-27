using FrostySdk.Attributes;
using FrostySdk.Ebx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FrostySdk.IO
{
	[Serializable]
	public class EbxAsset
	{
		internal Guid fileGuid;

		internal List<object> objects;

		internal List<Guid> dependencies;

		internal List<int> refCounts;

		private static Type PointerType = typeof(PointerRef);

		private static Type ValueType = typeof(ValueType);

		public Guid FileGuid => fileGuid;

		public Guid RootInstanceGuid => ((AssetClassGuid)((dynamic)RootObject).GetInstanceGuid()).ExportedGuid;

		public IEnumerable<Guid> Dependencies
		{
			get
			{
				for (int i = 0; i < dependencies.Count; i++)
				{
					yield return dependencies[i];
				}
			}
		}

		public IEnumerable<object> RootObjects
		{
			get
			{
				for (int i = 0; i < objects.Count; i++)
				{
					if (refCounts[i] == 0 || i == 0)
					{
						yield return objects[i];
					}
				}
			}
		}

		public IEnumerable<object> Objects
		{
			get
			{
				for (int i = 0; i < objects.Count; i++)
				{
					yield return objects[i];
				}
			}
		}

		public IEnumerable<object> ExportedObjects
		{
			get
			{
				for (int i = 0; i < objects.Count; i++)
				{
					dynamic val = objects[i];
					if (((AssetClassGuid)val.GetInstanceGuid()).IsExported)
					{
						yield return val;
					}
				}
			}
		}

		public object RootObject => objects.Any() ? objects[0] : null;

		public bool IsValid => objects.Count != 0;

		public bool TransientEdit
		{
			get;
			set;
		}


		public EbxAsset()
		{
		}

		public EbxAsset(params object[] rootObjects)
		{
			fileGuid = Guid.NewGuid();
			objects = new List<object>();
			refCounts = new List<int>();
			dependencies = new List<Guid>();
			if (rootObjects != null)
			{
				foreach (dynamic val in rootObjects)
				{
					val.SetInstanceGuid(new AssetClassGuid(Guid.NewGuid(), objects.Count));
					objects.Add(val);
					refCounts.Add(0);
				}
			}
		}

		public dynamic GetObject(Guid guid)
		{
			foreach (dynamic exportedObject in ExportedObjects)
			{
				if (exportedObject.GetInstanceGuid() == guid)
				{
					return exportedObject;
				}
			}
			return null;
		}

		public bool AddDependency(Guid guid)
		{
			if (dependencies.Contains(guid))
			{
				return false;
			}
			dependencies.Add(guid);
			return true;
		}

		public void SetFileGuid(Guid guid)
		{
			fileGuid = guid;
		}

		//[Deprecated("This should only be used for testing Editor controls", )]
		/// <summary>
		/// DO NOT USE , other than for testing
		/// </summary>
		/// <param name="obj"></param>
		public void SetRootObject(dynamic obj)
		{
			objects = new List<Object>();
			objects.Add(obj);
		}

		public void AddObject(dynamic obj, bool root = false)
		{
			AssetClassGuid assetClassGuid = obj.GetInstanceGuid();
			if (assetClassGuid.InternalId == -1)
			{
				assetClassGuid = new AssetClassGuid(assetClassGuid.ExportedGuid, objects.Count);
				obj.SetInstanceGuid(assetClassGuid);
			}
			refCounts.Add(1);
			objects.Add(obj);
		}

		public void AddRootObject(dynamic obj)
		{
			Type type = obj.GetType();
			AssetClassGuid assetClassGuid = new AssetClassGuid(Utils.GenerateDeterministicGuid(objects, type, fileGuid), objects.Count);
			obj.SetInstanceGuid(assetClassGuid);
			if (objects.Contains(obj))
			{
				int index = objects.IndexOf(obj);
				refCounts[index] = 0;
			}
			else
			{
				refCounts.Add(0);
				objects.Add(obj);
			}
		}

		public void RemoveObject(object obj)
		{
			int num = objects.IndexOf(obj);
			if (num != -1)
			{
				refCounts[num]--;
				if (refCounts[num] <= 0)
				{
					refCounts.RemoveAt(num);
					objects.RemoveAt(num);
				}
			}
		}

		public void Update()
		{
			dependencies.Clear();
			Dictionary<object, int> mapping = new Dictionary<object, int>();
			List<int> newRefCnts = new List<int>();
			for (int i = 0; i < objects.Count; i++)
			{
				mapping.Add(objects[i], i);
				newRefCnts.Add(0);
			}
			List<int> list = new List<int>();
			List<Tuple<PropertyInfo, object>> refProps = new List<Tuple<PropertyInfo, object>>();
			List<Tuple<object, Guid>> externalProps = new List<Tuple<object, Guid>>();
			List<object> objsToProcess = new List<object>();
			objsToProcess.AddRange(RootObjects);
			list.Add(0);
			while (objsToProcess.Count > 0)
			{
				int num = mapping[objsToProcess[0]];
				if (refCounts[num] == 0 && !list.Contains(num))
				{
					list.Add(num);
				}
				CountRefs(objsToProcess[0], objsToProcess[0], ref newRefCnts, ref mapping, ref refProps, ref externalProps, ref objsToProcess);
				objsToProcess.RemoveAt(0);
			}
			for (int num2 = newRefCnts.Count - 1; num2 >= 0; num2--)
			{
				if (!list.Contains(num2) && newRefCnts[num2] == 0)
				{
					newRefCnts.RemoveAt(num2);
					objects.RemoveAt(num2);
				}
			}
			foreach (Tuple<object, Guid> item in externalProps)
			{
				if (objects.Contains(item.Item1) && !dependencies.Contains(item.Item2))
				{
					dependencies.Add(item.Item2);
				}
			}
			foreach (Tuple<PropertyInfo, object> item2 in refProps)
			{
				if (item2.Item1.PropertyType == PointerType)
				{
					PointerRef pointerRef = (PointerRef)item2.Item1.GetValue(item2.Item2);
					if (!objects.Contains(pointerRef.Internal))
					{
						item2.Item1.SetValue(item2.Item2, default(PointerRef));
					}
				}
				else
				{
					IList list2 = (IList)item2.Item1.GetValue(item2.Item2);
					int count = list2.Count;
					bool flag = false;
					for (int j = 0; j < count; j++)
					{
						PointerRef pointerRef2 = (PointerRef)list2[j];
						if (pointerRef2.Type == PointerRefType.Internal && !objects.Contains(pointerRef2.Internal))
						{
							list2[j] = default(PointerRef);
							flag = true;
						}
					}
					if (flag)
					{
						item2.Item1.SetValue(item2.Item2, list2);
					}
				}
			}
			refCounts = newRefCnts;
		}

		private void CountRefs(object obj, object classObj, ref List<int> newRefCnts, ref Dictionary<object, int> mapping, ref List<Tuple<PropertyInfo, object>> refProps, ref List<Tuple<object, Guid>> externalProps, ref List<object> objsToProcess)
		{
			PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType.IsEnum)
				{
					continue;
				}
				Type propertyType = propertyInfo.PropertyType;
				if (propertyType == PointerType)
				{
					bool flag = propertyInfo.GetCustomAttribute<IsReferenceAttribute>() != null;
					PointerRef pointerRef = (PointerRef)propertyInfo.GetValue(obj);
					if (pointerRef.Type == PointerRefType.Internal)
					{
						if (flag)
						{
							refProps.Add(new Tuple<PropertyInfo, object>(propertyInfo, obj));
							continue;
						}
						int index = mapping[pointerRef.Internal];
						if (newRefCnts[index] == 0)
						{
							newRefCnts[index]++;
							objsToProcess.Add(pointerRef.Internal);
						}
					}
					else if (pointerRef.Type == PointerRefType.External)
					{
						externalProps.Add(new Tuple<object, Guid>(classObj, pointerRef.External.FileGuid));
					}
				}
				else if (propertyType.GenericTypeArguments.Length != 0)
				{
					Type left = propertyType.GenericTypeArguments[0];
					IList list = (IList)propertyInfo.GetValue(obj);
					int count = list.Count;
					if (count <= 0)
					{
						continue;
					}
					if (left == PointerType)
					{
						bool flag2 = propertyInfo.GetCustomAttribute<IsReferenceAttribute>() != null;
						if (flag2)
						{
							refProps.Add(new Tuple<PropertyInfo, object>(propertyInfo, obj));
						}
						for (int j = 0; j < count; j++)
						{
							PointerRef pointerRef2 = ((List<PointerRef>)list)[j];
							if (pointerRef2.Type == PointerRefType.Internal && !flag2)
							{
								int index2 = mapping[pointerRef2.Internal];
								if (newRefCnts[index2] == 0)
								{
									newRefCnts[index2]++;
									objsToProcess.Add(pointerRef2.Internal);
								}
							}
							else if (pointerRef2.Type == PointerRefType.External)
							{
								externalProps.Add(new Tuple<object, Guid>(classObj, pointerRef2.External.FileGuid));
							}
						}
					}
					else if (left != ValueType)
					{
						for (int k = 0; k < count; k++)
						{
							CountRefs(list[k], classObj, ref newRefCnts, ref mapping, ref refProps, ref externalProps, ref objsToProcess);
						}
					}
				}
				else if (propertyType != ValueType)
				{
					CountRefs(propertyInfo.GetValue(obj), classObj, ref newRefCnts, ref mapping, ref refProps, ref externalProps, ref objsToProcess);
				}
			}
		}
	}
}
