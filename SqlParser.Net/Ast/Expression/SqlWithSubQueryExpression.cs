using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlWithSubQueryExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlWithSubQueryExpression(this);
    }
    public SqlWithSubQueryExpression()
    {
        this.Type = SqlExpressionType.WithSubQuery;
    }

    public SqlIdentifierExpression Alias { get; set; }

    public List<SqlIdentifierExpression> Columns { get; set; }

    public SqlSelectExpression FromSelect { get; set; }

    protected bool Equals(SqlWithSubQueryExpression other)
    {
        if (!CompareTwoSqlExpressionList(Columns, other.Columns))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(FromSelect, other.FromSelect))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Alias, other.Alias))
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
        return Equals((SqlWithSubQueryExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Alias.GetHashCode();
            hashCode = (hashCode * 397) ^ Columns.GetHashCode();
            hashCode = (hashCode * 397) ^ FromSelect.GetHashCode();
            return hashCode;
        }
    }
}