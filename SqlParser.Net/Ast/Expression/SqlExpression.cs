using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlExpression : IAcceptVisitor
{
    public virtual void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlExpression(this);
    }
    public virtual SqlExpressionType Type { get; protected set; }

    public SqlExpression()
    {

    }



}