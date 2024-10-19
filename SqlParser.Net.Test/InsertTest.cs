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
                    Value = "name"
                },
                new SqlIdentifierExpression()
                {
                    Value = "id"
                },
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "test11"
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
                    Value = "name"
                },
                new SqlIdentifierExpression()
                {
                    Value = "id"
                },
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "test11"
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
                    Value = "name"
                },
                new SqlIdentifierExpression()
                {
                    Value = "id"
                },
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "test11"
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
                    Value = "TEST"
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
                    Value = "name"
                }
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "TEST2"
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
                                Value = "name2"
                            },
                            Body = new SqlIdentifierExpression()
                            {
                                Value = "name"
                            }
                        }
                    },
                    From = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "TEST"
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
                    Value = "sms"
                }
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "message.dbo.TempSMS"
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

    [Fact]
    public void TestInsert7()
    {
        var sql = "INSERT INTO \"TEST\"  \r\n           (\"Value\",\"Age\")\r\n     VALUES\r\n           (:Value,:Age) ";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlInsertExpression()
        {
            Columns = new List<SqlExpression>()
            {
                new SqlIdentifierExpression()
                {
                    Value = "\"Value\""
                },
                new SqlIdentifierExpression()
                {
                    Value = "\"Age\""
                },
            },
            Table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "\"TEST\""
                }
            },
            ValuesList = new List<List<SqlExpression>>()
            {
                new List<SqlExpression>()
                {
                    new SqlVariableExpression()
                    {
                        Name = "Value",
                        Prefix = ":"
                    },
                    new SqlVariableExpression()
                    {
                        Name = "Age",
                        Prefix = ":"
                    },
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }
}