using System.Collections.Generic;
using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlTableExpression : SqlExpression
{

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlTableExpression(this);
    }
    public SqlTableExpression()
    {
        this.Type = SqlExpressionType.Table;
    }

    public SqlIdentifierExpression Alias { get; set; }

    public SqlIdentifierExpression Name { get; set; }
    /// <summary>
    /// Schema,such as:select * from test.test
    /// 数据库模式,如select * from test.test
    /// </summary>
    public SqlIdentifierExpression Schema { get; set; }
    /// <summary>
    /// oracle support db link,such as:SELECT * FROM remote_table@remote_db_link;
    /// oracle数据库支持的dblink,例如:SELECT * FROM remote_table@remote_db_link;
    /// </summary>
    public SqlIdentifierExpression DbLink { get; set; }
    /// <summary>
    /// Hints are instructions for the query optimizer on how to execute a query.such as sql:select * from RouteData with(nolock)
    /// Hints 是用于指导查询优化器如何执行查询的指令,例如sql:select * from RouteData with(nolock)
    /// </summary>
    public List<SqlHintExpression> Hints { get; set; }

    protected bool Equals(SqlTableExpression other)
    {
        var result = true;
        if (DbLink == null ^ other.DbLink == null)
        {
            return false;
        }
        else if (DbLink != null && other.DbLink != null)
        {
            if (!DbLink.Equals(other.DbLink))
            {
                return false;
            }
        }

        if (Hints == null ^ other.Hints == null)
        {
            return false;
        }

        if (Hints.Count != other.Hints.Count)
        {
            return false;
        }
        for (var i = 0; i < Hints.Count; i++)
        {
            var item = Hints[i];
            var item2 = other.Hints[i];
            if (!item.Equals(item2))
            {
                return false;
            }
        }

        if (Schema == null ^ other.Schema == null)
        {
            return false;
        }
        else if (Schema != null && other.Schema != null)
        {
            if (!Schema.Equals(other.Schema))
            {
                return false;
            }
        }

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

        result &= Name.Equals(other.Name);
        return result;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SqlTableExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Alias.GetHashCode() * 397) ^ Name.GetHashCode();
        }
    }
}