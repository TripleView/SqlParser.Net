using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlIdentifierExpression : SqlExpression, IQualifierExpression, ICollateExpression
{
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>

    private SqlCollateExpression collate;
  
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlIdentifierExpression(this);
    }
    public SqlIdentifierExpression()
    {
        this.Type = SqlExpressionType.Identifier;
    }
    /// <summary>
    /// Left Qualifiers
    /// 左限定符
    /// </summary>
    public string LeftQualifiers { get; set; }
    /// <summary>
    /// right Qualifiers
    /// 右限定符
    /// </summary>
    public string RightQualifiers { get; set; }
    public string Value { get; set; }

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

    protected bool Equals(SqlIdentifierExpression other)
    {
        if (LeftQualifiers != other.LeftQualifiers || RightQualifiers != other.RightQualifiers)
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Collate, other.Collate))
        {
            return false;
        }

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlIdentifierExpression)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

}