using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlJoinTableExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlJoinTableExpression(this);
    }
    public SqlJoinTableExpression()
    {
        this.Type = SqlExpressionType.JoinTable;
    }

    public SqlExpression Left { get; set; }

    public SqlJoinType JoinType { get; set; }
    public SqlExpression Right { get; set; }

    public SqlExpression Conditions { get; set; }

    protected bool Equals(SqlJoinTableExpression other)
    {
        if (JoinType != other.JoinType)
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Left, other.Left))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Right, other.Right))
        {
            return false;
        }
        
        if (!CompareTwoSqlExpression(Conditions, other.Conditions))
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
        return Equals((SqlJoinTableExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Left.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)JoinType;
            hashCode = (hashCode * 397) ^ Right.GetHashCode();
            hashCode = (hashCode * 397) ^ Conditions.GetHashCode();
            return hashCode;
        }
    }
}