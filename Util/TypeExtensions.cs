using System;

namespace Rhinox.Perceptor
{
    public static class TypeExtensions
    {
        public static bool IsDefinedTypeOf<T>(this Type type)
        {
            if (type == null)
                return false;
            
            if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters)
                return false;

            return typeof(T).IsAssignableFrom(type);
        }
    }
}