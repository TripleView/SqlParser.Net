using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlAtTimeZoneExpression : SqlExpression
{
    private SqlExpression body;
    private SqlStringExpression timeZone;

    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlAtTimeZoneExpression(this);
    }
    public SqlAtTimeZoneExpression()
    {
        this.Type = SqlExpressionType.AtTimeZone;
    }

    /// <summary>
    /// Time Zone，such as 'Asia/ShangHai';时区，比如'Asia/ShangHai'
    /// </summary>
    public SqlStringExpression TimeZone
    {
        get => timeZone;
        set
        {
            timeZone = value;
        }
    }

    public SqlExpression Body
    {
        get => body;
        set
        {
            body = value;
        }
    }

    protected bool Equals(SqlAtTimeZoneExpression other)
    {
        if (!CompareTwoSqlExpression(TimeZone, other.TimeZone))
        {
            return false;
        }

        return CompareTwoSqlExpression(Body,other.Body);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlAtTimeZoneExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlAtTimeZoneExpression()
        {
            DbType = this.DbType,
            Body = this.Body.Clone(),
            TimeZone = this.TimeZone.Clone()
        };
        return result;
    }
}