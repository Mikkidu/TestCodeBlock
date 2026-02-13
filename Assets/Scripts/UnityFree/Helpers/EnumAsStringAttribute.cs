
using System;

namespace PU.UnityFree.Helpers
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnumAsStringAttribute : Attribute
    {
        public Type enumType;

        public EnumAsStringAttribute(Type enumType) => this.enumType = enumType;
    }
}
