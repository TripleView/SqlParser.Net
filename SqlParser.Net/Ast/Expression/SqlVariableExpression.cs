namespace SqlParser.Net.Ast.Expression;

public class SqlVariableExpression : SqlExpression
{
    public SqlVariableExpression()
    {
        this.Type = SqlExpressionType.Variable;
    }
    /// <summary>
    /// Prefix eg. : or @
    /// 前缀，例如:或者@
    /// </summary>
    public string Prefix { get; set; }

    public string Name { get; set; }

    protected bool Equals(SqlVariableExpression other)
    {
        return Prefix == other.Prefix && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlVariableExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Prefix.GetHashCode() * 397) ^ Name.GetHashCode();
        }
    }
}