namespace SqlParser.Net.Ast.Expression;

/// <summary>
/// Expressions with aliases
/// ���б����ı��ʽ
/// </summary>
public interface ICloneableExpression
{
    public SqlExpression InternalClone();
}
