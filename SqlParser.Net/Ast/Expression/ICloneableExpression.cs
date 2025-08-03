namespace SqlParser.Net.Ast.Expression;

/// <summary>
/// Expressions with aliases
/// 具有别名的表达式
/// </summary>
public interface ICloneableExpression<T> where T:SqlExpression
{
    public T Clone();
}
