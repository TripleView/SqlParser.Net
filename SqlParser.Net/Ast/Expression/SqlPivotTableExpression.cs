using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Net.Ast.Expression;

public class SqlPivotTableExpression : SqlExpression, IAliasExpression
{
    private SqlIdentifierExpression alias;
    private SqlExpression subQuery;
    private SqlFunctionCallExpression functionCall;
    private SqlExpression forValue;
    private List<SqlExpression> inValue;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlPivotTableExpression(this);
    }
    public SqlPivotTableExpression()
    {
        this.Type = SqlExpressionType.PivotTable;
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

    public SqlExpression SubQuery
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

    public SqlFunctionCallExpression FunctionCall
    {
        get => functionCall;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            functionCall = value;
        }
    }

    public SqlExpression For
    {
        get => forValue;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            forValue = value;
        }
    }

    public List<SqlExpression> In
    {
        get => inValue;
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
            inValue = value;
        }
    }

    protected bool Equals(SqlPivotTableExpression other)
    {
        if (!CompareTwoSqlExpression(Alias, other.Alias))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(SubQuery, other.SubQuery))
        {
            return false;
        }


        if (!CompareTwoSqlExpression(FunctionCall, other.FunctionCall))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(For, other.For))
        {
            return false;
        }
        if (!CompareTwoSqlExpressionList(In, other.In))
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
        return Equals((SqlPivotTableExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override SqlExpression Clone()
    {
        var result = new SqlPivotTableExpression()
        {
            DbType = this.DbType,
            In = this.In.Select(x => x.Clone()).ToList(),
            Alias = (SqlIdentifierExpression)this.Alias.Clone(),
            SubQuery = this.SubQuery.Clone(),
            FunctionCall = (SqlFunctionCallExpression)this.FunctionCall.Clone(),
            For = this.For.Clone(),
        };
        return result;
    }
}