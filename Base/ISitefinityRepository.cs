using System;

namespace Impey.Sitefinity.Repository.Base
{
    public interface ISitefinityRepository<TModel>
    {
        ISitefinityQuery<TModel> GetAll();
        TModel Get(Guid id);
    }
}
