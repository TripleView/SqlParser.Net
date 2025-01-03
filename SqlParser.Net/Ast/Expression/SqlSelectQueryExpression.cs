using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlSelectQueryExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlSelectQueryExpression(this);
    }
    public SqlSelectQueryExpression()
    {
        this.Type = SqlExpressionType.SelectQuery;
    }

    /// <summary>
    /// cte sub query
    /// cte公共表表达式子查询
    /// </summary>
    public List<SqlWithSubQueryExpression> WithSubQuerys { get; set; }

    public List<SqlSelectItemExpression> Columns { get; set; }
    /// <summary>
    /// SQL result set return options, such as all, distinct
    /// sql结果集返回选项，例如all，distinct
    /// </summary>
    public SqlResultSetReturnOption? ResultSetReturnOption { get; set; }
    /// <summary>
    /// sqlserver suport,such as sql: SELECT id,name into test14 from TEST t
    /// sqlserver 支持,比如sql:  SELECT id,name into test14 from TEST t
    /// </summary>
    public SqlExpression Into { get; set; }
    public SqlExpression From { get; set; }

    /// <summary>
    /// sqlserver 支持,比如sql:SELECT   TOP  100 * from test 
    /// </summary>
    public SqlTopExpression Top { get; set; }

    public SqlExpression Where { get; set; }

    public SqlGroupByExpression GroupBy { get; set; }
    /// <summary>
    /// just for oracle,such as sql:SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e START WITH MANAGERID IS NULL CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID ;
    /// 仅oracle中有用，例如sql:SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e START WITH MANAGERID IS NULL CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID ;
    /// </summary>
    public SqlConnectByExpression ConnectBy { get; set; }


    public SqlOrderByExpression OrderBy { get; set; }

    public SqlLimitExpression Limit { get; set; }

    public List<SqlHintExpression> Hints { get; set; }

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

}