using Impey.Sitefinity.Repository.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Impey.Sitefinity.Repository.Extensions
{
    public static class TypeExtensions
    {
        public static string InferDynamicTypeName(this Type type)
        {
            var dynamicModule = DynamicModuleHelper.GetDynamicModuleType(type.Name);

            return dynamicModule != null ?
                dynamicModule.GetFullTypeName() :
                string.Format("Telerik.Sitefinity.DynamicTypes.Model.{0}s.{0}", type.Name);
        }
    }
}
