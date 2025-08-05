using SqlParser.Net.Ast.Visitor;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;

public class SqlBetweenAndExpression : SqlExpression
{
    private SqlExpression body;
    private SqlExpression begin;
    private SqlExpression end;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlBetweenAndExpression(this);
    }
    public SqlBetweenAndExpression()
    {
        this.Type = SqlExpressionType.BetweenAnd;
    }

    public SqlBetweenAndExpressionTokenContext TokenContext { get; set; }

    /// <summary>
    /// not in
    /// </summary>
    public bool IsNot { get; set; }

    public SqlExpression Body
    {
        get => body;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            body = value;
        }
    }

    public SqlExpression Begin
    {
        get => begin;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            begin = value;
        }
    }

    public SqlExpression End
    {
        get => end;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            end = value;
        }
    }

    protected bool Equals(SqlBetweenAndExpression other)
    {
        if (IsNot != other.IsNot)
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Body,other.Body))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Begin, other.Begin))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(End, other.End))
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
        return Equals((SqlBetweenAndExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Body.GetHashCode();
            hashCode = (hashCode * 397) ^ Begin.GetHashCode();
            hashCode = (hashCode * 397) ^ End.GetHashCode();
            return hashCode;
        }
    }

    public override SqlExpression Clone()
    {
        var result = new SqlBetweenAndExpression()
        {
            DbType = this.DbType,
            Body = this.Body.Clone(),
            Begin = this.Begin.Clone(),
            End = this.End.Clone(),
            IsNot = IsNot
        };
        return result;
    }
}

public class SqlBetweenAndExpressionTokenContext
{
    public Token? Between { get; set; }
    public Token? And { get; set; }
    public Token? Not { get; set; }
}

