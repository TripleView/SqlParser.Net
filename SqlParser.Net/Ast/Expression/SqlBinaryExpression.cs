using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlBinaryExpression : SqlExpression, ICollateExpression, ICloneableExpression<SqlBinaryExpression>
{
    private SqlExpression left;
    private SqlExpression right;
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>

    private SqlCollateExpression collate;

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

    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>
    public SqlCollateExpression Collate
    {
        get => collate;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            collate = value;
        }
    }


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

        if (!CompareTwoSqlExpression(Collate, other.Collate))
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

    public SqlBinaryExpression Clone()
    {
        throw new System.NotImplementedException();
    }
}

