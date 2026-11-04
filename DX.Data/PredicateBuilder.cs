using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DX.Data
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, Inline(expr2, expr1.Parameters[0])), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, Inline(expr2, expr1.Parameters[0])), expr1.Parameters);
        }

        /// <summary>
        /// Returns the body of <paramref name="expr"/> with its parameter replaced by <paramref name="parameter"/>.
        /// Query providers (XPO, EF) can translate the result, unlike an Expression.Invoke of the lambda.
        /// </summary>
        private static Expression Inline<T>(Expression<Func<T, bool>> expr, ParameterExpression parameter)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));
            var source = expr.Parameters[0];
            if (source == parameter)
                return expr.Body;
            return new ReplaceParameterVisitor(source, parameter).Visit(expr.Body)!;
        }

        private sealed class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _from;
            private readonly Expression _to;

            public ReplaceParameterVisitor(ParameterExpression from, Expression to)
            {
                _from = from;
                _to = to;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == _from ? _to : base.VisitParameter(node);
        }
    }
}
