using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class IsInlineAttribute : Attribute
    {
    }
}
