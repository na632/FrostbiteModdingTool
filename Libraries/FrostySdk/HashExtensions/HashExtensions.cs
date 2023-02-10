//using FrostySdk.IO;
//using HashDepot;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;

//namespace Frosty.Hash
//{
//    public static class Fnv1
//    {
//        public static int HashString(string s)
//        {
//            //return (int)HashDepot.Fnv1.Hash32(Encoding.UTF8.GetBytes(s));
//            return (int)HashDepot.Fnv1a.Hash32(Encoding.UTF8.GetBytes(s));
//        }
//    }

//    public static class Fnv1a
//    {
//        public static int HashString(string s)
//        {
//            return (int)HashDepot.Fnv1a.Hash32(Encoding.UTF8.GetBytes(s));
//        }
//    }

//    public static class Murmur2
//    {
//        public static UInt64 HashString64(string s, ulong seed)
//        {
//            return BitConverter.ToUInt64(HashDepot.MurmurHash3.Hash128(Encoding.UTF8.GetBytes(s), (uint)seed));
//        }
//    }
//}
