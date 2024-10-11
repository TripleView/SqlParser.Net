namespace SqlParser.Net.Ast.Expression;

public class SqlIdentifierExpression : SqlExpression
{
    public SqlIdentifierExpression()
    {
        this.Type = SqlExpressionType.Identifier;
    }

    public string Name { get; set; }

    protected bool Equals(SqlIdentifierExpression other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlIdentifierExpression)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

}