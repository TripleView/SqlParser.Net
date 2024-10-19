using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlFunctionCallExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlFunctionCallExpression(this);
    }
    public SqlFunctionCallExpression()
    {
        this.Type = SqlExpressionType.FunctionCall;
    }

    public List<SqlExpression> Arguments { get; set; }

    public SqlIdentifierExpression Name { get; set; }

    /// <summary>
    /// 是否不重复
    /// </summary>
    public bool IsDistinct { get; set; }

    public SqlOverExpression Over { get; set; }

    public SqlWithinGroupExpression WithinGroup { get; set; }

    protected bool Equals(SqlFunctionCallExpression other)
    {
        if (IsDistinct != other.IsDistinct)
        {
            return false;
        }

        if (!Name.Equals(other.Name))
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

        var result = true;
        if (Over == null ^ other.Over == null)
        {
            return false;
        }
        else if (Over != null && other.Over != null)
        {
            result &= Over.Equals(other.Over);
        }

        if (!result)
        {
            return result;
        }

        if (WithinGroup == null ^ other.WithinGroup == null)
        {
            return false;
        }
        else if (WithinGroup != null && other.WithinGroup != null)
        {
            result &= WithinGroup.Equals(other.WithinGroup);
        }
        return result;
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