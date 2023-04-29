using System.Linq.Expressions;
using Database.Interfaces;

namespace WebApi.Misc;

public static class LongExpressionHandlers
{
    public static Expression<Func<TIntEntity, bool>> And<TIntEntity>(this Expression<Func<TIntEntity, bool>> expression,
        Expression<Func<TIntEntity, bool>> andExpression) where TIntEntity : IIntEntity

    {
        return Expression.Lambda<Func<TIntEntity, bool>>(Expression.AndAlso(
            new SwapVisitor(expression.Parameters[0], andExpression.Parameters[0]).Visit(expression.Body),
            andExpression.Body), andExpression.Parameters);
    }

    public static Expression<Func<TIntEntity, bool>> Or<TIntEntity>(this Expression<Func<TIntEntity, bool>> expression,
        Expression<Func<TIntEntity, bool>> orExpression) where TIntEntity : IIntEntity

    {
        return Expression.Lambda<Func<TIntEntity, bool>>(Expression.OrElse(
            new SwapVisitor(expression.Parameters[0], orExpression.Parameters[0]).Visit(expression.Body),
            orExpression.Body), orExpression.Parameters);
    }
}

public static class IntExpressionHandlers
{
    public static Expression<Func<TLongEntity, bool>> And<TLongEntity>(
        this Expression<Func<TLongEntity, bool>> expression,
        Expression<Func<TLongEntity, bool>> andExpression) where TLongEntity : ILongEntity

    {
        return Expression.Lambda<Func<TLongEntity, bool>>(Expression.AndAlso(
            new SwapVisitor(expression.Parameters[0], andExpression.Parameters[0]).Visit(expression.Body),
            andExpression.Body), andExpression.Parameters);
    }

    public static Expression<Func<TIntEntity, bool>> Or<TIntEntity>(this Expression<Func<TIntEntity, bool>> expression,
        Expression<Func<TIntEntity, bool>> orExpression) where TIntEntity : IIntEntity

    {
        return Expression.Lambda<Func<TIntEntity, bool>>(Expression.OrElse(
            new SwapVisitor(expression.Parameters[0], orExpression.Parameters[0]).Visit(expression.Body),
            orExpression.Body), orExpression.Parameters);
    }
}

internal class SwapVisitor : ExpressionVisitor
{
    private readonly Expression _from, _to;

    public SwapVisitor(Expression from, Expression to)
    {
        _from = from;
        _to = to;
    }

    public override Expression Visit(Expression node)
    {
        return node == _from ? _to : base.Visit(node);
    }
}