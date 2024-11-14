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

    public SqlExpression Where { get; set; }

    public SqlGroupByExpression GroupBy { get; set; }
    /// <summary>
    /// just for oracle,such as sql:SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e START WITH MANAGERID IS NULL CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID ;
    /// 仅oracle中有用，例如sql:SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e START WITH MANAGERID IS NULL CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID ;
    /// </summary>
    public SqlConnectByExpression ConnectBy { get; set; }


    public SqlOrderByExpression OrderBy { get; set; }

    public SqlLimitExpression Limit { get; set; }

    protected bool Equals(SqlSelectQueryExpression other)
    {
        var result = true;
        if (ResultSetReturnOption != other.ResultSetReturnOption)
        {
            return false;
        }
        if (Columns.Count != other.Columns.Count)
        {
            return false;
        }
        for (var i = 0; i < Columns.Count; i++)
        {
            var item = Columns[i];
            var item2 = other.Columns[i];
            if (!item.Equals(item2))
            {
                return false;
            }
        }

        if (WithSubQuerys == null ^ other.WithSubQuerys == null)
        {
            return false;
        }
        else if (WithSubQuerys != null && other.WithSubQuerys != null)
        {
            if (WithSubQuerys.Count != other.WithSubQuerys.Count)
            {
                return false;
            }
            for (var i = 0; i < WithSubQuerys.Count; i++)
            {
                var item = WithSubQuerys[i];
                var item2 = other.WithSubQuerys[i];
                if (!item.Equals(item2))
                {
                    return false;
                }
            }
        }

        if (!result)
        {
            return false;
        }

        if (Into == null ^ other.Into == null)
        {
            return false;
        }
        else if (Into != null && other.Into != null)
        {
            result &= Into.Equals(other.Into);
        }
        if (!result)
        {
            return false;
        }
        if (From == null ^ other.From == null)
        {
            return false;
        }
        else if (From != null && other.From != null)
        {
            result &= From.Equals(other.From);
        }
        if (!result)
        {
            return false;
        }

        if (Where == null ^ other.Where == null)
        {
            return false;
        }
        else if (Where != null && other.Where != null)
        {
            result &= Where.Equals(other.Where);
        }
        if (!result)
        {
            return false;
        }

        if (GroupBy == null ^ other.GroupBy == null)
        {
            return false;
        }
        else if (GroupBy != null && other.GroupBy != null)
        {
            result &= GroupBy.Equals(other.GroupBy);
        }
        if (!result)
        {
            return false;
        }

        if (OrderBy == null ^ other.OrderBy == null)
        {
            return false;
        }
        else if (OrderBy != null && other.OrderBy != null)
        {
            result &= OrderBy.Equals(other.OrderBy);
        }
        if (!result)
        {
            return false;
        }

        if (Limit == null ^ other.Limit == null)
        {
            return false;
        }
        else if (Limit != null && other.Limit != null)
        {
            result &= Limit.Equals(other.Limit);
        }
        return result;
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