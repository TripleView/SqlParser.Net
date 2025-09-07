using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlReturningExpression : SqlExpression
{
    private List<SqlExpression> items;
    private List<SqlExpression> intoVariables;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlReturningExpression(this);
    }
    public SqlReturningExpression()
    {
        this.Type = SqlExpressionType.Returning;
        this.items = new List<SqlExpression>();
        this.intoVariables = new List<SqlExpression>();
    }

    public List<SqlExpression> Items
    {
        get => items;
        set
        {
            if (value != null)
            {
                foreach (var expression in value)
                {
                    expression.Parent = this;
                }
            }
            items = value;
        }
    }

    public List<SqlExpression> IntoVariables
    {
        get => intoVariables;
        set
        {
            if (value != null)
            {
                foreach (var expression in value)
                {
                    expression.Parent = this;
                }
            }
            intoVariables = value;
        }
    }

    protected bool Equals(SqlReturningExpression other)
    {
        if (!CompareTwoSqlExpressionList(Items, other.Items))
        {
            return false;
        }
        if (!CompareTwoSqlExpressionList(IntoVariables, other.IntoVariables))
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
        return Equals((SqlReturningExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Items.GetHashCode() ^ IntoVariables.GetHashCode();
        }
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlReturningExpression()
        {
            DbType = this.DbType,
            Items = this.Items.Select(x => x.Clone()).ToList(),
            IntoVariables = this.IntoVariables.Select(x => x.Clone()).ToList(),
        };
        return result;
    }
}