using Impey.Sitefinity.Repository.Cache;
using Impey.Sitefinity.Repository.Fields;
using System;
using System.Linq.Expressions;
using System.Reflection;
using Telerik.OpenAccess;
using Telerik.Sitefinity.Model;

namespace Impey.Sitefinity.Repository.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression ConvertToContentExpression<TContent>(this Expression expression, ParameterExpression parameter)
        {
            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null)
            {
                return Expression.MakeBinary(
                    binaryExpression.NodeType,
                    binaryExpression.Left.ConvertToContentExpression<TContent>(parameter),
                    binaryExpression.Right.ConvertToContentExpression<TContent>(parameter)
                );
            }

            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null)
            {
                var newObject = methodCallExpression.Object.ConvertToContentExpression<TContent>(parameter);
                if (newObject.Type == typeof(TrackedList<Guid>))
                {
                    var newMethod = newObject.Type.GetMethod(methodCallExpression.Method.Name);

                    var argumentExpression = methodCallExpression.Arguments[0] as UnaryExpression;
                    if (argumentExpression != null)
                    {
                        var constantExpression = argumentExpression.Operand as ConstantExpression;
                        if (constantExpression != null)
                        {
                            Category category = (string)constantExpression.Value;
                            return Expression.Call(newObject, newMethod, Expression.Constant(category.Id));
                        }
                    }
                }
            }

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                var info = memberExpression.Member as PropertyInfo;
                var name = info.GetName();

                var prop = typeof(TContent).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                    return Expression.Property(parameter, prop);

                var type = info.PropertyType;
                if (type == typeof(string)) type = typeof(Lstring); // GetString doesn't work in queries :/
                var method = GetMethodCache.ConstructForType(type);

                return Expression.Call(method, parameter, Expression.Constant(name));
            }

            return expression;
        }

        public static Expression<Func<TContent, bool>> ConvertToContentExpression<TModel, TContent>(this Expression<Func<TModel, bool>> whereExpression)
        {
            var parameter = Expression.Parameter(typeof(TContent));
            var contentExpression = whereExpression.Body.ConvertToContentExpression<TContent>(parameter);

            return Expression.Lambda<Func<TContent, bool>>(contentExpression, parameter);
        }

        public static Expression<Func<TContent, TKey>> ConvertToContentExpression<TModel, TContent, TKey>(this Expression<Func<TModel, TKey>> selectorExpression)
        {
            var parameter = Expression.Parameter(typeof(TContent));
            var contentExpression = selectorExpression.Body.ConvertToContentExpression<TContent>(parameter);

            return Expression.Lambda<Func<TContent, TKey>>(contentExpression, parameter);
        }
    }
}
