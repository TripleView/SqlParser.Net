namespace SqlParser.Net.Ast.Expression;

/// <summary>
/// Expressions with aliases
/// 具有别名的表达式
/// </summary>
public interface IAliasExpression
{
    public SqlIdentifierExpression Alias { set; get; }
}