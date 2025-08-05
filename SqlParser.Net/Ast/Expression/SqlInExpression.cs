using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlInExpression : SqlExpression
{
    private SqlExpression body;
    private List<SqlExpression> targetList;
    private SqlSelectExpression subQuery;

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

    public SqlExpression Body
    {
        get => body;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            body = value;
        }
    }

    public List<SqlExpression> TargetList
    {
        get => targetList;
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
            targetList = value;
        }
    }

    public SqlSelectExpression SubQuery
    {
        get => subQuery;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            subQuery = value;
        }
    }

    protected bool Equals(SqlInExpression other)
    {
        if (IsNot != other.IsNot)
        {
            return false;
        }

        if (!CompareTwoSqlExpressionList(TargetList, other.TargetList))
        {
            return false;
        }
        
        if (!CompareTwoSqlExpression(Body, other.Body))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(SubQuery, other.SubQuery))
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
        return Equals((SqlInExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Body.GetHashCode() * 397) ^ TargetList.GetHashCode();
        }
    }

    public override SqlExpression Clone()
    {
        var result = new SqlInExpression()
        {
            DbType = this.DbType,
            TargetList = this.TargetList.Select(x =>x.Clone()).ToList(),
            Body = this.Body.Clone(),
            SubQuery = (SqlSelectExpression)this.SubQuery.Clone(),
            IsNot = IsNot
        };
        return result;
    }
}