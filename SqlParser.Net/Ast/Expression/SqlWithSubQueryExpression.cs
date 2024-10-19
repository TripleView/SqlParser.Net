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
        if (Alias is null ^ other.Alias is null)
        {
            return false;
        }
        else if (Alias != null && other.Alias != null)
        {
            if (!Alias.Equals(other.Alias))
            {
                return false;
            }
        }

        if (Columns is null ^ other.Columns is null)
        {
            return false;
        }
        else if (Columns != null && other.Columns != null)
        {
            if (Columns.Count != other.Columns.Count)
            {
                return false;
            }
            for (var i = 0; i < Columns.Count; i++)
            {
                var item = Columns[i];
                var item2 = other.Columns[i];
                if (!item.Equals(item2))
                {
                    return false;
                }
            }
        }

        if (FromSelect == null ^ other.FromSelect == null)
        {
            return false;
        }
        else if (FromSelect != null && other.FromSelect != null)
        {
            return FromSelect.Equals(other.FromSelect);
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