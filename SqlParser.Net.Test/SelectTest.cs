using SqlParser.Net.Ast.Expression;
using System.Xml.Linq;

namespace SqlParser.Net.Test;

public class SelectTest
{
    [Fact]
    public void TestSelectAll()
    {
        var sql = "select * from RouteData";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestIdentifierColumn()
    {
        var sql = "select Id from RouteData";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Name = "Id"
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestPropertyColumn()
    {
        var sql = "select rd.name as bname from RouteData rd";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "bname"
                        },
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Name = "name"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "rd"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "rd"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestStringColumn()
    {
        var sql = "select ''' ''' from RouteData";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlStringExpression()
                        {
                            Value = "' '"
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestStringColumn2()
    {
        var sql = "select '2'' ''2' from RouteData";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlStringExpression()
                        {
                            Value = "2' '2"
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestNumberColumn()
    {
        var sql = "select 5.20 from RouteData";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlNumberExpression()
                        {
                            Value = 5.2M
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestFunctionCall()
    {
        var sql = "select LOWER(fa.Id)  from FlowActivity fa";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlFunctionCallExpression()
                        {
                            Name = "LOWER",
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression() { Name = "Id" },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Name = "fa"
                                    }
                                }
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestComplexColumn()
    {
        var sql = "select rd.Active*2+5  from RouteData rd";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlBinaryExpression()
                        {
                            Right = new SqlNumberExpression()
                            {
                                Value = 5
                            },
                            Operator = SqlBinaryOperator.Add,
                            Left = new SqlBinaryExpression()
                            {
                                Operator = SqlBinaryOperator.Multiply,
                                Right = new SqlNumberExpression()
                                {
                                    Value = 2
                                },
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression() { Name = "Active" },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Name = "rd"
                                    }
                                }
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "rd"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestSelectSinpleMultiColumns()
    {
        var sql = "select Id,Name,Active from RouteData";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Name = "Id"
                        }
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Name = "Name"
                        }
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Name = "Active"
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(new object[] { "select * from RouteData a" })]
    [InlineData(new object[] { "select * from RouteData as a" })]
    public void TestTableAlias(string sql)
    {
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "a"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(new object[] { "select Id as bid from RouteData as a" })]
    [InlineData(new object[] { "select Id bid  from RouteData a" })]
    public void TestIdentifierColumnAndAs(string sql)
    {
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "bid"
                        },
                        Body = new SqlIdentifierExpression()
                        {
                            Name = "Id"
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "a"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestSubQuery()
    {
        var sql = "select * from (select * from RouteData) a";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlSelectExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "a"
                    },
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlAllColumnExpression()
                            }
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Name = "RouteData"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestSubQuery2()
    {
        var sql = "select * from (SELECT * FROM TEST t3) ";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlSelectExpression()
                {
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlAllColumnExpression()
                            }
                        },
                        From = new SqlTableExpression()
                        {
                            Alias = new SqlIdentifierExpression()
                            {
                                Name = "t3"
                            },
                            Name = new SqlIdentifierExpression()
                            {
                                Name = "TEST"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestWhere()
    {
        var sql = "select * from RouteData rd where id='805B9CFC-1671-4BD8-B011-003EB7398FB0'";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "rd"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Name = "id"
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "805B9CFC-1671-4BD8-B011-003EB7398FB0"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestWhere2()
    {
        var sql = "SELECT * FROM customer t3 where t3.Name =(select city from address a)";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "t3"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "customer"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "Name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "t3"
                        }
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
                                        Name = "city"
                                    }
                                }
                            },
                            From = new SqlTableExpression()
                            {
                                Alias = new SqlIdentifierExpression()
                                {
                                    Name = "a"
                                },
                                Name = new SqlIdentifierExpression()
                                {
                                    Name = "address"
                                }
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestBetweenAnd()
    {
        var sql = "select * from FlowActivity fa where fa.CreateOn BETWEEN '2024-01-01' and '2024-10-10'";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                },
                Where = new SqlBetweenAndExpression()
                {
                    Body = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "CreateOn" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "fa"
                        }
                    },
                    Begin = new SqlStringExpression()
                    {
                        Value = "2024-01-01"
                    },
                    End = new SqlStringExpression()
                    {
                        Value = "2024-10-10"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestBetweenAnd2()
    {
        var sql =
            "select * from FlowActivity fa where fa.OnlyMainJobProcessing BETWEEN (0+0.5)*2 and 1 and fa.Active =1";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlBetweenAndExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "OnlyMainJobProcessing" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        },
                        Begin = new SqlBinaryExpression()
                        {
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlNumberExpression()
                                {
                                    Value = 0
                                },
                                Operator = SqlBinaryOperator.Add,
                                Right = new SqlNumberExpression()
                                {
                                    Value = 0.5M
                                }
                            },
                            Operator = SqlBinaryOperator.Multiply,
                            Right = new SqlNumberExpression()
                            {
                                Value = 2
                            }
                        },
                        End = new SqlNumberExpression()
                        {
                            Value = 1
                        }
                    },
                    Operator = SqlBinaryOperator.And,
                    Right = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "Active" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlNumberExpression()
                        {
                            Value = 1
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestJoin()
    {
        var sql =
            "select * from Customer c join Address a on ((c.Id =a.CustomerId or c.Age>=a.CustomerId) and c.CustomerNo !='abc' )";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "c"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "Customer"
                        }
                    },
                    Right = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "a"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "Address"
                        }
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlBinaryExpression()
                        {
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression() { Name = "Id" },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Name = "c"
                                    }
                                },
                                Operator = SqlBinaryOperator.EqualTo,
                                Right = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression() { Name = "CustomerId" },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Name = "a"
                                    }
                                }
                            },
                            Operator = SqlBinaryOperator.Or,
                            Right = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression() { Name = "Age" },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Name = "c"
                                    }
                                },
                                Operator = SqlBinaryOperator.GreaterThenOrEqualTo,
                                Right = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression() { Name = "CustomerId" },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Name = "a"
                                    }
                                }
                            }
                        },
                        Operator = SqlBinaryOperator.And,
                        Right = new SqlBinaryExpression()
                        {
                            Left = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Name = "CustomerNo" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Name = "c"
                                }
                            },
                            Operator = SqlBinaryOperator.NotEqualTo,
                            Right = new SqlStringExpression()
                            {
                                Value = "abc"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestJoin2()
    {
        var sql = "select * from (SELECT * FROM TEST t3)  t join (select * from test1 t) t2 on t.name=t2.test";
        var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlSelectExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "t"
                        },
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                            {
                                new SqlSelectItemExpression()
                                {
                                    Body = new SqlAllColumnExpression()
                                }
                            },
                            From = new SqlTableExpression()
                            {
                                Alias = new SqlIdentifierExpression()
                                {
                                    Name = "t3"
                                },
                                Name = new SqlIdentifierExpression()
                                {
                                    Name = "TEST"
                                }
                            }
                        }
                    },
                    Right = new SqlSelectExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "t2"
                        },
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                            {
                                new SqlSelectItemExpression()
                                {
                                    Body = new SqlAllColumnExpression()
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
                                    Name = "test1"
                                }
                            }
                        }
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "name" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "t"
                            }
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "test" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "t2"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(new object[]
        { "select * from Customer c inner join Address a on c.Id=a.CustomerId ", SqlJoinType.InnerJoin })]
    [InlineData(new object[]
        { "select * from Customer c left join Address a on c.Id=a.CustomerId ", SqlJoinType.LeftJoin })]
    [InlineData(new object[]
        { "select * from Customer c right join Address a on c.Id=a.CustomerId ", SqlJoinType.RightJoin })]
    [InlineData(new object[]
        { "select * from Customer c full join Address a on c.Id=a.CustomerId ", SqlJoinType.FullJoin })]
    public void TestJoinType(string sql, SqlJoinType joinType)
    {
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "c"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "Customer"
                        }
                    },
                    Right = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "a"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "Address"
                        }
                    },
                    JoinType = joinType,
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "Id" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "c"
                            }
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "CustomerId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "a"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestGroupBy()
    {
        var sql = "select fa.FlowId  from FlowActivity fa group by fa.FlowId,fa.Id ";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "FlowId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                },
                GroupBy = new SqlGroupByExpression()
                {
                    Items = new List<SqlExpression>()
                    {
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "FlowId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        },
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "Id" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestGroupByHaving()
    {
        var sql = "select fa.FlowId  from FlowActivity fa group by fa.FlowId,fa.Id HAVING count(fa.Id)>1";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "FlowId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                },
                GroupBy = new SqlGroupByExpression()
                {
                    Items = new List<SqlExpression>()
                    {
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "FlowId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        },
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "Id" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        }
                    },
                    Having = new SqlBinaryExpression()
                    {
                        Left = new SqlFunctionCallExpression()
                        {
                            Name = "count",
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression() { Name = "Id" },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Name = "fa"
                                    }
                                }
                            }
                        },
                        Operator = SqlBinaryOperator.GreaterThen,
                        Right = new SqlNumberExpression()
                        {
                            Value = 1
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestGroupByHaving2()
    {
        var sql = "select fa.FlowId  from FlowActivity fa group by fa.FlowId,fa.Id HAVING 1+2>3";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "FlowId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                },
                GroupBy = new SqlGroupByExpression()
                {
                    Items = new List<SqlExpression>()
                    {
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "FlowId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        },
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "Id" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        }
                    },
                    Having = new SqlBinaryExpression()
                    {
                        Left = new SqlBinaryExpression()
                        {
                            Left = new SqlNumberExpression()
                            {
                                Value = 1
                            },
                            Operator = SqlBinaryOperator.Add,
                            Right = new SqlNumberExpression()
                            {
                                Value = 2
                            }
                        },
                        Operator = SqlBinaryOperator.GreaterThen,
                        Right = new SqlNumberExpression()
                        {
                            Value = 3
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestOrderBy()
    {
        var sql = "select fa.FlowId  from FlowActivity fa order by fa.FlowId desc,fa.Id asc";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "FlowId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Expression = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Name = "FlowId" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Name = "fa"
                                }
                            },
                            OrderByType = SqlOrderByType.Desc
                        },
                        new SqlOrderByItemExpression()
                        {
                            Expression = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Name = "Id" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Name = "fa"
                                }
                            },
                            OrderByType = SqlOrderByType.Asc
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestDistinct()
    {
        var sql = "select DISTINCT  fa.FlowId  from FlowActivity fa";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                ResultSetReturnOption = SqlResultSetReturnOption.Distinct,
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "FlowId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestDistinct2()
    {
        var sql = "select count(DISTINCT  fa.FlowId)  from FlowActivity fa";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlFunctionCallExpression()
                        {
                            Name = "count",
                            IsDistinct = true,
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression() { Name = "FlowId" },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Name = "fa"
                                    }
                                }
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestEscapeCharacter()
    {
        var sql = "select rd.[System] from RouteData rd where rd.[System] ='ats'";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "[System]" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "rd"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "rd"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "[System]" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "rd"
                        }
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "ats"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestIsNull()
    {
        var sql = "select * from RouteData rd where rd.name is null";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "rd"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "rd"
                        }
                    },
                    Operator = SqlBinaryOperator.Is,
                    Right = new SqlNullExpression()
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestIsNotNull()
    {
        var sql = "select * from RouteData rd where rd.name is not null";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "rd"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "RouteData"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "rd"
                        }
                    },
                    Operator = SqlBinaryOperator.IsNot,
                    Right = new SqlNullExpression()
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestExists()
    {
        var sql = "select * from TEST t where EXISTS(select * from TEST1 t2) OR 1=1";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlExistsExpression()
                    {
                        SelectExpression = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlAllColumnExpression()
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Name = "t2"
                                    },
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "TEST1"
                                    }
                                }
                            }
                        }
                    },
                    Operator = SqlBinaryOperator.Or,
                    Right = new SqlBinaryExpression()
                    {
                        Left = new SqlNumberExpression()
                        {
                            Value = 1
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlNumberExpression()
                        {
                            Value = 1
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestLike()
    {
        var sql = "SELECT * from TEST t WHERE name LIKE '%a%'";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Name = "name"
                    },
                    Operator = SqlBinaryOperator.Like,
                    Right = new SqlStringExpression()
                    {
                        Value = "%a%"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    public void TestCommaJoin()
    {
        var sql = "select * from test t,test11 t1 where t.name =t1.name";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "t"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "test"
                        }
                    },
                    Right = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "t1"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "test11"
                        }
                    },
                    JoinType = SqlJoinType.InnerJoin
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "t"
                        }
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "t1"
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestUnionQuery()
    {
        var sql = "select name from test union select name from test11 Except select name from test11";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlUnionQueryExpression()
            {
                Left = new SqlUnionQueryExpression()
                {
                    Left = new SqlSelectExpression()
                    {
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                            {
                                new SqlSelectItemExpression()
                                {
                                    Body = new SqlIdentifierExpression()
                                    {
                                        Name = "name"
                                    }
                                }
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Name = "test"
                                }
                            }
                        }
                    },
                    UnionType = SqlUnionType.Union,
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
                                        Name = "name"
                                    }
                                }
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Name = "test11"
                                }
                            }
                        }
                    }
                },
                UnionType = SqlUnionType.Except,
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
                                    Name = "name"
                                }
                            }
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Name = "test11"
                            }
                        }
                    }
                }
            }
        };
        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestUnionQuery2()
    {
        var sql = "((select name from test union all select name from test11) Except  select name from test11)";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlUnionQueryExpression()
            {
                Left = new SqlSelectExpression()
                {
                    Query = new SqlUnionQueryExpression()
                    {
                        Left = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlIdentifierExpression()
                                        {
                                            Name = "name"
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "test"
                                    }
                                }
                            }
                        },
                        UnionType = SqlUnionType.UnionAll,
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
                                            Name = "name"
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "test11"
                                    }
                                }
                            }
                        }
                    }
                },
                UnionType = SqlUnionType.Except,
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
                                    Name = "name"
                                }
                            }
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Name = "test11"
                            }
                        }
                    }
                }
            }
        };
        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestUnionQuery3()
    {
        var sql = "(select name from test union (select name from test11 Except select name from test11))";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlUnionQueryExpression()
            {
                Right = new SqlSelectExpression()
                {
                    Query = new SqlUnionQueryExpression()
                    {
                        Left = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlIdentifierExpression()
                                        {
                                            Name = "name"
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "test11"
                                    }
                                }
                            }
                        },
                        UnionType = SqlUnionType.Except,
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
                                            Name = "name"
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "test11"
                                    }
                                }
                            }
                        }
                    }
                },
                UnionType = SqlUnionType.Union,
                Left = new SqlSelectExpression()
                {
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlIdentifierExpression()
                                {
                                    Name = "name"
                                }
                            }
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Name = "test"
                            }
                        }
                    }
                }
            }
        };
        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestAll()
    {
        var sql = "select all  fa.FlowId  from FlowActivity fa";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                ResultSetReturnOption = SqlResultSetReturnOption.All,
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "FlowId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "fa"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "FlowActivity"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestAll2()
    {
        var sql = "select * from customer c where c.Age >all(select o.Quantity  from orderdetail o)";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "c"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "customer"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "Age" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "c"
                        }
                    },
                    Operator = SqlBinaryOperator.GreaterThen,
                    Right = new SqlAllExpression()
                    {
                        SelectExpression = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "Quantity" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "o"
                                            }
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Name = "o"
                                    },
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "orderdetail"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestAny()
    {
        var sql = "select * from customer c where c.Age >any(select o.Quantity  from orderdetail o)";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "c"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "customer"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "Age" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "c"
                        }
                    },
                    Operator = SqlBinaryOperator.GreaterThen,
                    Right = new SqlAnyExpression()
                    {
                        SelectExpression = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "Quantity" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "o"
                                            }
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Name = "o"
                                    },
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "orderdetail"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestUnique()
    {
        var sql = "SELECT  unique * from TEST t";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                ResultSetReturnOption = SqlResultSetReturnOption.Unique,
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestIn()
    {
        var sql = "SELECT  * from TEST t WHERE t.NAME IN ('a','b','c')";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
                },
                Where = new SqlInExpression()
                {
                    Field = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "NAME" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "t"
                        }
                    },
                    TargetList = new List<SqlExpression>()
                    {
                        new SqlStringExpression()
                        {
                            Value = "a"
                        },
                        new SqlStringExpression()
                        {
                            Value = "b"
                        },
                        new SqlStringExpression()
                        {
                            Value = "c"
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestIn2()
    {
        var sql = "SELECT * from TEST t JOIN TEST2 t2 ON t.NAME IN ('a') AND t.NAME =t2.NAME";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "t"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "TEST"
                        }
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "t2"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "TEST2"
                        }
                    },
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlInExpression()
                        {
                            Field = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Name = "NAME" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Name = "t"
                                }
                            },
                            TargetList = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "a"
                                }
                            }
                        },
                        Operator = SqlBinaryOperator.And,
                        Right = new SqlBinaryExpression()
                        {
                            Left = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Name = "NAME" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Name = "t"
                                }
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Name = "NAME" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Name = "t2"
                                }
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(new object[] { "as name", "name" })]
    [InlineData(new object[] { "", null })]
    public void TestCase(string asName, string name)
    {
        var sql =
            $"select case when t.name ='a' then '1' when t.name in ('b','c') then '2' else '3' end {asName} from test t join test11 t2 on case when t.name ='a' then '1' when t.name is null then '2'  end=t2.name  ";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var alias = new SqlIdentifierExpression()
        {
            Name = name
        };
        if (string.IsNullOrWhiteSpace(name)) alias = null;

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Alias = alias,
                        Body = new SqlCaseExpression()
                        {
                            Items = new List<SqlCaseItemExpression>()
                            {
                                new SqlCaseItemExpression()
                                {
                                    Condition = new SqlBinaryExpression()
                                    {
                                        Left = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "name" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "t"
                                            }
                                        },
                                        Operator = SqlBinaryOperator.EqualTo,
                                        Right = new SqlStringExpression()
                                        {
                                            Value = "a"
                                        }
                                    },
                                    Value = new SqlStringExpression()
                                    {
                                        Value = "1"
                                    }
                                },
                                new SqlCaseItemExpression()
                                {
                                    Condition = new SqlInExpression()
                                    {
                                        Field = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "name" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "t"
                                            }
                                        },
                                        TargetList = new List<SqlExpression>()
                                        {
                                            new SqlStringExpression()
                                            {
                                                Value = "b"
                                            },
                                            new SqlStringExpression()
                                            {
                                                Value = "c"
                                            }
                                        }
                                    },
                                    Value = new SqlStringExpression()
                                    {
                                        Value = "2"
                                    }
                                }
                            },
                            Else = new SqlStringExpression()
                            {
                                Value = "3"
                            }
                        }
                    }
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "t"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "test"
                        }
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "t2"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "test11"
                        }
                    },
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlCaseExpression()
                        {
                            Items = new List<SqlCaseItemExpression>()
                            {
                                new SqlCaseItemExpression()
                                {
                                    Condition = new SqlBinaryExpression()
                                    {
                                        Left = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "name" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "t"
                                            }
                                        },
                                        Operator = SqlBinaryOperator.EqualTo,
                                        Right = new SqlStringExpression()
                                        {
                                            Value = "a"
                                        }
                                    },
                                    Value = new SqlStringExpression()
                                    {
                                        Value = "1"
                                    }
                                },
                                new SqlCaseItemExpression()
                                {
                                    Condition = new SqlBinaryExpression()
                                    {
                                        Left = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "name" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "t"
                                            }
                                        },
                                        Operator = SqlBinaryOperator.Is,
                                        Right = new SqlNullExpression()
                                    },
                                    Value = new SqlStringExpression()
                                    {
                                        Value = "2"
                                    }
                                }
                            }
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "name" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "t2"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestSelectInto()
    {
        var sql = "SELECT name into test14 from TEST t ";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Name = "name"
                        }
                    }
                },
                Into = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "test14"
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
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestLineBreak()
    {
        var sql = @"select 
                    * 
                    from 
                    test";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "test"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestLineBreak2()
    {
        var sql = @"select '\r\n' from test;";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlStringExpression()
                        {
                            Value = "\\r\\n"
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "test"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestSingleLineComment()
    {
        var sql = @"select *--abc from LR_BASE_USER lbu WHERE F_USERID ='System'--aaaaaa
                   FROM test";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "test"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
        var sqlAstExpression = sqlAst as SqlSelectExpression;
        ;
        Assert.Equal(1, sqlAstExpression.Comments.Count);
        Assert.Equal("abc from LR_BASE_USER lbu WHERE F_USERID ='System'--aaaaaa", sqlAstExpression.Comments[0]);
    }

    [Fact]
    public void TestSingleLineComment2()
    {
        var sql = @"SELECT--
                    *--abc   
                    FROM--ccd
                    TEST t--";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
        };

        Assert.True(sqlAst.Equals(expect));
        var sqlAstExpression = sqlAst as SqlSelectExpression;

        Assert.Equal(4, sqlAstExpression.Comments.Count);
        Assert.Equal("", sqlAstExpression.Comments[0]);
        Assert.Equal("abc   ", sqlAstExpression.Comments[1]);
        Assert.Equal("ccd", sqlAstExpression.Comments[2]);
        Assert.Equal("", sqlAstExpression.Comments[3]);
    }

    [Fact]
    public void TestMultiLineComment()
    {
        var sql = @"/*aaa
                    这是一个
                    多行注释
                    */
                    select *--abc
                    FROM test";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "test"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
        var sqlAstExpression = sqlAst as SqlSelectExpression;

        Assert.Equal(2, sqlAstExpression.Comments.Count);
        Assert.Equal(
            "aaa\r\n                    这是一个\r\n                    多行注释\r\n                    ".Replace("\r\n",
                "\n"), sqlAstExpression.Comments[0]);
        Assert.Equal("abc", sqlAstExpression.Comments[1]);
    }

    [Fact]
    public void TestMultiLineComment2()
    {
        var sql = @"/*这
                    是
                    顶部*/
                    select *--abc
                    FROM test/*这
                    是
                    底部*/";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "test"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
        var sqlAstExpression = sqlAst as SqlSelectExpression;

        Assert.Equal(3, sqlAstExpression.Comments.Count);
        Assert.Equal("这\r\n                    是\r\n                    顶部".Replace("\r\n", "\n"),
            sqlAstExpression.Comments[0]);
        Assert.Equal("abc", sqlAstExpression.Comments[1]);
        Assert.Equal("这\r\n                    是\r\n                    底部".Replace("\r\n", "\n"),
            sqlAstExpression.Comments[2]);
    }

    [Theory]
    [InlineData(new object[] { "<>" })]
    [InlineData(new object[] { "!=" })]
    public void TestNotEqual(string @operator)
    {
        var sql = $"select * from test t where t.name {@operator}'abc'";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
                        Name = "test"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "t"
                        }
                    },
                    Operator = SqlBinaryOperator.NotEqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "abc"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(new object[] { " limit 1" })]
    [InlineData(new object[] { " limit 1,5" })]
    public void TestLimitForMysql(string limitString)
    {
        var sql = $"select * from customer a where a.name!='''123' order by a.Id {limitString}";
        var sqlAst = DbUtils.Parse(sql, DbType.MySql);
        SqlLimitExpression limit = null;
        if (limitString == " limit 1,5")
            limit = new SqlLimitExpression()
            {
                RowCount = new SqlNumberExpression()
                {
                    Value = 5
                },
                Offset = new SqlNumberExpression()
                {
                    Value = 1
                }
            };
        else if (limitString == " limit 1")
            limit = new SqlLimitExpression()
            {
                RowCount = new SqlNumberExpression()
                {
                    Value = 1
                }
            };

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "a"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "customer"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "a"
                        }
                    },
                    Operator = SqlBinaryOperator.NotEqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "'123"
                    }
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Expression = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Name = "Id" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Name = "a"
                                }
                            }
                        }
                    }
                },
                Limit = limit
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestLimitForSqlServer()
    {
        var sql = "select * from test t order by t.name OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
                        Name = "test"
                    }
                },

                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Expression = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Name = "name" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Name = "t"
                                }
                            }
                        }
                    }
                },
                Limit = new SqlLimitExpression()
                {
                    Offset = new SqlNumberExpression()
                    {
                        Value = 5
                    },
                    RowCount = new SqlNumberExpression()
                    {
                        Value = 10
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(new object[] { " limit 1 offset 10" })]
    [InlineData(new object[] { " limit 1 " })]
    [InlineData(new object[] { " offset 10" })]
    public void TestLimitForPgsql(string limitString)
    {
        var sql = $"select * from test t where t.test !='abc' order by t.test {limitString}";
        var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
        SqlLimitExpression limit = null;
        if (limitString == " limit 1 offset 10")
            limit = new SqlLimitExpression()
            {
                RowCount = new SqlNumberExpression()
                {
                    Value = 1
                },
                Offset = new SqlNumberExpression()
                {
                    Value = 10
                }
            };
        else if (limitString == " limit 1 ")
            limit = new SqlLimitExpression()
            {
                RowCount = new SqlNumberExpression()
                {
                    Value = 1
                }
            };
        else if (limitString == " offset 10")
            limit = new SqlLimitExpression()
            {
                Offset = new SqlNumberExpression()
                {
                    Value = 10
                }
            };


        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
                        Name = "test"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Name = "test" },
                        Table = new SqlIdentifierExpression()
                        {
                            Name = "t"
                        }
                    },
                    Operator = SqlBinaryOperator.NotEqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "abc"
                    }
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Expression = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Name = "test" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Name = "t"
                                }
                            }
                        }
                    }
                },
                Limit = limit
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(new object[] { "PARTITION BY NAME ,ID " })]
    [InlineData(new object[] { "" })]
    public void TestWindowFunctionCall(string partitionByString)
    {
        var sql = $"SELECT t.*, ROW_NUMBER() OVER ( {partitionByString} ORDER BY t.NAME,t.ID) as rnum FROM TEST t";
        var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
        SqlPartitionByExpression partitionBy = null;
        if (partitionByString == "PARTITION BY NAME ,ID ")
            partitionBy = new SqlPartitionByExpression()
            {
                Items = new List<SqlExpression>()
                {
                    new SqlIdentifierExpression()
                    {
                        Name = "NAME"
                    },
                    new SqlIdentifierExpression()
                    {
                        Name = "ID"
                    }
                }
            };

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "*" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "t"
                            }
                        }
                    },
                    new SqlSelectItemExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "rnum"
                        },
                        Body = new SqlFunctionCallExpression()
                        {
                            Name = "ROW_NUMBER",
                            Over = new SqlOverExpression()
                            {
                                OrderBy = new SqlOrderByExpression()
                                {
                                    Items = new List<SqlOrderByItemExpression>()
                                    {
                                        new SqlOrderByItemExpression()
                                        {
                                            Expression = new SqlPropertyExpression()
                                            {
                                                Name = new SqlIdentifierExpression() { Name = "NAME" },
                                                Table = new SqlIdentifierExpression()
                                                {
                                                    Name = "t"
                                                }
                                            }
                                        },
                                        new SqlOrderByItemExpression()
                                        {
                                            Expression = new SqlPropertyExpression()
                                            {
                                                Name = new SqlIdentifierExpression() { Name = "ID" },
                                                Table = new SqlIdentifierExpression()
                                                {
                                                    Name = "t"
                                                }
                                            }
                                        }
                                    }
                                },
                                PartitionBy = partitionBy
                            }
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
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestNot()
    {
        var sql = "select * from TEST t WHERE not t.NAME ='abc'";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
                },
                Where = new SqlNotExpression()
                {
                    Expression = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "NAME" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "t"
                            }
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlStringExpression()
                        {
                            Value = "abc"
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestNot2()
    {
        var sql = "select * from TEST t WHERE not EXISTS (SELECT * FROM TEST1 t2)";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
                },
                Where = new SqlNotExpression()
                {
                    Expression = new SqlExistsExpression()
                    {
                        SelectExpression = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlAllColumnExpression()
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Name = "t2"
                                    },
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "TEST1"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestDatabaseScheme()
    {
        var sql = "select * from EPF.dbo.test t";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
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
                        Name = "EPF.dbo.test"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestDatabaseScheme2()
    {
        var sql = "select * from epf.dbo.Ability a left join ATL_Login.dbo.ATLAdUsers au on a.Id =au.Id";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "a"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "epf.dbo.Ability"
                        }
                    },
                    Right = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "au"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "ATL_Login.dbo.ATLAdUsers"
                        }
                    },
                    JoinType = SqlJoinType.LeftJoin,
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "Id" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "a"
                            }
                        },
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "Id" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "au"
                            }
                        },
                        Operator = SqlBinaryOperator.EqualTo
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }


    [Fact]
    public void TestComplexSelectItem()
    {
        var sql =
            "select c.*, (select a.name as province_name from portal_area a where a.id = c.province_id) as province_name, (select a.name as city_name from portal_area a where a.id = c.city_id) as city_name, (CASE WHEN c.area_id IS NULL THEN NULL ELSE (select a.name as area_name from portal_area a where a.id = c.area_id)  END )as area_name  from portal.portal_company c where no =:a";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "*" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "c"
                            }
                        }
                    },
                    new SqlSelectItemExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "province_name"
                        },
                        Body = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Alias = new SqlIdentifierExpression()
                                        {
                                            Name = "province_name"
                                        },
                                        Body = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "name" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "a"
                                            }
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Name = "a"
                                    },
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "portal_area"
                                    }
                                },
                                Where = new SqlBinaryExpression()
                                {
                                    Left = new SqlPropertyExpression()
                                    {
                                        Name = new SqlIdentifierExpression() { Name = "id" },
                                        Table = new SqlIdentifierExpression()
                                        {
                                            Name = "a"
                                        }
                                    },
                                    Operator = SqlBinaryOperator.EqualTo,
                                    Right = new SqlPropertyExpression()
                                    {
                                        Name = new SqlIdentifierExpression() { Name = "province_id" },
                                        Table = new SqlIdentifierExpression()
                                        {
                                            Name = "c"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new SqlSelectItemExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "city_name"
                        },
                        Body = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Alias = new SqlIdentifierExpression()
                                        {
                                            Name = "city_name"
                                        },
                                        Body = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "name" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "a"
                                            }
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Name = "a"
                                    },
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "portal_area"
                                    }
                                },
                                Where = new SqlBinaryExpression()
                                {
                                    Left = new SqlPropertyExpression()
                                    {
                                        Name = new SqlIdentifierExpression() { Name = "id" },
                                        Table = new SqlIdentifierExpression()
                                        {
                                            Name = "a"
                                        }
                                    },
                                    Operator = SqlBinaryOperator.EqualTo,
                                    Right = new SqlPropertyExpression()
                                    {
                                        Name = new SqlIdentifierExpression() { Name = "city_id" },
                                        Table = new SqlIdentifierExpression()
                                        {
                                            Name = "c"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new SqlSelectItemExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "area_name"
                        },
                        Body = new SqlCaseExpression()
                        {
                            Items = new List<SqlCaseItemExpression>()
                            {
                                new SqlCaseItemExpression()
                                {
                                    Condition = new SqlBinaryExpression()
                                    {
                                        Left = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "area_id" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "c"
                                            }
                                        },
                                        Operator = SqlBinaryOperator.Is,
                                        Right = new SqlNullExpression()
                                    },
                                    Value = new SqlNullExpression()
                                }
                            },
                            Else = new SqlSelectExpression()
                            {
                                Query = new SqlSelectQueryExpression()
                                {
                                    Columns = new List<SqlSelectItemExpression>()
                                    {
                                        new SqlSelectItemExpression()
                                        {
                                            Alias = new SqlIdentifierExpression()
                                            {
                                                Name = "area_name"
                                            },
                                            Body = new SqlPropertyExpression()
                                            {
                                                Name = new SqlIdentifierExpression() { Name = "name" },
                                                Table = new SqlIdentifierExpression()
                                                {
                                                    Name = "a"
                                                }
                                            }
                                        }
                                    },
                                    From = new SqlTableExpression()
                                    {
                                        Alias = new SqlIdentifierExpression()
                                        {
                                            Name = "a"
                                        },
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Name = "portal_area"
                                        }
                                    },
                                    Where = new SqlBinaryExpression()
                                    {
                                        Left = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "id" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "a"
                                            }
                                        },
                                        Operator = SqlBinaryOperator.EqualTo,
                                        Right = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression() { Name = "area_id" },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Name = "c"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Name = "c"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Name = "portal.portal_company"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Name = "no"
                    },
                    Right = new SqlVariableExpression()
                    {
                        Prefix = ":",
                        Name = "a"
                    },
                    Operator = SqlBinaryOperator.EqualTo
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestWithSubQuery()
    {
        var sql =
            "with c1(name) as (select name from test t) , c2(name) AS (SELECT name FROM test3 t3 ) select *from c1 JOIN c2 ON c1.name=c2.name";
        var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                WithSubQuery = new List<SqlWithSubQueryExpression>()
                {
                    new SqlWithSubQueryExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "c1"
                        },
                        Columns = new List<SqlIdentifierExpression>()
                        {
                            new SqlIdentifierExpression()
                            {
                                Name = "name"
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
                                        Name = "test"
                                    }
                                }
                            }
                        }
                    },
                    new SqlWithSubQueryExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Name = "c2"
                        },
                        Columns = new List<SqlIdentifierExpression>()
                        {
                            new SqlIdentifierExpression()
                            {
                                Name = "name"
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
                                        Name = "t3"
                                    },
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Name = "test3"
                                    }
                                }
                            }
                        }
                    }
                },
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    }
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "c1"
                        }
                    },
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Name = "c2"
                        }
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "name" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "c1"
                            }
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Name = "name" },
                            Table = new SqlIdentifierExpression()
                            {
                                Name = "c2"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestNoFrom()
    {
        var sql = "SELECT DATEDIFF(day, '2023-01-01', '2023-01-10')";
        var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlFunctionCallExpression()
                        {
                            Name = "DATEDIFF",
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlIdentifierExpression()
                                {
                                    Name = "day"
                                },
                                new SqlStringExpression()
                                {
                                    Value = "2023-01-01"
                                },
                                new SqlStringExpression()
                                {
                                    Value = "2023-01-10"
                                }
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }
}