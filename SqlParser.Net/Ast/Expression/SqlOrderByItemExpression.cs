namespace SqlParser.Net.Ast.Expression;

public class SqlOrderByItemExpression : SqlExpression
{
    public SqlOrderByItemExpression()
    {
        this.Type = SqlExpressionType.OrderByItem;
    }

    public SqlExpression Expression { get; set; }

    public SqlOrderByType? OrderByType { get; set; }

    protected bool Equals(SqlOrderByItemExpression other)
    {
        if (!Expression.Equals(other.Expression))
        {
            return false;
        }


        if (OrderByType == other.OrderByType)
        {
            return true;
        }
        else if (OrderByType == null && other.OrderByType == SqlOrderByType.Asc ||
                 (OrderByType == SqlOrderByType.Asc && other.OrderByType == null))
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlOrderByItemExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Expression.GetHashCode() * 397) ^ OrderByType.GetHashCode();
        }
    }
}