using System.Collections.Generic;
using System.Data;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Test;

public class SelectTest
{
    private ITestOutputHelper testOutputHelper;

    public SelectTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TestSelectAll()
    {
        var sql = "select * from RouteData";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                        Value = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from RouteData", generationSql);
    }

    [Fact]
    public void TestReferenceTable()
    {
        var sql = "SELECT * FROM TABLE(splitstr('a;b',';')) ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                From = new SqlReferenceTableExpression()
                {
                    FunctionCall = new SqlFunctionCallExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "TABLE",
                        },
                        Arguments = new List<SqlExpression>()
                        {
                            new SqlFunctionCallExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "splitstr",
                                },
                                Arguments = new List<SqlExpression>()
                                {
                                    new SqlStringExpression()
                                    {
                                        Value = "a;b"
                                    },
                                    new SqlStringExpression()
                                    {
                                        Value = ";"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TABLE(splitstr('a;b', ';'))", generationSql);
    }

    [Fact]
    public void TestIdentifierColumn()
    {
        var sql = "select Id from RouteData";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Value = "Id"
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select Id from RouteData", generationSql);
    }

    [Fact]
    public void TestPropertyColumn()
    {
        var sql = "select rd.name as bname from RouteData rd";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Value = "bname"
                        },
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "name"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "rd"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "rd"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select rd.name as bname from RouteData as rd", generationSql);
    }

    [Fact]
    public void TestEquationColumn()
    {
        var sql = "select  'a' is not null =true";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlStringExpression()
                                {
                                    Value = "a"
                                },
                                Operator = SqlBinaryOperator.IsNot,
                                Right = new SqlNullExpression()
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlBoolExpression()
                            {
                                Value = true
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Pgsql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select(('a' is not null) = true )", generationSql);
    }

    [Fact]
    public void TestStringColumn()
    {
        var sql = "select ''' ''' from RouteData";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                        Value = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select ''' ''' from RouteData", generationSql);
    }

    [Fact]
    public void TestStringColumn2()
    {
        var sql = "select '2'' ''2' from RouteData";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select '2'' ''2' from RouteData", generationSql);
    }

    [Fact]
    public void TestStringColumn3ForUnicode()
    {
        var sql = "select N'2'' ''2' from RouteData";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            IsUniCode = true,
                            Value = "2' '2"
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData",
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select N'2'' ''2' from RouteData", generationSql);
    }

    [Fact]
    public void TestNumberColumn()
    {
        var sql = "select 5.20 from RouteData";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                        Value = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select 5.20 from RouteData", generationSql);
    }

    [Fact]
    public void TestFunctionCall()
    {
        var sql = "select LOWER(fa.Id)  from FlowActivity fa";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "LOWER",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression() { Value = "Id" },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "fa"
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
                        Value = "fa"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select LOWER(fa.Id) from FlowActivity as fa", generationSql);
    }

    [Fact]
    public void TestFunctionCall2()
    {
        var sql = "select * from TEST5  WHERE NAME = (lower(UPPER('a')))";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "NAME"
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlFunctionCallExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "lower"
                        },
                        Arguments = new List<SqlExpression>()
                        {
                            new SqlFunctionCallExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "UPPER"
                                },
                                Arguments = new List<SqlExpression>()
                                {
                                    new SqlStringExpression()
                                    {
                                        Value = "a"
                                    },
                                },
                            }
                        },
                    }
                },
            }
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST5 where(NAME = lower(UPPER('a')))", generationSql);
    }

    [Fact]
    public void TestFunctionCall3ForCaseAs1()
    {
        var sql = "SELECT CAST('123' AS UNSIGNED INTEGER);";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "CAST",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "123"
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "UNSIGNED INTEGER",
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select CAST('123' as UNSIGNED INTEGER)", generationSql);
    }

    [Fact]
    public void TestFunctionCall3ForCaseAs2()
    {
        var sql = "SELECT CAST('123' AS INT)";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "CAST",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "123"
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "INT",
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select CAST('123' as INT)", generationSql);
    }

    [Fact]
    public void TestFunctionCall3ForCaseAs3()
    {
        var sql = "SELECT CAST('1' as bit varying);";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "CAST",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "1"
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "bit varying",
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select CAST('1' as bit varying)", generationSql);
    }

    [Fact]
    public void TestFunctionCall3ForCaseAs4()
    {
        var sql = "SELECT CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS DATE) AS utc_date FROM dual";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "CAST",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlFunctionCallExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "SYS_EXTRACT_UTC",
                                    },
                                    Arguments = new List<SqlExpression>()
                                    {
                                        new SqlIdentifierExpression()
                                        {
                                            Value = "SYSTIMESTAMP",
                                        },
                                    },
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "DATE",
                            },
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "utc_date",
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "dual",
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) as DATE) as utc_date from dual", generationSql);
    }

    [Fact]
    public void TestFunctionCall3ForCaseAs5()
    {
        var sql = "SELECT '2023-10-15'::TIMESTAMP;";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "cast",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "2023-10-15"
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "TIMESTAMP",
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select cast('2023-10-15' as TIMESTAMP)", generationSql);
    }

    [Fact]
    public void TestFunctionCall3ForCaseAs6()
    {
        var sql = "SELECT '12.34'::float8::numeric::money;";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "cast",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlFunctionCallExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "cast",
                                    },
                                    Arguments = new List<SqlExpression>()
                                    {
                                        new SqlFunctionCallExpression()
                                        {
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "cast",
                                            },
                                            Arguments = new List<SqlExpression>()
                                            {
                                                new SqlStringExpression()
                                                {
                                                    Value = "12.34"
                                                },
                                            },
                                            CaseAsTargetType = new SqlIdentifierExpression()
                                            {
                                                Value = "float8",
                                            },
                                        },
                                    },
                                    CaseAsTargetType = new SqlIdentifierExpression()
                                    {
                                        Value = "numeric",
                                    },
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "money",
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select cast(cast(cast('12.34' as float8) as numeric) as money)", generationSql);
    }

    [Fact]
    public void TestFunctionCall3ForCaseAs7()
    {
        var sql =
            @"SELECT case '1'||'01'::bit varying :: varchar ='1' when '1'='1'||'01'::bit varying :: varchar  then '1' ='1'||'01'::bit varying :: varchar  end  from test a join test2 b 
on  '1'='1'||'01'::bit varying :: varchar  and '2'='1'||'01'::bit varying :: varchar  or '1'||'01'::bit varying :: varchar ='1'
left join test3 on '1'='1'||'01'::bit varying :: varchar
right join test4 on '1'='1'||'01'::bit varying :: varchar
full join test5 on '1'='1'||'01'::bit varying :: varchar
 where '2'='1'||'01'::bit varying :: varchar  group by '1'||'01'::bit varying :: varchar order by '1'||'01'::bit varying :: varchar limit 1::smallint ::float8 offset 1";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlCaseExpression()
                {
                    Items = new List<SqlCaseItemExpression>()
                    {
                        new SqlCaseItemExpression()
                        {
                            Condition = new SqlBinaryExpression()
                            {
                                Left = new SqlStringExpression()
                                {
                                    Value = "1"
                                },
                                Operator = SqlBinaryOperator.EqualTo,
                                Right = new SqlBinaryExpression()
                                {
                                    Left = new SqlStringExpression()
                                    {
                                        Value = "1"
                                    },
                                    Operator = SqlBinaryOperator.Concat,
                                    Right = new SqlFunctionCallExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "cast",
                                        },
                                        Arguments = new List<SqlExpression>()
                                        {
                                            new SqlFunctionCallExpression()
                                            {
                                                Name = new SqlIdentifierExpression()
                                                {
                                                    Value = "cast",
                                                },
                                                Arguments = new List<SqlExpression>()
                                                {
                                                    new SqlStringExpression()
                                                    {
                                                        Value = "01"
                                                    },
                                                },
                                                CaseAsTargetType = new SqlIdentifierExpression()
                                                {
                                                    Value = "bit varying",
                                                },
                                            },
                                        },
                                        CaseAsTargetType = new SqlIdentifierExpression()
                                        {
                                            Value = "varchar",
                                        },
                                    },
                                },
                            },
                            Value = new SqlBinaryExpression()
                            {
                                Left = new SqlStringExpression()
                                {
                                    Value = "1"
                                },
                                Operator = SqlBinaryOperator.EqualTo,
                                Right = new SqlBinaryExpression()
                                {
                                    Left = new SqlStringExpression()
                                    {
                                        Value = "1"
                                    },
                                    Operator = SqlBinaryOperator.Concat,
                                    Right = new SqlFunctionCallExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "cast",
                                        },
                                        Arguments = new List<SqlExpression>()
                                        {
                                            new SqlFunctionCallExpression()
                                            {
                                                Name = new SqlIdentifierExpression()
                                                {
                                                    Value = "cast",
                                                },
                                                Arguments = new List<SqlExpression>()
                                                {
                                                    new SqlStringExpression()
                                                    {
                                                        Value = "01"
                                                    },
                                                },
                                                CaseAsTargetType = new SqlIdentifierExpression()
                                                {
                                                    Value = "bit varying",
                                                },
                                            },
                                        },
                                        CaseAsTargetType = new SqlIdentifierExpression()
                                        {
                                            Value = "varchar",
                                        },
                                    },
                                },
                            },
                        },
                    },
                    Value = new SqlBinaryExpression()
                    {
                        Left = new SqlBinaryExpression()
                        {
                            Left = new SqlStringExpression()
                            {
                                Value = "1"
                            },
                            Operator = SqlBinaryOperator.Concat,
                            Right = new SqlFunctionCallExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "cast",
                                },
                                Arguments = new List<SqlExpression>()
                                {
                                    new SqlFunctionCallExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "cast",
                                        },
                                        Arguments = new List<SqlExpression>()
                                        {
                                            new SqlStringExpression()
                                            {
                                                Value = "01"
                                            },
                                        },
                                        CaseAsTargetType = new SqlIdentifierExpression()
                                        {
                                            Value = "bit varying",
                                        },
                                    },
                                },
                                CaseAsTargetType = new SqlIdentifierExpression()
                                {
                                    Value = "varchar",
                                },
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlStringExpression()
                        {
                            Value = "1"
                        },
                    },
                },
            },
        },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlJoinTableExpression()
                    {
                        Left = new SqlJoinTableExpression()
                        {
                            Left = new SqlJoinTableExpression()
                            {
                                Left = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "test",
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
                                        Value = "test2",
                                    },
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Value = "b",
                                    },
                                },
                                Conditions = new SqlBinaryExpression()
                                {
                                    Left = new SqlBinaryExpression()
                                    {
                                        Left = new SqlBinaryExpression()
                                        {
                                            Left = new SqlStringExpression()
                                            {
                                                Value = "1"
                                            },
                                            Operator = SqlBinaryOperator.EqualTo,
                                            Right = new SqlBinaryExpression()
                                            {
                                                Left = new SqlStringExpression()
                                                {
                                                    Value = "1"
                                                },
                                                Operator = SqlBinaryOperator.Concat,
                                                Right = new SqlFunctionCallExpression()
                                                {
                                                    Name = new SqlIdentifierExpression()
                                                    {
                                                        Value = "cast",
                                                    },
                                                    Arguments = new List<SqlExpression>()
                                            {
                                                new SqlFunctionCallExpression()
                                                {
                                                    Name = new SqlIdentifierExpression()
                                                    {
                                                        Value = "cast",
                                                    },
                                                    Arguments = new List<SqlExpression>()
                                                    {
                                                        new SqlStringExpression()
                                                        {
                                                            Value = "01"
                                                        },
                                                    },
                                                    CaseAsTargetType = new SqlIdentifierExpression()
                                                    {
                                                        Value = "bit varying",
                                                    },
                                                },
                                            },
                                                    CaseAsTargetType = new SqlIdentifierExpression()
                                                    {
                                                        Value = "varchar",
                                                    },
                                                },
                                            },
                                        },
                                        Operator = SqlBinaryOperator.And,
                                        Right = new SqlBinaryExpression()
                                        {
                                            Left = new SqlStringExpression()
                                            {
                                                Value = "2"
                                            },
                                            Operator = SqlBinaryOperator.EqualTo,
                                            Right = new SqlBinaryExpression()
                                            {
                                                Left = new SqlStringExpression()
                                                {
                                                    Value = "1"
                                                },
                                                Operator = SqlBinaryOperator.Concat,
                                                Right = new SqlFunctionCallExpression()
                                                {
                                                    Name = new SqlIdentifierExpression()
                                                    {
                                                        Value = "cast",
                                                    },
                                                    Arguments = new List<SqlExpression>()
                                            {
                                                new SqlFunctionCallExpression()
                                                {
                                                    Name = new SqlIdentifierExpression()
                                                    {
                                                        Value = "cast",
                                                    },
                                                    Arguments = new List<SqlExpression>()
                                                    {
                                                        new SqlStringExpression()
                                                        {
                                                            Value = "01"
                                                        },
                                                    },
                                                    CaseAsTargetType = new SqlIdentifierExpression()
                                                    {
                                                        Value = "bit varying",
                                                    },
                                                },
                                            },
                                                    CaseAsTargetType = new SqlIdentifierExpression()
                                                    {
                                                        Value = "varchar",
                                                    },
                                                },
                                            },
                                        },
                                    },
                                    Operator = SqlBinaryOperator.Or,
                                    Right = new SqlBinaryExpression()
                                    {
                                        Left = new SqlBinaryExpression()
                                        {
                                            Left = new SqlStringExpression()
                                            {
                                                Value = "1"
                                            },
                                            Operator = SqlBinaryOperator.Concat,
                                            Right = new SqlFunctionCallExpression()
                                            {
                                                Name = new SqlIdentifierExpression()
                                                {
                                                    Value = "cast",
                                                },
                                                Arguments = new List<SqlExpression>()
                                        {
                                            new SqlFunctionCallExpression()
                                            {
                                                Name = new SqlIdentifierExpression()
                                                {
                                                    Value = "cast",
                                                },
                                                Arguments = new List<SqlExpression>()
                                                {
                                                    new SqlStringExpression()
                                                    {
                                                        Value = "01"
                                                    },
                                                },
                                                CaseAsTargetType = new SqlIdentifierExpression()
                                                {
                                                    Value = "bit varying",
                                                },
                                            },
                                        },
                                                CaseAsTargetType = new SqlIdentifierExpression()
                                                {
                                                    Value = "varchar",
                                                },
                                            },
                                        },
                                        Operator = SqlBinaryOperator.EqualTo,
                                        Right = new SqlStringExpression()
                                        {
                                            Value = "1"
                                        },
                                    },
                                },
                            },
                            JoinType = SqlJoinType.LeftJoin,
                            Right = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "test3",
                                },
                            },
                            Conditions = new SqlBinaryExpression()
                            {
                                Left = new SqlStringExpression()
                                {
                                    Value = "1"
                                },
                                Operator = SqlBinaryOperator.EqualTo,
                                Right = new SqlBinaryExpression()
                                {
                                    Left = new SqlStringExpression()
                                    {
                                        Value = "1"
                                    },
                                    Operator = SqlBinaryOperator.Concat,
                                    Right = new SqlFunctionCallExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "cast",
                                        },
                                        Arguments = new List<SqlExpression>()
                                {
                                    new SqlFunctionCallExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "cast",
                                        },
                                        Arguments = new List<SqlExpression>()
                                        {
                                            new SqlStringExpression()
                                            {
                                                Value = "01"
                                            },
                                        },
                                        CaseAsTargetType = new SqlIdentifierExpression()
                                        {
                                            Value = "bit varying",
                                        },
                                    },
                                },
                                        CaseAsTargetType = new SqlIdentifierExpression()
                                        {
                                            Value = "varchar",
                                        },
                                    },
                                },
                            },
                        },
                        JoinType = SqlJoinType.RightJoin,
                        Right = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "test4",
                            },
                        },
                        Conditions = new SqlBinaryExpression()
                        {
                            Left = new SqlStringExpression()
                            {
                                Value = "1"
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlBinaryExpression()
                            {
                                Left = new SqlStringExpression()
                                {
                                    Value = "1"
                                },
                                Operator = SqlBinaryOperator.Concat,
                                Right = new SqlFunctionCallExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "cast",
                                    },
                                    Arguments = new List<SqlExpression>()
                            {
                                new SqlFunctionCallExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "cast",
                                    },
                                    Arguments = new List<SqlExpression>()
                                    {
                                        new SqlStringExpression()
                                        {
                                            Value = "01"
                                        },
                                    },
                                    CaseAsTargetType = new SqlIdentifierExpression()
                                    {
                                        Value = "bit varying",
                                    },
                                },
                            },
                                    CaseAsTargetType = new SqlIdentifierExpression()
                                    {
                                        Value = "varchar",
                                    },
                                },
                            },
                        },
                    },
                    JoinType = SqlJoinType.FullJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "test5",
                        },
                    },
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlStringExpression()
                        {
                            Value = "1"
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlBinaryExpression()
                        {
                            Left = new SqlStringExpression()
                            {
                                Value = "1"
                            },
                            Operator = SqlBinaryOperator.Concat,
                            Right = new SqlFunctionCallExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "cast",
                                },
                                Arguments = new List<SqlExpression>()
                        {
                            new SqlFunctionCallExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "cast",
                                },
                                Arguments = new List<SqlExpression>()
                                {
                                    new SqlStringExpression()
                                    {
                                        Value = "01"
                                    },
                                },
                                CaseAsTargetType = new SqlIdentifierExpression()
                                {
                                    Value = "bit varying",
                                },
                            },
                        },
                                CaseAsTargetType = new SqlIdentifierExpression()
                                {
                                    Value = "varchar",
                                },
                            },
                        },
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlStringExpression()
                    {
                        Value = "2"
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlBinaryExpression()
                    {
                        Left = new SqlStringExpression()
                        {
                            Value = "1"
                        },
                        Operator = SqlBinaryOperator.Concat,
                        Right = new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "cast",
                            },
                            Arguments = new List<SqlExpression>()
                    {
                        new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "cast",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "01"
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "bit varying",
                            },
                        },
                    },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "varchar",
                            },
                        },
                    },
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
            {
                new SqlOrderByItemExpression()
                {
                    Body = new SqlBinaryExpression()
                    {
                        Left = new SqlStringExpression()
                        {
                            Value = "1"
                        },
                        Operator = SqlBinaryOperator.Concat,
                        Right = new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "cast",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlFunctionCallExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "cast",
                                    },
                                    Arguments = new List<SqlExpression>()
                                    {
                                        new SqlStringExpression()
                                        {
                                            Value = "01"
                                        },
                                    },
                                    CaseAsTargetType = new SqlIdentifierExpression()
                                    {
                                        Value = "bit varying",
                                    },
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "varchar",
                            },
                        },
                    },
                },
            },
                },
                GroupBy = new SqlGroupByExpression()
                {
                    Items = new List<SqlExpression>()
            {
            new SqlBinaryExpression()
            {
                Left = new SqlStringExpression()
                {
                    Value = "1"
                },
                Operator = SqlBinaryOperator.Concat,
                Right = new SqlFunctionCallExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "cast",
                    },
                    Arguments = new List<SqlExpression>()
                    {
                        new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "cast",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "01"
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "bit varying",
                            },
                        },
                    },
                    CaseAsTargetType = new SqlIdentifierExpression()
                    {
                        Value = "varchar",
                    },
                },
            },
            },
                },
                Limit = new SqlLimitExpression()
                {
                    Offset = new SqlNumberExpression()
                    {
                        Value = 1M,
                    },
                    RowCount = new SqlFunctionCallExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "cast",
                        },
                        Arguments = new List<SqlExpression>()
                {
                    new SqlFunctionCallExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "cast",
                        },
                        Arguments = new List<SqlExpression>()
                        {
                            new SqlNumberExpression()
                            {
                                Value = 1M,
                            },
                        },
                        CaseAsTargetType = new SqlIdentifierExpression()
                        {
                            Value = "smallint",
                        },
                    },
                },
                        CaseAsTargetType = new SqlIdentifierExpression()
                        {
                            Value = "float8",
                        },
                    },
                },
            },
        };



        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Pgsql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select case(('1' || cast(cast('01' as bit varying) as varchar)) = '1') when('1' =('1' || cast(cast('01' as bit varying) as varchar))) then('1' =('1' || cast(cast('01' as bit varying) as varchar))) end from test as a inner join test2 as b on((('1' =('1' || cast(cast('01' as bit varying) as varchar))) and('2' =('1' || cast(cast('01' as bit varying) as varchar)))) or(('1' || cast(cast('01' as bit varying) as varchar)) = '1')) left join test3 on('1' =('1' || cast(cast('01' as bit varying) as varchar))) right join test4 on('1' =('1' || cast(cast('01' as bit varying) as varchar))) full join test5 on('1' =('1' || cast(cast('01' as bit varying) as varchar))) where('2' =('1' || cast(cast('01' as bit varying) as varchar))) group by('1' || cast(cast('01' as bit varying) as varchar)) order by('1' || cast(cast('01' as bit varying) as varchar)) limit cast(cast(1 as smallint) as float8) offset 1", generationSql);
    }

    [Fact]
    public void TestFunctionCall4()
    {
        var sql = "select DBMS_LOB.GETLENGTH(NAME) from TEST5";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "DBMS_LOB.GETLENGTH",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlIdentifierExpression()
                                {
                                    Value = "NAME",
                                },
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5",
                    },
                },
            },
        };



        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select DBMS_LOB.GETLENGTH(NAME) from TEST5", generationSql);
    }

    [Fact]
    public void TestComplexColumn()
    {
        var sql = "select rd.Active*2+5  from RouteData rd";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "Active"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "rd"
                                    },
                                },
                                Operator = SqlBinaryOperator.Multiply,
                                Right = new SqlNumberExpression()
                                {
                                    Value = 2
                                },
                            },
                            Operator = SqlBinaryOperator.Add,
                            Right = new SqlNumberExpression()
                            {
                                Value = 5
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "rd"
                    },
                },
            }
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select((rd.Active * 2) + 5) from RouteData as rd", generationSql);
    }

    [Fact]
    public void TestSelectSinpleMultiColumns()
    {
        var sql = "select Id,Value,Active from RouteData";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Value = "Id"
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "Value"
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "Active"
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    },
                },
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select Id, Value, Active from RouteData", generationSql);
    }

    [Theory]
    [InlineData(new object[] { "select * from RouteData a" })]
    [InlineData(new object[] { "select * from RouteData as a" })]
    public void TestTableAlias(string sql)
    {
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "a"
                    },
                },
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from RouteData as a", generationSql);
    }

    [Theory]
    [InlineData(new object[] { "select Id as bid from RouteData as a" })]
    [InlineData(new object[] { "select Id bid  from RouteData a" })]
    public void TestIdentifierColumnAndAs(string sql)
    {
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Value = "Id"
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "bid"
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "a"
                    },
                },
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select Id as bid from RouteData as a", generationSql);
    }

    [Fact]
    public void TestSubQuery()
    {
        var sql = "select * from (select * from RouteData) a";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlSelectExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "a"
                    },
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlAllColumnExpression()
                            },
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "RouteData"
                            },
                        },
                    },
                }
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from(select * from RouteData) as a", generationSql);
    }

    [Fact]
    public void TestSubQuery2()
    {
        var sql = "select * from (SELECT * FROM TEST t3) ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                                Value = "t3"
                            },
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "TEST"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from(select * from TEST t3)", generationSql);
    }

    [Fact]
    public void TestWhere()
    {
        var sql = "select * from RouteData rd where id='805B9CFC-1671-4BD8-B011-003EB7398FB0'";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "rd"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "id"
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "805B9CFC-1671-4BD8-B011-003EB7398FB0"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from RouteData as rd where(id = '805B9CFC-1671-4BD8-B011-003EB7398FB0')", generationSql);
    }

    [Fact]
    public void TestWhere2()
    {
        var sql = "SELECT * FROM customer t3 where t3.Name =(select city from address a)";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "customer"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t3"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Name"
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t3"
                        },
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
                                        Value = "city"
                                    },
                                },
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "address"
                                },
                                Alias = new SqlIdentifierExpression()
                                {
                                    Value = "a"
                                },
                            },
                        },
                    }
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from customer as t3 where(t3.Name =(select city from address as a))", generationSql);
    }

    [Fact]
    public void TestBetweenAnd()
    {
        var sql = "select * from FlowActivity fa where fa.CreateOn BETWEEN '2024-01-01' and '2024-10-10'";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
                Where = new SqlBetweenAndExpression()
                {
                    Body = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "CreateOn"
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "fa"
                        },
                    },
                    Begin = new SqlStringExpression()
                    {
                        Value = "2024-01-01"
                    },
                    End = new SqlStringExpression()
                    {
                        Value = "2024-10-10"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from FlowActivity as fa where fa.CreateOn between '2024-01-01' and '2024-10-10'",
            generationSql);
    }

    [Fact]
    public void TestBetweenAnd2()
    {
        var sql =
            "select * from FlowActivity fa where fa.OnlyMainJobProcessing BETWEEN (0+0.5)*2 and 1 and fa.Active =1";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlBetweenAndExpression()
                    {
                        Body = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "OnlyMainJobProcessing"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                        Begin = new SqlBinaryExpression()
                        {
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlNumberExpression()
                                {
                                    Value = 0M
                                },
                                Operator = SqlBinaryOperator.Add,
                                Right = new SqlNumberExpression()
                                {
                                    Value = 0.5M
                                },
                            },
                            Operator = SqlBinaryOperator.Multiply,
                            Right = new SqlNumberExpression()
                            {
                                Value = 2M
                            },
                        },
                        End = new SqlNumberExpression()
                        {
                            Value = 1M
                        },
                    },
                    Operator = SqlBinaryOperator.And,
                    Right = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "Active"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlNumberExpression()
                        {
                            Value = 1M
                        },
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select * from FlowActivity as fa where(fa.OnlyMainJobProcessing between((0 + 0.5) * 2) and 1 and(fa.Active = 1))",
            generationSql);
    }

    [Fact]
    public void TestJoin()
    {
        var sql =
            "select * from Customer c join Address a on ((c.Id =a.CustomerId or c.Age>=a.CustomerId) and c.CustomerNo !='abc' )";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Customer"
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "c"
                        },
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Address"
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "a"
                        },
                    },
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlBinaryExpression()
                        {
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "Id"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "c"
                                    },
                                },
                                Operator = SqlBinaryOperator.EqualTo,
                                Right = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "CustomerId"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "a"
                                    },
                                },
                            },
                            Operator = SqlBinaryOperator.Or,
                            Right = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "Age"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "c"
                                    },
                                },
                                Operator = SqlBinaryOperator.GreaterThenOrEqualTo,
                                Right = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "CustomerId"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "a"
                                    },
                                },
                            },
                        },
                        Operator = SqlBinaryOperator.And,
                        Right = new SqlBinaryExpression()
                        {
                            Left = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "CustomerNo"
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "c"
                                },
                            },
                            Operator = SqlBinaryOperator.NotEqualTo,
                            Right = new SqlStringExpression()
                            {
                                Value = "abc"
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select * from Customer as c inner join Address as a on(((c.Id = a.CustomerId) or(c.Age >= a.CustomerId)) and(c.CustomerNo != 'abc'))",
            generationSql);
    }

    [Fact]
    public void TestJoin2()
    {
        var sql = "select * from (SELECT * FROM TEST t3)  t join (select * from test1 t) t2 on t.name=t2.test";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlSelectExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t"
                        },
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                            {
                                new SqlSelectItemExpression()
                                {
                                    Body = new SqlAllColumnExpression()
                                },
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "TEST"
                                },
                                Alias = new SqlIdentifierExpression()
                                {
                                    Value = "t3"
                                },
                            },
                        },
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlSelectExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t2"
                        },
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                            {
                                new SqlSelectItemExpression()
                                {
                                    Body = new SqlAllColumnExpression()
                                },
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "test1"
                                },
                                Alias = new SqlIdentifierExpression()
                                {
                                    Value = "t"
                                },
                            },
                        },
                    },
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "name"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "t"
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "test"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "t2"
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Pgsql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select * from(select * from TEST as t3) as t inner join(select * from test1 as t) as t2 on(t.name = t2.test)",
            generationSql);
    }

    [Fact]
    public void TestJoin3()
    {
        var sql =
            "select * from Customer  join Address  on 1=1";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Customer",
                        },
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Address",
                        },
                    },
                    Conditions = new SqlBinaryExpression()
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
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select * from Customer inner join Address on(1 = 1)",
            generationSql);
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
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                            Value = "c"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Customer"
                        }
                    },
                    Right = new SqlTableExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "a"
                        },
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Address"
                        }
                    },
                    JoinType = joinType,
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Value = "Id" },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "c"
                            }
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression() { Value = "CustomerId" },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "a"
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        var joinTypeName = "";
        switch (joinType)
        {
            case SqlJoinType.InnerJoin:
                joinTypeName = "inner";
                break;
            case SqlJoinType.FullJoin:
                joinTypeName = "full";
                break;
            case SqlJoinType.LeftJoin:
                joinTypeName = "left";
                break;
            case SqlJoinType.RightJoin:
                joinTypeName = "right";
                break;
        }

        Assert.Equal($"select * from Customer as c {joinTypeName} join Address as a on(c.Id = a.CustomerId)",
            generationSql);
    }

    [Fact]
    public void TestGroupBy()
    {
        var sql = "select fa.FlowId  from FlowActivity fa group by fa.FlowId,fa.Id ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);

        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
                GroupBy = new SqlGroupByExpression()
                {
                    Items = new List<SqlExpression>()
                    {
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "Id"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select fa.FlowId from FlowActivity as fa group by fa.FlowId, fa.Id", generationSql);
    }

    [Fact]
    public void TestGroupBy2()
    {
        var sql = "select * from (SELECT name FROM test5 GROUP BY NAME) t ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlSelectExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlIdentifierExpression()
                                {
                                    Value = "name"
                                },
                            },
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "test5"
                            },
                        },
                        GroupBy = new SqlGroupByExpression()
                        {
                            Items = new List<SqlExpression>()
                            {
                                new SqlIdentifierExpression()
                                {
                                    Value = "NAME"
                                },
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from(select name from test5 group by NAME) t", generationSql);
    }

    [Fact]
    public void TestGroupBy3()
    {
        var sql = "select * from (SELECT name FROM test5 GROUP BY (NAME),(1+2)) t ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "t"
                    },
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlIdentifierExpression() { Value = "name" },
                            }
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "test5"
                            }
                        },
                        GroupBy = new SqlGroupByExpression()
                        {
                            Items = new List<SqlExpression>()
                            {
                                new SqlIdentifierExpression()
                                {
                                    Value = "NAME"
                                },
                                new SqlBinaryExpression()
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
                                }
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from(select name from test5 group by NAME,(1 + 2)) t", generationSql);
    }


    [Fact]
    public void TestGroupByHaving()
    {
        var sql = "select fa.FlowId  from FlowActivity fa group by fa.FlowId,fa.Id HAVING count(fa.Id)>1";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
                GroupBy = new SqlGroupByExpression()
                {
                    Items = new List<SqlExpression>()
                    {
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "Id"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                    Having = new SqlBinaryExpression()
                    {
                        Left = new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "count"
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "Id"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "fa"
                                    },
                                },
                            },
                        },
                        Operator = SqlBinaryOperator.GreaterThen,
                        Right = new SqlNumberExpression()
                        {
                            Value = 1M
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select fa.FlowId from FlowActivity as fa group by fa.FlowId, fa.Id having(count(fa.Id) > 1)",
            generationSql);
    }

    [Fact]
    public void TestGroupByHaving2()
    {
        var sql = "select fa.FlowId  from FlowActivity fa group by fa.FlowId,fa.Id HAVING 1+2>3";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
                GroupBy = new SqlGroupByExpression()
                {
                    Items = new List<SqlExpression>()
                    {
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "Id"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                    Having = new SqlBinaryExpression()
                    {
                        Left = new SqlBinaryExpression()
                        {
                            Left = new SqlNumberExpression()
                            {
                                Value = 1M
                            },
                            Operator = SqlBinaryOperator.Add,
                            Right = new SqlNumberExpression()
                            {
                                Value = 2M
                            },
                        },
                        Operator = SqlBinaryOperator.GreaterThen,
                        Right = new SqlNumberExpression()
                        {
                            Value = 3M
                        },
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select fa.FlowId from FlowActivity as fa group by fa.FlowId, fa.Id having((1 + 2) > 3)",
            generationSql);
    }

    [Fact]
    public void TestOrderBy()
    {
        var sql = "select fa.FlowId  from FlowActivity fa order by fa.FlowId desc,fa.Id asc";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Body =
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "FlowId"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "fa"
                                    },
                                },
                            OrderByType = SqlOrderByType.Desc
                        },
                        new SqlOrderByItemExpression()
                        {
                            Body =
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "Id"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "fa"
                                    },
                                },
                            OrderByType = SqlOrderByType.Asc
                        },
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select fa.FlowId from FlowActivity as fa order by fa.FlowId desc, fa.Id asc", generationSql);
    }

    [Fact]
    public void TestOrderBy2()
    {
        var sql = "select * from (SELECT name FROM test5 order BY NAME) t ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlSelectExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlIdentifierExpression()
                                {
                                    Value = "name"
                                },
                            },
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "test5"
                            },
                        },
                        OrderBy = new SqlOrderByExpression()
                        {
                            Items = new List<SqlOrderByItemExpression>()
                            {
                                new SqlOrderByItemExpression()
                                {
                                    Body =
                                        new SqlIdentifierExpression()
                                        {
                                            Value = "NAME"
                                        },
                                },
                            },
                        },
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from(select name from test5 order by NAME) t", generationSql);
    }

    [Theory]
    [InlineData(new object[] { DbType.Oracle })]
    [InlineData(new object[] { DbType.Sqlite })]
    [InlineData(new object[] { DbType.Pgsql })]
    public void TestOrderBy3(DbType dbType)
    {
        var sql = "  select * from TEST5 t order by t.NAME  desc nulls FIRST,t.AGE ASC NULLS  last ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, dbType); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t",
                    },
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Body = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "NAME",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t",
                                },
                            },
                            OrderByType = SqlOrderByType.Desc,
                            NullsType = SqlOrderByNullsType.First,
                        },
                        new SqlOrderByItemExpression()
                        {
                            Body = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "AGE",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t",
                                },
                            },
                            OrderByType = SqlOrderByType.Asc,
                            NullsType = SqlOrderByNullsType.Last,
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(dbType);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        var expectGenerationSql = "select * from TEST5 as t order by t.NAME desc nulls first, t.AGE asc nulls last";
        if (dbType == DbType.Oracle)
        {
            expectGenerationSql = "select * from TEST5 t order by t.NAME desc nulls first, t.AGE asc nulls last";
        }

        Assert.Equal(expectGenerationSql, generationSql);
    }

    [Fact]
    public void TestOrderBy4()
    {
        var sql = @"SELECT name, age 
                    FROM customer 
                    ORDER BY 
                      IF(age IS NULL, 1, 0),
                      age DESC;";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Value = "name",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "age",
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "customer",
                    },
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Body = new SqlFunctionCallExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "IF",
                                },
                                Arguments = new List<SqlExpression>()
                                {
                                    new SqlBinaryExpression()
                                    {
                                        Left = new SqlIdentifierExpression()
                                        {
                                            Value = "age",
                                        },
                                        Operator = SqlBinaryOperator.Is,
                                        Right = new SqlNullExpression()
                                    },
                                    new SqlNumberExpression()
                                    {
                                        Value = 1M,
                                    },
                                    new SqlNumberExpression()
                                    {
                                        Value = 0M,
                                    },
                                },
                            },
                        },
                        new SqlOrderByItemExpression()
                        {
                            Body = new SqlIdentifierExpression()
                            {
                                Value = "age",
                            },
                            OrderByType = SqlOrderByType.Desc,
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select name, age from customer order by IF((age is null), 1, 0), age desc", generationSql);
    }

    [Fact]
    public void TestOrderBy5()
    {
        var sql = @"SELECT name, age
FROM customer
ORDER BY 
  CASE WHEN Name  IS NULL THEN 1 ELSE 0 END,
  age DESC;";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Value = "name",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "age",
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "customer",
                    },
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Body = new SqlCaseExpression()
                            {
                                Items = new List<SqlCaseItemExpression>()
                                {
                                    new SqlCaseItemExpression()
                                    {
                                        Condition = new SqlBinaryExpression()
                                        {
                                            Left = new SqlIdentifierExpression()
                                            {
                                                Value = "Name",
                                            },
                                            Operator = SqlBinaryOperator.Is,
                                            Right = new SqlNullExpression()
                                        },
                                        Value = new SqlNumberExpression()
                                        {
                                            Value = 1M,
                                        },
                                    },
                                },
                                Else = new SqlNumberExpression()
                                {
                                    Value = 0M,
                                },
                            },
                        },
                        new SqlOrderByItemExpression()
                        {
                            Body = new SqlIdentifierExpression()
                            {
                                Value = "age",
                            },
                            OrderByType = SqlOrderByType.Desc,
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select name, age from customer order by case when(Name is null) then 1 else 0 end, age desc",
            generationSql);
    }

    [Fact]
    public void TestOrderBy6()
    {
        var sql = @"SELECT name, age 
                    FROM customer 
                    ORDER BY 
                      (select name from test t),
                      age DESC;";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                    Value = "name",
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "age",
                },
            },
        },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "customer",
                    },
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
            {
                new SqlOrderByItemExpression()
                {
                    Body = new SqlSelectExpression()
                    {
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                            {
                                new SqlSelectItemExpression()
                                {
                                    Body = new SqlIdentifierExpression()
                                    {
                                        Value = "name",
                                    },
                                },
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "test",
                                },
                                Alias = new SqlIdentifierExpression()
                                {
                                    Value = "t",
                                },
                            },
                        },
                    },
                },
                new SqlOrderByItemExpression()
                {
                    Body = new SqlIdentifierExpression()
                    {
                        Value = "age",
                    },
                    OrderByType = SqlOrderByType.Desc,
                },
            },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select name, age from customer order by(select name from test as t), age desc", generationSql);
    }


    [Fact]
    public void TestDistinct()
    {
        var sql = "select DISTINCT  fa.FlowId  from FlowActivity fa";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                },
                ResultSetReturnOption = SqlResultSetReturnOption.Distinct,
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select distinct fa.FlowId from FlowActivity as fa", generationSql);
    }

    [Fact]
    public void TestDistinct2()
    {
        var sql = "select count(DISTINCT  fa.FlowId)  from FlowActivity fa";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "count"
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "FlowId"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "fa"
                                    },
                                },
                            },
                            IsDistinct = true,
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select count(distinct fa.FlowId) from FlowActivity as fa", generationSql);
    }

    [Fact]
    public void TestEscapeCharacterForSqlServer()
    {
        var sql = "select rd.[System] from RouteData rd where rd.[System] ='ats'";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() =>
        {
            sqlAst = DbUtils.Parse(sql, DbType.SqlServer, ((l, s) => { testOutputHelper.WriteLine(s + ":" + l); }));
        }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "System",
                                LeftQualifiers = "[",
                                RightQualifiers = "]",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "rd",
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "rd",
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "System",
                            LeftQualifiers = "[",
                            RightQualifiers = "]",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "rd",
                        },
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "ats"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select rd.[System] from RouteData as rd where(rd.[System] = 'ats')", generationSql);
    }

    [Fact]
    public void TestEscapeCharacterForMysql()
    {
        var sql = "select rd.`NAME` from `TEST` rd";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() =>
        {
            sqlAst = DbUtils.Parse(sql, DbType.MySql, ((l, s) => { testOutputHelper.WriteLine(s + ":" + l); }));
        }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "NAME",
                                LeftQualifiers = "`",
                                RightQualifiers = "`",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "rd",
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST",
                        LeftQualifiers = "`",
                        RightQualifiers = "`",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "rd",
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select rd.`NAME` from `TEST` as rd", generationSql);
    }

    [Fact]
    public void TestEscapeCharacterForOracle()
    {
        var sql = "select rd.\"NAME\" from \"TEST3\" rd ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() =>
        {
            sqlAst = DbUtils.Parse(sql, DbType.Oracle, ((l, s) => { testOutputHelper.WriteLine(s + ":" + l); }));
        }));
        testOutputHelper.WriteLine("time:" + t);

        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "NAME",
                                LeftQualifiers = "\"",
                                RightQualifiers = "\"",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "rd",
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST3",
                        LeftQualifiers = "\"",
                        RightQualifiers = "\"",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "rd",
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select rd.\"NAME\" from \"TEST3\" rd", generationSql);
    }


    [Fact]
    public void TestIsNull()
    {
        var sql = "select * from RouteData rd where rd.name is null";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "rd"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "name"
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "rd"
                        },
                    },
                    Operator = SqlBinaryOperator.Is,
                    Right = new SqlNullExpression()
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from RouteData as rd where(rd.name is null)", generationSql);
    }

    [Fact]
    public void TestIsNotNull()
    {
        var sql = "select * from RouteData rd where rd.name is not null";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "rd"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Value = "name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "rd"
                        }
                    },
                    Operator = SqlBinaryOperator.IsNot,
                    Right = new SqlNullExpression()
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from RouteData as rd where(rd.name is not null)", generationSql);
    }

    [Fact]
    public void TestExists()
    {
        var sql = "select * from TEST t where EXISTS(select * from TEST1 t2) OR 1=1";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlExistsExpression()
                    {
                        Body = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlAllColumnExpression()
                                    },
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "TEST1"
                                    },
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Value = "t2"
                                    },
                                },
                            },
                        },
                    },
                    Operator = SqlBinaryOperator.Or,
                    Right = new SqlBinaryExpression()
                    {
                        Left = new SqlNumberExpression()
                        {
                            Value = 1M
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlNumberExpression()
                        {
                            Value = 1M
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST t where(exists((select * from TEST1 t2)) or(1 = 1))", generationSql);
    }

    [Fact]
    public void TestLike()
    {
        var sql = "SELECT * from TEST t WHERE name LIKE '%a%'";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "t"
                    },

                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "name"
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

    [Fact]
    public void TestCommaJoin()
    {
        var sql = "select * from test t,test11 t1 where t.name =t1.name";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "test",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t",
                        },
                    },
                    JoinType = SqlJoinType.CommaJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "test11",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t1",
                        },
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "name",
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
                            Value = "name",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t1",
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from test t , test11 t1 where(t.name = t1.name)", generationSql);
    }

    [Fact]
    public void TestUnionQuery()
    {
        var sql = "select name from test union select name from test11 Except select name from test11";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                                        Value = "name"
                                    },
                                },
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "test"
                                },
                            },
                        },
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
                                        Value = "name"
                                    },
                                },
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "test11"
                                },
                            },
                        },
                    },
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
                                    Value = "name"
                                },
                            },
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "test11"
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("( ((select name from test) union(select name from test11) ) except(select name from test11) )",
            generationSql);
    }

    [Fact]
    public void TestUnionQuery2()
    {
        var sql = "((select name from test union all select name from test11) Except  select name from test11)";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                                            Value = "name"
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "test"
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
                                            Value = "name"
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "test11"
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
                                    Value = "name"
                                }
                            }
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "test11"
                            }
                        }
                    }
                }
            }
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "((((select name from test) union all(select name from test11) )) except(select name from test11) )",
            generationSql);
    }

    [Fact]
    public void TestUnionQuery3()
    {
        var sql = "(select name from test union (select name from test11 Except select name from test11))";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                                            Value = "name"
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "test11"
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
                                            Value = "name"
                                        }
                                    }
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "test11"
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
                                    Value = "name"
                                }
                            }
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "test"
                            }
                        }
                    }
                }
            }
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("((select name from test) union(((select name from test11) except(select name from test11) )) )",
            generationSql);
    }

    [Fact]
    public void TestAll()
    {
        var sql = "select all  fa.FlowId  from FlowActivity fa";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                },
                ResultSetReturnOption = SqlResultSetReturnOption.All,
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select all fa.FlowId from FlowActivity as fa", generationSql);
    }

    [Fact]
    public void TestAll2()
    {
        var sql = "select * from customer c where c.Age >all(select o.Quantity  from orderdetail o)";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "customer"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "c"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Age"
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "c"
                        },
                    },
                    Operator = SqlBinaryOperator.GreaterThen,
                    Right = new SqlAllExpression()
                    {
                        Body = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "Quantity"
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "o"
                                            },
                                        },
                                    },
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "orderdetail"
                                    },
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Value = "o"
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from customer as c where(c.Age > all((select o.Quantity from orderdetail as o)))",
            generationSql);
    }

    [Fact]
    public void TestAny()
    {
        var sql = "select * from customer c where c.Age >any(select o.Quantity  from orderdetail o)";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "customer"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "c"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Age"
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "c"
                        },
                    },
                    Operator = SqlBinaryOperator.GreaterThen,
                    Right = new SqlAnyExpression()
                    {
                        Body = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "Quantity"
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "o"
                                            },
                                        },
                                    },
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "orderdetail"
                                    },
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Value = "o"
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from customer as c where(c.Age > any((select o.Quantity from orderdetail as o)))",
            generationSql);
    }

    [Fact]
    public void TestUnique()
    {
        var sql = "SELECT  unique * from TEST t";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "t"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));


        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select unique * from TEST t", generationSql);
    }

    [Fact]
    public void TestIn()
    {
        var sql = "SELECT  * from TEST t WHERE t.NAME IN ('a','b','c')";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                },
                Where = new SqlInExpression()
                {
                    Body = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "NAME"
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t"
                        },
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
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST t where t.NAME in('a', 'b', 'c')", generationSql);
    }

    [Fact]
    public void TestIn2()
    {
        var sql = "SELECT * from TEST t JOIN TEST2 t2 ON t.NAME IN ('a') AND t.NAME =t2.NAME";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "TEST"
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t"
                        },
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "TEST2"
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t2"
                        },
                    },
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlInExpression()
                        {
                            Body = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "NAME"
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t"
                                },
                            },
                            TargetList = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "a"
                                },
                            },
                        },
                        Operator = SqlBinaryOperator.And,
                        Right = new SqlBinaryExpression()
                        {
                            Left = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "NAME"
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t"
                                },
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "NAME"
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t2"
                                },
                            },
                        },
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST t inner join TEST2 t2 on(t.NAME in('a') and(t.NAME = t2.NAME))",
            generationSql);
    }

    [Fact]
    public void TestIn3()
    {
        var sql = "select * from TEST5  WHERE \"Number\"  IN (1+2,3)";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5",
                    },
                },
                Where = new SqlInExpression()
                {
                    Body = new SqlIdentifierExpression()
                    {
                        Value = "Number",
                        LeftQualifiers = "\"",
                        RightQualifiers = "\"",
                    },
                    TargetList = new List<SqlExpression>()
                    {
                        new SqlBinaryExpression()
                        {
                            Left = new SqlNumberExpression()
                            {
                                Value = 1M,
                            },
                            Operator = SqlBinaryOperator.Add,
                            Right = new SqlNumberExpression()
                            {
                                Value = 2M,
                            },
                        },
                        new SqlNumberExpression()
                        {
                            Value = 3M,
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST5 where \"Number\" in((1 + 2), 3)", generationSql);
    }

    [Fact]
    public void TestIn4()
    {
        var sql = "select * from TEST5  WHERE NAME IN (lower('a'),'b')";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "TEST5"
                    }
                },
                Where = new SqlInExpression()
                {
                    Body = new SqlIdentifierExpression() { Value = "NAME" },
                    TargetList = new List<SqlExpression>()
                    {
                        new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "lower",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "a"
                                }
                            }
                        },
                        new SqlStringExpression()
                        {
                            Value = "b"
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST5 where NAME in(lower('a'), 'b')", generationSql);
    }

    [Fact]
    public void TestIn5()
    {
        var sql = "select * from TEST5  WHERE NAME IN (SELECT NAME  FROM TEST3)";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "TEST5"
                    }
                },
                Where = new SqlInExpression()
                {
                    Body = new SqlIdentifierExpression() { Value = "NAME" },
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
                                        Value = "NAME"
                                    }
                                }
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "TEST3"
                                }
                            }
                        }
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST5 where NAME in(select NAME from TEST3)", generationSql);
    }

    [Theory]
    [InlineData(new object[] { "as name", "name" })]
    [InlineData(new object[] { "", null })]
    public void TestCase(string asName, string name)
    {
        var sql =
            $"select case when t.name ='a' then '1' when t.name in ('b','c') then '2' else '3' end {asName} from test t join test5 t2 on case when t.name ='a' then '1' when t.name is null then '2'  end=t2.name  ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() =>
        {
            sqlAst = DbUtils.Parse(sql, DbType.Oracle, ((l, s) => { testOutputHelper.WriteLine(s + ":" + l); }));
        }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var alias = new SqlIdentifierExpression()
        {
            Value = name
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
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "name"
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "t"
                                            },
                                        },
                                        Operator = SqlBinaryOperator.EqualTo,
                                        Right = new SqlStringExpression()
                                        {
                                            Value = "a"
                                        },
                                    },
                                    Value = new SqlStringExpression()
                                    {
                                        Value = "1"
                                    },
                                },
                                new SqlCaseItemExpression()
                                {
                                    Condition = new SqlInExpression()
                                    {
                                        Body = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "name"
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "t"
                                            },
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
                                            },
                                        },
                                    },
                                    Value = new SqlStringExpression()
                                    {
                                        Value = "2"
                                    },
                                },
                            },
                            Else = new SqlStringExpression()
                            {
                                Value = "3"
                            },
                        },
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "test"
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t"
                        },
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "test5"
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t2"
                        },
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
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "name"
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "t"
                                            },
                                        },
                                        Operator = SqlBinaryOperator.EqualTo,
                                        Right = new SqlStringExpression()
                                        {
                                            Value = "a"
                                        },
                                    },
                                    Value = new SqlStringExpression()
                                    {
                                        Value = "1"
                                    },
                                },
                                new SqlCaseItemExpression()
                                {
                                    Condition = new SqlBinaryExpression()
                                    {
                                        Left = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "name"
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "t"
                                            },
                                        },
                                        Operator = SqlBinaryOperator.Is,
                                        Right = new SqlNullExpression()
                                    },
                                    Value = new SqlStringExpression()
                                    {
                                        Value = "2"
                                    },
                                },
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "name"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "t2"
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();


        if (string.IsNullOrWhiteSpace(name))
        {
            name = "";
        }
        else
        {
            name = "as " + name + " ";
        }

        Assert.Equal(
            $"select case when(t.name = 'a') then '1' when t.name in('b', 'c') then '2' else '3' end {name}from test t inner join test5 t2 on(case when(t.name = 'a') then '1' when(t.name is null) then '2' end = t2.name)",
            generationSql);
    }

    [Fact]
    public void TestCase2()
    {
        var sql =
            $"select case (SELECT name FROM test5 FETCH FIRST 1 ROWS only) when 'a' then 1 else 2 end  from test5 t ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() =>
        {
            sqlAst = DbUtils.Parse(sql, DbType.Oracle, ((l, s) => { testOutputHelper.WriteLine(s + ":" + l); }));
        }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlCaseExpression()
                        {
                            Items = new List<SqlCaseItemExpression>()
                            {
                                new SqlCaseItemExpression()
                                {
                                    Condition = new SqlStringExpression()
                                    {
                                        Value = "a"
                                    },
                                    Value = new SqlNumberExpression()
                                    {
                                        Value = 1M
                                    },
                                },
                            },
                            Else = new SqlNumberExpression()
                            {
                                Value = 2M
                            },
                            Value = new SqlSelectExpression()
                            {
                                Query = new SqlSelectQueryExpression()
                                {
                                    Columns = new List<SqlSelectItemExpression>()
                                    {
                                        new SqlSelectItemExpression()
                                        {
                                            Body = new SqlIdentifierExpression()
                                            {
                                                Value = "name"
                                            },
                                        },
                                    },
                                    From = new SqlTableExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "test5"
                                        },
                                    },
                                    Limit = new SqlLimitExpression()
                                    {
                                        RowCount = new SqlNumberExpression()
                                        {
                                            Value = 1M
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test5"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                },
            },
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select case(select name from test5 fetch first 1 rows only) when 'a' then 1 else 2 end from test5 t",
            generationSql);
    }

    [Fact]
    public void TestSelectInto()
    {
        var sql = "SELECT name into test14 from TEST as t ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Value = "name"
                        },
                    },
                },
                Into = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test14"
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select name into test14 from TEST as t", generationSql);
    }

    [Fact]
    public void TestLineBreak()
    {
        var sql = @"select 
                    * 
                    from 
                    test";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "test"
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
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "test"
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
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "test"
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
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "t"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
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
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "test"
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

        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "test"
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

        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "t"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Value = "name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t"
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
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() =>
        {
            sqlAst = DbUtils.Parse(sql, DbType.MySql, ((l, s) => { testOutputHelper.WriteLine(s + ":" + l); }));
        }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        SqlLimitExpression limit = null;
        var expectSql = "";
        if (limitString == " limit 1,5")
        {
            expectSql = "select * from customer as a where(a.name != '''123') order by a.Id limit 1, 5";
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
        }
        else if (limitString == " limit 1")
        {
            expectSql = "select * from customer as a where(a.name != '''123') order by a.Id limit 1";
            limit = new SqlLimitExpression()
            {
                RowCount = new SqlNumberExpression()
                {
                    Value = 1
                }
            };
        }

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "customer"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "a"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "name"
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "a"
                        },
                    },
                    Operator = SqlBinaryOperator.NotEqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "'123"
                    },
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Body =
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "Id"
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "a"
                                    },
                                },
                        },
                    },
                },
                Limit = limit
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(expectSql, generationSql);
    }

    [Fact]
    public void TestLimitForSqlServer()
    {
        var sql = "select * from test t order by t.name OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "t"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test"
                    }
                },

                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Body = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Value = "name" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t"
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

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from test as t order by t.name OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY", generationSql);
    }

    [Fact]
    public void TestLimitForOracle()
    {
        var sql = "SELECT * FROM TEST3 t  ORDER BY t.NAME  DESC FETCH FIRST 2 rows ONLY";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                        Value = "t"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST3"
                    }
                },

                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            OrderByType = SqlOrderByType.Desc,
                            Body = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Value = "NAME" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t"
                                }
                            }
                        }
                    }
                },
                Limit = new SqlLimitExpression()
                {
                    RowCount = new SqlNumberExpression()
                    {
                        Value = 2
                    }
                }
            }
        };
        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST3 t order by t.NAME desc fetch first 2 rows only", generationSql);
    }

    [Theory]
    [InlineData(new object[] { " limit 1 offset 10" })]
    [InlineData(new object[] { " limit 1 " })]
    [InlineData(new object[] { " offset 10" })]
    public void TestLimitForPgsql(string limitString)
    {
        var sql = $"select * from test t where t.test !='abc' order by t.test {limitString}";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        SqlLimitExpression limit = null;
        var expectSql = "";
        if (limitString == " limit 1 offset 10")
        {
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
            expectSql = "select * from test as t where(t.test != 'abc') order by t.test limit 1 offset 10";
        }
        else if (limitString == " limit 1 ")
        {
            limit = new SqlLimitExpression()
            {
                RowCount = new SqlNumberExpression()
                {
                    Value = 1
                }
            };
            expectSql = "select * from test as t where(t.test != 'abc') order by t.test limit 1";
        }
        else if (limitString == " offset 10")
        {
            limit = new SqlLimitExpression()
            {
                Offset = new SqlNumberExpression()
                {
                    Value = 10
                }
            };
            expectSql = "select * from test as t where(t.test != 'abc') order by t.test offset 10";
        }


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
                        Value = "t"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test"
                    }
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Value = "test" },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t"
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
                            Body = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression() { Value = "test" },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t"
                                }
                            }
                        }
                    }
                },
                Limit = limit
            }
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Pgsql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(expectSql, generationSql);
    }

    [Theory]
    [InlineData(new object[] { "PARTITION BY NAME ,ID " })]
    [InlineData(new object[] { "" })]
    public void TestWindowFunctionCall(string partitionByString)
    {
        var sql = $"SELECT t.*, ROW_NUMBER() OVER ( {partitionByString} ORDER BY t.NAME,t.ID) as rnum FROM TEST t";

        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        SqlPartitionByExpression partitionBy = null;
        var expectSql = "";
        if (partitionByString == "PARTITION BY NAME ,ID ")
        {
            partitionBy = new SqlPartitionByExpression()
            {
                Items = new List<SqlExpression>()
                {
                    new SqlIdentifierExpression()
                    {
                        Value = "NAME"
                    },
                    new SqlIdentifierExpression()
                    {
                        Value = "ID"
                    }
                }
            };
            expectSql =
                "select t.*, ROW_NUMBER() over(partition by NAME, ID order by t.NAME, t.ID) as rnum from TEST as t";
        }
        else
        {
            expectSql = "select t.*, ROW_NUMBER() over(order by t.NAME, t.ID) as rnum from TEST as t";
        }

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "*"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "t"
                            },
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "ROW_NUMBER"
                            },
                            Over = new SqlOverExpression()
                            {
                                PartitionBy = partitionBy,
                                OrderBy = new SqlOrderByExpression()
                                {
                                    Items = new List<SqlOrderByItemExpression>()
                                    {
                                        new SqlOrderByItemExpression()
                                        {
                                            Body =
                                                new SqlPropertyExpression()
                                                {
                                                    Name = new SqlIdentifierExpression()
                                                    {
                                                        Value = "NAME"
                                                    },
                                                    Table = new SqlIdentifierExpression()
                                                    {
                                                        Value = "t"
                                                    },
                                                },
                                        },
                                        new SqlOrderByItemExpression()
                                        {
                                            Body =
                                                new SqlPropertyExpression()
                                                {
                                                    Name = new SqlIdentifierExpression()
                                                    {
                                                        Value = "ID"
                                                    },
                                                    Table = new SqlIdentifierExpression()
                                                    {
                                                        Value = "t"
                                                    },
                                                },
                                        },
                                    },
                                },
                            },
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "rnum"
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Pgsql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(expectSql, generationSql);
    }

    [Fact]
    public void TestNot()
    {
        var sql = "select * from TEST t WHERE not t.NAME ='abc'";

        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                },
                Where = new SqlNotExpression()
                {
                    Body = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "NAME"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "t"
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlStringExpression()
                        {
                            Value = "abc"
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST t where not((t.NAME = 'abc'))", generationSql);
    }

    [Fact]
    public void TestNot2()
    {
        var sql = "select * from TEST t WHERE not EXISTS (SELECT * FROM TEST1 t2)";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                },
                Where = new SqlExistsExpression()
                {
                    IsNot = true,
                    Body = new SqlSelectExpression()
                    {
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                            {
                                new SqlSelectItemExpression()
                                {
                                    Body = new SqlAllColumnExpression()
                                },
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "TEST1"
                                },
                                Alias = new SqlIdentifierExpression()
                                {
                                    Value = "t2"
                                },
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST t where not exists((select * from TEST1 t2))", generationSql);
    }

    [Fact]
    public void TestNot3()
    {
        var sql = "select * from TEST5  WHERE NAME not IN (lower('a'),'b')";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5"
                    },
                },
                Where = new SqlInExpression()
                {
                    IsNot = true,
                    Body = new SqlIdentifierExpression()
                    {
                        Value = "NAME"
                    },
                    TargetList = new List<SqlExpression>()
                    {
                        new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "lower"
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlStringExpression()
                                {
                                    Value = "a"
                                },
                            },
                        },
                        new SqlStringExpression()
                        {
                            Value = "b"
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST5 where NAME not in(lower('a'), 'b')", generationSql);
    }

    [Fact]
    public void TestNot4()
    {
        var sql = "SELECT * from TEST t WHERE name not LIKE '%a%'";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t"
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "name"
                    },
                    Operator = SqlBinaryOperator.NotLike,
                    Right = new SqlStringExpression()
                    {
                        Value = "%a%"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST as t where(name not like '%a%')", generationSql);
    }

    [Fact]
    public void TestNot5()
    {
        var sql = "select * from FlowActivity fa where fa.CreateOn not BETWEEN '2024-01-01' and '2024-10-10'";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
                Where = new SqlBetweenAndExpression()
                {
                    IsNot = true,
                    Body = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "CreateOn"
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "fa"
                        },
                    },
                    Begin = new SqlStringExpression()
                    {
                        Value = "2024-01-01"
                    },
                    End = new SqlStringExpression()
                    {
                        Value = "2024-10-10"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from FlowActivity as fa where fa.CreateOn not between '2024-01-01' and '2024-10-10'",
            generationSql);
    }

    [Fact]
    public void TestNot6()
    {
        var sql = "select * from TEST5  WHERE NOT (name='a' AND AGE='16')";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5"
                    },
                },
                Where = new SqlNotExpression()
                {
                    Body = new SqlBinaryExpression()
                    {
                        Left = new SqlBinaryExpression()
                        {
                            Left = new SqlIdentifierExpression()
                            {
                                Value = "name"
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlStringExpression()
                            {
                                Value = "a"
                            },
                        },
                        Operator = SqlBinaryOperator.And,
                        Right = new SqlBinaryExpression()
                        {
                            Left = new SqlIdentifierExpression()
                            {
                                Value = "AGE"
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlStringExpression()
                            {
                                Value = "16"
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST5 where not(((name = 'a') and(AGE = '16')))", generationSql);
    }


    [Fact]
    public void TestDatabaseScheme()
    {
        var sql = "select * from EPF.dbo.test t";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test",
                    },
                    Schema = new SqlIdentifierExpression()
                    {
                        Value = "EPF.dbo",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t",
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from EPF.dbo.test as t", generationSql);
    }

    [Fact]
    public void TestDatabaseScheme2()
    {
        var sql = "select * from epf.dbo.Ability a left join ATL_Login.dbo.ATLAdUsers au on a.Id =au.Id";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);

        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "Ability",
                        },
                        Schema = new SqlIdentifierExpression()
                        {
                            Value = "epf.dbo",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "a",
                        },
                    },
                    JoinType = SqlJoinType.LeftJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "ATLAdUsers",
                        },
                        Schema = new SqlIdentifierExpression()
                        {
                            Value = "ATL_Login.dbo",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "au",
                        },
                    },
                    Conditions = new SqlBinaryExpression()
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
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "Id",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "au",
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from epf.dbo.Ability as a left join ATL_Login.dbo.ATLAdUsers as au on(a.Id = au.Id)",
            generationSql);
    }

    [Fact]
    public void TestDatabaseScheme3()
    {
        var sql = "select * from \"ATLAdUsers\"@login";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);

        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "ATLAdUsers",
                        LeftQualifiers = "\"",
                        RightQualifiers = "\"",
                    },
                    DbLink = new SqlIdentifierExpression()
                    {
                        Value = "login",
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from \"ATLAdUsers\"@login",
            generationSql);
    }

    [Fact]
    public void TestDatabaseScheme4()
    {
        var sql = "select * from [EPF].[dbo].[test]";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);

        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test",
                        LeftQualifiers = "[",
                        RightQualifiers = "]",
                    },
                    Schema = new SqlIdentifierExpression()
                    {
                        Value = "[EPF].[dbo]",
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from [EPF].[dbo].[test]",
            generationSql);
    }

    [Fact]
    public void TestComplexSelectItem()
    {
        var sql =
            "select c.*, (select a.name as province_name from portal_area a where a.id = c.province_id) as province_name, (select a.name as city_name from portal_area a where a.id = c.city_id) as city_name, (CASE WHEN c.area_id IS NULL THEN NULL ELSE (select a.name as area_name from portal_area a where a.id = c.area_id)  END )as area_name  from portal.portal_company c where no =:a";

        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "*",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "c",
                            },
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "name",
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "a",
                                            },
                                        },
                                        Alias = new SqlIdentifierExpression()
                                        {
                                            Value = "province_name",
                                        },
                                    },
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "portal_area",
                                    },
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Value = "a",
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
                                            Value = "a",
                                        },
                                    },
                                    Operator = SqlBinaryOperator.EqualTo,
                                    Right = new SqlPropertyExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "province_id",
                                        },
                                        Table = new SqlIdentifierExpression()
                                        {
                                            Value = "c",
                                        },
                                    },
                                },
                            },
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "province_name",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlSelectExpression()
                        {
                            Query = new SqlSelectQueryExpression()
                            {
                                Columns = new List<SqlSelectItemExpression>()
                                {
                                    new SqlSelectItemExpression()
                                    {
                                        Body = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "name",
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "a",
                                            },
                                        },
                                        Alias = new SqlIdentifierExpression()
                                        {
                                            Value = "city_name",
                                        },
                                    },
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "portal_area",
                                    },
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Value = "a",
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
                                            Value = "a",
                                        },
                                    },
                                    Operator = SqlBinaryOperator.EqualTo,
                                    Right = new SqlPropertyExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "city_id",
                                        },
                                        Table = new SqlIdentifierExpression()
                                        {
                                            Value = "c",
                                        },
                                    },
                                },
                            },
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "city_name",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
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
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "area_id",
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "c",
                                            },
                                        },
                                        Operator = SqlBinaryOperator.Is,
                                        Right = new SqlNullExpression()
                                    },
                                    Value = new SqlNullExpression()
                                },
                            },
                            Else = new SqlSelectExpression()
                            {
                                Query = new SqlSelectQueryExpression()
                                {
                                    Columns = new List<SqlSelectItemExpression>()
                                    {
                                        new SqlSelectItemExpression()
                                        {
                                            Body = new SqlPropertyExpression()
                                            {
                                                Name = new SqlIdentifierExpression()
                                                {
                                                    Value = "name",
                                                },
                                                Table = new SqlIdentifierExpression()
                                                {
                                                    Value = "a",
                                                },
                                            },
                                            Alias = new SqlIdentifierExpression()
                                            {
                                                Value = "area_name",
                                            },
                                        },
                                    },
                                    From = new SqlTableExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "portal_area",
                                        },
                                        Alias = new SqlIdentifierExpression()
                                        {
                                            Value = "a",
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
                                                Value = "a",
                                            },
                                        },
                                        Operator = SqlBinaryOperator.EqualTo,
                                        Right = new SqlPropertyExpression()
                                        {
                                            Name = new SqlIdentifierExpression()
                                            {
                                                Value = "area_id",
                                            },
                                            Table = new SqlIdentifierExpression()
                                            {
                                                Value = "c",
                                            },
                                        },
                                    },
                                },
                            },
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "area_name",
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "portal_company",
                    },
                    Schema = new SqlIdentifierExpression()
                    {
                        Value = "portal",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "c",
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlIdentifierExpression()
                    {
                        Value = "no",
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlVariableExpression()
                    {
                        Name = "a",
                        Prefix = ":",
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(new object[] { "(name)" })]
    [InlineData(new object[] { "" })]
    public void TestCte(string columns)
    {
        var sql =
            $"with c1{columns} as (select name from test t) , c2{columns} AS (SELECT name FROM test3 t3 ) select *from c1 JOIN c2 ON c1.name=c2.name";

        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        List<SqlIdentifierExpression> cteColumn = null;
        var expectSql = "";
        if (columns == "(name)")
        {
            expectSql =
                "with c1(name) as((select name from test t)), c2(name) as((select name from test3 t3)) select * from c1 inner join c2 on(c1.name = c2.name)";
            cteColumn = new List<SqlIdentifierExpression>()
            {
                new SqlIdentifierExpression()
                {
                    Value = "name"
                }
            };
        }
        else
        {
            expectSql =
                "with c1 as((select name from test t)), c2 as((select name from test3 t3)) select * from c1 inner join c2 on(c1.name = c2.name)";
        }

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                WithSubQuerys = new List<SqlWithSubQueryExpression>()
                {
                    new SqlWithSubQueryExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "c1"
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
                                            Value = "name"
                                        },
                                    },
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "test"
                                    },
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Value = "t"
                                    },
                                },
                            },
                        },
                        Columns = cteColumn
                    },
                    new SqlWithSubQueryExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "c2"
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
                                            Value = "name"
                                        },
                                    },
                                },
                                From = new SqlTableExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "test3"
                                    },
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Value = "t3"
                                    },
                                },
                            },
                        },
                        Columns = cteColumn
                    },
                },
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "c1"
                        },
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "c2"
                        },
                    },
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "name"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "c1"
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "name"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "c2"
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(expectSql, generationSql);
    }


    [Fact]
    public void TestNoFrom()
    {
        var sql = "SELECT DATEDIFF(day, '2023-01-01', '2023-01-10')";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "DATEDIFF"
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlIdentifierExpression()
                                {
                                    Value = "day"
                                },
                                new SqlStringExpression()
                                {
                                    Value = "2023-01-01"
                                },
                                new SqlStringExpression()
                                {
                                    Value = "2023-01-10"
                                },
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select DATEDIFF(day, '2023-01-01', '2023-01-10')", generationSql);
    }

    [Fact]
    public void TestConcatForOracle()
    {
        var sql = "select NAME ||AGE  from TEST5";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Left = new SqlIdentifierExpression()
                            {
                                Value = "NAME"
                            },
                            Operator = SqlBinaryOperator.Concat,
                            Right = new SqlIdentifierExpression()
                            {
                                Value = "AGE"
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5"
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select(NAME || AGE) from TEST5", generationSql);
    }

    [Fact]
    public void TestConcatForSqlserver()
    {
        var sql = "select ActivityEnglishName  +ActivityName  from FlowActivity ;";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Left = new SqlIdentifierExpression()
                            {
                                Value = "ActivityEnglishName"
                            },
                            Operator = SqlBinaryOperator.Add,
                            Right = new SqlIdentifierExpression()
                            {
                                Value = "ActivityName"
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowActivity"
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select(ActivityEnglishName + ActivityName) from FlowActivity", generationSql);
    }

    [Fact]
    public void TestConcatForOracle2()
    {
        var sql = "select NAME ||'-'||AGE  from TEST5";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlIdentifierExpression()
                                {
                                    Value = "NAME"
                                },
                                Operator = SqlBinaryOperator.Concat,
                                Right = new SqlStringExpression()
                                {
                                    Value = "-"
                                },
                            },
                            Operator = SqlBinaryOperator.Concat,
                            Right = new SqlIdentifierExpression()
                            {
                                Value = "AGE"
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5"
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select((NAME || '-') || AGE) from TEST5", generationSql);
    }

    [Fact]
    public void TestCommaForOracle()
    {
        var sql = "select NAME，AGE  from TEST5 ";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
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
                            Value = "NAME"
                        }
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "AGE"
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Fact]
    public void TestWithinGroupForSqlServer()
    {
        var sql =
            "select name,percentile_cont(0.5) within group(order by test5.[number]) OVER(PARTITION BY NAME) as b from TEST5";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);

        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Value = "name",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "percentile_cont",
                            },
                            WithinGroup = new SqlWithinGroupExpression()
                            {
                                OrderBy = new SqlOrderByExpression()
                                {
                                    Items = new List<SqlOrderByItemExpression>()
                                    {
                                        new SqlOrderByItemExpression()
                                        {
                                            Body = new SqlPropertyExpression()
                                            {
                                                Name = new SqlIdentifierExpression()
                                                {
                                                    Value = "number",
                                                    LeftQualifiers = "[",
                                                    RightQualifiers = "]",
                                                },
                                                Table = new SqlIdentifierExpression()
                                                {
                                                    Value = "test5",
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                            Over = new SqlOverExpression()
                            {
                                PartitionBy = new SqlPartitionByExpression()
                                {
                                    Items = new List<SqlExpression>()
                                    {
                                        new SqlIdentifierExpression()
                                        {
                                            Value = "NAME",
                                        },
                                    },
                                },
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlNumberExpression()
                                {
                                    Value = 0.5M,
                                },
                            },
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "b",
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5",
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select name, percentile_cont(0.5) within group(order by test5.[number]) over(partition by NAME) as b from TEST5",
            generationSql);
    }

    [Fact]
    public void TestWithinGroupForOracle()
    {
        var sql =
            " select name,percentile_cont(0.5) within group(order by \"Number\") OVER(PARTITION BY NAME) from TEST5";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Value = "name",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "percentile_cont",
                            },
                            WithinGroup = new SqlWithinGroupExpression()
                            {
                                OrderBy = new SqlOrderByExpression()
                                {
                                    Items = new List<SqlOrderByItemExpression>()
                                    {
                                        new SqlOrderByItemExpression()
                                        {
                                            Body = new SqlIdentifierExpression()
                                            {
                                                Value = "Number",
                                                LeftQualifiers = "\"",
                                                RightQualifiers = "\"",
                                            },
                                        },
                                    },
                                },
                            },
                            Over = new SqlOverExpression()
                            {
                                PartitionBy = new SqlPartitionByExpression()
                                {
                                    Items = new List<SqlExpression>()
                                    {
                                        new SqlIdentifierExpression()
                                        {
                                            Value = "NAME",
                                        },
                                    },
                                },
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlNumberExpression()
                                {
                                    Value = 0.5M,
                                },
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5",
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select name, percentile_cont(0.5) within group(order by \"Number\") over(partition by NAME) from TEST5",
            generationSql);
    }

    [Fact]
    public void TestWithinGroupForPgsql()
    {
        var sql = "select name,PERCENTILE_CONT(0.5) within group(order by \"number\") from TEST5 group by name";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
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
                            Value = "name",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "PERCENTILE_CONT",
                            },
                            WithinGroup = new SqlWithinGroupExpression()
                            {
                                OrderBy = new SqlOrderByExpression()
                                {
                                    Items = new List<SqlOrderByItemExpression>()
                                    {
                                        new SqlOrderByItemExpression()
                                        {
                                            Body = new SqlIdentifierExpression()
                                            {
                                                Value = "number",
                                                LeftQualifiers = "\"",
                                                RightQualifiers = "\"",
                                            },
                                        },
                                    },
                                },
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlNumberExpression()
                                {
                                    Value = 0.5M,
                                },
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5",
                    },
                },
                GroupBy = new SqlGroupByExpression()
                {
                    Items = new List<SqlExpression>()
                    {
                        new SqlIdentifierExpression()
                        {
                            Value = "name",
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Pgsql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select name, PERCENTILE_CONT(0.5) within group(order by \"number\") from TEST5 group by name",
            generationSql);
    }

    [Fact]
    public void TestDataBaseLinkForOracle()
    {
        var sql = "select * from test@db2";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test",
                    },
                    DbLink = new SqlIdentifierExpression()
                    {
                        Value = "db2",
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from test@db2",
            generationSql);
    }


    [Fact]
    public void AForTest()
    {
        var oracleFields =
            "\r\nACCESS\tELSE\tMODIFY\tSTART\r\nADD\tEXCLUSIVE\tNOAUDIT\tSELECT\r\nALL\tEXISTS\tNOCOMPRESS\tSESSION\r\nALTER\tFILE\tNOT\tSET\r\nAND\tFLOAT\tNOTFOUND\tSHARE\r\nANY\tFOR\tNOWAIT\tSIZE\r\nARRAYLEN\tFROM\tNULL\tSMALLINT\r\nAS\tGRANT\tNUMBER\tSQLBUF\r\nASC\tGROUP\tOF\tSUCCESSFUL\r\nAUDIT\tHAVING\tOFFLINE\tSYNONYM\r\nBETWEEN\tIDENTIFIED\tON\tSYSDATE\r\nBY\tIMMEDIATE\tONLINE\tTABLE\r\nCHAR\tIN\tOPTION\tTHEN\r\nCHECK\tINCREMENT\tOR\tTO\r\nCLUSTER\tINDEX\tORDER\tTRIGGER\r\nCOLUMN\tINITIAL\tPCTFREE\tUID\r\nCOMMENT\tINSERT\tPRIOR\tUNION\r\nCOMPRESS\tINTEGER\tPRIVILEGES\tUNIQUE\r\nCONNECT\tINTERSECT\tPUBLIC\tUPDATE\r\nCREATE\tINTO\tRAW\tUSER\r\nCURRENT\tIS\tRENAME\tVALIDATE\r\nDATE\tLEVEL\tRESOURCE\tVALUES\r\nDECIMAL\tLIKE\tREVOKE\tVARCHAR\r\nDEFAULT\tLOCK\tROW\tVARCHAR2\r\nDELETE\tLONG\tROWID\tVIEW\r\nDESC\tMAXEXTENTS\tROWLABEL\tWHENEVER\r\nDISTINCT\tMINUS\tROWNUM\tWHERE\r\nDROP\tMODE\tROWS\tWITH"
                .Replace("\t", ";").Replace("\r\n", ";");

        var sql = @" SELECT CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS DATE) AS utc_date FROM dual";

        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
    }

    [Fact]
    public void TestAt()
    {
        var sql = "select at.* from RouteData at";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "*"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "at"
                            }
                        }
                    }
                },
                From = new SqlTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "at"
                    },
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData"
                    }
                }
            }
        };

        Assert.True(sqlAst.Equals(expect));
    }

    [Theory]
    [InlineData(new object[] { "d" })]
    [InlineData(new object[] { "" })]
    public void TestPivotForOracle(string aliasName)
    {
        var sql =
            $"SELECT {(aliasName == "d" ? "d." : "")}* FROM (SELECT * FROM test6) t PIVOT (SUM(amount) FOR MONTH IN (1 AS j, 2 AS b))  {aliasName}";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);

        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        var alias = new SqlIdentifierExpression()
        {
            Value = "d"
        };
        SqlExpression fieldBody;
        var expectSql = "";
        if (string.IsNullOrWhiteSpace(aliasName))
        {
            fieldBody = new SqlAllColumnExpression();
            expectSql = "select * from(select * from test6) t pivot(SUM(amount) for MONTH in(1 as j, 2 as b))";

            alias = null;
        }
        else
        {
            fieldBody = new SqlPropertyExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "*"
                },
                Table = new SqlIdentifierExpression()
                {
                    Value = "d"
                },
            };
            expectSql = "select d.* from(select * from test6) t pivot(SUM(amount) for MONTH in(1 as j, 2 as b)) d";
        }

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = fieldBody,
                    },
                },
                From = new SqlPivotTableExpression()
                {
                    Alias = alias,
                    For = new SqlIdentifierExpression()
                    {
                        Value = "MONTH"
                    },
                    FunctionCall = new SqlFunctionCallExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "SUM"
                        },
                        Arguments = new List<SqlExpression>()
                        {
                            new SqlIdentifierExpression()
                            {
                                Value = "amount"
                            },
                        },
                    },
                    SubQuery = new SqlSelectExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t"
                        },
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                            {
                                new SqlSelectItemExpression()
                                {
                                    Body = new SqlAllColumnExpression()
                                },
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "test6"
                                },
                            },
                        },
                    },
                    In = new List<SqlExpression>()
                    {
                        new SqlSelectItemExpression()
                        {
                            Body = new SqlNumberExpression()
                            {
                                Value = 1M
                            },
                            Alias = new SqlIdentifierExpression()
                            {
                                Value = "j"
                            },
                        },
                        new SqlSelectItemExpression()
                        {
                            Body = new SqlNumberExpression()
                            {
                                Value = 2M
                            },
                            Alias = new SqlIdentifierExpression()
                            {
                                Value = "b"
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(expectSql,
            generationSql);
    }

    [Fact]
    public void TestPivotForSqlServer()
    {
        var sql =
            $"select year,[1],[2] from (select t.[year] ,t.[month] ,t.amount  from test6 t ) as sourceTable PIVOT (sum(amount) FOR [month] IN ([1],[2] )) AS PivotTable";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);

        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                    Value = "year",
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "1",
                    LeftQualifiers = "[",
                    RightQualifiers = "]",
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "2",
                    LeftQualifiers = "[",
                    RightQualifiers = "]",
                },
            },
        },
                From = new SqlPivotTableExpression()
                {
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "PivotTable",
                    },
                    For = new SqlIdentifierExpression()
                    {
                        Value = "month",
                        LeftQualifiers = "[",
                        RightQualifiers = "]",
                    },
                    FunctionCall = new SqlFunctionCallExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "sum",
                        },
                        Arguments = new List<SqlExpression>()
                {
                    new SqlIdentifierExpression()
                    {
                        Value = "amount",
                    },
                },
                    },
                    SubQuery = new SqlSelectExpression()
                    {
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "sourceTable",
                        },
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                    {
                        new SqlSelectItemExpression()
                        {
                            Body = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "year",
                                    LeftQualifiers = "[",
                                    RightQualifiers = "]",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t",
                                },
                            },
                        },
                        new SqlSelectItemExpression()
                        {
                            Body = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "month",
                                    LeftQualifiers = "[",
                                    RightQualifiers = "]",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t",
                                },
                            },
                        },
                        new SqlSelectItemExpression()
                        {
                            Body = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "amount",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "t",
                                },
                            },
                        },
                    },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "test6",
                                },
                                Alias = new SqlIdentifierExpression()
                                {
                                    Value = "t",
                                },
                            },
                        },
                    },
                    In = new List<SqlExpression>()
            {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "1",
                            LeftQualifiers = "[",
                            RightQualifiers = "]",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "2",
                            LeftQualifiers = "[",
                            RightQualifiers = "]",
                        },
                    },
            },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select year, [1], [2] from(select t.[year], t.[month], t.amount from test6 as t) as sourceTable pivot(sum(amount) for [month] in([1], [2])) as PivotTable",
            generationSql);
    }

    [Fact]
    public void TestPassword()
    {
        var sql = "select Password from RouteData";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Value = "Password",
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "RouteData",
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select Password from RouteData", generationSql);
    }

    [Theory]
    [InlineData(new object[] { true })]
    [InlineData(new object[] { false })]
    public void TestBool(bool value)
    {
        var sql = $"select {value.ToString().ToLowerInvariant()}";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlBoolExpression()
                        {
                            Value = value
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(sql, generationSql);
    }

    [Fact]
    public void TestBool2()
    {
        var sql = $"select * from customer a where true and a.name='a';";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "customer",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "a",
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlBoolExpression()
                    {
                        Value = true
                    },
                    Operator = SqlBinaryOperator.And,
                    Right = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "name",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "a",
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlStringExpression()
                        {
                            Value = "a"
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from customer as a where( true and(a.name = 'a'))", generationSql);
    }


    [Fact]
    public void TestBar()
    {
        var sql = $"select 3|5";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Left = new SqlNumberExpression()
                            {
                                Value = 3M,
                            },
                            Operator = SqlBinaryOperator.BitwiseOr,
                            Right = new SqlNumberExpression()
                            {
                                Value = 5M,
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select(3 | 5)", generationSql);
    }

    [Fact]
    public void TestBitwiseAnd()
    {
        var sql = $"select 3&5";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Left = new SqlNumberExpression()
                            {
                                Value = 3M,
                            },
                            Operator = SqlBinaryOperator.BitwiseAnd,
                            Right = new SqlNumberExpression()
                            {
                                Value = 5M,
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select(3 & 5)", generationSql);
    }

    [Fact]
    public void TestBitwiseXor()
    {
        var sql = $"select 3^5";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Left = new SqlNumberExpression()
                            {
                                Value = 3M,
                            },
                            Operator = SqlBinaryOperator.BitwiseXor,
                            Right = new SqlNumberExpression()
                            {
                                Value = 5M,
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select(3 ^ 5)", generationSql);
    }

    [Fact]
    public void TestNegativenumber()
    {
        var sql = $"select -3*5";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Left = new SqlNumberExpression()
                            {
                                Value = -3M,
                            },
                            Operator = SqlBinaryOperator.Multiply,
                            Right = new SqlNumberExpression()
                            {
                                Value = 5M,
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select(-3 * 5)", generationSql);
    }

    [Fact]
    public void TestNegativenumber2()
    {
        var sql = $"select 5*-3";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.MySql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Left = new SqlNumberExpression()
                            {
                                Value = 5M,
                            },
                            Operator = SqlBinaryOperator.Multiply,
                            Right = new SqlNumberExpression()
                            {
                                Value = -3M,
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.MySql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select(5 * -3)", generationSql);
    }

    [Fact]
    public void TestKeywordAsIdentifier()
    {
        var sql = $"select * from test6 t where t.partition ='a'";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test6",
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
                            Value = "partition",
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
            },
        };

        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from test6 as t where(t.partition = 'a')", generationSql);
    }

    [Fact]
    public void TestKeywordAsIdentifier3()
    {
        var sql = $" select PARTITION.PARTITION from PARTITION";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "PARTITION",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "PARTITION",
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "PARTITION",
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select PARTITION.PARTITION from PARTITION", generationSql);
    }

    [Theory]
    [InlineData(new object[] { "left" })]
    [InlineData(new object[] { "right" })]
    [InlineData(new object[] { "inner" })]
    [InlineData(new object[] { "full" })]
    public void TestKeywordAsIdentifier4(string joinType)
    {
        var sql = $"SELECT LEFT.id from ADDRESS LEFT {joinType} JOIN test ON 1=1";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        SqlJoinType sqlJoinType = SqlJoinType.CommaJoin;
        switch (joinType)
        {
            case "left":
                sqlJoinType = SqlJoinType.LeftJoin;
                break;
            case "right":
                sqlJoinType = SqlJoinType.RightJoin;
                break;
            case "inner":
                sqlJoinType = SqlJoinType.InnerJoin;
                break;
            case "full":
                sqlJoinType = SqlJoinType.FullJoin;
                break;
        }

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "id",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "LEFT",
                            },
                        },
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "ADDRESS",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "LEFT",
                        },
                    },
                    JoinType = sqlJoinType,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "test",
                        },
                    },
                    Conditions = new SqlBinaryExpression()
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
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal($"select LEFT.id from ADDRESS LEFT {joinType} join test on(1 = 1)", generationSql);
    }

    [Fact]
    public void TestKeywordAsIdentifier2()
    {
        var sql = $"select 5*3 AS PARTITION FROM dual  PARTITION JOIN TEST t ON 1=1";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Left = new SqlNumberExpression()
                            {
                                Value = 5M,
                            },
                            Operator = SqlBinaryOperator.Multiply,
                            Right = new SqlNumberExpression()
                            {
                                Value = 3M,
                            },
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "PARTITION",
                        },
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "dual",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "PARTITION",
                        },
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "TEST",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t",
                        },
                    },
                    Conditions = new SqlBinaryExpression()
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
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select(5 * 3) as PARTITION from dual PARTITION inner join TEST t on(1 = 1)", generationSql);
    }

    [Fact]
    public void TestKeywordAsIdentifier5()
    {
        var sql = $"SELECT LEFT.id from ADDRESS left";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "id",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "LEFT",
                            },
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "ADDRESS",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "left",
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select LEFT.id from ADDRESS left", generationSql);
    }

    [Fact]
    public void TestKeywordAsIdentifier6()
    {
        var sql =
            $"select g.group_ \"GROUP\" from group_ g inner join group_class gc on g.groupclassid = gc.id where gc.name = 'CellGroup' order by group_";

        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "group_",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "g",
                            },
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "GROUP",
                            LeftQualifiers = "\"",
                            RightQualifiers = "\"",
                        },
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "group_",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "g",
                        },
                    },
                    JoinType = SqlJoinType.InnerJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "group_class",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "gc",
                        },
                    },
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "groupclassid",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "g",
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "id",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "gc",
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
                            Value = "name",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "gc",
                        },
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlStringExpression()
                    {
                        Value = "CellGroup"
                    },
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Body = new SqlIdentifierExpression()
                            {
                                Value = "group_",
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select g.group_ as \"GROUP\" from group_ g inner join group_class gc on(g.groupclassid = gc.id) where(gc.name = 'CellGroup') order by group_",
            generationSql);
    }

    [Fact]
    public void TestKeywordAsIdentifier7()
    {
        var sql =
            "SELECT \"first name\" FROM \"user data\"";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Value = "first name",
                            LeftQualifiers = "\"",
                            RightQualifiers = "\"",
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "user data",
                        LeftQualifiers = "\"",
                        RightQualifiers = "\"",
                    },
                },
            },
        };



        Assert.True(sqlAst.Equals(expect));
        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select \"first name\" from \"user data\"",
            generationSql);
    }


    [Fact]
    public void TestOracleSpecialParen()
    {
        var sql = "select * from TEST5 t WHERE t.NAME  IN （'a','b','c'）";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "TEST5",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t",
                    },
                },
                Where = new SqlInExpression()
                {
                    Body = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "NAME",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t",
                        },
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
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select * from TEST5 t where t.NAME in('a', 'b', 'c')", generationSql);
    }

    [Fact]
    public void TestEmptyChar()
    {
        var sql =
            "SELECT F.FACILITY, F.FACILITY || '__' ||TT.MEDIUM AS NAME　FROM FACILITY  F   LEFT JOIN TEXT_TRANSLATION TT ON TT.TEXTID = F.TEXTID WHERE TT.LANGUAGEID ='2052' AND F.OBJECTCLASS = '1'";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FACILITY",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "F",
                            },
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlBinaryExpression()
                        {
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "FACILITY",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "F",
                                    },
                                },
                                Operator = SqlBinaryOperator.Concat,
                                Right = new SqlStringExpression()
                                {
                                    Value = "__"
                                },
                            },
                            Operator = SqlBinaryOperator.Concat,
                            Right = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "MEDIUM",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "TT",
                                },
                            },
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "NAME",
                        },
                    },
                },
                From = new SqlJoinTableExpression()
                {
                    Left = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "FACILITY",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "F",
                        },
                    },
                    JoinType = SqlJoinType.LeftJoin,
                    Right = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "TEXT_TRANSLATION",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "TT",
                        },
                    },
                    Conditions = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "TEXTID",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "TT",
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "TEXTID",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "F",
                            },
                        },
                    },
                },
                Where = new SqlBinaryExpression()
                {
                    Left = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "LANGUAGEID",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "TT",
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlStringExpression()
                        {
                            Value = "2052"
                        },
                    },
                    Operator = SqlBinaryOperator.And,
                    Right = new SqlBinaryExpression()
                    {
                        Left = new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "OBJECTCLASS",
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "F",
                            },
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlStringExpression()
                        {
                            Value = "1"
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select F.FACILITY,((F.FACILITY || '__') || TT.MEDIUM) as NAME from FACILITY F left join TEXT_TRANSLATION TT on(TT.TEXTID = F.TEXTID) where((TT.LANGUAGEID = '2052') and(F.OBJECTCLASS = '1'))",
            generationSql);
    }

    [Theory]
    [InlineData(new object[] { "fetch first 2 rows only" })]
    [InlineData(new object[] { "" })]
    public void TestConnectByForOracle(string limitStr)
    {
        var sql =
            "SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e START WITH MANAGERID IS NULL CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID " +
            limitStr;
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();
        SqlLimitExpression limit = null;
        if (!string.IsNullOrWhiteSpace(limitStr))
        {
            limit = new SqlLimitExpression()
            {
                RowCount = new SqlNumberExpression()
                {
                    Value = 2
                }
            };
        }

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
                            Value = "EMPLOYEEID",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "MANAGERID",
                        },
                    },
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "LEVEL",
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "EMPLOYEE",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "e",
                    },
                },
                ConnectBy = new SqlConnectByExpression()
                {
                    StartWith = new SqlBinaryExpression()
                    {
                        Left = new SqlIdentifierExpression()
                        {
                            Value = "MANAGERID",
                        },
                        Operator = SqlBinaryOperator.Is,
                        Right = new SqlNullExpression()
                    },
                    Body = new SqlBinaryExpression()
                    {
                        Left = new SqlIdentifierExpression()
                        {
                            Value = "EMPLOYEEID",
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlIdentifierExpression()
                        {
                            Value = "MANAGERID",
                        },
                    },
                    IsNocycle = true,
                    IsPrior = true,
                    OrderBy = new SqlOrderByExpression()
                    {
                        Items = new List<SqlOrderByItemExpression>()
                        {
                            new SqlOrderByItemExpression()
                            {
                                Body = new SqlIdentifierExpression()
                                {
                                    Value = "EMPLOYEEID",
                                },
                            },
                        },
                        IsSiblings = true,
                    },
                },
                Limit = limit
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();

        var expectSql =
            "select EMPLOYEEID, MANAGERID, LEVEL from EMPLOYEE e start with(MANAGERID is null) connect by nocycle prior EMPLOYEEID = MANAGERID order siblings by EMPLOYEEID";


        if (!string.IsNullOrWhiteSpace(limitStr))
        {
            expectSql += " " + limitStr;
        }

        Assert.Equal(expectSql, generationSql);
    }

    [Fact]
    public void TestConnectByForOracle2()
    {
        var sql =
            "SELECT LEVEL l FROM DUAL  CONNECT BY NOCYCLE LEVEL<=100";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Oracle); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Value = "LEVEL",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "l",
                        },
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "DUAL",
                    },
                },
                ConnectBy = new SqlConnectByExpression()
                {
                    Body = new SqlBinaryExpression()
                    {
                        Left = new SqlIdentifierExpression()
                        {
                            Value = "LEVEL",
                        },
                        Operator = SqlBinaryOperator.LessThenOrEqualTo,
                        Right = new SqlNumberExpression()
                        {
                            Value = 100M,
                        },
                    },
                    IsNocycle = true,
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select LEVEL as l from DUAL connect by nocycle LEVEL <= 100",
            generationSql);
    }

    /// <summary>
    /// 运算符优先级
    /// Operator precedence
    /// </summary>
    [Fact]
    public void TestOperatorPrecedence()
    {
        var sql =
            "select 5^3*2";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlNumberExpression()
                                {
                                    Value = 5M,
                                },
                                Operator = SqlBinaryOperator.BitwiseXor,
                                Right = new SqlNumberExpression()
                                {
                                    Value = 3M,
                                },
                            },
                            Operator = SqlBinaryOperator.Multiply,
                            Right = new SqlNumberExpression()
                            {
                                Value = 2M,
                            },
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Pgsql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select((5 ^ 3) * 2)",
            generationSql);
    }

    /// <summary>
    /// 运算符优先级
    /// Operator precedence
    /// </summary>
    [Fact]
    public void TestOperatorPrecedence2()
    {
        var sql =
            "select  'a' is not null =true";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                            Left = new SqlBinaryExpression()
                            {
                                Left = new SqlStringExpression()
                                {
                                    Value = "a"
                                },
                                Operator = SqlBinaryOperator.IsNot,
                                Right = new SqlNullExpression()
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlBoolExpression()
                            {
                                Value = true
                            },
                        },
                    },
                },
            },
        };


        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Pgsql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select(('a' is not null) = true )",
            generationSql);
    }

    /// <summary>
    /// 运算符优先级
    /// Operator precedence
    /// </summary>
    [Fact]
    public void TestOperatorPrecedence3()
    {
        var sql =
            "select '101'='1'||'01'::bit varying :: varchar";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.Pgsql); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

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
                    Left = new SqlStringExpression()
                    {
                        Value = "101"
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlBinaryExpression()
                    {
                        Left = new SqlStringExpression()
                        {
                            Value = "1"
                        },
                        Operator = SqlBinaryOperator.Concat,
                        Right = new SqlFunctionCallExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "cast",
                            },
                            Arguments = new List<SqlExpression>()
                            {
                                new SqlFunctionCallExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "cast",
                                    },
                                    Arguments = new List<SqlExpression>()
                                    {
                                        new SqlStringExpression()
                                        {
                                            Value = "01"
                                        },
                                    },
                                    CaseAsTargetType = new SqlIdentifierExpression()
                                    {
                                        Value = "bit varying",
                                    },
                                },
                            },
                            CaseAsTargetType = new SqlIdentifierExpression()
                            {
                                Value = "varchar",
                            },
                        },
                    },
                },
            },
        },
            },
        };



        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Pgsql);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal(
            "select('101' =('1' || cast(cast('01' as bit varying) as varchar)))",
            generationSql);
    }

    [Fact]
    public void TestTopNForSqlServer()
    {
        var sql = "SELECT TOP 100 * FROM [sys].[objects] ORDER BY [object_id] DESC";
        var sqlAst = new SqlExpression();
        var t = TimeUtils.TestMicrosecond((() => { sqlAst = DbUtils.Parse(sql, DbType.SqlServer); }));
        testOutputHelper.WriteLine("time:" + t);
        var unitTestAstVisitor = new UnitTestAstVisitor();
        sqlAst.Accept(unitTestAstVisitor);
        var result = unitTestAstVisitor.GetResult();

        var expect = new SqlSelectExpression()
        {
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                Top = new SqlTopExpression()
                {
                    Body = new SqlNumberExpression()
                    {
                        Value = 100M,
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "objects",
                        LeftQualifiers = "[",
                        RightQualifiers = "]",
                    },
                    Schema = new SqlIdentifierExpression()
                    {
                        Value = "[sys]",
                    },
                },
                OrderBy = new SqlOrderByExpression()
                {
                    Items = new List<SqlOrderByItemExpression>()
                    {
                        new SqlOrderByItemExpression()
                        {
                            Body = new SqlIdentifierExpression()
                            {
                                Value = "object_id",
                                LeftQualifiers = "[",
                                RightQualifiers = "]",
                            },
                            OrderByType = SqlOrderByType.Desc,
                        },
                    },
                },
            },
        };

        Assert.True(sqlAst.Equals(expect));

        var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.SqlServer);
        sqlAst.Accept(sqlGenerationAstVisitor);
        var generationSql = sqlGenerationAstVisitor.GetResult();
        Assert.Equal("select top 100 * from [sys].[objects] order by [object_id] desc", generationSql);
    }
}