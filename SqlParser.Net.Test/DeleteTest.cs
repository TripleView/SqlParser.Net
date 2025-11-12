using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;
using System.Data;
using System.Xml.Linq;
using Xunit.Sdk;

namespace SqlParser.Net.Test;

public class DeleteTest
{
    [Fact]
    public void TestDelete()
    {
        var sql = "delete from test where name=4";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlDeleteExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "test"
                },
            },
            Where = new SqlBinaryExpression()
            {
                Left = new SqlIdentifierExpression()
                {
                    Value = "name"
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlNumberExpression()
                {
                    Value = 4M
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
    }
    [Fact]
    public void TestDeleteCheckIfParsingIsComplete()
    {
        var sql = "delete from RouteData wher code='abc'";
        var sqlAst = new SqlExpression();
        Assert.Throws<SqlParsingErrorException>(() =>
        {
            var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        });
    }
    [Fact]
    public void TestDeleteWithoutFromKeywordInSqlServer()
    {
        var sql = "delete address where id=1";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlDeleteExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "address",
                },
            },
            Where = new SqlBinaryExpression()
            {
                Left = new SqlIdentifierExpression()
                {
                    Value = "id",
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlNumberExpression()
                {
                    Value = 1M,
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.MySql)]
    public void TestDelete2(DbType dbType)
    {
        var sql = "DELETE t FROM T2 t";
        var sqlAst = DbUtils.Parse(sql, dbType);
     
        var result = sqlAst.ToFormat();

        var expect = new SqlDeleteExpression()
        {
            Body = new SqlIdentifierExpression()
            {
                Value = "t",
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "T2",
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "t",
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal("delete t from T2 as t", newSql);
    }

    [Fact]
    public void TestDelete3()
    {
        var sql = "DELETE T2";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);

        var result = sqlAst.ToFormat();

        var expect = new SqlDeleteExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "T2",
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.MySql)]
    public void TestDelete4(DbType dbType)
    {
        var sql = "DELETE t from T3 t join T4 t4 on t.id=t4.Pid where t.id='abc'";
        var sqlAst = DbUtils.Parse(sql, dbType);

        var result = sqlAst.ToFormat();

        var expect = new SqlDeleteExpression()
        {
            Body = new SqlIdentifierExpression()
            {
                Value = "t",
            },
            Table = new SqlJoinTableExpression()
            {
                Left = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "T3",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t",
                    },
                },
                JoinType = SqlJoinType.InnerJoin,
                Right = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "T4",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t4",
                    },
                },
                Conditions = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "id",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t",
                        },
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Pid",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t4",
                        },
                    },
                },
            },
            Where = new SqlBinaryExpression()
            {
                Left = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "id",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "t",
                    },
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlStringExpression()
                {
                    Value = "abc"
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var newSql = sqlAst.ToSql();
        Assert.Equal("delete t from T3 as t inner join T4 as t4 on (t.id = t4.Pid) where (t.id = 'abc')",newSql);
    }

    [Theory]
    [InlineData(DbType.SqlServer)]
    [InlineData(DbType.MySql)]
    [InlineData(DbType.Sqlite)]
    [InlineData(DbType.Pgsql)]
    public void TestDelete5(DbType dbType)
    {
        var sql = "WITH to_delete AS (  SELECT id FROM source_table ) DELETE from target_table where id in (select id from to_delete)";
        var sqlAst = DbUtils.Parse(sql, dbType);

        var result = sqlAst.ToFormat();

        var expect = new SqlDeleteExpression()
        {
            WithSubQuerys = new List<SqlWithSubQueryExpression>()
    {
        new SqlWithSubQueryExpression()
        {
            Alias = new SqlIdentifierExpression()
            {
                Value = "to_delete",
            },
            FromSelect = new SqlSelectExpression()
            {
                Query = new SqlSelectQueryExpression()
                {
                    Columns = new List<SqlSelectItemExpression>()
                    {
                        new SqlSelectItemExpression()
                        {
                            Body = new SqlIdentifierExpression()
                            {
                                Value = "id",
                            },
                        },
                    },
                    From = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "source_table",
                        },
                    },
                },
            },
        },
    },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "target_table",
                },
            },
            Where = new SqlInExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "id",
                },
                SubQuery = new SqlSelectExpression()
                {
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "id",
                        },
                    },
                },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "to_delete",
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var newSql = sqlAst.ToSql();
        Assert.Equal("with to_delete as (select id from source_table) delete from target_table where id in (select id from to_delete)", newSql);
    }

    [Fact]
    public void TestDelete6()
    {
        var sql = "WITH to_delete AS (  SELECT id FROM source_table ) DELETE from target_table where id in (select id from to_delete)";
        Assert.Throws<SqlParsingErrorException>(() =>
        {
            var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        });
    }
}