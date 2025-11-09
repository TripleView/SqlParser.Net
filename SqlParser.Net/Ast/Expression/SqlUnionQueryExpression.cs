using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlUnionQueryExpression : SqlExpression
{
    private SqlExpression left;
    private SqlExpression right;

    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlUnionQueryExpression(this);
    }
    public SqlUnionQueryExpression()
    {
        this.Type = SqlExpressionType.UnionQuery;
    }

    public SqlExpression Left
    {
        get => left;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            left = value;
        }
    }

    public SqlUnionType UnionType { get; set; }

    public SqlExpression Right
    {
        get => right;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            right = value;
        }
    }

    protected bool Equals(SqlUnionQueryExpression other)
    {
        if (!CompareTwoSqlExpression(Left, other.Left))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Right, other.Right))
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

    public override SqlExpression InternalClone()
    {
        var result = new SqlUnionQueryExpression()
        {
            DbType = this.DbType,
            Left = this.Left.Clone(),
            Right = this.Right.Clone(),
            UnionType = UnionType
        };
        return result;
    }
}