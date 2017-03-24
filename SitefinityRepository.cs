using Impey.Sitefinity.Repository.Base;
using Impey.Sitefinity.Repository.Extensions;
using System;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;

namespace Impey.Sitefinity.Repository
{
    public class SitefinityRepository<TModel, TContent> : ISitefinityRepository<TModel>
        where TContent : ILifecycleDataItem, IScheduleable
    {
        private readonly string dynamicTypeName;

        public SitefinityRepository(string dynamicTypeName)
        {
            this.dynamicTypeName = dynamicTypeName;
        }

        public SitefinityRepository()
            : this(typeof(TContent) == typeof(DynamicContent) ? typeof(TModel).InferDynamicTypeName() : null)
        {
        }

        public TModel Get(Guid id)
        {
            return new SitefinityQuery<TModel, TContent>(dynamicTypeName)
                .SitefinityWhere(c => c.Id == id)
                .FirstOrDefault();
        }

        public ISitefinityQuery<TModel> GetAll()
        {
            return new SitefinityQuery<TModel, TContent>(dynamicTypeName);
        }
    }
}
