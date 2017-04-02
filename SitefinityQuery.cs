using Impey.Sitefinity.Repository.Base;
using Impey.Sitefinity.Repository.Cache;
using Impey.Sitefinity.Repository.Extensions;
using Impey.Sitefinity.Repository.Fields;
using Impey.Sitefinity.Repository.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Telerik.OpenAccess;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace Impey.Sitefinity.Repository
{
    public class SitefinityQuery<TModel, TContent> : ISitefinityQuery<TModel>
        where TContent : ILifecycleDataItem, IScheduleable
    {
        private static readonly Dictionary<string, Action<TModel, object>> SetterCache;
        private static readonly Dictionary<string, Func<TContent, object>> GetterCache;

        private IQueryable<TContent> query;

        static SitefinityQuery()
        {
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
                .ForEach(s => GetterCache.Add(s.Info.Name, s.Info.CreateGetMethodDelegate<TContent>()));
        }

        public SitefinityQuery(string dynamicTypeName, bool published = true)
        {
            query = WorkWithMethodCache<TContent>.GetQuery(published);

            if (query == null)
            {
                var dynamicType = TypeResolutionService.ResolveType(dynamicTypeName);
                var dynamicModuleType = DynamicModuleHelper.GetDynamicModuleType(dynamicType.Name, dynamicType.Namespace);

                query = DynamicModuleManager
                    .GetManager(DynamicModuleManager.GetDefaultProviderName(dynamicModuleType.ModuleName))
                    .GetDataItems(dynamicType)
                    .Cast<TContent>();

                if (published)
                {
                    query = query.Published();
                }
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
                else if (value is Telerik.Sitefinity.Libraries.Model.Image)
                {
                    Image temp = (Telerik.Sitefinity.Libraries.Model.Image)value;
                    value = temp;
                }
                else if (value is TrackedList<Guid>)
                {
                    var listType = typeof(TModel).GetProperty(setter.Key).PropertyType;
                    var categoryType = listType.GetGenericArguments()[0];

                    var list = (IList)Activator.CreateInstance(listType);
                    foreach (var id in (TrackedList<Guid>)value)
                    {
                        list.Add(Activator.CreateInstance(categoryType, id));
                    }

                    value = list;
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
            return SitefinityWhere(whereExpression.ConvertToContentExpression<TModel, TContent>()).FirstOrDefault();
        }

        public ISitefinityQuery<TModel> Where(Expression<Func<TModel, bool>> whereExpression)
        {
            return SitefinityWhere(whereExpression.ConvertToContentExpression<TModel, TContent>());
        }

        public ISitefinityQuery<TModel> OrderBy<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression)
            where TComparable : IComparable
        {
            return SitefinityOrderBy(orderByExpression.ConvertToContentExpression<TModel, TContent, TComparable>());
        }

        public ISitefinityQuery<TModel> OrderByDescending<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression)
            where TComparable : IComparable
        {
            return SitefinityOrderByDescending(orderByExpression.ConvertToContentExpression<TModel, TContent, TComparable>());
        }

        public ISitefinityQuery<TModel> ThenBy<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression)
            where TComparable : IComparable
        {
            return SitefinityThenBy(orderByExpression.ConvertToContentExpression<TModel, TContent, TComparable>());
        }

        public ISitefinityQuery<TModel> ThenByDescending<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression)
            where TComparable : IComparable
        {
            return SitefinityThenByDescending(orderByExpression.ConvertToContentExpression<TModel, TContent, TComparable>());
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
            return query.Select(Map).ToList();
        }

        public TModel FirstOrDefault()
        {
            return Map(query.FirstOrDefault());
        }

        public IEnumerator<TModel> GetEnumerator()
        {
            return query.Select(Map).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
