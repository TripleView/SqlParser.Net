using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlIdentifierExpression : SqlExpression, IQualifierExpression
{
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

    protected bool Equals(SqlIdentifierExpression other)
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
        return Equals((SqlIdentifierExpression)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

}