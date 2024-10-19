using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlExistsExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlExistsExpression(this);
    }
    public SqlExistsExpression()
    {
        this.Type = SqlExpressionType.Exists;
    }

    public SqlSelectExpression SelectExpression { get; set; }

    protected bool Equals(SqlExistsExpression other)
    {
        return SelectExpression.Equals(other.SelectExpression);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlExistsExpression)obj);
    }

    public override int GetHashCode()
    {
        return SelectExpression.GetHashCode();
    }
}