using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlDistinctOnExpression : SqlExpression
{
    private List<SqlExpression> items;

    public override SqlExpression Accept(IAstVisitor visitor, VisitContext context = null)
    {
        return visitor.VisitSqlDistinctOnExpression(this, context);
    }
    public SqlDistinctOnExpression()
    {
        this.Type = SqlExpressionType.DistinctOn;
        this.Items = new List<SqlExpression>();
    }

    public List<SqlExpression> Items
    {
        get => items;
        set
        {
            items = value;
        }
    }

    protected bool Equals(SqlDistinctOnExpression other)
    {
        if (!CompareTwoSqlExpressionList(Items, other.Items))
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
        return Equals((SqlDistinctOnExpression)obj);
    }

    public override int GetHashCode()
    {
        return Items.GetHashCode();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlDistinctOnExpression()
        {
            DbType = this.DbType,
            Items = this.Items?.Select(x => x.Clone()).ToList() ?? new List<SqlExpression>(),
        };
        return result;
    }
}