using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IsExpandedByDefaultAttribute : Attribute
    {
    }
}
