using System.Linq;
using Telerik.Sitefinity.DynamicModules.Builder;
using Telerik.Sitefinity.DynamicModules.Builder.Model;

namespace Impey.Sitefinity.Repository.Helpers
{
    public static class DynamicModuleHelper
    {
        public static DynamicModuleType GetDynamicModuleType(string typeName, string typeNamespace = null)
        {
            return ModuleBuilderManager
               .GetManager().Provider
               .GetDynamicModuleTypes()
               .FirstOrDefault(t => t.TypeName == typeName && (typeNamespace == null || t.TypeNamespace == typeNamespace));
        }
    }
}
