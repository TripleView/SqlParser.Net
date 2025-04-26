using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlDeleteExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlDeleteExpression(this);
    }
    public SqlDeleteExpression()
    {
        this.Type = SqlExpressionType.Delete;
    }

    /// <summary>
    /// The subject of the deletion, such as t in DELETE t from T3 t join T4 t4 on t.id=t4.Pid where t.id='abc'
    /// 删除的主体，比如DELETE t from T3 t join T4 t4 on t.id=t4.Pid where t.id='abc'中的t
    /// </summary>
    public SqlExpression Body { get; set; }

    public SqlExpression Table { get; set; }

    public SqlExpression Where { get; set; }

    public List<string> Comments { get; set; }

    protected bool Equals(SqlDeleteExpression other)
    {
        if (!CompareTwoSqlExpression(Body, other.Body))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Table, other.Table))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Where, other.Where))
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
        return Equals((SqlDeleteExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Table.GetHashCode() * 397) ^ Where.GetHashCode();
        }
    }
}

