using System.Reflection;

namespace Impey.Sitefinity.Repository.Models
{
    public class WorkWithMethodCache
    {
        public MethodInfo WorkWithMethod { get; set; }
        public MethodInfo PublishedMethod { get; set; }
        public MethodInfo GetMethod { get; set; }
        public bool IsExtension { get; set; }
    }
}
