using Impey.Sitefinity.Repository.Extensions;
using Impey.Sitefinity.Repository.Fields;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Telerik.OpenAccess;
using Telerik.Sitefinity.Model;

namespace Impey.Sitefinity.Repository.Cache
{
    internal static class GetMethodCache
    {
        private static readonly MethodInfo GetString;
        private static readonly MethodInfo GetValue;
        private static readonly MethodInfo GetRelatedItem;
        private static readonly MethodInfo GetRelatedItems;

        private static readonly ConcurrentDictionary<Type, MethodInfo> ConstructedMethodCache;

        static GetMethodCache()
        {
            GetString = typeof(DataExtensions).GetMethod("GetString", new[] { typeof(IDynamicFieldsContainer), typeof(string) });
            GetValue = typeof(DataExtensions).GetMethods().Single(m => m.Name == "GetValue" && m.IsGenericMethod);
            GetRelatedItem = typeof(RelatedDataExtensions).GetMethod("GetRelatedItem");
            GetRelatedItems = typeof(RelatedDataExtensions).GetMethod("GetRelatedItems");

            ConstructedMethodCache = new ConcurrentDictionary<Type, MethodInfo>();
        }

        public static MethodInfo ConstructForType(Type type)
        {
            return ConstructedMethodCache.GetOrAdd(type, t =>
            {
                if (t == typeof(string))
                {
                    return GetString;
                }

                if (t == typeof(Lstring))
                {
                    return GetValue.MakeGenericMethod(typeof(string));
                }

                if (t == typeof(Image))
                {
                    return GetRelatedItem.MakeGenericMethod(typeof(Telerik.Sitefinity.Libraries.Model.Image));
                }

                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var genericType = t.GetGenericArguments()[0];

                    if (genericType == typeof(Category) || genericType == typeof(Tag))
                    {
                        return GetValue.MakeGenericMethod(typeof(TrackedList<Guid>));
                    }
                    else
                    {
                        return GetRelatedItems.MakeGenericMethod(genericType);
                    }
                }

                return GetValue.MakeGenericMethod(t);
            });
        }
    }
}
