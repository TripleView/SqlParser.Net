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
        if (Alias == null ^ other.Alias == null)
        {
            return false;
        }
        else if (Alias != null && other.Alias != null)
        {
            if (!Alias.Equals(other.Alias))
            {
                return false;
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

        if (FunctionCall == null ^ other.FunctionCall == null)
        {
            return false;
        }
        else if (FunctionCall != null && other.FunctionCall != null)
        {
            if (!FunctionCall.Equals(other.FunctionCall))
            {
                return false;
            }
        }

        if (For == null ^ other.For == null)
        {
            return false;
        }
        else if (For != null && other.For != null)
        {
            if (!For.Equals(other.For))
            {
                return false;
            }
        }

        if (In == null ^ other.In == null)
        {
            return false;
        }
        else if (In != null && other.In != null)
        {

            if (In.Count != other.In.Count)
            {
                return false;
            }
            for (var i = 0; i < In.Count; i++)
            {
                var item = In[i];
                var item2 = other.In[i];
                if (!item.Equals(item2))
                {
                    return false;
                }
            }
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