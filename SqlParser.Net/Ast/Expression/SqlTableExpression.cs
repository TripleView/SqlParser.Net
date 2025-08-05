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

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitSqlTableExpression(this);
    }
    public SqlTableExpression()
    {
        this.Type = SqlExpressionType.Table;
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

    public SqlIdentifierExpression Name
    {
        get => name;
        set
        {
            if (value != null)
            {
                value.Parent = this;
            }
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
            if (value != null)
            {
                value.Parent = this;
            }
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
            if (value != null)
            {
                value.Parent = this;
            }
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
            if (value != null)
            {
                value.Parent = this;
            }
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
            return (Alias.GetHashCode() * 397) ^ Name.GetHashCode();
        }
    }

    public override SqlExpression Clone()
    {
        var result = new SqlTableExpression()
        {
            DbType = this.DbType,
            DbLink = (SqlIdentifierExpression)this.DbLink.Clone(),
            Database = (SqlIdentifierExpression)this.Database.Clone(),
            Schema = (SqlIdentifierExpression)this.Schema.Clone(),
            Alias = (SqlIdentifierExpression)this.Alias.Clone(),
            Name = (SqlIdentifierExpression)this.Name.Clone(),
            Hints = this.Hints.Select(x => (SqlHintExpression)x.Clone()).ToList(),
        };
        return result;
    }
}