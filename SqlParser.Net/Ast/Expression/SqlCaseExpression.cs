using System.Collections.Generic;
using System.Xml.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlCaseExpression : SqlExpression
{

    public SqlCaseExpression()
    {
        this.Type = SqlExpressionType.Case;
    }

    public List<SqlCaseItemExpression> Items { get; set; }

    public SqlExpression Else { get; set; }

    protected bool Equals(SqlCaseExpression other)
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

        if (Else is null ^ other.Else is null)
        {
            return false;
        }
        else if (Else != null && other.Else != null)
        {
            if (!Else.Equals(other.Else))
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
        return Equals((SqlCaseExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Items.GetHashCode() * 397) ^ Else.GetHashCode();
        }
    }
}