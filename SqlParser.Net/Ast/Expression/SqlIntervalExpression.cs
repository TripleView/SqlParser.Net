using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlIntervalExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlIntervalExpression(this);
    }
    public SqlIntervalExpression()
    {
        this.Type = SqlExpressionType.Interval;
    }

    /// <summary>
    /// Time interval value;时间间隔数值
    /// </summary>
    public SqlExpression Body { get; set; }
    /// <summary>
    /// Unit,such as hour,For PostgreSQL, this is null;单位,比如hour,对于PostgreSQL，这里为null
    /// </summary>
    public SqlTimeUnitExpression Unit { get; set; }

    protected bool Equals(SqlIntervalExpression other)
    {
        if (!CompareTwoSqlExpression(Unit, other.Unit))
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
        return Equals((SqlIntervalExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }


}