using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlGroupByExpression : SqlExpression, ICloneableExpression<SqlGroupByExpression>
{
    private List<SqlExpression> items;
    private SqlExpression having;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlGroupByExpression(this);
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

    public SqlGroupByExpression Clone()
    {
        throw new System.NotImplementedException();
    }
}

