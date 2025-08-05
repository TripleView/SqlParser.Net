using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlWithSubQueryExpression : SqlExpression, IAliasExpression
{
    private SqlIdentifierExpression alias;
    private List<SqlIdentifierExpression> columns;
    private SqlSelectExpression fromSelect;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlWithSubQueryExpression(this);
    }
    public SqlWithSubQueryExpression()
    {
        this.Type = SqlExpressionType.WithSubQuery;
    }

    public SqlIdentifierExpression Alias
    {
        get => alias;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            alias = value;
        }
    }

    public List<SqlIdentifierExpression> Columns
    {
        get => columns;
        set
        {
            if (value != null)
            {
                foreach (var expression in value)
                {
                    if (expression != null)
                    {
                        expression.Parent = this;
                    }
                }
            }
            columns = value;
        }
    }

    public SqlSelectExpression FromSelect
    {
        get => fromSelect;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            fromSelect = value;
        }
    }

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

    public override SqlExpression Clone()
    {
        var result = new SqlWithSubQueryExpression()
        {
            DbType = this.DbType,
            Columns = this.Columns.Select(x => (SqlIdentifierExpression)x.Clone()).ToList(),
            FromSelect = (SqlSelectExpression)this.FromSelect.Clone(),
            Alias = (SqlIdentifierExpression)this.Alias.Clone(),
        };
        return result;
    }
}