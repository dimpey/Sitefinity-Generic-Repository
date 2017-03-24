using System.Reflection;

namespace Impey.Sitefinity.Repository.Models
{
    public class GetMethodCache
    {
        public MethodInfo GetString { get; set; }
        public MethodInfo GetValue { get; set; }
        public MethodInfo GetRelatedItem { get; set; }
        public MethodInfo GetRelatedItems { get; set; }
    }
}
