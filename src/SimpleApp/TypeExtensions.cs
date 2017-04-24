using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SimpleApp
{
    internal static class TypeExtensions
    {
        public static IEnumerable<Type> GetImplementedTypesAndInterfaces(this Type type)
        {
            if (type == null)
            {
                yield break;
            }

            yield return type;

            var currentBaseType = type.GetTypeInfo().BaseType;
            while (currentBaseType != null)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.GetTypeInfo().BaseType;
            }

            foreach (var i in type.GetTypeInfo().ImplementedInterfaces)
            {
                yield return i;
            }
        }
    }
}
