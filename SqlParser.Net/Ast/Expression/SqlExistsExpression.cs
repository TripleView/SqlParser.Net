using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlExistsExpression : SqlExpression
{
    private SqlSelectExpression body;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlExistsExpression(this);
    }

    public SqlExistsExpression()
    {
        this.Type = SqlExpressionType.Exists;
    }

    /// <summary>
    /// not exist
    /// </summary>
    public bool IsNot { get; set; }

    public SqlSelectExpression Body
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

    protected bool Equals(SqlExistsExpression other)
    {
        if (IsNot != other.IsNot)
        {
            return false;
        }
        return Body.Equals(other.Body);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlExistsExpression)obj);
    }

    public override int GetHashCode()
    {
        return Body.GetHashCode();
    }
}