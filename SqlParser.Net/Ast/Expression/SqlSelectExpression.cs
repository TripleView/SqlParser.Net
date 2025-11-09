using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlSelectExpression : SqlExpression
{
    private SqlExpression query;
    private SqlIdentifierExpression alias;
    private SqlOrderByExpression orderBy;
    private SqlLimitExpression limit;

    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlSelectExpression(this);
    }
    public SqlSelectExpression()
    {
        this.Type = SqlExpressionType.Select;
    }

    public SqlExpression Query
    {
        get => query;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            query = value;
        }
    }

    public SqlIdentifierExpression Alias
    {
        get => alias;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            alias = value;
        }
    }


    public SqlOrderByExpression OrderBy
    {
        get => orderBy;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            orderBy = value;
        }
    }

    public SqlLimitExpression Limit
    {
        get => limit;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            limit = value;
        }
    }

    public List<string> Comments { get; set; }

    protected bool Equals(SqlSelectExpression other)
    {
        if (!CompareTwoSqlExpression(Limit, other.Limit))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(OrderBy, other.OrderBy))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Alias, other.Alias))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Query, other.Query))
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
        return Equals((SqlSelectExpression)obj);
    }

    public override int GetHashCode()
    {
        return Query.GetHashCode();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlSelectExpression()
        {
            DbType = this.DbType,
            Limit = this.Limit.Clone(),
            OrderBy = this.OrderBy.Clone(),
            Alias= this.Alias.Clone(),
            Query = this.Query.Clone()
        };
        return result;
    }
}