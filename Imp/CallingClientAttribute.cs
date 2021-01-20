using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class CallingClientAttribute : Attribute
    {
    }
}
