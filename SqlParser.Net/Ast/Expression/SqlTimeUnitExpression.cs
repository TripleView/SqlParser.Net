using SqlParser.Net.Ast.Visitor;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;

public class SqlTimeUnitExpression : SqlExpression
{

    public SqlTimeUnitExpression()
    {
        this.Type = SqlExpressionType.TimeUnit;
    }

    public string Unit { get; set; }

    protected bool Equals(SqlTimeUnitExpression other)
    {
        return Unit==other.Unit;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlTimeUnitExpression)obj);
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlTimeUnitExpression(this);
    }

    public override SqlExpression Clone()
    {
        var result = new SqlTimeUnitExpression()
        {
            DbType = this.DbType,
            Unit = Unit
        };
        return result;
    }
}
