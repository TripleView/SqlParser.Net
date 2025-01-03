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
    /// Only for case as functions,such as sql:SELECT CAST('123' AS INT)
    /// 只为case as函数,比如sql：SELECT CAST('123' AS INT)
    /// </summary>
    public SqlIdentifierExpression CaseAsTargetType { get; set; }

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

        if (!CompareTwoSqlExpressionList(Arguments, other.Arguments))
        {
            return false;
        }
        

        if (!CompareTwoSqlExpression(Name, other.Name))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(Over, other.Over))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(CaseAsTargetType, other.CaseAsTargetType))
        {
            return false;
        }


        if (!CompareTwoSqlExpression(WithinGroup, other.WithinGroup))
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