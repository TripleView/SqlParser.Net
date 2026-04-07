using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlSelectItemExpression : SqlExpression, IAliasExpression
{
    private SqlExpression body;
    private SqlIdentifierExpression alias;

    public override SqlExpression Accept(IAstVisitor visitor, VisitContext context = null)
    {
        return visitor.VisitSqlSelectItemExpression(this, context);
    }
    public SqlSelectItemExpression()
    {
        this.Type = SqlExpressionType.SelectItem;
    }

    public SqlExpression Body
    {
        get => body;
        set
        {
            body = value;
        }
    }

    public SqlIdentifierExpression Alias
    {
        get => alias;
        set
        {
            alias = value;
        }
    }

    protected bool Equals(SqlSelectItemExpression other)
    {
        if (!CompareTwoSqlExpression(Body, other.Body))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Alias, other.Alias))
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
        return Equals((SqlSelectItemExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Body.GetHashCode() * 397) ^ Alias.GetHashCode();
        }
    }
    public override SqlExpression InternalClone()
    {
        var result = new SqlSelectItemExpression()
        {
            DbType = this.DbType,
            Body = this.Body.Clone(),
            Alias = this.Alias.Clone(),
        };
        return result;
    }
}