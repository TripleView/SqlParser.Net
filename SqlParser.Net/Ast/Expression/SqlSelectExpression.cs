using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlSelectExpression : SqlExpression
{
    public SqlSelectExpression()
    {
        this.Type = SqlExpressionType.Select;
    }

    public SqlExpression Query { get; set; }

    public SqlIdentifierExpression Alias { get; set; }

    public List<string> Comments { get; set; }

    protected bool Equals(SqlSelectExpression other)
    {
        var result = true;
        if (Alias == null ^ other.Alias == null)
        {
            return false;
        }
        else if (Alias != null && other.Alias != null)
        {
            result &= Alias.Equals(other.Alias);
        }

        result &= Query.Equals(other.Query);
        return result;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlSelectExpression)obj);
    }

    public override int GetHashCode()
    {
        return Query.GetHashCode();
    }
}