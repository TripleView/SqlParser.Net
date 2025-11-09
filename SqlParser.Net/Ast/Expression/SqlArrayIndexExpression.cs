using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlParser.Net.Ast.Expression;

public class SqlArrayIndexExpression : SqlExpression, IArrayRelatedExpression
{
    private SqlExpression body;
    private SqlNumberExpression index;

    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlArrayIndexExpression(this);
    }
    public SqlArrayIndexExpression()
    {
        this.Type = SqlExpressionType.ArrayIndex;
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

    public SqlNumberExpression Index
    {
        get => index;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            index = value;
        }
    }

    protected bool Equals(SqlArrayIndexExpression other)
    {
        if (!Body.Equals(other.Body))
        {
            return false;
        }
        if (!Index.Equals(other.Index))
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
        return Equals((SqlArrayIndexExpression)obj);
    }

    public override int GetHashCode()
    {
        return Body.GetHashCode()^Index.GetHashCode();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlArrayIndexExpression()
        {
            DbType = this.DbType,
            Body = this.Body.Clone(),
            Index = this.Index.Clone()
        };
        return result;
    }
}