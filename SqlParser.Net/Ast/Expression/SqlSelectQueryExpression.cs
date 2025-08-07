using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;

public class SqlSelectQueryExpression : SqlExpression
{
    private List<SqlWithSubQueryExpression> withSubQuerys;
    private List<SqlSelectItemExpression> columns;
    private SqlExpression into;
    private SqlExpression from;
    private SqlTopExpression top;
    private SqlExpression where;
    private SqlGroupByExpression groupBy;
    private SqlConnectByExpression connectBy;
    private SqlOrderByExpression orderBy;
    private SqlLimitExpression limit;
    private List<SqlHintExpression> hints;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlSelectQueryExpression(this);
    }
    public SqlSelectQueryExpression()
    {
        this.Type = SqlExpressionType.SelectQuery;
        this.WithSubQuerys = new List<SqlWithSubQueryExpression>();
        this.Columns = new List<SqlSelectItemExpression>();
        this.Hints = new List<SqlHintExpression>();
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

    public List<SqlSelectItemExpression> Columns
    {
        get => columns;
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
            columns = value;
        }
    }

    /// <summary>
    /// SQL result set return options, such as all, distinct
    /// sql结果集返回选项，例如all，distinct
    /// </summary>
    public SqlResultSetReturnOption? ResultSetReturnOption { get; set; }

    /// <summary>
    /// sqlserver suport,such as sql: SELECT id,name into test14 from TEST t
    /// sqlserver 支持,比如sql:  SELECT id,name into test14 from TEST t
    /// </summary>
    public SqlExpression Into
    {
        get => into;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            into = value;
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

    /// <summary>
    /// sqlserver 支持,比如sql:SELECT   TOP  100 * from test 
    /// </summary>
    public SqlTopExpression Top
    {
        get => top;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }

            top = value;
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

    public SqlGroupByExpression GroupBy
    {
        get => groupBy;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            groupBy = value;
        }
    }

    /// <summary>
    /// just for oracle,such as sql:SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e START WITH MANAGERID IS NULL CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID ;
    /// 仅oracle中有用，例如sql:SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e START WITH MANAGERID IS NULL CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID ;
    /// </summary>
    public SqlConnectByExpression ConnectBy
    {
        get => connectBy;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            connectBy = value;
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

    public List<SqlHintExpression> Hints
    {
        get => hints;
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
            hints = value;
        }
    }

    protected bool Equals(SqlSelectQueryExpression other)
    {
        var result = true;
        if (ResultSetReturnOption != other.ResultSetReturnOption)
        {
            return false;
        }

        if (!CompareTwoSqlExpressionList(Columns, other.Columns))
        {
            return false;
        }
        if (!CompareTwoSqlExpressionList(WithSubQuerys, other.WithSubQuerys))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Into, other.Into))
        {
            return false;
        }

        //--
        if (!CompareTwoSqlExpression(Top, other.Top))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(From, other.From))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Where, other.Where))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(GroupBy, other.GroupBy))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(OrderBy, other.OrderBy))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Limit, other.Limit))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(ConnectBy, other.ConnectBy))
        {
            return false;
        }

        if (!CompareTwoSqlExpressionList(Hints, other.Hints))
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
        return Equals((SqlSelectQueryExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = WithSubQuerys.GetHashCode();
            hashCode = (hashCode * 397) ^ Columns.GetHashCode();
            hashCode = (hashCode * 397) ^ ResultSetReturnOption.GetHashCode();
            hashCode = (hashCode * 397) ^ Into.GetHashCode();
            hashCode = (hashCode * 397) ^ From.GetHashCode();
            hashCode = (hashCode * 397) ^ Where.GetHashCode();
            hashCode = (hashCode * 397) ^ GroupBy.GetHashCode();
            hashCode = (hashCode * 397) ^ OrderBy.GetHashCode();
            hashCode = (hashCode * 397) ^ Limit.GetHashCode();
            return hashCode;
        }

    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlSelectQueryExpression()
        {
            DbType = this.DbType,
            Into = this.Into.Clone(),
            WithSubQuerys = this.WithSubQuerys.Select(x => x.Clone()).ToList(),
            Columns = this.Columns.Select(x => x.Clone()).ToList(),
            ResultSetReturnOption = this.ResultSetReturnOption,
            Top = this.Top.Clone(),
            From = this.From.Clone(),
            Where = this.Where.Clone(),
            GroupBy = this.GroupBy.Clone(),
            OrderBy = this.OrderBy.Clone(),
            Limit = this.Limit.Clone(),
            ConnectBy = this.ConnectBy.Clone(),
            Hints = this.Hints.Select(x => x.Clone()).ToList(),
        };
        return result;
    }

}

public class SqlSelectQueryExpressionTokenContext
{
    public Token? Select { get; set; }
    public Token? Top { get; set; }

    public Token? From { get; set; }
}