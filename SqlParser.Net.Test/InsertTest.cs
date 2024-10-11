using SqlParser.Net.Ast.Expression;
using System.Xml.Linq;

namespace SqlParser.Net.Test;

public class InsertTest
{
    [Fact]
    public void TestInsert()
    {
        var sql = "insert into test11(name,id) values('a1','a2'),('a3','a4')";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var expect = new SqlInsertExpression()
        {
            Columns = new List<SqlExpression>()
            {
                new SqlIdentifierExpression()
                {
                    Name = "name"
                },
                new SqlIdentifierExpression()
                {
                    Name = "id"
                },
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Name = "test11"
                }
            },
            ValuesList = new List<List<SqlExpression>>()
            {
                new List<SqlExpression>()
                {
                    new SqlStringExpression()
                    {
                        Value = "a1"
                    },
                    new SqlStringExpression()
                    {
                        Value = "a2"
                    }
                },
                new List<SqlExpression>()
                {
                    new SqlStringExpression()
                    {
                        Value = "a3"
                    },
                    new SqlStringExpression()
                    {
                        Value = "a4"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestInsert2()
    {
        var sql = "insert into test11(name,id) values('a1','a2')";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlInsertExpression()
        {
            Columns = new List<SqlExpression>()
            {
                new SqlIdentifierExpression()
                {
                    Name = "name"
                },
                new SqlIdentifierExpression()
                {
                    Name = "id"
                },
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Name = "test11"
                }
            },
            ValuesList = new List<List<SqlExpression>>()
            {
                new List<SqlExpression>()
                {
                    new SqlStringExpression()
                    {
                        Value = "a1"
                    },
                    new SqlStringExpression()
                    {
                        Value = "a2"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }
    [Fact]
    public void TestInsert3()
    {
        var sql = "insert into test11(name,id) values(@a,@b)";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlInsertExpression()
        {
            Columns = new List<SqlExpression>()
            {
                new SqlIdentifierExpression()
                {
                    Name = "name"
                },
                new SqlIdentifierExpression()
                {
                    Name = "id"
                },
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Name = "test11"
                }
            },
            ValuesList = new List<List<SqlExpression>>()
            {
                new List<SqlExpression>()
                {
                    new SqlVariableExpression()
                    {
                        Prefix = "@",
                        Name = "a"
                    },
                    new SqlVariableExpression()
                    {
                        Prefix = "@",
                        Name = "b"
                    },
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestInsert4()
    {
        var sql = "INSERT INTO TEST VALUES ('a1')";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlInsertExpression()
        {
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Name = "TEST"
                }
            },
            ValuesList = new List<List<SqlExpression>>()
            {
                new List<SqlExpression>()
                {
                    new SqlStringExpression()
                    {
                        Value = "a1"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestInsert5()
    {
        var sql = "INSERT INTO TEST2(name) SELECT name AS name2 FROM TEST t";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var expect = new SqlInsertExpression()
        {
            Columns = new List<SqlExpression>()
            {
                new SqlIdentifierExpression()
                {
                    Name = "name"
                }
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Name = "TEST2"
                }
            },
            FromSelect = new SqlSelectExpression()
            {
                Query = new SqlSelectQueryExpression()
                {
                    Columns = new List<SqlSelectItemExpression>()
                    {
                        new SqlSelectItemExpression()
                        {
                            Alias = new SqlIdentifierExpression()
                            {
                                Name = "name2"
                            },
                            Body = new SqlIdentifierExpression()
                            {
                                Name = "name"
                            }
                        }
                    },
                    From = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "t"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "TEST"
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestInsert6()
    {
        var sql = "insert into message.dbo.TempSMS(sms) values ('333')";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var expect = new SqlInsertExpression()
        {
            Columns = new List<SqlExpression>()
            {
                new SqlIdentifierExpression()
                {
                    Name = "sms"
                }
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Name = "message.dbo.TempSMS"
                }
            },
            ValuesList = new List<List<SqlExpression>>()
            {
                new List<SqlExpression>()
                {
                    new SqlStringExpression()
                    {
                        Value = "333"
                    }
                }

            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

}