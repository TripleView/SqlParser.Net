using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

/// <summary>
/// Hints are instructions for the query optimizer on how to execute a query.
/// Hints ������ָ����ѯ�Ż������ִ�в�ѯ��ָ��
/// </summary>
public class SqlHintExpression : SqlExpression, ICloneableExpression<SqlHintExpression>
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

    public SqlHintExpression Clone()
    {
        throw new System.NotImplementedException();
    }
}