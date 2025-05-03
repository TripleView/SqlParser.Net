using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SqlParser.Net.Test;

public class UpdateTest
{
    private ITestOutputHelper testOutputHelper;

    public UpdateTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }
    [Fact]
    public void TestUpdate()
    {

        var sql = "update test set name ='4',d='2024-11-22 08:19:47.243' where name ='1'";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);

        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
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
                Right = new SqlStringExpression()
                {
                    Value = "1"
                },
            },
            Items = new List<SqlExpression>()
            {
                new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "name"
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "4"
                    },
                },
                new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "d"
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "2024-11-22 08:19:47.243"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal("update test set name = '4',d = '2024-11-22 08:19:47.243' where(name = '1')", newSql);
    }

    [Fact]
    public void TestUpdate2()
    {
        var sql = "update test set name =4 where name =1";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "test",
                },
            },
            Where = new SqlBinaryExpression()
            {
                Left = new SqlIdentifierExpression()
                {
                    Value = "name",
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlNumberExpression()
                {
                    Value = 1M,
                },
            },
            Items = new List<SqlExpression>()
            {
                new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "name",
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlNumberExpression()
                    {
                        Value = 4M,
                    },
                },
            },
        };


        var newSql = sqlAst.ToSql();
        Assert.Equal("update test set name = 4 where(name = 1)", newSql);
    }

    [Fact]
    public void TestUpdateCheckIfParsingIsComplete()
    {
        var sql = "update test set name =4 wher name =1";
        var sqlAst = new SqlExpression();
        Assert.Throws<SqlParsingErrorException>(() =>
        {
            sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        });
    }

    [Fact]
    public void TestUpdateJoinPgsql()
    {

        var sql = "update t3 set Id ='1' from  T4 where t3.id = t4.Pid ";
        var sqlAst = new SqlExpression();

        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "t3",
                },
            },
            From = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "T4",
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
                        Value = "t3",
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
            Items = new List<SqlExpression>()
            {
                new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "Id",
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "1"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal("update t3 set Id = '1' from T4 where(t3.id = t4.Pid)", newSql);
    }

    [Fact]
    public void TestUpdateJoinPgsql2()
    {

        var sql = "update t3 set Id ='1' from  T4 t where t3.id = t.Pid";
        var sqlAst = new SqlExpression();

        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "t3",
                },
            },
            From = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "T4",
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "t",
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
                        Value = "t3",
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
                        Value = "t",
                    },
                },
            },
            Items = new List<SqlExpression>()
            {
                new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "Id",
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "1"
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal("update t3 set Id = '1' from T4 as t where(t3.id = t.Pid)", newSql);
    }

    [Fact]
    public void TestUpdateJoinPgsql3()
    {

        var sql = "update t3 set Id ='1' from t3 t join T4 t2 on t.id = t2.Pid where t.id='a'";
        var sqlAst = new SqlExpression();

        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "t3",
                },
            },
            From = new SqlJoinTableExpression()
            {
                Left = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "t3",
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
                        Value = "t2",
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
                            Value = "t2",
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
                    Value = "a"
                },
            },
            Items = new List<SqlExpression>()
    {
        new SqlBinaryExpression()
        {
            Left = new SqlIdentifierExpression()
            {
                Value = "Id",
            },
            Operator = SqlBinaryOperator.EqualTo,
            Right = new SqlStringExpression()
            {
                Value = "1"
            },
        },
    },
        };



        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal("update t3 set Id = '1' from t3 as t inner join T4 as t2 on(t.id = t2.Pid) where(t.id = 'a')", newSql);
    }

    [Fact]
    public void TestUpdateJoinSqlserver()
    {

        var sql = "update t3 set t3.Id ='1' from T3  join T4 t4 on t3.id = t4.Pid where t3.id = 'abc'";
        var sqlAst = new SqlExpression();

        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "t3",
                },
            },
            From = new SqlJoinTableExpression()
            {
                Left = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "T3",
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
                            Value = "t3",
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
                        Value = "t3",
                    },
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlStringExpression()
                {
                    Value = "abc"
                },
            },
            Items = new List<SqlExpression>()
    {
        new SqlBinaryExpression()
        {
            Left = new SqlPropertyExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "Id",
                },
                Table = new SqlIdentifierExpression()
                {
                    Value = "t3",
                },
            },
            Operator = SqlBinaryOperator.EqualTo,
            Right = new SqlStringExpression()
            {
                Value = "1"
            },
        },
    },
        };


        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal("update t3 set t3.Id = '1' from T3 inner join T4 as t4 on(t3.id = t4.Pid) where(t3.id = 'abc')", newSql);
    }

    [Fact]
    public void TestUpdateJoinSqlserver2()
    {

        var sql = "update t set t.Id ='1' from T3 as t inner join T4 as t4 on t.id = t4.Pid where t.id = 'abc'";
        var sqlAst = new SqlExpression();

        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "t",
                },
            },
            From = new SqlJoinTableExpression()
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
            Items = new List<SqlExpression>()
    {
        new SqlBinaryExpression()
        {
            Left = new SqlPropertyExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "Id",
                },
                Table = new SqlIdentifierExpression()
                {
                    Value = "t",
                },
            },
            Operator = SqlBinaryOperator.EqualTo,
            Right = new SqlStringExpression()
            {
                Value = "1"
            },
        },
    },
        };

        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal("update t set t.Id = '1' from T3 as t inner join T4 as t4 on(t.id = t4.Pid) where(t.id = 'abc')", newSql);
    }


    [Theory]
    [InlineData("left")]
    [InlineData("right")]
    [InlineData("")]
    [InlineData("inner")]
    [InlineData("cross")]
    public void TestUpdateJoinMysql(string joinTypeStr)
    {

        var sql = $"update t3 {joinTypeStr} join T4  on t3.id = t4.Pid  set t3.Id ='1'  where 1=1";
        var sqlAst = new SqlExpression();

        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var result = sqlAst.ToFormat();
        var joinType = SqlJoinType.InnerJoin;
        var resultJoinString = "";
        if (joinTypeStr == "" || joinTypeStr == "inner")
        {
            joinType = SqlJoinType.InnerJoin;
            resultJoinString = "inner";
        }
        else if (joinTypeStr == "left")
        {
            joinType = SqlJoinType.LeftJoin;
            resultJoinString = "left";
        }
        else if (joinTypeStr == "right")
        {
            joinType = SqlJoinType.RightJoin;
            resultJoinString = "right";
        }
        else if (joinTypeStr == "cross")
        {
            joinType = SqlJoinType.CrossJoin;
            resultJoinString = "cross";
        }

        var expect = new SqlUpdateExpression()
            {
                Table = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "t3",
                        },
                    },
                    JoinType = joinType,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "T4",
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
                                Value = "t3",
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
                    Left = new SqlNumberExpression()
                    {
                        Value = 1M,
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlNumberExpression()
                    {
                        Value = 1M,
                    },
                },
                Items = new List<SqlExpression>()
    {
        new SqlBinaryExpression()
        {
            Left = new SqlPropertyExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "Id",
                },
                Table = new SqlIdentifierExpression()
                {
                    Value = "t3",
                },
            },
            Operator = SqlBinaryOperator.EqualTo,
            Right = new SqlStringExpression()
            {
                Value = "1"
            },
        },
    },
            };


        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal($"update t3 {resultJoinString} join T4 on(t3.id = t4.Pid) set t3.Id = '1' where(1 = 1)", newSql);
    }

    [Fact]
    public void TestUpdateJoinMysql2()
    {

        var sql = "update t3 a inner join T4  b on a.id = b.Pid  set a.Id ='1'  where 1=1";
        var sqlAst = new SqlExpression();

        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlJoinTableExpression()
            {
                Left = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "t3",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "a",
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
                        Value = "b",
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
                            Value = "a",
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
                            Value = "b",
                        },
                    },
                },
            },
            Where = new SqlBinaryExpression()
            {
                Left = new SqlNumberExpression()
                {
                    Value = 1M,
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlNumberExpression()
                {
                    Value = 1M,
                },
            },
            Items = new List<SqlExpression>()
    {
        new SqlBinaryExpression()
        {
            Left = new SqlPropertyExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "Id",
                },
                Table = new SqlIdentifierExpression()
                {
                    Value = "a",
                },
            },
            Operator = SqlBinaryOperator.EqualTo,
            Right = new SqlStringExpression()
            {
                Value = "1"
            },
        },
    },
        };


        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        Assert.Equal("update t3 as a inner join T4 as b on(a.id = b.Pid) set a.Id = '1' where(1 = 1)", newSql);
    }

    [Fact]
    public void TestUpdateJoinOracle()
    {

        var sql = @"UPDATE t3 SET id=(SELECT pid FROM t4 WHERE t3.id=t4.PID) WHERE EXISTS (
                SELECT 1
                FROM t4
                WHERE t3.id = t4.pid
            )";//oracle
        var sqlAst = new SqlExpression();

        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var result = sqlAst.ToFormat();
        var expect = new SqlUpdateExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "t3",
                },
            },
            Where = new SqlExistsExpression()
            {
                Body = new SqlSelectExpression()
                {
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlNumberExpression()
                        {
                            Value = 1M,
                        },
                    },
                },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "t4",
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
                                    Value = "t3",
                                },
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "pid",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t4",
                                },
                            },
                        },
                    },
                },
            },
            Items = new List<SqlExpression>()
    {
        new SqlBinaryExpression()
        {
            Left = new SqlIdentifierExpression()
            {
                Value = "id",
            },
            Operator = SqlBinaryOperator.EqualTo,
            Right = new SqlSelectExpression()
            {
                Query = new SqlSelectQueryExpression()
                {
                    Columns = new List<SqlSelectItemExpression>()
                    {
                        new SqlSelectItemExpression()
                        {
                            Body = new SqlIdentifierExpression()
                            {
                                Value = "pid",
                            },
                        },
                    },
                    From = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "t4",
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
                                Value = "t3",
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "PID",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "t4",
                            },
                        },
                    },
                },
            },
        },
    },
        };



        Assert.True(sqlAst.Equals(expect));
        var newSql = sqlAst.ToSql();
        //Assert.Equal("update t3 as a inner join T4 as b on(a.id = b.Pid) set a.Id = '1' where(1 = 1)", newSql);
    }
}