namespace SqlParser.Net.Ast.Expression;

public class SqlAllColumnExpression : SqlExpression
{

    public SqlAllColumnExpression()
    {
        this.Type = SqlExpressionType.AllColumn;
    }

    protected bool Equals(SqlAllColumnExpression other)
    {
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlAllColumnExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}