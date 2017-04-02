using System;
using Telerik.Sitefinity.Taxonomies;
using Telerik.Sitefinity.Taxonomies.Model;
using System.Linq;

namespace Impey.Sitefinity.Repository.Fields
{
    public class Category
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string UrlName { get; private set; }

        public Category(Guid id)
            : this(TaxonomyManager.GetManager().GetTaxon(id))
        {
        }

        public Category(string urlName)
            : this(TaxonomyManager.GetManager().GetTaxa<HierarchicalTaxon>().FirstOrDefault(t => t.UrlName == urlName.ToLower()))
        {
        }

        public Category(ITaxon taxon)
        {
            if (taxon != null)
            {
                Id = taxon.Id;
                Title = taxon.Title;
                Description = taxon.Description;
                UrlName = taxon.UrlName;
            }
        }

        public static implicit operator Category(Guid id)
        {
            return new Category(id);
        }

        public static implicit operator Category(string urlName)
        {
            return new Category(urlName);
        }
    }
}
