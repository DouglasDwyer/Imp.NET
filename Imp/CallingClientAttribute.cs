using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Indicates that the value of this parameter should be replaced with the client that remotely called the method. The parameter is only replaced for client-to-server calls, and its type must derive from <see cref="IImpClient"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class CallingClientAttribute : Attribute
    {
    }
}
