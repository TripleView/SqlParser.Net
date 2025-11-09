using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlGroupByExpression : SqlExpression
{
    private List<SqlExpression> items;
    private SqlExpression having;

    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlGroupByExpression(this);
    }
    public SqlGroupByExpression()
    {
        this.Type = SqlExpressionType.GroupBy;
        this.Items = new List<SqlExpression>();
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
                    if (expression != null)
                    {
                        expression.Parent = this;
                    }
                }
            }
            items = value;
        }
    }

    public SqlExpression Having
    {
        get => having;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            having = value;
        }
    }

    protected bool Equals(SqlGroupByExpression other)
    {
        if (!CompareTwoSqlExpressionList(Items, other.Items))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(Having, other.Having))
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
        return Equals((SqlGroupByExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Items.GetHashCode() * 397) ^ Having.GetHashCode();
        }
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlGroupByExpression()
        {
            DbType = this.DbType,
            Items = this.Items.Select(x => x.Clone()).ToList(),
            Having = this.Having.Clone(),
           
        };
        return result;
    }
}

