using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

/// <summary>
/// Hints are instructions for the query optimizer on how to execute a query.
/// Hints 是用于指导查询优化器如何执行查询的指令
/// </summary>
public class SqlHintExpression : SqlExpression
{
    private SqlExpression body;

    public SqlHintExpression()
    {
        this.Type = SqlExpressionType.Hint;
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

    protected bool Equals(SqlHintExpression other)
    {
        return Body.Equals(other.Body);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlHintExpression)obj);
    }

    public override int GetHashCode()
    {
        return Body.GetHashCode();
    }
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlHintExpression(this);
    }
}