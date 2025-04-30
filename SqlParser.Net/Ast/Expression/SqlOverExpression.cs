using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlOverExpression : SqlExpression
{
    private SqlOrderByExpression orderBy;
    private SqlPartitionByExpression partitionBy;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlOverExpression(this);
    }
    public SqlOverExpression()
    {
        this.Type = SqlExpressionType.Over;
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

    public SqlPartitionByExpression PartitionBy
    {
        get => partitionBy;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            partitionBy = value;
        }
    }

    protected bool Equals(SqlOverExpression other)
    {
        if (!CompareTwoSqlExpression(OrderBy, other.OrderBy))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(PartitionBy, other.PartitionBy))
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
        return Equals((SqlOverExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (OrderBy.GetHashCode() * 397) ^ PartitionBy.GetHashCode();
        }
    }
}