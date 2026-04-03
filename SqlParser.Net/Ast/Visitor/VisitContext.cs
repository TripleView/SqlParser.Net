using SqlParser.Net.Ast.Expression;

namespace SqlParser.Net.Ast.Visitor;

public class VisitContext
{
    /// <summary>
    /// Parent Expression
    /// </summary>
    public SqlExpression Parent { get; set; }
}