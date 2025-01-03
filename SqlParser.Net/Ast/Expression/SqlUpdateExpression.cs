using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlUpdateExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlUpdateExpression(this);
    }
    public SqlUpdateExpression()
    {
        this.Type = SqlExpressionType.Update;
    }

    public SqlExpression Table { get; set; }

    public List<SqlExpression> Items { get; set; }

    public SqlExpression Where { get; set; }

    public List<string> Comments { get; set; }

    protected bool Equals(SqlUpdateExpression other)
    {
        if (!CompareTwoSqlExpressionList(Items, other.Items))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(Where, other.Where))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Table, other.Table))
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
        return Equals((SqlUpdateExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Table.GetHashCode();
            hashCode = (hashCode * 397) ^ Items.GetHashCode();
            hashCode = (hashCode * 397) ^ Where.GetHashCode();
            hashCode = (hashCode * 397) ^ Comments.GetHashCode();
            return hashCode;
        }
    }
}

