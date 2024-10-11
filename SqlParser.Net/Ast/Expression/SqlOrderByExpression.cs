using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlOrderByExpression : SqlExpression
{
    public SqlOrderByExpression() 
    {
        this.Type = SqlExpressionType.OrderBy;
    }

    public List<SqlOrderByItemExpression> Items { get; set; }

    protected bool Equals(SqlOrderByExpression other)
    {
        if (Items.Count != other.Items.Count)
        {
            return false;
        }
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var item2 = other.Items[i];
            if (!item.Equals(item2))
            {
                return false;
            }
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