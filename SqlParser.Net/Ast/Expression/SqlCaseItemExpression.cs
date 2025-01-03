using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlCaseItemExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlCaseItemExpression(this);
    }
    public SqlCaseItemExpression()
    {
        this.Type = SqlExpressionType.CaseItem;
    }

    public SqlExpression Condition { get; set; }

    public SqlExpression Value { get; set; }

    protected bool Equals(SqlCaseItemExpression other)
    {

        if (!CompareTwoSqlExpression(Condition, other.Condition))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Value, other.Value))
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
        return Equals((SqlCaseItemExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Condition.GetHashCode() * 397) ^ Value.GetHashCode();
        }
    }
}