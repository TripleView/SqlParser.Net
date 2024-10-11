namespace SqlParser.Net.Ast.Expression;

public class SqlOverExpression : SqlExpression
{

    public SqlOverExpression()
    {
        this.Type = SqlExpressionType.Over;
    }

    public SqlOrderByExpression OrderBy { get; set; }

    public SqlPartitionByExpression PartitionBy { get; set; }

    protected bool Equals(SqlOverExpression other)
    {
        var result = true;
        if (OrderBy == null ^ other.OrderBy == null)
        {
            return false;
        }
        else if (OrderBy != null && other.OrderBy != null)
        {
            result &= OrderBy.Equals(other.OrderBy);
        }

        if (PartitionBy == null ^ other.PartitionBy == null)
        {
            return false;
        }
        else if (PartitionBy != null && other.PartitionBy != null)
        {
            result &= PartitionBy.Equals(other.PartitionBy);
        }

        return result;
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