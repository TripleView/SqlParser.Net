using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlWithinGroupExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlWithinGroupExpression(this);
    }

    public SqlWithinGroupExpression()
    {
        this.Type = SqlExpressionType.WithinGroup;
    }

    public SqlOrderByExpression OrderBy { get; set; }

    protected bool Equals(SqlWithinGroupExpression other)
    {
        return OrderBy.Equals(other.OrderBy);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlWithinGroupExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}