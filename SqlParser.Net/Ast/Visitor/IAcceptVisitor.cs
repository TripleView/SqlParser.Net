namespace SqlParser.Net.Ast.Visitor;

public interface IAcceptVisitor
{
    void Accept(IAstVisitor visitor);
}