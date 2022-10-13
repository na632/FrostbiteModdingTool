//// (c) Electronic Arts.  All Rights Reserved.

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using EA.Granite;

//namespace EA.Ant.Tool.Data
//{
//    /// <summary>
//    /// Collection of <see cref="TOCEntry"/> items.
//    /// </summary>
//    public class TOCEntryCollection : IList<TOCEntry>, IEnumerable<Granite.IKey>
//    {
//        #region Members

//		private const uint HASHSET_THRESHOLD = 4;
//        private const uint ASSET_REMOVE_THRESHOLD = 3000;
//		private HashSet<Granite.IKey> mKeys = null;
//        protected List<TOCEntry> mEntries = new List<TOCEntry>();

//        public static readonly TOCEntryCollection Empty = new TOCEntryCollection();
//        public static event EventHandler<KeyResolvingEventArgs> KeyResolving;

//        #endregion

//        #region Constructors

//        /// <summary>
//        /// Initializes a new instance of the <see cref="TOCEntryCollection"/> class.
//        /// </summary>
//        public TOCEntryCollection()
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="TOCEntryCollection"/> class with the given keys
//        /// </summary>
//        /// <param name="keys">The keys.</param>
//        public TOCEntryCollection(IEnumerable<Granite.IKey> keys)
//        {
//            foreach (Granite.IKey key in keys)
//            {
//                TOCEntry entry = key as TOCEntry;
//                if (entry != null)
//                {
//                    AddIfUnique(entry);
//                }
//            }
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="TOCEntryCollection"/> class with the given TOC entries.
//        /// </summary>
//        /// <param name="entries">The entries.</param>
//        public TOCEntryCollection(IEnumerable<TOCEntry> entries)
//        {
//            foreach (TOCEntry entry in entries)
//            {
//                AddIfUnique(entry);
//            }
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="TOCEntryCollection"/> class from another <see cref="TOCEntryCollection"/>.
//        /// </summary>
//        /// <param name="entries">The entries.</param>
//        public TOCEntryCollection(TOCEntryCollection entries)
//        {
//            foreach (TOCEntry entry in entries)
//            {
//                AddIfUnique(entry);
//            }
//        }

//        #endregion


//        #region Public Methods

//        public Type GetAssetType(Granite.IKey guid)
//        {
//            TOCEntry a = Find(guid);
//            if (object.ReferenceEquals(null, a))
//                return null;
//            return a.Type;
//        }

//        public void Clear()
//        {
//            mEntries.Clear();
//			mKeys = null;
//        }

//        public void GetGuidsForType(HashSet<Granite.IKey> results, Type t, bool includeDerivedTypes)
//        {
//            foreach (TOCEntry e in mEntries)
//            {
//                if (e.Type == t || (includeDerivedTypes && e.IsA(t)))
//                {
//                    results.Add(e.GUID);
//                }
//            }
//        }

//        public TOCEntry this[int index]
//        {
//            get { return mEntries[index]; }
//            set
//            {
//				TOCEntry oldValue = mEntries[index];

//				if (mKeys != null)
//				{
//					if (mKeys.Contains(value.GUID) && IndexOf(Find(value.GUID)) != index)
//					{
//						System.Diagnostics.Debug.Fail("You shouldn't add the same entry in the TOCEntryCollection more than once.");
//						return;
//					}

//					if ((object)oldValue != null)
//					{
//						mKeys.Remove(oldValue.GUID);
//					}
//					mKeys.Add(value.GUID);
//				}
//				else
//				{
//					if (IndexOf(Find(value.GUID)) != index)
//					{
//						System.Diagnostics.Debug.Fail("You shouldn't add the same entry in the TOCEntryCollection more than once.");
//						return;
//					}
//				}

//                mEntries[index] = value;
//                Track(value);

//                if (null != oldValue && null != value)
//                {
//                    System.Diagnostics.Debug.Assert(oldValue.GUID == value.GUID, "Index shouldn't be used to replace assets.");
//                    return;
//                }
//            }
//        }

//        public void RemoveAt(int index)
//        {
//            Remove(mEntries[index].GUID);
//        }

//        public void RemoveIfExist(TOCEntryCollection assets)
//        {
//            if (this.Count == 0 || assets.Count == 0)
//                return;

//            // Not all assets are actually removed because some might not exist in the collection
//            // So this collection can be considered to be the assets we are actually removing.
//            // This is also the same set as the intersection of this set with the other set.
//            TOCEntryCollection intersection = this.Intersection(assets);

//            if (intersection.Count > ASSET_REMOVE_THRESHOLD || (intersection.Count * 100) / this.Count > 75)
//            {
//                // Instead of removing assets one by one, we recreate the collection with the assets that are not getting removed
//                List<TOCEntry> newInnerList = new List<TOCEntry>();

//                // Recreate collection with newInnerList
//                foreach (TOCEntry ah in mEntries)
//                {
//                    if (!intersection.Contains(ah.GUID))
//                    {
//                        newInnerList.Add(ah);
//                    }
//                    else
//                    {
//                        // Make sure the asset are removed from indices
//                        RemoveAssetFromIndices(ah.GUID);
//                    }
//                }

//                // Replace the innerList
//                mEntries.Clear();
//                mEntries.AddRange(newInnerList);
//            }
//            else
//            {
//                foreach (TOCEntry asset in intersection)
//                {
//                    Remove(asset.GUID);
//                }
//            }
//        }

//        public void Add(Granite.IKey value)
//        {
//            Add((TOCEntry)value);
//        }

//        public void Add(TOCEntry value)
//        {
//            if ((object)value == null)
//                return;

//            if (Contains(value.GUID))
//            {
//                System.Diagnostics.Debug.Fail("You shouldn't add the same asset to the TOCEntryCollection more than once.");
//                return;
//            }

//            Track(value);
//            mEntries.Add(value);
//			if (mEntries.Count >= HASHSET_THRESHOLD)
//			{
//				if (mKeys == null)
//				{
//					mKeys = new HashSet<Granite.IKey>(mEntries);
//				}
//				else
//				{
//					mKeys.Add(value);
//				}
//			}
//        }

//        public bool AddIfUnique(TOCEntry value)
//        {
//            if ((object)value == null)
//                return false;

//            if (Contains(value.GUID))
//                return false;

//            Add(value);

//            return true;
//        }

//        public void Add(IEnumerable<TOCEntry> assets)
//        {
//            if (assets == null)
//                return;
//            foreach (TOCEntry asset in assets)
//                Add(asset);
//        }

//        public void AddIfUnique(IEnumerable<TOCEntry> assets)
//        {
//            if (assets == null)
//                return;
//            foreach (TOCEntry asset in assets)
//                AddIfUnique(asset);
//        }

//        public void Insert(int index, TOCEntry value)
//        {
//            if (Contains(value.GUID))
//            {
//                System.Diagnostics.Debug.Fail("You shouldn't add the same asset to the TOCEntryCollection more than once.");
//                return;
//            }

//			mEntries.Insert(index, value);

//			if (mEntries.Count >= HASHSET_THRESHOLD)
//			{
//				if (mKeys == null)
//				{
//					mKeys = new HashSet<Granite.IKey>(mEntries);
//				}
//				else
//				{
//					mKeys.Add(value);
//				}
//			}

//            Track(value);
//        }

//        public bool Contains(TOCEntry value)
//        {
//            if ((object)value != null)
//                return Contains(value.GUID);
//            return false;
//        }

//        public bool Contains(Granite.IKey guid)
//        {
//			if (mKeys != null)
//			{
//				return mKeys.Contains(guid);
//			}
//			else
//			{
//				return mEntries.Contains(guid as TOCEntry);
//			}
//        }

//        public int IndexOf(TOCEntry a)
//        {
//            return this.mEntries.IndexOf(a);
//        }

//        protected TOCEntry RemoveAssetFromIndices(Granite.IKey value)
//        {
//			if (mKeys!= null)
//			{
//				mKeys.Remove(value);
//			}

//            return value as TOCEntry;
//        }

//        public bool Remove(Granite.IKey value)
//        {
//			if (mKeys != null)
//			{
//				if (!mKeys.Contains(value))
//					return false;

//				TOCEntry storedValue = RemoveAssetFromIndices(value);
//				mEntries.Remove(storedValue);
//			}
//			else
//			{
//				int index = mEntries.IndexOf(value as TOCEntry);
//				if (index == -1)
//					return false;

//				mEntries.RemoveAt(index);
//			}

//            return true;
//        }

//        public bool Remove(TOCEntry asset)
//        {
//            return Remove(asset.GUID);
//        }

//        public TOCEntryCollection Intersection(TOCEntryCollection other)
//        {
//            // It's faster to iterate over the set with smaller cardinality
//            if (other.Count < this.Count)
//                return other.Intersection(this);

//            TOCEntryCollection result = new TOCEntryCollection();
//            foreach (TOCEntry asset in this)
//            {
//                if (other.Contains(asset))
//                {
//                    result.Add(asset);
//                }
//            }
//            return result;
//        }

//        public TOCEntry[] ToArray()
//        {
//            return mEntries.ToArray();
//        }

//        public IList<Granite.IKey> ToKeyList()
//        {
//            IList<Granite.IKey> keyList = new List<Granite.IKey>(mEntries.Count);
//            foreach (TOCEntry asset in mEntries)
//            {
//                keyList.Add(asset.GUID);
//            }
//            return keyList;
//        }

//        public void CopyTo(TOCEntry[] array, int idx)
//        {
//            mEntries.CopyTo(array, idx);
//        }

//        public bool IsReadOnly { get { return false; } }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return mEntries.GetEnumerator();
//        }

//        public IEnumerator<TOCEntry> GetEnumerator()
//        {
//            return mEntries.GetEnumerator();
//        }

//        public int Count { get { return mEntries.Count; } }

//        public TOCEntry Find(Granite.IKey key)
//        {
//            TOCEntry ret = null;

//			TOCEntry entry = key as TOCEntry;

//			if (mKeys != null)
//			{
//				if (mKeys.Contains(key))
//				{
//					ret = entry;
//				}
//			}
//			else
//			{
//				if (mEntries.Contains(entry))
//				{
//					ret = entry;
//				}
//			}

//            return ret;
//        }

//        public void Sort(IComparer<TOCEntry> comparer)
//        {
//            mEntries.Sort(comparer);
//        }

//        public void Sort(Comparison<TOCEntry> comparsion)
//        {
//            mEntries.Sort(comparsion);
//        }

//        #endregion

//        #region IEnumerable<Granite.IKey> Members

//        IEnumerator<Granite.IKey> IEnumerable<Granite.IKey>.GetEnumerator()
//        {
//            return mEntries.Select(tocEntry => tocEntry.GUID).GetEnumerator();
//        }

//        #endregion

//        #region Key Comparer

//        private sealed class KeyComparer : IEqualityComparer<Granite.IKey>
//        {
//            public bool Equals(Granite.IKey x, Granite.IKey y)
//            {
//                return object.ReferenceEquals(x, y);
//            }

//            public int GetHashCode(Granite.IKey obj)
//            {
//                return RuntimeHelpers.GetHashCode(obj);
//            }
//        }

//        #endregion

//        #region Private Methods

//        private void Track(Granite.IKey key)
//        {
//            var keyResolving = KeyResolving;
//            if (key != null && keyResolving != null)
//            {
//                keyResolving(this, new KeyResolvingEventArgs(key));
//            }
//        }

//        #endregion
//    }
//}
