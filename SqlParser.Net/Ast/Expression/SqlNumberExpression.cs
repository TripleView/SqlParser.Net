namespace SqlParser.Net.Ast.Expression;

public class SqlNumberExpression : SqlExpression
{
    public SqlNumberExpression()
    {
        this.Type = SqlExpressionType.Number;
    }

    public decimal Value { get; set; }

    protected bool Equals(SqlNumberExpression other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlNumberExpression)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}