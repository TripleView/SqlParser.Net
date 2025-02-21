using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Xml.Linq;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;

public class SqlCaseExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlCaseExpression(this);
    }
    public SqlCaseExpression()
    {
        this.Type = SqlExpressionType.Case;
    }

    public SqlCaseExpressionTokenContext TokenContext { get; set; }

    public List<SqlCaseItemExpression> Items { get; set; }

    public SqlExpression Else { get; set; }

    public SqlExpression Value { get; set; }

    protected bool Equals(SqlCaseExpression other)
    {
        if (!CompareTwoSqlExpressionList(Items, other.Items))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(Else, other.Else))
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
        return Equals((SqlCaseExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Items.GetHashCode() * 397) ^ Else.GetHashCode();
        }
    }
}

public class SqlCaseExpressionTokenContext
{
    public Token? Case { get; set; }
    public Token? Else { get; set; }
    public Token? End { get; set; }
}