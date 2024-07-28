using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> True<T>() { return f => true; }
    public static Expression<Func<T, bool>> False<T>() { return f => false; }

    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var secondBody = expr2.Body.Replace(
            expr2.Parameters[0], expr1.Parameters[0]);
        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(expr1.Body, secondBody),
            expr1.Parameters);
    }

    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var secondBody = expr2.Body.Replace(
            expr2.Parameters[0], expr1.Parameters[0]);
        return Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(expr1.Body, secondBody),
                    expr1.Parameters);
    }

    public static Expression Replace(
        this Expression expression,
        Expression searchEx,
        Expression replaceEx)
    {
        return new ReplaceVisitor(searchEx, replaceEx).Visit(expression);
    }

    internal class ReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression from, to;
        public ReplaceVisitor(Expression from, Expression to)
        {
            this.from = from;
            this.to = to;
        }

        public override Expression Visit(Expression node)
        {
            return node == from ? to : base.Visit(node);
        }
    }
}