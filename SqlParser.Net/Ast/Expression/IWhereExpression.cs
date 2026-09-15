namespace SqlParser.Net.Ast.Expression;

/// <summary>
/// Expression with WHERE clause
/// 具有where子句的表达式
/// </summary>
public interface IWhereExpression
{
    public SqlExpression Where { set; get; }
}