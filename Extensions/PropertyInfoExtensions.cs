using Impey.Sitefinity.Repository.Cache;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Impey.Sitefinity.Repository.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static string GetName(this PropertyInfo info)
        {
            switch (info.Name)
            {
                case "Categories":
                    return "Category"; // Fudge for non-plural built-in field name :/
                default:
                    return info.Name;
            }
        }

        public static Func<T, object> CreateGetterDelegate<T>(this PropertyInfo info)
        {
            var getMethod = info.GetGetMethod();
            if (getMethod == null)
                return null;

            var target = Expression.Parameter(typeof(T));
            var body = Expression.Convert(Expression.Call(target, getMethod), typeof(object));

            return Expression.Lambda<Func<T, object>>(body, target).Compile();
        }

        public static Func<T, object> CreateGetMethodDelegate<T>(this PropertyInfo info)
        {
            var method = GetMethodCache.ConstructForType(info.PropertyType);
            var target = Expression.Parameter(typeof(T));
            var name = Expression.Constant(info.GetName());
            var body = Expression.Convert(Expression.Call(method, target, name), typeof(object));

            return Expression.Lambda<Func<T, object>>(body, target).Compile();
        }

        public static Action<T, object> CreateSetterDelegate<T>(this PropertyInfo info)
        {
            var setMethod = info.GetSetMethod();
            if (setMethod == null || setMethod.GetParameters().Length != 1)
                return null;

            var target = Expression.Parameter(typeof(T));
            var value = Expression.Parameter(typeof(object));
            var body = Expression.Call(target, setMethod, Expression.Convert(value, info.PropertyType));

            return Expression.Lambda<Action<T, object>>(body, target, value).Compile();
        }
    }
}
