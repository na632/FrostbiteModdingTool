// TODO: Paulov. An Idea that hasn't been followed through (yet). To decouple checks from AssetManager Instance into the Collection.

//using FrostySdk.Managers;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FrostySdk.Frostbite.Collection
//{
//    internal class AssetEntryCollection : IDictionary<string, AssetEntry>
//    {
//        private KeyValuePair<string, AssetEntry>[] items;
//        public AssetEntry this[ReadOnlySpan<char> key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public AssetEntry this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//        public ICollection<string> Keys => new List<string>();

//        public ICollection<AssetEntry> Values => throw new NotImplementedException();

//        public int Count => throw new NotImplementedException();

//        public bool IsReadOnly => throw new NotImplementedException();

//        public void Add(string key, AssetEntry value)
//        {
//            throw new NotImplementedException();
//        }

//        public void Add(KeyValuePair<string, AssetEntry> item)
//        {
//            throw new NotImplementedException();
//        }

//        public void Clear()
//        {
//            throw new NotImplementedException();
//        }

//        public bool Contains(KeyValuePair<string, AssetEntry> item)
//        {
//            throw new NotImplementedException();
//        }

//        public bool ContainsKey(string key)
//        {
//            throw new NotImplementedException();
//        }

//        public void CopyTo(KeyValuePair<string, AssetEntry>[] array, int arrayIndex)
//        {
//            throw new NotImplementedException();
//        }

//        public IEnumerator<KeyValuePair<string, AssetEntry>> GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }

//        public bool Remove(string key)
//        {
//            throw new NotImplementedException();
//        }

//        public bool Remove(KeyValuePair<string, AssetEntry> item)
//        {
//            throw new NotImplementedException();
//        }

//        public bool TryGetValue(string key, [MaybeNullWhen(false)] out AssetEntry value)
//        {
//            throw new NotImplementedException();
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return items.GetEnumerator();
//        }
//    }
//}
