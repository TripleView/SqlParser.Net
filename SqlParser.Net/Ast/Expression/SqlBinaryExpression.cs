using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlBinaryExpression : SqlExpression
{
    private SqlExpression left;
    private SqlExpression right;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlBinaryExpression(this);
    }


    public SqlBinaryExpression()
    {
        this.Type = SqlExpressionType.Binary;
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

    public SqlBinaryOperator Operator { get; set; }

    protected bool Equals(SqlBinaryExpression other)
    {

        if (!CompareTwoSqlExpression(Left, other.Left))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Right, other.Right))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Operator, other.Operator))
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
        return Equals((SqlBinaryExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Left.GetHashCode();
            hashCode = (hashCode * 397) ^ Right.GetHashCode();
            hashCode = (hashCode * 397) ^ Operator.GetHashCode();
            return hashCode;
        }
    }
}

