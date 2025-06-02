using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlOrderByItemExpression : SqlExpression, ICollateExpression
{
    private SqlExpression body;

    private SqlCollateExpression collate;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlOrderByItemExpression(this);
    }
    public SqlOrderByItemExpression()
    {
        this.Type = SqlExpressionType.OrderByItem;
    }

    public SqlExpression Body
    {
        get => body;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            body = value;
        }
    }

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


    public SqlOrderByType? OrderByType { get; set; }

    public SqlOrderByNullsType? NullsType { get; set; }

    protected bool Equals(SqlOrderByItemExpression other)
    {
        if (!CompareTwoSqlExpression(Body, other.Body))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Collate, other.Collate))
        {
            return false;
        }

        if (OrderByType == null && other.OrderByType == SqlOrderByType.Asc ||
                 (OrderByType == SqlOrderByType.Asc && other.OrderByType == null))
        {
           
        }
        else if (OrderByType != other.OrderByType)
        {
            return false;
        }

        if (NullsType != other.NullsType)
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
        return Equals((SqlOrderByItemExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Body.GetHashCode() * 397) ^ OrderByType.GetHashCode();
        }
    }
}