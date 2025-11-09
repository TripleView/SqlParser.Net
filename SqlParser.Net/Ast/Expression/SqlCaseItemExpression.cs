using SqlParser.Net.Ast.Visitor;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;

public class SqlCaseItemExpression : SqlExpression
{
    private SqlExpression condition;
    private SqlExpression value;

    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlCaseItemExpression(this);
    }
    public SqlCaseItemExpression()
    {
        this.Type = SqlExpressionType.CaseItem;
    }

    public SqlCaseItemExpressionTokenContext TokenContext { get; set; }

    public SqlExpression Condition
    {
        get => condition;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            condition = value;
        }
    }

    public SqlExpression Value
    {
        get => value;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            this.value = value;
        }
    }

    protected bool Equals(SqlCaseItemExpression other)
    {

        if (!CompareTwoSqlExpression(Condition, other.Condition))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Value, other.Value))
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
        return Equals((SqlCaseItemExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Condition.GetHashCode() * 397) ^ Value.GetHashCode();
        }
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlCaseItemExpression()
        {
            DbType = this.DbType,
            Condition = this.Condition.Clone(),
            Value = this.Value.Clone(),
        };
        return result;
    }
}

public class SqlCaseItemExpressionTokenContext
{
    public Token? When { get; set; }
    public Token? Then { get; set; }
}