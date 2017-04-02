using System;
using Telerik.Sitefinity.Taxonomies;
using Telerik.Sitefinity.Taxonomies.Model;
using System.Linq;

namespace Impey.Sitefinity.Repository.Fields
{
    public class Tag : Category
    {
        public Tag(Guid id)
            : base(id)
        {
        }

        public Tag(string urlName)
            : base(TaxonomyManager.GetManager().GetTaxa<FlatTaxon>().FirstOrDefault(t => t.UrlName == urlName.ToLower()))
        {
        }

        public static implicit operator Tag(Guid id)
        {
            return new Tag(id);
        }

        public static implicit operator Tag(string urlName)
        {
            return new Tag(urlName);
        }
    }
}
