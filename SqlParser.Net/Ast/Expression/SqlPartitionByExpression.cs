using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlPartitionByExpression : SqlExpression
{
    private List<SqlExpression> items;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlPartitionByExpression(this);
    }
    public SqlPartitionByExpression()
    {
        this.Type = SqlExpressionType.PartitionBy;
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

    protected bool Equals(SqlPartitionByExpression other)
    {
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
        return Equals((SqlPartitionByExpression)obj);
    }

    public override int GetHashCode()
    {
        return Items.GetHashCode();
    }
}