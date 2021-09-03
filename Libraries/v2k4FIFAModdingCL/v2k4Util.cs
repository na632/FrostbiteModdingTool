using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using System.Dynamic;

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
            Type t2 = value.GetType();
            var p = t.GetProperty(propName);
            var parseMethod = p.PropertyType.GetMethods().FirstOrDefault(x=>x.Name == "Parse");
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

        public static void SetPropertyValue<T>(object obj, string propName, T value)
        {
            Type t = obj.GetType();
            Type t2 = value.GetType();
            var p = t.GetProperty(propName);
            if (propName != "BaseField")
                p.SetValue(obj, value);
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
    }
}
