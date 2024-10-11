namespace SqlParser.Net.Ast.Expression;

public class SqlBinaryExpression : SqlExpression
{
    public SqlBinaryExpression()
    {
        this.Type = SqlExpressionType.Binary;
    }

    public SqlExpression Left { set; get; }

    public SqlExpression Right { set; get; }

    public SqlBinaryOperator Operator { get; set; }

    protected bool Equals(SqlBinaryExpression other)
    {
        if (Left is null ^ other.Left is null)
        {
            return false;
        }
        else if (Left != null && other.Left != null)
        {
            if (!Left.Equals(other.Left))
            {
                return false;
            }
        }

        if (Right is null ^ other.Right is null)
        {
            return false;
        }
        else if (Right != null && other.Right != null)
        {
            if (!Right.Equals(other.Right))
            {
                return false;
            }
        }

        if (Operator is null ^ other.Operator is null)
        {
            return false;
        }
        else if (Operator != null && other.Operator != null)
        {
            if (!Operator.Equals(other.Operator))
            {
                return false;
            }
        }
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlBinaryExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Left.GetHashCode();
            hashCode = (hashCode * 397) ^ Right.GetHashCode();
            hashCode = (hashCode * 397) ^ Operator.GetHashCode();
            return hashCode;
        }
    }
}

