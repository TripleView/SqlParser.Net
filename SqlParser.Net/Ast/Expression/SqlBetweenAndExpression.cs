using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlBetweenAndExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlBetweenAndExpression(this);
    }
    public SqlBetweenAndExpression()
    {
        this.Type = SqlExpressionType.BetweenAnd;
    }

    public SqlExpression Body { get; set; }
    public SqlExpression Begin { get; set; }
    public SqlExpression End { get; set; }

    protected bool Equals(SqlBetweenAndExpression other)
    {
        if (Body is null ^ other.Body is null)
        {
            return false;
        }
        else if (Body != null && other.Body != null)
        {
            if (!Body.Equals(other.Body))
            {
                return false;
            }
        }

        if (Begin is null ^ other.Begin is null)
        {
            return false;
        }
        else if (Begin != null && other.Begin != null)
        {
            if (!Begin.Equals(other.Begin))
            {
                return false;
            }
        }

        if (End is null ^ other.End is null)
        {
            return false;
        }
        else if (End != null && other.End != null)
        {
            if (!End.Equals(other.End))
            {
                return false;
            }
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
}

