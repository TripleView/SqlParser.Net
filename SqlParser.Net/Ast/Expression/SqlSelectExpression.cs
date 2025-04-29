using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlSelectExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlSelectExpression(this);
    }
    public SqlSelectExpression()
    {
        this.Type = SqlExpressionType.Select;
    }

    public SqlExpression Query { get; set; }

    public SqlIdentifierExpression Alias { get; set; }


    public SqlOrderByExpression OrderBy { get; set; }

    public SqlLimitExpression Limit { get; set; }

    public List<string> Comments { get; set; }

    protected bool Equals(SqlSelectExpression other)
    {
        if (!CompareTwoSqlExpression(Limit, other.Limit))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(OrderBy, other.OrderBy))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Alias, other.Alias))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Query, other.Query))
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
        return Equals((SqlSelectExpression)obj);
    }

    public override int GetHashCode()
    {
        return Query.GetHashCode();
    }
}