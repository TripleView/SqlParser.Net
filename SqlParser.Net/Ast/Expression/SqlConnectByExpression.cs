using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;

public class SqlConnectByExpression : SqlExpression
{
    private SqlExpression startWith;
    private SqlExpression body;
    private SqlOrderByExpression orderBy;

    public override SqlExpression Accept(IAstVisitor visitor)
    {
        return visitor.VisitSqlConnectByExpression(this);
    }
    public SqlConnectByExpression()
    {
        this.Type = SqlExpressionType.ConnectBy;
    }

    public bool IsNocycle { get; set; }

    public bool IsPrior { get; set; }

    public SqlExpression StartWith
    {
        get => startWith;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            startWith = value;
        }
    }

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

    public SqlOrderByExpression OrderBy
    {
        get => orderBy;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            orderBy = value;
        }
    }

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

    public override SqlExpression InternalClone()
    {
        var result = new SqlConnectByExpression()
        {
            DbType = this.DbType,
            StartWith = this.StartWith.Clone(),
            Body = this.Body.Clone(),
            OrderBy = this.OrderBy.Clone(),
            IsNocycle = IsNocycle,
            IsPrior = IsPrior
        };
        return result;
    }
}

public class SqlConnectByExpressionTokenContext
{
    public Token? Token { get; set; }
}

