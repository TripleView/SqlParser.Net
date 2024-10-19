using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlAnyExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlAnyExpression(this);
    }
    public SqlAnyExpression()
    {
        this.Type = SqlExpressionType.Any;
    }

    public SqlSelectExpression SelectExpression { get; set; }

    protected bool Equals(SqlAnyExpression other)
    {
        return SelectExpression.Equals(other.SelectExpression);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlAnyExpression)obj);
    }

    public override int GetHashCode()
    {
        return SelectExpression.GetHashCode();
    }
}