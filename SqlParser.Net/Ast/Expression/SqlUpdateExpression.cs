using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlUpdateExpression : SqlExpression
{
    private SqlExpression table;
    private List<SqlExpression> items;
    private SqlExpression where;
    private SqlExpression from;
    private List<string> comments;
    private List<SqlWithSubQueryExpression> withSubQuerys;
    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlUpdateExpression(this);
    }
    public SqlUpdateExpression()
    {
        this.Type = SqlExpressionType.Update;
        this.Items = new List<SqlExpression>();
        this.WithSubQuerys = new List<SqlWithSubQueryExpression>();
    }

    /// <summary>
    /// cte sub query
    /// cte公共表表达式子查询
    /// </summary>
    public List<SqlWithSubQueryExpression> WithSubQuerys
    {
        get => withSubQuerys;
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
            withSubQuerys = value;
        }
    }

    public SqlExpression Table
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

    public SqlExpression Where
    {
        get => where;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            where = value;
        }
    }

    public SqlExpression From
    {
        get => from;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            from = value;
        }
    }

    public List<string> Comments
    {
        get => comments;
        set => comments = value;
    }

    protected bool Equals(SqlUpdateExpression other)
    {
        if (!CompareTwoSqlExpressionList(Items, other.Items))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Where, other.Where))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Table, other.Table))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(From, other.From))
        {
            return false;
        }

        if (!CompareTwoSqlExpressionList(WithSubQuerys, other.WithSubQuerys))
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
        return Equals((SqlUpdateExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Table.GetHashCode();
            hashCode = (hashCode * 397) ^ Items.GetHashCode();
            hashCode = (hashCode * 397) ^ Where.GetHashCode();
            hashCode = (hashCode * 397) ^ Comments.GetHashCode();
            return hashCode;
        }
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlUpdateExpression()
        {
            DbType = this.DbType,
            Items = this.Items.Select(x => x.Clone()).ToList(),
            WithSubQuerys = this.WithSubQuerys.Select(x => x.Clone()).ToList(),
            Where = this.Where.Clone(),
            Table = this.Table.Clone(),
            From = this.From.Clone(),
        };
        return result;
    }
}

