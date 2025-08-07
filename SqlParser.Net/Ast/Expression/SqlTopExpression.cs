using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlTopExpression : SqlExpression
{
    private SqlNumberExpression body;

    public SqlTopExpression()
    {
        this.Type = SqlExpressionType.Top;
    }

    public SqlNumberExpression Body
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

    protected bool Equals(SqlTopExpression other)
    {
        if (!CompareTwoSqlExpression(Body, other.Body))
        {
            return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlTopExpression)obj);
    }

    public override int GetHashCode()
    {
        return Body.GetHashCode();
    }
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlTopExpression(this);
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlTopExpression()
        {
            DbType = this.DbType,
            Body = this.Body.Clone(),
        };
        return result;
    }
}