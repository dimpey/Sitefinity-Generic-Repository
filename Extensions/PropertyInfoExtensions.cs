using Impey.Sitefinity.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Telerik.Sitefinity.Libraries.Model;

namespace Impey.Sitefinity.Repository.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static Func<T, object> CreateGetterDelegate<T>(this PropertyInfo info)
        {
            var getMethod = info.GetGetMethod();
            if (getMethod == null)
                return null;

            var target = Expression.Parameter(typeof(T));
            var body = Expression.Convert(Expression.Call(target, getMethod), typeof(object));

            return Expression.Lambda<Func<T, object>>(body, target).Compile();
        }

        public static Func<T, object> CreateGetMethodDelegate<T>(this PropertyInfo info, GetMethodCache methodCache)
        {
            var target = Expression.Parameter(typeof(T));

            var method = methodCache.GetString;
            if (info.PropertyType != typeof(string))
            {
                if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    method = methodCache.GetRelatedItems.MakeGenericMethod(info.PropertyType.GetGenericArguments()[0]);
                }
                else
                {
                    method = (info.PropertyType == typeof(Image) ? methodCache.GetRelatedItem : methodCache.GetValue).MakeGenericMethod(info.PropertyType);
                }
            }

            var body = Expression.Convert(Expression.Call(method, target, Expression.Constant(info.Name)), typeof(object));

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
