using SqlParser.Net.Ast.Expression;

namespace SqlParser.Net.Ast.Visitor;

public interface IAcceptVisitor
{
    SqlExpression Accept(IAstVisitor visitor);
}