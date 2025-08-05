using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;
/// <summary>
/// Regular Expressions
/// 正则表达式
/// </summary>
public class SqlRegexExpression : SqlExpression, ICollateExpression
{
    private SqlCollateExpression collate;

    private SqlStringExpression regEx;

    private SqlExpression body;


    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlRegexExpression(this);
    }
    public SqlRegexExpression()
    {
        this.Type = SqlExpressionType.Regex;
    }

    /// <summary>
    /// Is it case sensitive
    /// 是否区分大小写
    /// </summary>
    public bool IsCaseSensitive { get; set; }

    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>
    public SqlCollateExpression Collate
    {
        get => collate;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            collate = value;
        }
    }

    public SqlStringExpression RegEx
    {
        get => regEx;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
            regEx = value;
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

    protected bool Equals(SqlRegexExpression other)
    {
        if (!CompareTwoSqlExpression(Collate, other.Collate))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(RegEx, other.RegEx))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(Body, other.Body))
        {
            return false;
        }
        return this.IsCaseSensitive == other.IsCaseSensitive;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlRegexExpression)obj);
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override SqlExpression Clone()
    {
        var result = new SqlRegexExpression()
        {
            DbType = this.DbType,
            Collate = (SqlCollateExpression)this.Collate.Clone(),
            RegEx = (SqlStringExpression)this.RegEx.Clone(),
            Body = this.Body.Clone(),
            IsCaseSensitive = IsCaseSensitive
        };
        return result;
    }
}