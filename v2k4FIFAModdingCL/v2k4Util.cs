using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace v2k4FIFAModding
{
    public static class v2k4Util
    {
        public static IEnumerable<DataRow> ToEnumerable(this DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
                yield return row;
        }

        public static bool PropertyExists(this object obj, string propName)
        {
            return obj.GetProperty(propName) != null;
        }

        public static PropertyInfo GetProperty(this object obj, string propName)
        {
            Type t = obj.GetType();
            return t.GetProperty(propName);
        }
        public static dynamic GetPropertyValue(this object obj, string propName)
        {
            Type t = obj.GetType();
            var p = t.GetProperty(propName);
            return p.GetValue(obj);
        }

        public static void SetPropertyValue(object obj, string propName, dynamic value)
        {
            Type t = obj.GetType();
            var p = t.GetProperty(propName);
            p.SetValue(obj, value);
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
    }
}
