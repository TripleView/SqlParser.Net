using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlWithinGroupExpression : SqlExpression
{
    private SqlOrderByExpression orderBy;

    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlWithinGroupExpression(this);
    }

    public SqlWithinGroupExpression()
    {
        this.Type = SqlExpressionType.WithinGroup;
    }

    public SqlOrderByExpression OrderBy
    {
        get => orderBy;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            orderBy = value;
        }
    }

    protected bool Equals(SqlWithinGroupExpression other)
    {
        if (!CompareTwoSqlExpression(OrderBy, other.OrderBy))
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
        return Equals((SqlWithinGroupExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlWithinGroupExpression()
        {
            DbType = this.DbType,
            OrderBy = this.OrderBy.Clone(),

        };
        return result;
    }
}