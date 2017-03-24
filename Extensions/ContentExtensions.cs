using System;
using System.Linq;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;

namespace Impey.Sitefinity.Repository.Extensions
{
    public static class ContentExtensions
    {
        public static IQueryable<T> Published<T>(this IQueryable<T> items) where T : ILifecycleDataItem, IScheduleable
        {
            return items.Where(item =>
                item.Visible &&
                item.Status == ContentLifecycleStatus.Live &&
                item.PublicationDate <= DateTime.UtcNow &&
                (!item.ExpirationDate.HasValue || item.ExpirationDate.Value > DateTime.UtcNow));
        }
    }
}
