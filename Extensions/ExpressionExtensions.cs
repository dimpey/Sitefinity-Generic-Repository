using Impey.Sitefinity.Repository.Models;
using System;
using System.Linq.Expressions;
using System.Reflection;
using Telerik.Sitefinity.Libraries.Model;

namespace Impey.Sitefinity.Repository.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression ConvertToContentExpression<TContent>(this Expression expression, ParameterExpression parameter, GetMethodCache methodCache)
        {
            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null)
            {
                return Expression.MakeBinary(
                    binaryExpression.NodeType,
                    binaryExpression.Left.ConvertToContentExpression<TContent>(parameter, methodCache),
                    binaryExpression.Right.ConvertToContentExpression<TContent>(parameter, methodCache)
                );
            }

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                var info = memberExpression.Member as PropertyInfo;

                var prop = typeof(TContent).GetProperty(info.Name, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                    return Expression.Property(parameter, prop);

                var method = info.PropertyType == typeof(Image) ? methodCache.GetRelatedItem : methodCache.GetValue;
                return Expression.Call(method.MakeGenericMethod(info.PropertyType), parameter, Expression.Constant(info.Name));
            }

            return expression;
        }

        public static Expression<Func<TContent, bool>> ConvertToContentExpression<TModel, TContent>(this Expression<Func<TModel, bool>> whereExpression, GetMethodCache methodCache)
        {
            var parameter = Expression.Parameter(typeof(TContent));

            var oldBinaryExpression = (BinaryExpression)whereExpression.Body;
            var newBinaryExpression = Expression.MakeBinary(
                oldBinaryExpression.NodeType,
                oldBinaryExpression.Left.ConvertToContentExpression<TContent>(parameter, methodCache),
                oldBinaryExpression.Right.ConvertToContentExpression<TContent>(parameter, methodCache)
            );

            return Expression.Lambda<Func<TContent, bool>>(newBinaryExpression, parameter);
        }

        public static Expression<Func<TContent, TKey>> ConvertToContentExpression<TModel, TContent, TKey>(this Expression<Func<TModel, TKey>> selectorExpression, GetMethodCache methodCache)
        {
            var parameter = Expression.Parameter(typeof(TContent));
            var contentExpression = selectorExpression.Body.ConvertToContentExpression<TContent>(parameter, methodCache);

            return Expression.Lambda<Func<TContent, TKey>>(contentExpression, parameter);
        }
    }
}
