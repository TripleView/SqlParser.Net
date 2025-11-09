using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlStringExpression : SqlExpression, ICollateExpression
{
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>

    private SqlCollateExpression collate;
    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlStringExpression(this);
    }
    public SqlStringExpression()
    {
        this.Type = SqlExpressionType.String;
    }

    public string Value { get; set; }

    public bool IsUniCode { get; set; }
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
    protected bool Equals(SqlStringExpression other)
    {
        if (!CompareTwoSqlExpression(Collate, other.Collate))
        {
            return false;
        }
        return IsUniCode == other.IsUniCode && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlStringExpression)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlStringExpression()
        {
            DbType = this.DbType,
            Collate = this.Collate.Clone(),
            IsUniCode = IsUniCode,
            Value= Value
        };
        return result;
    }
}