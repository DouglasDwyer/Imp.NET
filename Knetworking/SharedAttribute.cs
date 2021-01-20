using System;

namespace DouglasDwyer.Knetworking
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SharedAttribute : Attribute
    {
    }
}
