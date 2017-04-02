using System;
using System.Linq;
using System.Reflection;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Fluent;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;

namespace Impey.Sitefinity.Repository.Cache
{
    public static class WorkWithMethodCache<TContent>
        where TContent : ILifecycleDataItem, IScheduleable
    {
        private static readonly MethodInfo WorkWithMethod;
        private static readonly MethodInfo PublishedMethod;
        private static readonly MethodInfo GetMethod;
        private static readonly bool IsExtension;

        static WorkWithMethodCache()
        {
            string typeName = typeof(TContent).Name + "s";
            WorkWithMethod = typeof(FluentSitefinity).GetMethod(typeName);

            if (WorkWithMethod == null)
            {
                WorkWithMethod = typeof(ContentModulesFluentExtensions).GetMethod(typeName, new[] { typeof(FluentSitefinity) });
                IsExtension = true;
            }

            if (WorkWithMethod != null)
            {
                var facade = WorkWithMethod.Invoke(
                    IsExtension ? null : App.WorkWith(),
                    !IsExtension ? null : new object[] { App.WorkWith() });

                PublishedMethod = facade.GetType().GetMethod("Published");
                GetMethod = facade.GetType().GetMethod("Get", new Type[0]);
            }
        }

        public static IQueryable<TContent> GetQuery(bool published = true)
        {
            if (WorkWithMethod == null)
                return null;

            var facade = WorkWithMethod.Invoke(
                    IsExtension ? null : App.WorkWith(),
                    !IsExtension ? null : new object[] { App.WorkWith() });

            if (published)
            {
                facade = PublishedMethod.Invoke(facade, null);
            }

            facade = GetMethod.Invoke(facade, null);

            return (IQueryable<TContent>)facade;
        }
    }
}
