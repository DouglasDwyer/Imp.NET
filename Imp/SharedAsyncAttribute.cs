using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SharedAsyncAttribute : Attribute
    {
    }
}
