using SqlParser.Net.Ast.Visitor;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast.Expression;
/// <summary>
///CAST(expression AS dataType); CAST(表达式 AS 数据类型)
/// </summary>
public class SqlCastAsExpression : SqlExpression
{
    /// <summary>
    /// Only for case as functions,such as sql:SELECT CAST('123' AS INT)
    /// 只为case as函数,比如sql：SELECT CAST('123' AS INT)
    /// </summary>
    public SqlExpression TargetType { set; get; }
    /// <summary>
    /// Function Type;函数类型
    /// </summary>
    public CastAsFunctionType FunctionType { get; set; }
    public SqlCastAsExpression()
    {
        this.Type = SqlExpressionType.CastAs;
    }

    public SqlExpression Body { set; get; }

    protected bool Equals(SqlCastAsExpression other)
    {
        if (this.FunctionType != other.FunctionType)
        {
            return false;
        }
        if (!this.TargetType.Equals(other.TargetType))
        {
            return false;
        }
        return Body.Equals(other.Body);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlCastAsExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Body.GetHashCode() * 397) ^ TargetType.GetHashCode();
        }
    }
    public override SqlExpression Accept(IAstVisitor visitor, VisitContext context = null)
    {
		return visitor.VisitSqlCastAsExpressionExpression(this, context);
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlCastAsExpression()
        {
            DbType = this.DbType,
            Body = this.Body.Clone(),
            FunctionType = this.FunctionType,
            TargetType = this.TargetType.Clone()
        };
        return result;
    }
}

public enum CastAsFunctionType
{
    /// <summary>
    /// Regular Function Call;普通的函数调用
    /// </summary>
    Function = 1,
    /// <summary>
    /// Usage of :: in PostgreSQL;pgsql的::用法
    /// </summary>
    ColonColon = 2,
    //Usage of type string in PostgreSQL;pgsql的type string用法
    TypeString = 3
}
