using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class LocalAttribute : Attribute
    {
    }
}
