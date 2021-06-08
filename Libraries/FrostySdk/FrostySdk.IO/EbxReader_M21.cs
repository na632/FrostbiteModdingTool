using FrostySdk.Attributes;
using FrostySdk.Ebx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FrostySdk.IO
{
    public class EbxReader_M21 : EbxReaderV2
    {
        public EbxReader_M21(Stream InStream, bool inPatched, string name = "FromCas")
            : base(InStream, inPatched)
        {
        }
    }
}
//	{
//		public override string RootType
//		{
//			get
//			{
//				if (classGuids.Count > 0 && instances.Count > 0)
//				{
//					Type type = TypeLibrary.GetType(classGuids[instances[0].ClassRef]);
//					if (type != null)
//					{
//						return type.Name;
//					}
//				}
//				return string.Empty;
//				//return ClassTypes[instances[0].ClassRef].Name;
//			}
//		}

//		protected string InBoundName { get; private set; }
//		protected Type ConcreteTypeOfEbx { get; private set; }
//		protected Object ConcreteObjectOfEbx { get; private set; }


//		public EbxReader_M21(Stream InStream, FileSystem fs, bool inPatched, string name = "FromCas")
//			: base(InStream, fs, inPatched)
//		{
//			InBoundName = name;

//			FileStream fileStreamFromCas = new FileStream($"Debugging/EBX/{name}.dat", FileMode.OpenOrCreate);
//			var pos = InStream.Position;
//			InStream.Position = 0;
//			InStream.CopyTo(fileStreamFromCas);
//			InStream.Position = pos;
//			fileStreamFromCas.Close();
//			fileStreamFromCas.Dispose();

//			if (TypeLibrary.Reflection.ConcreteTypes != null && TypeLibrary.Reflection.ConcreteTypes.Any(x => x.Name == InBoundName))
//			{
//				ConcreteTypeOfEbx = TypeLibrary.Reflection.ConcreteTypes.FirstOrDefault(x => x.Name == InBoundName);
//				ConcreteObjectOfEbx = TypeLibrary.CreateObject(ConcreteTypeOfEbx);
//			}

//			if (std == null || patchStd == null)
//			{
//				std = new EbxSharedTypeDescriptors(fs, "SharedTypeDescriptors.ebx", patch: false);
//				if (fs.HasFileInMemoryFs("SharedTypeDescriptors_Patch.ebx"))
//				{
//					patchStd = new EbxSharedTypeDescriptors(fs, "SharedTypeDescriptors_Patch.ebx", patch: true);
//				}
//			}

//			isValid = true;
//		}

//		public override void InternalReadObjects()
//		{
//			new List<Type>();
//			foreach (EbxInstance instance in instances)
//			{
//				Type type = TypeLibrary.GetType(classGuids[instance.ClassRef]);
//				for (int i = 0; i < instance.Count; i++)
//				{
//					objects.Add(TypeLibrary.CreateObject(type));
//					refCounts.Add(0);
//				}
//			}
//			int num = 0;
//			int num2 = 0;
//			foreach (EbxInstance instance2 in instances)
//			{
//				for (int j = 0; j < instance2.Count; j++)
//				{
//					dynamic val = objects[num++];
//					Type objType = val.GetType();
//					EbxClass @class = GetClass(objType);
//					while (Position % (long)@class.Alignment != 0L)
//					{
//						Position++;
//					}
//					Guid inGuid = Guid.Empty;
//					if (instance2.IsExported)
//					{
//						inGuid = ReadGuid();
//						if (inGuid == Guid.Empty)
//							break;
//					}
//					if (@class.Alignment != 4)
//					{
//						Position += 8L;
//					}
//					val.SetInstanceGuid(new AssetClassGuid(inGuid, num2++));
//					this.ReadClass(@class, val, Position - 8);
//				}
//			}
//		}
//	}


//}