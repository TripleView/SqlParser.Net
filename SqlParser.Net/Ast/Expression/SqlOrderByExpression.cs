using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlOrderByExpression : SqlExpression
{
    private List<SqlOrderByItemExpression> items;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlOrderByExpression(this);
    }
    public SqlOrderByExpression()
    {
        this.Type = SqlExpressionType.OrderBy;
        this.Items = new List<SqlOrderByItemExpression>();
    }

    public List<SqlOrderByItemExpression> Items
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

    /// <summary>
    /// just for oracle ,such as:SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e  CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID
    /// 仅oracle使用，例如sql:SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e  CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID
    /// </summary>
    public bool IsSiblings { get; set; }


    protected bool Equals(SqlOrderByExpression other)
    {
        if (IsSiblings != other.IsSiblings)
        {
            return false;
        }

        if (!CompareTwoSqlExpressionList(Items, other.Items))
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
        return Equals((SqlOrderByExpression)obj);
    }

    public override int GetHashCode()
    {
        return Items.GetHashCode();
    }
}