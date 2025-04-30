using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlReferenceTableExpression : SqlExpression
{
    private SqlFunctionCallExpression functionCall;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlReferenceTableExpression(this);
    }
    public SqlReferenceTableExpression()
    {
        this.Type = SqlExpressionType.ReferenceTable;
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

    protected bool Equals(SqlReferenceTableExpression other)
    {
        if (!CompareTwoSqlExpression(FunctionCall, other.FunctionCall))
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
        return Equals((SqlReferenceTableExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}