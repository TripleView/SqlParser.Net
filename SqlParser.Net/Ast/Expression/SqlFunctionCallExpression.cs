using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlFunctionCallExpression : SqlExpression
{
    public SqlFunctionCallExpression()
    {
        this.Type = SqlExpressionType.FunctionCall;
    }

    public List<SqlExpression> Arguments { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// 是否不重复
    /// </summary>
    public bool IsDistinct { get; set; }

    public SqlOverExpression Over { get; set; }

    protected bool Equals(SqlFunctionCallExpression other)
    {
        if (IsDistinct != other.IsDistinct)
        {
            return false;
        }

        if (Name != other.Name)
        {
            return false;
        }

        if (Arguments == null ^ other.Arguments == null)
        {
            return false;
        }
        else if (Arguments != null && other.Arguments != null)
        {
            if (Arguments.Count != other.Arguments.Count)
            {
                return false;
            }

            for (var i = 0; i < Arguments.Count; i++)
            {
                var argument = Arguments[i];
                var argument2 = other.Arguments[i];
                if (!argument.Equals(argument2))
                {
                    return false;
                }
            }
        }

        if (Over == null ^ other.Over == null)
        {
            return false;
        }
        else if (Over != null && other.Over != null)
        {
            return Over.Equals(other.Over);
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlFunctionCallExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Arguments.GetHashCode();
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ IsDistinct.GetHashCode();
            hashCode = (hashCode * 397) ^ Over.GetHashCode();
            return hashCode;
        }
    }
}