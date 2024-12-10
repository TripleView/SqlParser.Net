using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlInExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlInExpression(this);
    }

    public SqlInExpression()
    {
        this.Type = SqlExpressionType.In;
    }

    /// <summary>
    /// not in
    /// </summary>
    public bool IsNot { get; set; }

    public SqlExpression Body { get; set; }

    public List<SqlExpression> TargetList { get; set; }

    public SqlSelectExpression SubQuery { get; set; }
    protected bool Equals(SqlInExpression other)
    {
        if (IsNot != other.IsNot)
        {
            return false;
        }
        if (!Body.Equals(other.Body))
        {
            return false;
        }

        if (TargetList == null ^ other.TargetList == null)
        {
            return false;
        }
        else if (TargetList != null && other.TargetList != null)
        {
            if (TargetList.Count != other.TargetList.Count)
            {
                return false;
            }
            for (var i = 0; i < TargetList.Count; i++)
            {
                var item = TargetList[i];
                var item2 = other.TargetList[i];
                if (!item.Equals(item2))
                {
                    return false;
                }
            }
        }

        if (SubQuery == null ^ other.SubQuery == null)
        {
            return false;
        }
        else if (SubQuery != null && other.SubQuery != null)
        {
            if (!SubQuery.Equals(other.SubQuery))
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
        return Equals((SqlInExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Body.GetHashCode() * 397) ^ TargetList.GetHashCode();
        }
    }
}