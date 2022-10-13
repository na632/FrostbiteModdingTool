//// (c) Electronic Arts.  All Rights Reserved.

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using EA.Ant.Tool.Data;
//using EA.Blox.Extensions;
//using EA.Blox.PropertyEditor;
//using EA.Granite;

//namespace EA.Ant.Tool
//{
//    public class TOCEntryProxyCollection : IPropertyEditorSelection
//    {
//		public const string AssetCategory = "Asset";
//        public const string AdvancedCategory = "Advanced";
//		public const string MiscCategory = "Misc";

//        private TOCEntryCollection mEntries;
//        private static IComparer<string> sCategorizer = new SortByCategory();

//        private class SortByCategory : IComparer<string>
//        {
//            const int XbeforeY = -1;
//            const int XafterY = 1;
//            const int YbeforeX = 1;
//            const int YafterX = -1;

//            public int Compare(string x, string y)
//            {
//                if (x == y)
//                    return 0;
//                // Asset is always first, then Misc is second last, and Advanced is last
//                if (x == AssetCategory)
//                {
//                    return XbeforeY;
//                }
//                if (x == AdvancedCategory)
//                {
//                    return XafterY;
//                }
//                // negate for y case
//                if (y == AssetCategory)
//                {
//                    return YbeforeX;
//                }
//                // negate for y case
//                if (y == AdvancedCategory)
//                {
//                    return YafterX;
//                }
//                // y is not AdvancedCategory, so x is after y
//                if (x == MiscCategory)
//                {
//                    return XafterY;
//                }
//                // negate y case - x is not AdvancedCategory, so y is after x
//                if (y == MiscCategory)
//                {
//                    return YafterX;
//                }
//                // some other categories - sort by name
//                return x.CompareTo(y);
//            }
//        }

//        public TOCEntryProxyCollection(IEnumerable<Granite.IKey> selection)
//        {
//            mEntries = new TOCEntryCollection(selection);
//        }

//        public TOCEntryProxyCollection(TOCEntryCollection col)
//        {
//            mEntries = col;
//        }

//        //public event EventHandler Cancelled;

//        public TOCEntryCollection Entries
//        {
//            get { return mEntries; }
//            set { mEntries = value; }
//        }

//        public string GetClassName()
//        {
//            Refresh();

//            if (mEntries.Count == 1)
//            {
//                return mEntries[0].DisplayName;
//            }
//            return mEntries.Count.ToString() + " items";
//        }

//        public override string ToString()
//        {
//            return GetClassName();
//        }

//        public string DisplayName
//        {
//            get
//            {
//                return GetClassName();
//            }
//        }

//        public bool MergeValues(object a, object b, out object merged)
//        {
//            AssetRef ar = a as AssetRef;
//            if (ar != null) a = ar.RefGUID;
//            AssetRef br = b as AssetRef;
//            if (ar != null && br != null)
//            {
//                merged = ar.RefGUID.Equals(br.RefGUID) ? a : null;
//                return true;
//            }
//            merged = null;
//            return false;
//        }

//        public void Refresh()
//        {
//            bool found = false;
//            foreach (TOCEntry e in mEntries)
//            {
//                if (e.Type == null || e.Deleted)
//                {
//                    found = true;
//                    break;
//                }
//            }
//            if (!found) return;
//            TOCEntryCollection copy = new TOCEntryCollection();
//            foreach (TOCEntry e in mEntries)
//            {
//                if (e.Type == null || e.Deleted) continue;
//                copy.Add(e);
//            }
//            mEntries = copy;
//        }

//        public int Count
//        {
//            get { return mEntries.Count; }
//        }

//        public object this[int index]
//        {
//            get
//            {
//                return mEntries[index].LoadAsset();
//            }
//        }

//        public IEnumerator<object> GetEnumerator()
//        {
//            //ProgressDialog progress = ProgressDialog.CreateAndShowProgressDialog("Updating Properties", 0, mEntries.Count, true);
//            try
//            {
//                foreach (TOCEntry entry in mEntries)
//                {
//                    //bool cancel = progress.Progress();
//                    //if (cancel)
//                    //{
//                    //    Cancelled?.Invoke(this, EventArgs.Empty);
//                    //    break;
//                    //}

//                    yield return entry.LoadAsset();
//                }
//            }
//            finally
//            {
//                //ProgressDialog.CloseDialog(progress);
//            }
//        }


//        #region IEnumerable Members

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            foreach (TOCEntry entry in mEntries)
//            {
//                yield return entry.LoadAsset();
//            }
//        }

//        #endregion

//        public IComparer<string> Categorizer
//        {
//            get { return sCategorizer; }
//        }

//        public PropertyDescriptorCollection GetObjectProperties(ITypeDescriptorContext context, object obj, bool useDefault)
//        {
//            PropertyDescriptorCollection col = context.GetProperties(obj, new Attribute[] { }, useDefault);
//            return col;
//        }

//        public void SetValue(PropertyDescriptor propertyDescriptor, object component, object value)
//        {
//            AssetRef aref = value as AssetRef;
//            if (aref != null)
//            {
//                propertyDescriptor.SetValue(component, aref);
//                return;
//            }
//            propertyDescriptor.SetValue(component, value);
//        }
//    }
//}

