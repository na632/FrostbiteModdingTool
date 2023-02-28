using Newtonsoft.Json;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace FrostbiteSdk
{
    public static class Utilities
    {
        public static byte[] TOCKey { get; } = new byte[539]
        {
            82, 83, 65, 50, 0, 8, 0, 0, 3, 0,
            0, 0, 0, 1, 0, 0, 128, 0, 0, 0,
            128, 0, 0, 0, 1, 0, 1, 212, 21, 28,
            132, 38, 235, 95, 95, 86, 2, 145, 209, 198,
            20, 82, 63, 11, 253, 160, 37, 36, 191, 120,
            168, 71, 211, 23, 61, 89, 175, 59, 140, 234,
            110, 236, 139, 228, 154, 127, 61, 25, 232, 55,
            122, 146, 206, 91, 87, 122, 163, 33, 89, 231,
            115, 54, 125, 140, 79, 53, 220, 191, 13, 247,
            231, 214, 72, 213, 213, 47, 31, 165, 225, 229,
            213, 144, 225, 159, 76, 43, 119, 76, 52, 66,
            103, 65, 146, 3, 221, 75, 70, 248, 24, 135,
            41, 231, 165, 201, 62, 194, 116, 24, 213, 186,
            220, 227, 178, 134, 156, 57, 157, 160, 121, 163,
            205, 125, 15, 184, 36, 135, 173, 29, 208, 60,
            169, 190, 132, 6, 129, 172, 132, 177, 178, 185,
            99, 205, 65, 182, 40, 142, 20, 38, 148, 148,
            49, 151, 49, 5, 76, 10, 114, 135, 131, 0,
            88, 43, 177, 62, 80, 115, 31, 116, 99, 100,
            162, 78, 76, 14, 199, 175, 214, 63, 52, 238,
            2, 226, 211, 203, 110, 195, 50, 128, 166, 188,
            180, 210, 55, 177, 0, 85, 212, 176, 119, 83,
            93, 170, 244, 177, 209, 173, 190, 81, 135, 194,
            116, 145, 218, 41, 93, 139, 198, 141, 33, 24,
            181, 45, 252, 186, 207, 14, 107, 118, 199, 218,
            90, 55, 111, 171, 238, 126, 103, 70, 191, 195,
            29, 201, 110, 196, 225, 99, 145, 111, 172, 26,
            33, 216, 122, 125, 146, 121, 226, 46, 183, 78,
            13, 157, 7, 242, 107, 33, 9, 69, 206, 42,
            189, 113, 9, 84, 77, 253, 129, 229, 99, 248,
            218, 84, 237, 12, 43, 161, 1, 136, 110, 153,
            138, 102, 43, 117, 102, 219, 150, 35, 128, 57,
            232, 46, 230, 86, 241, 221, 198, 78, 170, 198,
            156, 179, 58, 220, 107, 139, 204, 182, 224, 222,
            16, 89, 206, 67, 171, 13, 73, 92, 0, 1,
            209, 173, 169, 173, 201, 41, 51, 55, 200, 101,
            154, 236, 86, 13, 200, 254, 111, 220, 6, 86,
            88, 129, 237, 163, 198, 182, 198, 37, 57, 222,
            194, 13, 52, 65, 55, 196, 95, 226, 200, 65,
            23, 25, 94, 207, 103, 57, 55, 47, 131, 169,
            127, 146, 171, 2, 171, 132, 176, 126, 159, 244,
            67, 223, 246, 227, 226, 88, 0, 132, 159, 220,
            189, 243, 245, 252, 6, 77, 5, 1, 236, 220,
            55, 44, 252, 30, 232, 150, 210, 121, 237, 149,
            142, 13, 160, 171, 181, 39, 178, 194, 45, 27,
            130, 160, 103, 11, 168, 211, 223, 33, 149, 146,
            70, 136, 159, 171, 107, 2, 16, 230, 36, 189,
            165, 149, 62, 125, 42, 86, 13, 143, 112, 110,
            192, 116, 44, 245, 106, 217, 236, 131, 93, 188,
            126, 15, 203, 129, 220, 255, 181, 244, 58, 247,
            196, 87, 101, 175, 135, 210, 67, 15, 175, 177,
            242, 152, 109, 65, 247, 253, 2, 51, 149, 197,
            22, 224, 0, 127, 167, 51, 120, 110, 208, 145,
            207, 149, 229, 74, 183, 52, 3, 105, 237
        };

        public unsafe static byte[] ToTOCSignature(this byte[] input)
        {
            return CreateTOCSignature(input);
        }

        /// <summary>
        /// SuperBundleBackend.cpp / Deploy.cpp
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public unsafe static byte[] CreateTOCSignature(byte[] input)
        {
            byte[] resultHash = null;
            byte[] frostbiteHash = new HMACSHA1(Encoding.ASCII.GetBytes("Powered by Frostbite \\o/ EA Digital Illusions CE AB")).ComputeHash(input);

            //System.Security.Cryptography.bc
            //BCrypt.Net.BCrypt.HashPassword(,,, BCrypt.Net.HashType.SHA256)

            BCrypt.SafeKeyHandle cryptPrivateKeyHandle = BCrypt.BCryptImportKeyPair(BCrypt.BCryptOpenAlgorithmProvider("RSA"), "RSAPRIVATEBLOB", TOCKey);

            fixed (char* BCRYPT_SHA1_ALGORITHM = &"SHA1".GetPinnableReference())
            {
                BCrypt.BCRYPT_PKCS1_PADDING_INFO bCRYPT_PKCS1_PADDING_INFO = default(BCrypt.BCRYPT_PKCS1_PADDING_INFO);
                bCRYPT_PKCS1_PADDING_INFO.pszAlgId = BCRYPT_SHA1_ALGORITHM;
                GCHandle handle = GCHandle.Alloc(bCRYPT_PKCS1_PADDING_INFO, GCHandleType.Pinned);
                try
                {

                    resultHash = BCrypt.BCryptSignHash(cryptPrivateKeyHandle, frostbiteHash, handle.AddrOfPinnedObject(), BCrypt.BCryptSignHashFlags.BCRYPT_PAD_PKCS1).ToArray();
                }
                finally
                {
                    handle.Free();
                }
            }

            if (resultHash == null || resultHash.Length != 256)
            {
                throw new Exception("Result Hash must be 256 bytes in Length");
            }

            return resultHash;
        }

        public static void RebuildTOCSignature(Stream stream)
        {
            if (!stream.CanWrite)
                throw new IOException("Unable to Write to this Stream!");

            if (!stream.CanRead)
                throw new IOException("Unable to Read to this Stream!");

            if (!stream.CanSeek)
                throw new IOException("Unable to Seek this Stream!");

            byte[] streamBuffer = new byte[stream.Length - 556];
            stream.Position = 556;
            stream.Read(streamBuffer, 0, (int)stream.Length - 556);
            var newTocSig = streamBuffer.ToTOCSignature();
            stream.Position = 8;
            stream.Write(newTocSig);
        }

        public static void RebuildTOCSignature(string filePath)
        {
            using (var fsTocSig = new FileStream(filePath, FileMode.Open))
                RebuildTOCSignature(fsTocSig);
        }

        public static IEnumerable<DataRow> ToEnumerable(this DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
                yield return row;
        }

        public static bool PropertyExists(object obj, string propName)
        {
            if (obj is ExpandoObject)
                return ((IDictionary<string, object>)obj).ContainsKey(propName);

            return obj.GetProperty(propName) != null;
        }

        public static bool PropertyExistsDynamic(this ExpandoObject obj, string propName)
        {
            if (obj is ExpandoObject)
                return ((IDictionary<string, object>)obj).ContainsKey(propName);

            return obj.GetProperty(propName) != null;
        }


        public static PropertyInfo[] GetProperties(this object obj)
        {
            Type t = obj.GetType();
            return t.GetProperties();
        }

        public static PropertyInfo GetProperty(this object obj, string propName)
        {
            Type t = obj.GetType();
            return t.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
        }

        public static object GetPropertyValue(this object obj, string propName)
        {
            Type t = obj.GetType();
            var p = t.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
            return p.GetValue(obj);
        }

        public static void SetPropertyValue<T>(object obj, string propName, T value)
        {
            Type t = obj.GetType();
            Type t2 = value.GetType();
            var p = t.GetProperty(propName);
            if (propName != "BaseField")
                p.SetValue(obj, value);
        }

        public static void SetPropertyValue(object obj, string propName, dynamic value)
        {
            Type t = obj.GetType();
            Type t2 = value.GetType();
            var p = t.GetProperty(propName);
            var parseMethod = p.PropertyType.GetMethods().FirstOrDefault(x => x.Name == "Parse");
            if (parseMethod != null && p.PropertyType != value.GetType())
            {
                var v = parseMethod.Invoke(p, new[] { value });
                if (propName != "BaseField")
                    p.SetValue(obj, v);
            }
            else
            {
                if (propName != "BaseField")
                    p.SetValue(obj, value);
            }
        }

        public static bool HasProperty(object obj, string propName)
        {
            return obj.GetType().GetProperty(propName) != null;
        }

        public static bool HasProperty(ExpandoObject obj, string propertyName)
        {
            return ((IDictionary<String, object>)obj).ContainsKey(propertyName);
        }

        //public static void SetPropertyValue(this object obj, string propName, dynamic value)
        //{
        //    Type t = obj.GetType();
        //    var p = t.GetProperty(propName);
        //    p.SetValue(obj, value);
        //}

        public static T GetObjectAsType<T>(this object obj)
        {
            Type t = obj.GetType();
            var s = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(s);
        }

        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialization method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        //public static T CloneJson<T>(this T source)
        //{
        //    // Don't serialize a null object, simply return the default for that object
        //    if (ReferenceEquals(source, null)) return default;

        //    // initialize inner objects individually
        //    // for example in default constructor some list property initialized with some values,
        //    // but in 'source' these items are cleaned -
        //    // without ObjectCreationHandling.Replace default constructor values will be added to result
        //    var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

        //    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        //}

        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialization method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static dynamic CloneJson(this object source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (source is null)
                return null;

            var sourceType = source.GetType();

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MaxDepth = 10
            };

            var s = JsonConvert.SerializeObject(source, deserializeSettings);
            return JsonConvert.DeserializeAnonymousType(s, sourceType, deserializeSettings);
        }

        public static string ApplicationDirectory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\";
            }
        }
    }

}
