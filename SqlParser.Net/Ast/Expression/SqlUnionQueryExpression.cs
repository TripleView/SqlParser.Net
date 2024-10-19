using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlUnionQueryExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlUnionQueryExpression(this);
    }
    public SqlUnionQueryExpression()
    {
        this.Type = SqlExpressionType.UnionQuery;
    }

    public SqlExpression Left { get; set; }
    public SqlUnionType UnionType { get; set; }
    public SqlExpression Right { get; set; }

    protected bool Equals(SqlUnionQueryExpression other)
    {
        if (!Left.Equals(other.Left))
        {
            return false;
        }
        if (!Right.Equals(other.Right))
        {
            return false;
        }
        if (!UnionType.Equals(other.UnionType))
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
        return Equals((SqlUnionQueryExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Left.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)UnionType;
            hashCode = (hashCode * 397) ^ Right.GetHashCode();
            return hashCode;
        }
    }
}