using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;

namespace Impey.Sitefinity.Repository.Extensions
{
    public static class RelatedDataExtensions
    {
        public static T GetRelatedItem<T>(this IDynamicFieldsContainer dataItem, string fieldName)
            where T : IDataItem, ILifecycleDataItem, IScheduleable
        {
            return Telerik.Sitefinity.RelatedData.RelatedDataExtensions.GetRelatedItems<T>(dataItem, fieldName).Published().FirstOrDefault();
        }

        public static List<T> GetRelatedItems<T>(this IDynamicFieldsContainer dataItem, string fieldName)
            where T : IDataItem, ILifecycleDataItem, IScheduleable
        {
            return Telerik.Sitefinity.RelatedData.RelatedDataExtensions.GetRelatedItems<T>(dataItem, fieldName).Published().ToList();
        }
    }
}
