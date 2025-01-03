using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlLimitExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlLimitExpression(this);
    }
    public SqlLimitExpression()
    {
        this.Type = SqlExpressionType.Limit;
    }

    public SqlExpression Offset { get; set; }
    public SqlExpression RowCount { get; set; }

    protected bool Equals(SqlLimitExpression other)
    {
        if (!CompareTwoSqlExpression(Offset, other.Offset))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(RowCount, other.RowCount))
        {
            return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlLimitExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Offset.GetHashCode() * 397) ^ RowCount.GetHashCode();
        }
    }
}