using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlPropertyExpression : SqlExpression
{
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
    public SqlIdentifierExpression Name { get; set; }
    public SqlIdentifierExpression Table { set; get; }

    protected bool Equals(SqlPropertyExpression other)
    {
        if (Name is null ^ other.Name is null)
        {
            return false;
        }
        else if (Name != null && other.Name != null)
        {
            if (!Name.Equals(other.Name))
            {
                return false;
            }
        }

        return Table.Equals(other.Table);
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