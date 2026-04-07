using System.Collections.Generic;
using System.Linq;
using SqlParser.Net.Ast.Visitor;

namespace SqlParser.Net.Ast.Expression;

public class SqlTableExpression : SqlExpression, IAliasExpression
{
    private SqlIdentifierExpression alias;
    private SqlIdentifierExpression name;
    private SqlIdentifierExpression schema;
    private SqlIdentifierExpression database;
    private SqlIdentifierExpression dbLink;
    private List<SqlHintExpression> hints;

    public override SqlExpression Accept(IAstVisitor visitor, VisitContext context = null)
    {
        return visitor.VisitSqlTableExpression(this, context);
    }
    public SqlTableExpression()
    {
        this.Type = SqlExpressionType.Table;
        this.Hints = new List<SqlHintExpression>();
    }

    public SqlIdentifierExpression Alias
    {
        get => alias;
        set
        {
            alias = value;
        }
    }

    public SqlIdentifierExpression Name
    {
        get => name;
        set
        {
            name = value;
        }
    }

    /// <summary>
    /// Schema,such as:select * from test.test
    /// 数据库模式,如select * from test.test
    /// </summary>
    public SqlIdentifierExpression Schema
    {
        get => schema;
        set
        {
            schema = value;
        }
    }

    /// <summary>
    /// Database name, such as epf in [EPF].[dbo].[test]
    /// 数据库名称,如[EPF].[dbo].[test]里的epf
    /// </summary>
    public SqlIdentifierExpression Database
    {
        get => database;
        set
        {
            database = value;
        }
    }

    /// <summary>
    /// oracle support db link,such as:SELECT * FROM remote_table@remote_db_link;
    /// oracle数据库支持的dblink,例如:SELECT * FROM remote_table@remote_db_link;
    /// </summary>
    public SqlIdentifierExpression DbLink
    {
        get => dbLink;
        set
        {
            dbLink = value;
        }
    }

    /// <summary>
    /// Hints are instructions for the query optimizer on how to execute a query.such as sql:select * from RouteData with(nolock)
    /// Hints 是用于指导查询优化器如何执行查询的指令,例如sql:select * from RouteData with(nolock)
    /// </summary>
    public List<SqlHintExpression> Hints
    {
        get => hints;
        set
        {
            hints = value;
        }
    }

    protected bool Equals(SqlTableExpression other)
    {
        var result = true;

        if (!CompareTwoSqlExpressionList(Hints, other.Hints))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(DbLink, other.DbLink))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Database, other.Database))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Schema, other.Schema))
        {
            return false;
        }

        if (!CompareTwoSqlExpression(Alias, other.Alias))
        {
            return false;
        }
        if (!CompareTwoSqlExpression(Name, other.Name))
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
        return Equals((SqlTableExpression)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Alias?.GetHashCode() ?? 0) * 397) ^ Name.GetHashCode();
        }
    }

    public override SqlExpression InternalClone()
    {
        var result = new SqlTableExpression()
        {
            DbType = this.DbType,
            DbLink = this.DbLink.Clone(),
            Database = this.Database.Clone(),
            Schema = this.Schema.Clone(),
            Alias = this.Alias.Clone(),
            Name = this.Name.Clone(),
            Hints = this.Hints.Select(x => (SqlHintExpression)x.Clone()).ToList(),
        };
        return result;
    }
}