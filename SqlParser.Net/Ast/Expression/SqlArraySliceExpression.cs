using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SqlParser.Net.Ast.Expression;

public class SqlArraySliceExpression : SqlExpression, IArrayRelatedExpression
{
    private SqlExpression body;
    private SqlNumberExpression startIndex;
    private SqlNumberExpression endIndex;
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlArraySliceExpression(this);
    }
    public SqlArraySliceExpression()
    {
        this.Type = SqlExpressionType.ArraySlice;
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

    public SqlNumberExpression StartIndex
    {
        get => startIndex;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            startIndex = value;
        }
    }

    public SqlNumberExpression EndIndex
    {
        get => endIndex;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            endIndex = value;
        }
    }

    protected bool Equals(SqlArraySliceExpression other)
    {
        if (!StartIndex.Equals(other.StartIndex))
        {
            return false;
        }
        if (!EndIndex.Equals(other.EndIndex))
        {
            return false;
        }
        if (!Body.Equals(other.Body))
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
        return Equals((SqlArraySliceExpression)obj);
    }

    public override int GetHashCode()
    {
        return Body.GetHashCode()^StartIndex.GetHashCode() ^ EndIndex.GetHashCode();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlArraySliceExpression()
        {
            DbType = this.DbType,
            Body = this.Body.Clone(),
            StartIndex = this.StartIndex.Clone(),
            EndIndex = this.EndIndex.Clone()
        };
        return result;
    }
}