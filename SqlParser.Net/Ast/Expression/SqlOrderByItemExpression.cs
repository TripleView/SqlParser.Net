using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlOrderByItemExpression : SqlExpression
{
    private SqlExpression body;

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

    public SqlOrderByType? OrderByType { get; set; }

    public SqlOrderByNullsType? NullsType { get; set; }

    protected bool Equals(SqlOrderByItemExpression other)
    {
        if (!CompareTwoSqlExpression(Body, other.Body))
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