using SqlParser.Net.Ast.Visitor;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;

public class SqlAllExpression : SqlExpression
{

    private SqlExpression body;
    public SqlAllExpression()
    {
        this.Type = SqlExpressionType.All;
    }

    public SqlExpression Body
    {
        get => body;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            body = value;
        }
    }

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
    public override SqlExpression Accept(IAstVisitor visitor)
    {
		return visitor.VisitSqlAllExpression(this);
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlAllExpression()
        {
            DbType = this.DbType,
            Body = this.Body.Clone()
        };
        return result;
    }
}

public class SqlAllExpressionTokenContext
{
    public Token? All { get; set; }
}