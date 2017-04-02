using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impey.Sitefinity.Repository.Base
{
    public interface ISitefinityQuery<TModel> : IEnumerable<TModel>
    {
        List<TModel> ToList();
        TModel FirstOrDefault();
        TModel FirstOrDefault(Expression<Func<TModel, bool>> whereExpression);
        ISitefinityQuery<TModel> Where(Expression<Func<TModel, bool>> whereExpression);
        ISitefinityQuery<TModel> OrderBy<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression) where TComparable : IComparable;
        ISitefinityQuery<TModel> OrderByDescending<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression) where TComparable : IComparable;
        ISitefinityQuery<TModel> ThenBy<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression) where TComparable : IComparable;
        ISitefinityQuery<TModel> ThenByDescending<TComparable>(Expression<Func<TModel, TComparable>> orderByExpression) where TComparable : IComparable;
        ISitefinityQuery<TModel> Skip(int count);
        ISitefinityQuery<TModel> Take(int count);
    }
}
