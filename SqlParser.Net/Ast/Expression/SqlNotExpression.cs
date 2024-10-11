namespace SqlParser.Net.Ast.Expression;

public class SqlNotExpression : SqlExpression
{

    public SqlNotExpression()
    {
        this.Type = SqlExpressionType.Not;
    }

    public SqlExpression Expression { get; set; }

    protected bool Equals(SqlNotExpression other)
    {
        return Expression.Equals(other.Expression);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlNotExpression)obj);
    }

    public override int GetHashCode()
    {
        return Expression.GetHashCode();
    }
}