using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlUpdateExpression : SqlExpression
{
    public SqlUpdateExpression()
    {
        this.Type = SqlExpressionType.Update;
    }

    public SqlExpression Table { get; set; }

    public List<SqlExpression> Items { get; set; }

    public SqlExpression Where { get; set; }

    public List<string> Comments { get; set; }

    protected bool Equals(SqlUpdateExpression other)
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


        if (!Table.Equals(other.Table))
        {
            return false;
        }
        if (Where == null ^ other.Where == null)
        {
            return false;
        }
        else if (Where != null && other.Where != null)
        {
            return Where.Equals(other.Where);
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
}

