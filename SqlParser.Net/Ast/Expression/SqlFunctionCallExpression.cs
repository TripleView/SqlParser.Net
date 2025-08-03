using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlFunctionCallExpression : SqlExpression, ICollateExpression, ICloneableExpression<SqlFunctionCallExpression>
{
    private List<SqlExpression> arguments;
    private SqlIdentifierExpression name;
    private SqlIdentifierExpression caseAsTargetType;
    private SqlExpression fromSource;
    private SqlOverExpression over;
    private SqlWithinGroupExpression withinGroup;
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>

    private SqlCollateExpression collate;
  
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlFunctionCallExpression(this);
    }
    public SqlFunctionCallExpression()
    {
        this.Type = SqlExpressionType.FunctionCall;
    }

    public List<SqlExpression> Arguments
    {
        get => arguments;
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
            arguments = value;
        }
    }

    public SqlIdentifierExpression Name
    {
        get => name;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            name = value;
        }
    }

    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>
    public SqlCollateExpression Collate
    {
        get => collate;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            collate = value;
        }
    }

    /// <summary>
    /// Only for case as functions,such as sql:SELECT CAST('123' AS INT)
    /// 只为case as函数,比如sql：SELECT CAST('123' AS INT)
    /// </summary>
    public SqlIdentifierExpression CaseAsTargetType
    {
        get => caseAsTargetType;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            caseAsTargetType = value;
        }
    }

    /// <summary>
    /// Only for EXTRACT functions,such as sql:EXTRACT(YEAR FROM order_date)
    /// 只为EXTRACT函数,比如sql：EXTRACT(YEAR FROM order_date)
    /// </summary>
    public SqlExpression FromSource
    {
        get => fromSource;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            fromSource = value;
        }
    }

    /// <summary>
    /// 是否不重复
    /// </summary>
    public bool IsDistinct { get; set; }

    public SqlOverExpression Over
    {
        get => over;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            over = value;
        }
    }

    public SqlWithinGroupExpression WithinGroup
    {
        get => withinGroup;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            withinGroup = value;
        }
    }

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

        if (!CompareTwoSqlExpression(Collate, other.Collate))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(WithinGroup, other.WithinGroup))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(FromSource, other.FromSource))
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

    public SqlFunctionCallExpression Clone()
    {
        throw new System.NotImplementedException();
    }
}