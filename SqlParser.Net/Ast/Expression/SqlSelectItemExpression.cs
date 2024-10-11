namespace SqlParser.Net.Ast.Expression;

public class SqlSelectItemExpression : SqlExpression
{
    public SqlSelectItemExpression()
    {
        this.Type = SqlExpressionType.SelectItem;
    }

    public SqlExpression Body { get; set; }

    public SqlIdentifierExpression Alias { get; set; }

    protected bool Equals(SqlSelectItemExpression other)
    {
        var result = true;
        if (Alias == null ^ other.Alias == null)
        {
            return false;
        }
        else if (Alias != null && other.Alias != null)
        {
            result &= Alias.Equals(other.Alias);
        }

        result &= Body.Equals(other.Body);
        return result;
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
}