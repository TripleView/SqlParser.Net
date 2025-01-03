using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlConnectByExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlConnectByExpression(this);
    }
    public SqlConnectByExpression()
    {
        this.Type = SqlExpressionType.ConnectBy;
    }

    public bool IsNocycle { get; set; }

    public bool IsPrior { get; set; }

    public SqlExpression StartWith { get; set; }

    public SqlExpression Body { get; set; }

    public SqlOrderByExpression OrderBy { get; set; }
    protected bool Equals(SqlConnectByExpression other)
    {
        if (IsNocycle != other.IsNocycle)
        {
            return false;
        }

        if (IsPrior != other.IsPrior)
        {
            return false;
        }

        if (!CompareTwoSqlExpression(StartWith, other.StartWith))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Body, other.Body))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(OrderBy, other.OrderBy))
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
        return Equals((SqlConnectByExpression)obj);
    }

}

