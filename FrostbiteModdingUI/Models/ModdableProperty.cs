using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FMT.Models
{

    public class ModdableProperty : INotifyPropertyChanged
    {
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public string PropertyParentName { get; set; }

        private object propValue;

        public object PropertyOriginalValue { get; private set; }

        public object PropertyValue
        {
            get { return propValue; }
            set
            {
                if (propValue != value)
                {
                    propValue = value;

                    _ = RootObject;
                    _ = Property;
                    if (Property.PropertyType.FullName.Contains("List`1") && ArrayType != null && ArrayIndex.HasValue)
                    {
                        //IList sourceList = (IList)Property.GetValue(RootObject);
                        //Type t = typeof(List<>).MakeGenericType(ArrayType);
                        //IList res = (IList)Activator.CreateInstance(t);
                        //foreach (var item in sourceList)
                        //{
                        //	res.Add(item);
                        //}
                        //res.RemoveAt(ArrayIndex.Value);
                        //                     res.GetType().GetMethod("Insert")
                        //	.Invoke(res, new object[2] { ArrayIndex.Value, Convert.ChangeType(value, ArrayType) });
                        if (PropertyChanged != null)
                        {
                            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(value.ToString()));
                        }
                    }
                    else
                    {
                        try
                        {
                            if (Property.PropertyType.Name.Equals("AssetClassGuid"))
                                return;

                            if (Property.CanWrite && Property.SetMethod != null)
                            {
                                Property.SetValue(RootObject, Convert.ChangeType(value, Property.PropertyType));
                                if (PropertyChanged != null)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(PropertyParentName != null ? PropertyParentName : PropertyName));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }

                    }
                }
            }
        }

        public object RootObject { get; set; }
        public object VanillaRootObject { get; set; }
        public PropertyInfo RootObjectPropertyInfo { get; set; }
        public PropertyInfo Property { get; set; }
        public Type ArrayType { get; set; }
        public int? ArrayIndex { get; set; }

        public bool IsInList => ArrayType != null || Property.PropertyType.IsGenericType;

        public string PropertyDescription
        {
            get
            {
                return GetPropertyDescription();
            }
        }

        public string GetPropertyDescription()
        {
            try
            {

                if (EBXDescriptions.CachedDescriptions != null)
                {
                    foreach (var tDescription in EBXDescriptions.CachedDescriptions.Descriptions)
                    {
                        var indexOfPropertyDescription = tDescription
                            .Properties
                            .FindIndex(x => x.PropertyName.Equals(Property.Name, StringComparison.OrdinalIgnoreCase));
                        if (indexOfPropertyDescription != -1)
                            return tDescription.Properties[indexOfPropertyDescription].Description;
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        public bool IsReadOnly
        {
            get { return !Property.CanWrite || Property.PropertyType.GetType().Name.Contains("CString", StringComparison.OrdinalIgnoreCase); }
        }

        public bool HasPropertyDescription
        {
            get
            {
                return !string.IsNullOrEmpty(PropertyDescription);
            }
        }

        public Visibility HasPropertyDescriptionVisibility
        {
            get
            {
                return HasPropertyDescription ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ModdableProperty(string n, string t, object v)
        {
            PropertyName = n;
            PropertyType = t;
            propValue = v;
        }

        public ModdableProperty(object rootObject, PropertyInfo property, int? arrayIndex, PropertyChangedEventHandler modpropchanged = null, object vanillaRootObject = null)
        {
            RootObject = rootObject;
            VanillaRootObject = vanillaRootObject;
            Property = property;
            PropertyName = property.Name;
            PropertyType = property.PropertyType.FullName;
            PropertyValue = property.GetValue(rootObject, BindingFlags.GetProperty, null, null, null);

            if (vanillaRootObject != null)
                PropertyOriginalValue = property.GetValue(vanillaRootObject, BindingFlags.GetProperty, null, null, null);
            else
                PropertyOriginalValue = property.GetValue(rootObject, BindingFlags.GetProperty, null, null, null);

            if (property.PropertyType.FullName.Contains("List`1"))
            {
                ArrayType = property.PropertyType.GetGenericArguments()[0];
                ArrayIndex = arrayIndex;
                if (ArrayIndex.HasValue)
                {
                    PropertyType = ArrayType.FullName;
                    PropertyName = ArrayIndex.Value.ToString();
                    PropertyValue = ((IList)property.GetValue(rootObject))[ArrayIndex.Value];

                    if (vanillaRootObject != null)
                        PropertyOriginalValue = ((IList)property.GetValue(vanillaRootObject))[ArrayIndex.Value];
                }

            }
            if (modpropchanged != null)
                PropertyChanged += modpropchanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(PropertyName))
            {
                return string.Format("{0} - {1}", PropertyName, PropertyType);
            }

            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static IEnumerable<ModdableProperty> GetModdableProperties(object obj, PropertyChangedEventHandler modpropchanged = null, object vanillaObj = null)
        {
            if (obj == null)
                yield return null;

            foreach (var p in obj.GetType().GetProperties().Where(x => x.CanWrite && x.SetMethod != null))
            {
                yield return new ModdableProperty(obj, p, null, modpropchanged, vanillaObj);
            }
        }
    }

    
}
