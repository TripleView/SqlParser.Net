using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlDeleteExpression : SqlExpression
{
    public SqlDeleteExpression()
    {
        this.Type = SqlExpressionType.Delete;
    }
    public SqlExpression Table { get; set; }

    public SqlExpression Where { get; set; }

    public List<string> Comments { get; set; }

    protected bool Equals(SqlDeleteExpression other)
    {
        if (!Table.Equals(other.Table))
        {
            return false;
        }

        if (Where is null ^ other.Where is null)
        {
            return false;
        }
        else if (Where != null && other.Where != null)
        {
            if (!Where.Equals(other.Where))
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
        return Equals((SqlDeleteExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Table.GetHashCode() * 397) ^ Where.GetHashCode();
        }
    }
}

