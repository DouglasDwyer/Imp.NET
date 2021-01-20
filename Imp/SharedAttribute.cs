using System;

namespace DouglasDwyer.Imp
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SharedAttribute : Attribute
    {
    }
}
