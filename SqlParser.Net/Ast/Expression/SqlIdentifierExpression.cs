using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlIdentifierExpression : SqlExpression, IQualifierExpression, ICollateExpression
{
    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate�Ӿ���Ҫ����ָ���ַ����ȽϺ�����Ĺ���
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
    /// ���޶���
    /// </summary>
    public string LeftQualifiers { get; set; }
    /// <summary>
    /// right Qualifiers
    /// ���޶���
    /// </summary>
    public string RightQualifiers { get; set; }
    public string Value { get; set; }

    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate�Ӿ���Ҫ����ָ���ַ����ȽϺ�����Ĺ���
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

    public override SqlExpression InternalClone()
    {
        var result = new SqlIdentifierExpression()
        {
            DbType = this.DbType,
            Collate = this.Collate.Clone(),
            Value = Value,
            LeftQualifiers = LeftQualifiers,
            RightQualifiers = RightQualifiers
        };
        return result;
    }
}