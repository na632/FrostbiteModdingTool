using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxDocumentInfo : FbxObject
	{
		private IntPtr mOriginal_ApplicationVendor;

		private IntPtr mOriginal_ApplicationName;

		private IntPtr mOriginal_ApplicationVersion;

		private IntPtr mLastSaved_ApplicationVendor;

		private IntPtr mLastSaved_ApplicationName;

		private IntPtr mLastSaved_ApplicationVersion;

		private IntPtr mTitle;

		private IntPtr mSubject;

		public string OriginalApplicationVendor
		{
			get
			{
				return FbxProperty.GetString(mOriginal_ApplicationVendor);
			}
			set
			{
				FbxProperty.Set(mOriginal_ApplicationVendor, value);
			}
		}

		public string OriginalApplicationName
		{
			get
			{
				return FbxProperty.GetString(mOriginal_ApplicationName);
			}
			set
			{
				FbxProperty.Set(mOriginal_ApplicationName, value);
			}
		}

		public string OriginalApplicationVersion
		{
			get
			{
				return FbxProperty.GetString(mOriginal_ApplicationVersion);
			}
			set
			{
				FbxProperty.Set(mOriginal_ApplicationVersion, value);
			}
		}

		public string LastSavedApplicationVendor
		{
			get
			{
				return FbxProperty.GetString(mLastSaved_ApplicationVendor);
			}
			set
			{
				FbxProperty.Set(mLastSaved_ApplicationVendor, value);
			}
		}

		public string LastSavedApplicationName
		{
			get
			{
				return FbxProperty.GetString(mLastSaved_ApplicationName);
			}
			set
			{
				FbxProperty.Set(mLastSaved_ApplicationName, value);
			}
		}

		public string LastSavedApplicationVersion
		{
			get
			{
				return FbxProperty.GetString(mLastSaved_ApplicationVersion);
			}
			set
			{
				FbxProperty.Set(mLastSaved_ApplicationVersion, value);
			}
		}

		public string Title
		{
			get
			{
				return FbxString.Get(mTitle);
			}
			set
			{
				FbxString.Assign(mTitle, value);
			}
		}

		public string Subject
		{
			get
			{
				return FbxString.Get(mSubject);
			}
			set
			{
				FbxString.Assign(mSubject, value);
			}
		}

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxDocumentInfo@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
		private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

		public FbxDocumentInfo(IntPtr handle)
			: base(handle)
		{
			mOriginal_ApplicationVendor = pHandle + 168;
			mOriginal_ApplicationName = pHandle + 184;
			mOriginal_ApplicationVersion = pHandle + 200;
			mLastSaved_ApplicationVendor = pHandle + 264;
			mLastSaved_ApplicationName = pHandle + 280;
			mLastSaved_ApplicationVersion = pHandle + 296;
			mTitle = pHandle + 344;
			mSubject = pHandle + 352;
		}

		public FbxDocumentInfo(FbxManager Manager, string Name)
			: this(CreateFromManager(Manager.Handle, Name))
		{
		}
	}
}
