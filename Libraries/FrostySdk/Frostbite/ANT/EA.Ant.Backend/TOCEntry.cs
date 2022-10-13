//// (c) Electronic Arts.  All Rights Reserved.

//using System;

//namespace EA.Ant.Tool.Data
//{
//    /// <summary>
//    /// An indexed entry that can be used for fast access into an asset.
//    /// </summary>
//    /// <remarks>
//    /// The ANT convention is that a TOCEntry is fast to access and safe to cache,
//    /// while an asset is slow and unsafe.
//    /// </remarks>
//    public abstract class TOCEntry : EA.Ant.Backend.IKeyedObject, IComparable, IEquatable<TOCEntry>
//    {
//        #region Properties

//        public abstract Key Key { get; }
//        public abstract Checksum Checksum { get; }
//        public abstract Granite.IKey GUID { get; }
//        public abstract Granite.IKey OwnerGUID { get; }
//		public abstract Granite.IKey MasterGuid { get; }
//        public abstract string AssetName { get; }
//        public abstract Type Type { get; }
//        public abstract string Path { get; set; }
//        public abstract bool Deleted { get; set; }
//		public abstract bool IsVirtual { get; set; }

//        /// <summary>
//        /// Gets the key of the associated project.
//        /// </summary>
//        public abstract Granite.IKey ProjectGUID { get; }

//        /// <summary>
//        /// Gets type information for the underlying asset.
//        /// </summary>
//        public abstract AssetTypeInfo TypeInfo { get; }

//        /// <summary>
//        /// Gets the asset type display name.
//        /// </summary>
//        [Obsolete("Use DisplayName")]
//        public string TypeName
//        {
//            get { return TypeInfo.DisplayName; }
//        }

//        /// <summary>
//        /// Gets the asset type display name.
//        /// </summary>
//        public string DisplayName
//        {
//            get { return TypeInfo.DisplayName; }
//        }

//        /// <summary>
//        /// Gets the ownership state of the asset associated with this TOCEntry.
//        /// </summary>
//        public AssetOwnershipState Ownership
//        {
//            get
//            {
//                return AssetOwnershipState.Unowned;
//            }
//        }

//		//public ToolAssetUsage Usage
//		//{
//		//	get
//		//	{
//		//		ToolAntAsset asset = LoadAsset();
//		//		if (asset != null)
//		//		{
//		//			return asset.Usage;
//		//		}
//		//		return ToolAssetUsage.NotUsedInGame;
//		//	}
//		//}

//		public bool IsOwned
//		{
//            get { return OwnerGUID != Guid.Empty; }
//		}

//        #endregion

//        #region Public Methods

//        /// <summary>
//        /// Gets the indexed value with the given name.
//        /// </summary>
//        /// <param name="columnName">Indexed property name.</param>
//        /// <returns>The value.</returns>
//        public abstract object GetIndexedValue(string columnName);

//        /// <summary>
//        /// Load the asset associated with this TOCEntry.
//        /// </summary>
//        /// <returns>The asset.</returns>
//        protected abstract ToolAntAsset LoadAsset();

//        /// <summary>
//        /// Returns the numerical value of the key
//        /// </summary>
//        public abstract long ToLong();

//        /// <summary>
//        /// Gets a value from the underlying asset.
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="columnName">Name of the column.</param>
//        /// <param name="defaultValue">The default value.</param>
//        /// <returns></returns>
//        public T GetValue<T>(string columnName, T defaultValue)
//        {
//            object val = GetIndexedValue(columnName);
//            if (val is T)
//            {
//                return (T)val;
//            }

//            // Convert uint to int
//            if (val is uint && typeof(T) == typeof(int))
//            {
//                val = (int)(uint)val;
//                return (T)val;
//            }

//            // Convert Granite.IKey to AssetRef
//            //if (typeof(T) == typeof(Granite.IKey) && (val is AssetRef))
//            //{
//            //    if (val is AssetRef)
//            //    {
//            //        val = ((AssetRef)val).RefGUID;
//            //        return (T)val;
//            //    }
//            //}

//            if (val != null && !(val is T))
//            {
//                throw new Exception("Data is an " + val.GetType().Name + ", not an " + typeof(T).Name);
//            }

//            return defaultValue;
//        }

//        /// <summary>
//        /// Determines whether asset is owned.
//        /// </summary>
//        /// <returns>
//        ///   <c>true</c> if owned; otherwise, <c>false</c>.
//        /// </returns>
//        public bool IsOwnedAsset()
//        {
//            return !OwnerGUID.IsNull;
//        }

//        /// <summary>
//        /// Determines whether this TOC entry describes nested data.
//        /// </summary>
//        /// <returns>
//        ///   <c>true</c> if nested data; otherwise, <c>false</c>.
//        /// </returns>
//        public bool IsNestedData()
//        {
//            return TypeInfo.IsNestedData;
//        }

//        /// <summary>
//        /// Determines whether asset is of the given type.
//        /// </summary>
//        /// <param name="type">The type.</param>
//        /// <returns>
//        ///   <c>true</c> if asset is of the specified type; otherwise, <c>false</c>.
//        /// </returns>
//        public bool IsA(Type type)
//        {
//            return type.IsAssignableFrom(Type);
//        }

//        /// <summary>
//        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
//        /// </summary>
//        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
//        /// <returns>
//        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
//        /// </returns>
//        public override bool Equals(object obj)
//        {
//            TOCEntry other = obj as TOCEntry;
//            return (!object.ReferenceEquals(null, other) && Key == other.Key);
//        }

//        /// <summary>
//        /// Returns a hash code for this instance.
//        /// </summary>
//        /// <returns>
//        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
//        /// </returns>
//        //public override int GetHashCode()
//        //{
//        //    long keyValue = Key.Value;
//        //    return (int)(keyValue ^ keyValue >> 32);
//        //}

//        #endregion

//        #region Granite.IKey Members

//        /// <summary>
//        /// Gets a value indicating whether this instance is null.
//        /// </summary>
//        /// <value>
//        ///   <c>true</c> if this instance is null; otherwise, <c>false</c>.
//        /// </value>
//        //public virtual bool IsNull
//        //{
//        //    get { return CompareTo(KeyFactory.Empty) == 0; }
//        //}

//        ///// <summary>
//        ///// Test if this key is equal to the one given.
//        ///// </summary>
//        ///// <param name="other">The other.</param>
//        ///// <returns></returns>
//        //public bool Equals(Granite.IKey other)
//        //{
//        //    return Equals(other as TOCEntry);
//        //}

//        ///// <summary>
//        ///// Convert key to a byte array.
//        ///// </summary>
//        ///// <returns></returns>
//        //public byte[] ToByteArray()
//        //{
//        //    return BitConverter.GetBytes(Key.Value);
//        //}

//        ///// <summary>
//        ///// Compares key to given key
//        ///// </summary>
//        ///// <param name="other">The other.</param>
//        ///// <returns></returns>
//        //public int CompareTo(Granite.IKey other)
//        //{
//        //    TOCEntry otherTOCEntry = other as TOCEntry;
//        //    if (Object.ReferenceEquals(otherTOCEntry, null))
//        //        return -1;
//        //    return Key.CompareTo(otherTOCEntry.Key);
//        //}

//        ///// <summary>
//        ///// Returns a <see cref="System.String" /> that represents this instance.
//        ///// </summary>
//        ///// <returns>
//        ///// A <see cref="System.String" /> that represents this instance.
//        ///// </returns>
//        //public override string ToString()
//        //{
//        //    return Key.ToString();
//        //}

//        #endregion

//        #region IComparable Members

//        /// <summary>
//        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
//        /// </summary>
//        /// <param name="obj">An object to compare with this instance.</param>
//        /// <returns>
//        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj" />. Zero This instance is equal to <paramref name="obj" />. Greater than zero This instance is greater than <paramref name="obj" />.
//        /// </returns>
//        public int CompareTo(object obj)
//        {
//            TOCEntry other = obj as TOCEntry;
//            return Key.CompareTo(other.Key);
//        }

//        #endregion

//        #region IEquatable<TOCEntry> Members

//        /// <summary>
//        /// Test if this <see cref="TOCEntry"/> is equal to the one given.
//        /// </summary>
//        /// <param name="other">The other.</param>
//        /// <returns></returns>
//        public virtual bool Equals(TOCEntry other)
//        {
//            return !object.ReferenceEquals(other, null) && (Key == other.Key);
//        }

//        #endregion
//    }
//}