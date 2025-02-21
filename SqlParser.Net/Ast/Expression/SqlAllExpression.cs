using SqlParser.Net.Ast.Visitor;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;

public class SqlAllExpression : SqlExpression
{

    public SqlAllExpression()
    {
        this.Type = SqlExpressionType.All;
    }

    public SqlSelectExpression Body { get; set; }

    public SqlAllExpressionTokenContext TokenContext { get; set; }

    protected bool Equals(SqlAllExpression other)
    {
        return Body.Equals(other.Body);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlAllExpression)obj);
    }

    public override int GetHashCode()
    {
        return Body.GetHashCode();
    }
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlAllExpression(this);
    }
}

public class SqlAllExpressionTokenContext
{
    public Token? All { get; set; }
}