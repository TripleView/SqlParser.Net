using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlInExpression : SqlExpression
{

    public SqlInExpression()
    {
        this.Type = SqlExpressionType.In;
    }

    public SqlExpression Field { get; set; }

    public List<SqlExpression> TargetList { get; set; }

    protected bool Equals(SqlInExpression other)
    {
        if (!Field.Equals(other.Field))
        {
            return false;
        }
        if (TargetList.Count != other.TargetList.Count)
        {
            return false;
        }
        for (var i = 0; i < TargetList.Count; i++)
        {
            var item = TargetList[i];
            var item2 = other.TargetList[i];
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
        return Equals((SqlInExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Field.GetHashCode() * 397) ^ TargetList.GetHashCode();
        }
    }
}