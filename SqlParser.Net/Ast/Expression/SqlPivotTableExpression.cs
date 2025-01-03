using SqlParser.Net.Ast.Visitor;
using System.Collections.Generic;

namespace SqlParser.Net.Ast.Expression;

public class SqlPivotTableExpression : SqlExpression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlPivotTableExpression(this);
    }
    public SqlPivotTableExpression()
    {
        this.Type = SqlExpressionType.PivotTable;
    }

    public SqlIdentifierExpression Alias { get; set; }

    public SqlExpression SubQuery { get; set; }

    public SqlFunctionCallExpression FunctionCall { get; set; }

    public SqlExpression For { get; set; }

    public List<SqlExpression> In { get; set; }

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
}