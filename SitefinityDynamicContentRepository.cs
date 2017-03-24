using Telerik.Sitefinity.DynamicModules.Model;

namespace Impey.Sitefinity.Repository
{
    public class SitefinityDynamicContentRepository<TModel> : SitefinityRepository<TModel, DynamicContent>
    {
        public SitefinityDynamicContentRepository(string dynamicTypeName)
            : base(dynamicTypeName)
        {
        }

        public SitefinityDynamicContentRepository()
        {
        }
    }
}
