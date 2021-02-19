using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Indicates that remote calls to this method should be conducted over UDP. Unreliable methods may be faster than normal remote method invocations, but unreliable methods are not guaranteed to be called on the remote host. Unreliable methods occur without reference-tracking or awaiting return values, so unreliable methods must return void and cannot take shared types as arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class UnreliableAttribute : Attribute
    {
    }
}
