using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlNumberExpression : SqlExpression, IQualifierExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlNumberExpression(this);
    }
    public SqlNumberExpression()
    {
        this.Type = SqlExpressionType.Number;
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

    public decimal Value { get; set; }

    protected bool Equals(SqlNumberExpression other)
    {
        if (LeftQualifiers != other.LeftQualifiers || RightQualifiers != other.RightQualifiers)
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
        return Equals((SqlNumberExpression)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override SqlExpression Clone()
    {
        var result = new SqlNumberExpression()
        {
            DbType = this.DbType,
            LeftQualifiers = LeftQualifiers,
            RightQualifiers = RightQualifiers,
            Value = Value
        };
        return result;
    }
}