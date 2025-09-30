using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Net.Ast.Expression;
/// <summary>
/// pgsql array,such as select ARRAY[1,2,3]
/// pgsql中的数组，比如select ARRAY[1,2,3]
/// </summary>
public class SqlArrayExpression : SqlExpression, IArrayRelatedExpression
{
    private List<SqlExpression> items;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlArrayExpression(this);
    }
    public SqlArrayExpression()
    {
        this.Type = SqlExpressionType.Array;
        this.Items = new List<SqlExpression>();
    }

    public List<SqlExpression> Items
    {
        get => items;
        set
        {
            if (value != null)
            {
                foreach (var expression in value)
                {
                    expression.Parent = this;
                }
            }
            items = value;
        }
    }


    protected bool Equals(SqlArrayExpression other)
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
        return Equals((SqlArrayExpression)obj);
    }

    public override int GetHashCode()
    {
        return Items.GetHashCode();
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlArrayExpression()
        {
            DbType = this.DbType,
            Items = this.Items.Select(x => x.Clone()).ToList(),
        };
        return result;
    }
}