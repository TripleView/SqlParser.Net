using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlStringExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlStringExpression(this);
    }
    public SqlStringExpression()
    {
        this.Type = SqlExpressionType.String;
    }

    public string Value { get; set; }

    public bool IsUniCode { get; set; }

    protected bool Equals(SqlStringExpression other)
    {
        return IsUniCode == other.IsUniCode && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlStringExpression)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}