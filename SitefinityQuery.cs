using Impey.Sitefinity.Repository.Base;
using Impey.Sitefinity.Repository.Extensions;
using Impey.Sitefinity.Repository.Helpers;
using Impey.Sitefinity.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Telerik.Sitefinity;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Fluent;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace Impey.Sitefinity.Repository
{
    public class SitefinityQuery<TModel, TContent> : ISitefinityQuery<TModel>
        where TContent : ILifecycleDataItem, IScheduleable
    {
        private static readonly GetMethodCache GetMethodCache;
        private static readonly WorkWithMethodCache WorkWithMethodCache;
        private static readonly Dictionary<string, Action<TModel, object>> SetterCache;
        private static readonly Dictionary<string, Func<TContent, object>> GetterCache;

        private IQueryable<TContent> query;

        static SitefinityQuery()
        {
            GetMethodCache = new GetMethodCache
            {
                GetString = typeof(DataExtensions).GetMethod("GetString", new[] { typeof(IDynamicFieldsContainer), typeof(string) }),
                GetValue = typeof(DataExtensions).GetMethods().Single(m => m.Name == "GetValue" && m.IsGenericMethod),
                GetRelatedItem = typeof(RelatedDataExtensions).GetMethod("GetRelatedItem"),
                GetRelatedItems = typeof(RelatedDataExtensions).GetMethod("GetRelatedItems")
            };

            string typeName = typeof(TContent).Name + "s";
            var workWithMethod = typeof(FluentSitefinity).GetMethod(typeName);
            bool isExtension = false;

            if (workWithMethod == null)
            {
                workWithMethod = typeof(ContentModulesFluentExtensions).GetMethod(typeName, new[] { typeof(FluentSitefinity) });
                isExtension = true;
            }

            if (workWithMethod != null)
            {
                var facade = workWithMethod.Invoke(
                    isExtension ? null : App.WorkWith(), 
                    !isExtension ? null : new object[] { App.WorkWith() });

                WorkWithMethodCache = new WorkWithMethodCache
                {
                    WorkWithMethod = workWithMethod,
                    IsExtension = isExtension,
                    PublishedMethod = facade.GetType().GetMethod("Published"),
                    GetMethod = facade.GetType().GetMethod("Get", new Type[0])
                };
            }

            var setters = typeof(TModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { Info = p, Setter = p.CreateSetterDelegate<TModel>() })
                .Where(s => s.Setter != null);

            var getters = typeof(TContent)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { p.Name, Getter = p.CreateGetterDelegate<TContent>() })
                .Where(p => p.Getter != null);

            SetterCache = setters.ToDictionary(s => s.Info.Name, s => s.Setter);
            GetterCache = getters.ToDictionary(p => p.Name, p => p.Getter);

            setters
                .Where(s => !GetterCache.ContainsKey(s.Info.Name))
                .ToList()
                .ForEach(s => GetterCache.Add(s.Info.Name, s.Info.CreateGetMethodDelegate<TContent>(GetMethodCache)));
        }

        public SitefinityQuery(string dynamicTypeName)
        {
            if (WorkWithMethodCache != null)
            {
                var facade = WorkWithMethodCache.WorkWithMethod.Invoke(
                    WorkWithMethodCache.IsExtension ? null : App.WorkWith(),
                    !WorkWithMethodCache.IsExtension ? null : new object[] { App.WorkWith() });

                facade = WorkWithMethodCache.PublishedMethod.Invoke(facade, null);
                facade = WorkWithMethodCache.GetMethod.Invoke(facade, null);

                query = (IQueryable<TContent>)facade;
            }
            else
            {
                var dynamicType = TypeResolutionService.ResolveType(dynamicTypeName);
                var dynamicModuleType = DynamicModuleHelper.GetDynamicModuleType(dynamicType.Name, dynamicType.Namespace);

                query = DynamicModuleManager
                    .GetManager(DynamicModuleManager.GetDefaultProviderName(dynamicModuleType.ModuleName))
                    .GetDataItems(dynamicType)
                    .Cast<TContent>();
            }
        }

        private static TModel Map(TContent item)
        {
            if (item == null) return default(TModel);

            var model = Activator.CreateInstance<TModel>();

            foreach (var setter in SetterCache)
            {
                object value = GetterCache[setter.Key](item);
                if (value is Lstring)
                {
                    string temp = (Lstring)value;
                    value = temp;
                }
                setter.Value(model, value);
            }

            return model;
        }

        public SitefinityQuery<TModel, TContent> SitefinityWhere(Expression<Func<TContent, bool>> whereExpression)
        {
            query = query.Where(whereExpression);
            return this;
        }

        public SitefinityQuery<TModel, TContent> SitefinityOrderBy<TComparable>(Expression<Func<TContent, TComparable>> orderByExpression)
        {
            query = query.OrderBy(orderByExpression);
            return this;
        }

        public SitefinityQuery<TModel, TContent> SitefinityOrderByDescending<TComparable>(Expression<Func<TContent, TComparable>> orderByExpression)
        {
            query = query.OrderByDescending(orderByExpression);
            return this;
        }

        public SitefinityQuery<TModel, TContent> SitefinityThenBy<TComparable>(Expression<Func<TContent, TComparable>> orderByExpression)
        {
            query = ((IOrderedQueryable<TContent>)query).ThenBy(orderByExpression);
            return this;
        }

        public SitefinityQuery<TModel, TContent> SitefinityThenByDescending<TComparable>(Expression<Func<TContent, TComparable>> orderByExpression)
        {
            query = ((IOrderedQueryable<TContent>)query).ThenByDescending(orderByExpression);
            return this;
        }

        public TModel FirstOrDefault(Expression<Func<TModel, bool>> whereExpression)
        {
            return SitefinityWhere(whereExpression.ConvertToContentExpression<TModel, TContent>(GetMethodCache)).FirstOrDefault();
        }

        public ISitefinityQuery<TModel> Where(Expression<Func<TModel, bool>> whereExpression)
        {
            return SitefinityWhere(whereExpression.ConvertToContentExpression<TModel, TContent>(GetMethodCache));
        }

        public ISitefinityQuery<TModel> OrderBy<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression)
            where TComparable : IComparable
        {
            return SitefinityOrderBy(orderByExpression.ConvertToContentExpression<TModel, TContent, TComparable>(GetMethodCache));
        }

        public ISitefinityQuery<TModel> OrderByDescending<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression)
            where TComparable : IComparable
        {
            return SitefinityOrderByDescending(orderByExpression.ConvertToContentExpression<TModel, TContent, TComparable>(GetMethodCache));
        }

        public ISitefinityQuery<TModel> ThenBy<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression)
            where TComparable : IComparable
        {
            return SitefinityThenBy(orderByExpression.ConvertToContentExpression<TModel, TContent, TComparable>(GetMethodCache));
        }

        public ISitefinityQuery<TModel> ThenByDescending<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression)
            where TComparable : IComparable
        {
            return SitefinityThenByDescending(orderByExpression.ConvertToContentExpression<TModel, TContent, TComparable>(GetMethodCache));
        }

        public ISitefinityQuery<TModel> Take(int count)
        {
            query = query.Take(count);
            return this;
        }

        public ISitefinityQuery<TModel> Skip(int count)
        {
            query = query.Skip(count);
            return this;
        }

        public List<TModel> ToList()
        {
            return query.Published().Select(Map).ToList();
        }

        public TModel FirstOrDefault()
        {
            return Map(query.Published().FirstOrDefault());
        }
    }
}
