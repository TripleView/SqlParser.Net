namespace SqlParser.Net.Ast.Expression;

public class SqlCaseItemExpression : SqlExpression
{

    public SqlCaseItemExpression()
    {
        this.Type = SqlExpressionType.CaseItem;
    }

    public SqlExpression Condition { get; set; }

    public SqlExpression Value { get; set; }

    protected bool Equals(SqlCaseItemExpression other)
    {
        if (Condition is null ^ other.Condition is null)
        {
            return false;
        }
        else if (Condition != null && other.Condition != null)
        {
            if (!Condition.Equals(other.Condition))
            {
                return false;
            }
        }
        if (Value is null ^ other.Value is null)
        {
            return false;
        }
        else if (Value != null && other.Value != null)
        {
            if (!Value.Equals(other.Value))
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
        return Equals((SqlCaseItemExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Condition.GetHashCode() * 397) ^ Value.GetHashCode();
        }
    }
}