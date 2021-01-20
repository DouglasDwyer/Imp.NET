using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class MakeSharedAttribute : Attribute
    {
        public Type SharedType { get; private set; }

        public MakeSharedAttribute(Type sharedType)
        {
            SharedType = sharedType;
        }
    }
}
