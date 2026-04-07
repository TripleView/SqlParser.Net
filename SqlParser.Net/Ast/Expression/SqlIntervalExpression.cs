using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlIntervalExpression : SqlExpression
{
    private SqlExpression body;
    private SqlTimeUnitExpression unit;

    public override SqlExpression Accept(IAstVisitor visitor, VisitContext context = null)
    {
        return visitor.VisitSqlIntervalExpression(this, context);
    }
    public SqlIntervalExpression()
    {
        this.Type = SqlExpressionType.Interval;
    }

    /// <summary>
    /// Time interval value;时间间隔数值
    /// </summary>
    public SqlExpression Body
    {
        get => body;
        set
        {
            body = value;
        }
    }

    /// <summary>
    /// Unit,such as hour,For PostgreSQL, this is null;单位,比如hour,对于PostgreSQL，这里为null
    /// </summary>
    public SqlTimeUnitExpression Unit
    {
        get => unit;
        set
        {
            unit = value;
        }
    }

    protected bool Equals(SqlIntervalExpression other)
    {
        if (!CompareTwoSqlExpression(Unit, other.Unit))
        {
            return false;
        }
        return CompareTwoSqlExpression(Body, other.Body);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlIntervalExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlIntervalExpression()
        {
            DbType = this.DbType,
            Unit = this.Unit.Clone(),
            Body = this.Body.Clone(),
        };
        return result;
    }

}