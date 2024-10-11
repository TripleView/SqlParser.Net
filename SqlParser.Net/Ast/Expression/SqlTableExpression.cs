namespace SqlParser.Net.Ast.Expression;

public class SqlTableExpression : SqlExpression
{
    public SqlTableExpression()
    {
        this.Type = SqlExpressionType.Table;
    }

    public SqlIdentifierExpression Alias { get; set; }

    public SqlIdentifierExpression Name { get; set; }

    protected bool Equals(SqlTableExpression other)
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

        result &= Name.Equals(other.Name);
        return result;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlTableExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Alias.GetHashCode() * 397) ^ Name.GetHashCode();
        }
    }
}