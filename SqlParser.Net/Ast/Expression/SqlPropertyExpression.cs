using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlPropertyExpression : SqlExpression
{
    private SqlIdentifierExpression name;
    private SqlIdentifierExpression table;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlPropertyExpression(this);
    }
    public SqlPropertyExpression()
    {
        this.Type = SqlExpressionType.Property;
    }

    /// <summary>
    /// property name
    /// 属性名称
    /// </summary>
    public SqlIdentifierExpression Name
    {
        get => name;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            name = value;
        }
    }

    public SqlIdentifierExpression Table
    {
        get => table;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            table = value;
        }
    }

    protected bool Equals(SqlPropertyExpression other)
    {
        if (!CompareTwoSqlExpression(Name, other.Name))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Table, other.Table))
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
        return Equals((SqlPropertyExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Name.GetHashCode() * 397) ^ Table.GetHashCode();
        }
    }
}