using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Lexer;

namespace SqlParser.Net.Ast;

public enum ParseType
{
    Select,
    Delete,
    Update,
    Insert
}

public class SqlParser
{
    private int pos = -1;

    private Token? currentToken;
    private Token? nextToken;
    private List<Token> tokens = new List<Token>();
    private List<string> comments = new List<string>();
    private DbType dbType;

    private bool IsOracle => this.dbType == DbType.Oracle;
    private bool IsSqlServer => this.dbType == DbType.SqlServer;
    private bool IsPgsql => this.dbType == DbType.Pgsql;

    private bool IsMySql => this.dbType == DbType.MySql;
    private bool IsSqlite => this.dbType == DbType.Sqlite;

    private static Dictionary<Token, bool> splitTokenDics = new Dictionary<Token, bool>();
    /// <summary>
    /// List of time units;时间单位列表
    /// </summary>
    private static HashSet<string> timeUnitSet = new HashSet<string>()
        { "year", "month", "day", "hour", "minute", "second" };

    /// <summary>
    /// Whether it is in the context of a merge result set operation
    /// 是否在合并结果集操作里的上下文
    /// </summary>
    private InTheMergeResultSetOperationContext inTheMergeResultSetOperationContext = new InTheMergeResultSetOperationContext()
    {
        IsInTheMergeResultSetOperation = false
    };
    /// <summary>
    /// Enable the flag in the merge result set operation
    /// 开启在合并结果集操作里的标记
    /// </summary>
    /// <param name="act"></param>
    private void EnableTheFlagInTheMergeResultSetOperation(Action act)
    {
        this.inTheMergeResultSetOperationContext.IsInTheMergeResultSetOperation = true;
        act();
        this.inTheMergeResultSetOperationContext.IsInTheMergeResultSetOperation = false;
    }

    /// <summary>
    /// while maximum number of loops, used to avoid infinite loops
    /// while最大循环次数，用来避免死循环
    /// </summary>
    private int whileMaximumNumberOfLoops = 100000;
    static SqlParser()
    {
        //group order from limit offset when then end where and or union except intersect join left right full cross
        var splitTokens = new List<Token>()
        {
            Token.Then,
            Token.When,
            Token.End,
            Token.Where,
            Token.Group,
            Token.Order,
            Token.Limit,
            Token.Offset,
            Token.And,
            Token.Or,
            Token.LeftParen,
            Token.RightParen,
            Token.From,
            Token.Join,
            Token.Union,
            Token.Except,
            Token.Intersect,
            Token.Left,
            Token.Right,
            Token.Full,
            Token.Cross,
            Token.Comma,
            Token.Dot,
            Token.Plus,
            Token.Sub,
            Token.Star,
            Token.Slash,
            Token.LessThenOrEqualTo,
            Token.GreaterThenOrEqualTo,
            Token.NotEqualTo,
            Token.LessThen,
            Token.GreaterThen,
            Token.EqualTo,
            Token.As,
        };
        splitTokenDics = splitTokens.ToDictionary(it => it, it => true);
    }

    /// <summary>
    /// sql parsing type
    /// sql 解析类型
    /// </summary>
    public ParseType? SqlParseType { get; private set; }

    public string Sql { get; set; }
    /// <summary>
    /// true:Only recursive arithmetic operations,false:recursive all logical operations
    /// true:仅递归四则运算,false:递归所有逻辑运算
    /// </summary>
    private bool isOnlyRecursiveFourArithmeticOperations = false;



    public SqlExpression Parse(List<Token> tokens, string sql, DbType dbType)
    {
        this.dbType = dbType;
        if (tokens.Last().IsToken(Token.Semicolon))
        {
            tokens.RemoveAt(tokens.Count - 1);
        }
        this.tokens = tokens;
        this.Sql = sql;

        RemoveAllComment();
        GetNextToken();
        //var t2 = TimeUtils.TestMicrosecond((() =>
        //{

        //}));

        if (CheckNextToken(Token.Select) || CheckNextToken(Token.With) || CheckNextToken(Token.LeftParen))
        {
            SqlParseType = ParseType.Select;
            var result = new SqlSelectExpression() { DbType = dbType, };
            result = AcceptSelectExpression();
            result.Comments = comments;
            result.DbType = dbType;
            CheckIfParsingIsComplete();
            return result;
        }
        else if (CheckNextToken(Token.Update))
        {
            SqlParseType = ParseType.Update;
            var result = AcceptUpdateExpression();
            result.Comments = comments;
            CheckIfParsingIsComplete();
            return result;
        }
        else if (CheckNextToken(Token.Delete))
        {
            SqlParseType = ParseType.Delete;
            var result = AcceptDeleteExpression();
            result.Comments = comments;
            CheckIfParsingIsComplete();
            return result;
        }
        else if (CheckNextToken(Token.Insert))
        {
            SqlParseType = ParseType.Insert;
            var result = AcceptInsertExpression();
            result.Comments = comments;
            CheckIfParsingIsComplete();
            return result;
        }

        throw new Exception("Unrecognized parsing type.不识别该种解析类型");
    }
    /// <summary>
    /// Check whether the SQL is parsed
    /// 检查sql是否解析完毕
    /// </summary>
    private void CheckIfParsingIsComplete()
    {
        if (pos != tokens.Count - 1)
        {
            ThrowSqlParsingErrorException();
        }
    }

    private void ThrowSqlParsingErrorException()
    {
        var startIndex = tokens[pos].StartPositionIndex;
        var endIndex = tokens[tokens.Count - 1].EndPositionIndex;
        throw new SqlParsingErrorException($"An error occurred at position :{startIndex},near sql is:   {Sql.Substring(startIndex, endIndex - startIndex + 2)}");
    }
    /// <summary>
    /// remove any comments
    /// 移除所有注释
    /// </summary>
    private void RemoveAllComment()
    {
        for (var i = 0; i < this.tokens.Count; i++)
        {
            var currentToken = this.tokens[i];
            if (currentToken.IsRemove)
            {
                continue;
            }

            if (currentToken.IsToken(Token.LineComment) || currentToken.IsToken(Token.MultiLineComment))
            {
                comments.Add(currentToken.Value?.ToString() ?? "");
                currentToken.IsRemove = true;
                this.tokens[i] = currentToken;
                continue;
            }

        }

        this.tokens.RemoveAll(it => it.IsRemove);
    }

    /// <summary>
    /// remove all comment
    /// 移除所有注释
    /// </summary>
    //private void RemoveAllComment()
    //{
    //    var tokens = new List<Token>();
    //    var removeIndexs = new List<int>();
    //    var isComment = false;

    //    for (int i = 0; i < this.tokens.Count; i++)
    //    {
    //        if (this.tokens[i].Value == Token.LineComment.Value || this.tokens[i].Value == Token.MultiLineComment.Value)
    //        {
    //            comments.Add(this.tokens[i].Value?.ToString());
    //            removeIndexs.Add(i);
    //        }
    //    }


    //    for (int i = 0; i < this.tokens.Count; i++)
    //    {
    //        if (removeIndexs.Contains(i))
    //        {
    //            continue;
    //        }

    //        tokens.Add(this.tokens[i]);
    //    }

    //    this.tokens = tokens;
    //}

    private SqlInsertExpression AcceptInsertExpression()
    {
        AcceptOrThrowException(Token.Insert);
        var result = new SqlInsertExpression() { DbType = dbType };
        AcceptOrThrowException(Token.Into);
        result.Table = AcceptTableExpression();
        result.Columns = AcceptInsertColumnsExpression();
        result.ValuesList = AcceptInsertValuesExpression();
        if (CheckNextToken(Token.Select))
        {
            result.FromSelect = AcceptSelectExpression();
        }

        return result;
    }

    private List<List<SqlExpression>> AcceptInsertValuesExpression()
    {
        var result = new List<List<SqlExpression>>();
        if (Accept(Token.Values))
        {
            var items = new List<SqlExpression>();
            //INSERT INTO table_name [ ( column1, column2, ... ) ] VALUES ( value1, value2, ... ),( value1, value2, ... )
            Accept(Token.Comma);
            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                Accept(Token.Comma);
                if (Accept(Token.LeftParen))
                {
                    var j = 0;
                    while (true)
                    {
                        if (j >= whileMaximumNumberOfLoops)
                        {
                            throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                        }

                        j++;
                        Accept(Token.Comma);
                        var item = AcceptNestedComplexExpression();
                        items.Add(item);
                        if (CheckNextToken(Token.RightParen) || nextToken == null)
                        {
                            break;
                        }
                    }
                    AcceptOrThrowException(Token.RightParen);
                    result.Add(items);
                    items = new List<SqlExpression>();
                }
                if (nextToken == null)
                {
                    break;
                }
            }

            return result;
        }

        return null;
    }

    private List<SqlExpression> AcceptInsertColumnsExpression()
    {
        var items = new List<SqlExpression>();
        //INSERT INTO table_name [ ( column1, column2, ... ) ] VALUES ( value1, value2, ... ),( value1, value2, ... )
        if (Accept(Token.LeftParen))
        {
            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                Accept(Token.Comma);
                var item = AcceptNestedComplexExpression();
                items.Add(item);
                if (CheckNextToken(Token.RightParen) || nextToken == null)
                {
                    break;
                }
            }
            AcceptOrThrowException(Token.RightParen);
            return items;
        }

        return null;
    }

    private SqlDeleteExpression AcceptDeleteExpression()
    {
        AcceptOrThrowException(Token.Delete);
        var result = new SqlDeleteExpression() { DbType = dbType };
        if (IsSqlServer || IsMySql)
        {
            if (CheckNextToken(Token.IdentifierString) && CheckNextNextToken(Token.From))
            {
                Accept(Token.IdentifierString);
                var bodyToken = currentToken;
                result.Body = new SqlIdentifierExpression()
                {
                    LeftQualifiers = bodyToken.HasValue ? bodyToken.Value.LeftQualifiers : "",
                    RightQualifiers = bodyToken.HasValue ? bodyToken.Value.RightQualifiers : "",
                    Value = GetTokenValue(bodyToken),
                    DbType = dbType
                };
            }

            if (IsSqlServer)
            {
                Accept(Token.From);
            }

            if (IsMySql)
            {
                AcceptOrThrowException(Token.From);
            }
        }
        else
        {
            AcceptOrThrowException(Token.From);
        }

        result.Table = AcceptTableSourceExpression();
        result.Where = AcceptWhereExpression();
        return result;
    }

    private SqlUpdateExpression AcceptUpdateExpression()
    {
        AcceptOrThrowException(Token.Update);
        var result = new SqlUpdateExpression() { DbType = dbType };

        if (IsMySql)
        {
            result.Table = AcceptTableSourceExpression();
        }
        else
        {
            result.Table = AcceptTableExpression();
        }

        result.Items = AcceptUpdateItemsExpression();

        //var hasFrom = false;
        if ((IsPgsql || IsSqlServer || IsSqlite) && Accept(Token.From))
        {
            result.From = AcceptTableSourceExpression();
        }
        if (CheckNextToken(Token.Where))
        {
            result.Where = AcceptWhereExpression();
        }

        return result;
    }

    private SqlExpression AcceptTableExpression()
    {
        //sub query子查询
        if (Accept(Token.LeftParen))
        {
            var subQuery = AcceptSelectExpression();
            AcceptOrThrowException(Token.RightParen);
            //oracle not support subQuery as
            if (dbType != DbType.Oracle)
            {
                Accept(Token.As);
            }

            if (Accept(Token.IdentifierString))
            {
                var subQueryAlias = GetCurrentTokenValue();
                subQuery.Alias = new SqlIdentifierExpression()
                {
                    LeftQualifiers = currentToken.HasValue ? currentToken.Value.LeftQualifiers : "",
                    RightQualifiers = currentToken.HasValue ? currentToken.Value.RightQualifiers : "",
                    Value = subQueryAlias,
                    DbType = dbType
                };
            }
            return subQuery;
        }

        var isFrom = CheckCurrentToken(Token.From);
        AcceptOrThrowException(Token.IdentifierString);

        var name = GetCurrentTokenValue();
        var mainToken = currentToken;
        Token? dbLinkToken = null;
        //var nameList = new List<string>() { name };
        var nameTokenList = new List<Token?>() { mainToken };

        var dbLinkName = "";
        //such as:SELECT * FROM TABLE(splitstr('a;b',';'))
        if (isFrom && CheckNextToken(Token.LeftParen))
        {
            var functionCall = AcceptFunctionCall(name);
            var table = new SqlReferenceTableExpression()
            {
                FunctionCall = functionCall,
                DbType = dbType
            };
            return table;
        }
        else
        {
            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                var isOracleDbLink = (IsOracle || IsPgsql) && Accept(Token.At);

                //select * from [a.test]..[test]
                var isSqlServerDotDot = IsSqlServer && Accept(Token.DotDot);

                if (Accept(Token.Dot))
                {
                    if (Accept(Token.IdentifierString))
                    {
                        var value = GetCurrentTokenValue();
                        mainToken = currentToken;
                        nameTokenList.Add(mainToken);
                    }
                    else
                    {
                        ThrowSqlParsingErrorException();
                    }
                }
                else if (isOracleDbLink)
                {
                    if (Accept(Token.IdentifierString))
                    {
                        dbLinkToken = currentToken;
                        dbLinkName = GetCurrentTokenValue();
                    }
                    else
                    {
                        ThrowSqlParsingErrorException();
                    }
                }
                else if (isSqlServerDotDot)
                {
                    var dboToken = Token.IdentifierString;
                    dboToken.Value = "dbo";
                    nameTokenList.Add(dboToken);
                    if (Accept(Token.IdentifierString))
                    {
                        var value = GetCurrentTokenValue();
                        mainToken = currentToken;
                        nameTokenList.Add(mainToken);
                    }
                    else
                    {
                        ThrowSqlParsingErrorException();
                    }
                }
                else
                {
                    break;
                }
            }

            var table = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    LeftQualifiers = mainToken.HasValue ? mainToken.Value.LeftQualifiers : "",
                    RightQualifiers = mainToken.HasValue ? mainToken.Value.RightQualifiers : "",
                    Value = GetTokenValue(nameTokenList.Last()),
                    DbType = dbType
                }
            };

            if (!string.IsNullOrWhiteSpace(dbLinkName))
            {
                table.DbLink = new SqlIdentifierExpression()
                {
                    LeftQualifiers = dbLinkToken.HasValue ? dbLinkToken.Value.LeftQualifiers : "",
                    RightQualifiers = dbLinkToken.HasValue ? dbLinkToken.Value.RightQualifiers : "",
                    Value = dbLinkName,
                    DbType = dbType
                };
            }

            if (nameTokenList.Count > 1)
            {
                nameTokenList.RemoveAt(nameTokenList.Count - 1);
                if (nameTokenList.Count == 2)
                {
                    var databaseToken = nameTokenList.First();
                    table.Database = new SqlIdentifierExpression()
                    {
                        LeftQualifiers = databaseToken.HasValue ? databaseToken.Value.LeftQualifiers : "",
                        RightQualifiers = databaseToken.HasValue ? databaseToken.Value.RightQualifiers : "",
                        Value = GetTokenValue(databaseToken),
                        DbType = dbType
                    };
                    nameTokenList.RemoveAt(0);
                }
                var schemaToken = nameTokenList.First();
                table.Schema = new SqlIdentifierExpression()
                {
                    LeftQualifiers = schemaToken.HasValue ? schemaToken.Value.LeftQualifiers : "",
                    RightQualifiers = schemaToken.HasValue ? schemaToken.Value.RightQualifiers : "",
                    Value = GetTokenValue(schemaToken),
                    DbType = dbType
                };
            }

            var alias = "";
            if (Accept(Token.As))
            {
                AcceptOrThrowException(Token.IdentifierString);
                alias = GetCurrentTokenValue();
            }
            else if (Accept(Token.IdentifierString))
            {
                alias = GetCurrentTokenValue();
            }

            if (!string.IsNullOrWhiteSpace(alias))
            {
                table.Alias = new SqlIdentifierExpression()
                {
                    LeftQualifiers = currentToken.HasValue ? currentToken.Value.LeftQualifiers : "",
                    RightQualifiers = currentToken.HasValue ? currentToken.Value.RightQualifiers : "",
                    Value = alias,
                    DbType = dbType
                };
            }

            table.Hints = AcceptHints();

            return table;
        }

    }

    private List<SqlHintExpression> AcceptHints()
    {
        if (IsSqlServer)
        {
            if (Accept(Token.HintsConstant))
            {
                var sqlHints = new SqlHintExpression()
                {
                    DbType = dbType,
                    Body = new SqlIdentifierExpression()
                    {
                        Value = GetCurrentTokenValue(),
                        DbType = dbType
                    }
                };
                var result = new List<SqlHintExpression>()
                {
                    sqlHints
                };
                return result;
            }
        }

        return null;
    }

    private List<SqlExpression> AcceptUpdateItemsExpression()
    {
        AcceptOrThrowException(Token.Set);

        var items = new List<SqlExpression>();
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;

            if (CheckNextToken(Token.Where)
                || nextToken == null
                || ((IsSqlServer || IsPgsql || IsSqlite) && CheckNextToken(Token.From)))
            {
                break;
            }

            Accept(Token.Comma);
            var tempSavePoint = pos;
            var item = AcceptNestedComplexExpression();
            if (item is not SqlBinaryExpression)
            {
                pos = tempSavePoint;
                CheckIfParsingIsComplete(); pos = tempSavePoint;
                CheckIfParsingIsComplete();
            }
            else
            {
                items.Add(item);
            }
        }

        return items;
    }

    private SqlSelectExpression AcceptSelectExpression()
    {
        var left = AcceptSelectExpression2();
        SqlSelectExpression right = null;
        SqlExpression total = left;
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            if (nextToken == null)
            {
                break;
            }

            SqlUnionType? unionType = null;
            if (Accept(Token.Union))
            {
                if (Accept(Token.All))
                {
                    unionType = SqlUnionType.UnionAll;
                }
                else
                {
                    unionType = SqlUnionType.Union;
                }
            }
            else if (Accept(Token.Intersect))
            {
                if (Accept(Token.All))
                {
                    unionType = SqlUnionType.IntersectAll;
                }
                else
                {
                    unionType = SqlUnionType.Intersect;
                }
            }
            else if (Accept(Token.Except))
            {
                if (Accept(Token.All))
                {
                    unionType = SqlUnionType.ExceptAll;
                }
                else
                {
                    unionType = SqlUnionType.Except;
                }
            }
            else if (Accept(Token.Minus))
            {
                unionType = SqlUnionType.Minus;
            }

            if (unionType == null)
            {
                break;
            }
            else
            {
                EnableTheFlagInTheMergeResultSetOperation(() =>
                {
                    right = AcceptSelectExpression2();
                });
                //right = AcceptSelectExpression2();
                total = new SqlUnionQueryExpression()
                {
                    DbType = dbType,
                    Left = total,
                    Right = right,
                    UnionType = unionType.Value
                };
            }
        }

        if (total == null)
        {
            return null;
        }
        if (total is SqlSelectExpression result)
        {
            return result;
        }

        var sqlSelectExpression = new SqlSelectExpression()
        {
            DbType = dbType,
            Query = total
        };

        sqlSelectExpression.OrderBy = AcceptOrderByExpression();
        sqlSelectExpression.Limit = AcceptLimitExpression();

        return sqlSelectExpression;
    }

    private SqlSelectExpression AcceptSelectExpression2()
    {
        if (Accept(Token.LeftParen))
        {
            if (this.inTheMergeResultSetOperationContext.IsInTheMergeResultSetOperation)
            {
                if (IsSqlite)
                {
                    ThrowSqlParsingErrorException();
                }

                this.inTheMergeResultSetOperationContext.IncreaseParenDepth();
            }

            var temp = AcceptSelectExpression();
            AcceptOrThrowException(Token.RightParen);

            return temp;
        }
        var result = AcceptSelectQueryExpression();
        return result;
    }

    private SqlSelectExpression AcceptSelectQueryExpression()
    {
        if (!(CheckNextToken(Token.Select) || CheckNextToken(Token.With)))
        {
            return null;
        }
        var query = new SqlSelectQueryExpression() { DbType = dbType };
        var result = new SqlSelectExpression()
        {
            DbType = dbType,
            Query = query
        };

        query.WithSubQuerys = AcceptWithSubQueryExpression();

        AcceptOrThrowException(Token.Select);

        query.ResultSetReturnOption = AcceptResultSetReturnOption();

        query.Top = AcceptTopN();

        query.Columns = AcceptSelectItemsExpression();

        query.Into = AcceptSelectInto();
        var hasFrom = false;

        if (Accept(Token.From) || (IsOracle && AcceptOrThrowException(Token.From)))
        {
            query.From = AcceptTableSourceExpression();
            hasFrom = true;
        }

        if (CheckNextToken(Token.Where))
        {
            if (hasFrom || IsSqlite)
            {
                query.Where = AcceptWhereExpression();
            }
            else
            {
                ThrowSqlParsingErrorException();
            }
        }

        if (CheckNextIsGroupBy())
        {
            if (hasFrom || (IsPgsql || IsMySql || IsSqlite))
            {
                query.GroupBy = AcceptGroupByExpression();
            }
            else
            {
                ThrowSqlParsingErrorException();
            }
        }

        if (this.inTheMergeResultSetOperationContext.IsInTheMergeResultSetOperation)
        {
            if (CheckNextIsOrderBy())
            {
                // select 3 as pn union (select 2 as pn order by pn)
                if ((IsOracle || IsSqlServer) && this.inTheMergeResultSetOperationContext.HasParenDepth)
                {
                    ThrowSqlParsingErrorException();
                }

                if (!this.inTheMergeResultSetOperationContext.HasParenDepth)
                {
                    return result;
                }

            }
        }

        query.OrderBy = AcceptOrderByExpression();


        if (IsOracle)
        {
            query.ConnectBy = AcceptConnectByExpression();
        }

        if (this.inTheMergeResultSetOperationContext.IsInTheMergeResultSetOperation)
        {
            if (CheckNextIsLimit())
            {
                if (!this.inTheMergeResultSetOperationContext.HasParenDepth)
                {
                    return result;
                }
            }
        }

        query.Limit = AcceptLimitExpression();


        query.Hints = AcceptHints();


        return result;
    }


    /// <summary>
    /// Virtual Advance
    /// 虚拟前进
    /// </summary>
    /// <param name="action"></param>
    private void VirtualAdvance(Action action)
    {
        this.SavePoint();
        action();
        this.RestoreSavePoint();
    }

    private bool CheckNextIsLimit()
    {
        var result = false;
        VirtualAdvance(() =>
        {
            result = AcceptLimitExpression() != null;
        });
        return result;
    }

    private SqlLimitExpression AcceptLimitExpression()
    {
        if (IsMySql || IsSqlite)
        {
            return AcceptMysqlLimitExpression();
        }
        else if (IsSqlServer)
        {
            return AcceptSqlServerLimitExpression();
        }
        else if (IsPgsql)
        {
            return AcceptPgsqlLimitExpression();
        }
        else if (IsOracle)
        {
            return AcceptOracleLimitExpression();
        }

        return null;
    }
    private SqlLimitExpression AcceptSqlServerLimitExpression()
    {
        if (CheckNextToken(Token.Offset) && CheckNextNextToken(Token.NumberConstant))
        {
            Accept(Token.Offset);
            var result = new SqlLimitExpression() { DbType = dbType };
            var first = AcceptNestedComplexExpression();
            AcceptOrThrowException(Token.Rows);
            AcceptOrThrowException(Token.Fetch);
            AcceptOrThrowException(Token.Next);
            var rowCount = AcceptNestedComplexExpression();
            AcceptOrThrowException(Token.Rows);
            AcceptOrThrowException(Token.Only);
            result.RowCount = rowCount;
            result.Offset = first;

            return result;
        }

        return null;
    }

    private SqlLimitExpression AcceptOracleLimitExpression()
    {
        if (CheckNextToken(Token.Fetch) && CheckNextNextToken(Token.First))
        {
            Accept(Token.Fetch);
            var result = new SqlLimitExpression() { DbType = dbType };
            AcceptOrThrowException(Token.First);
            var rowCount = AcceptNestedComplexExpression();
            AcceptOrThrowException(Token.Rows);
            AcceptOrThrowException(Token.Only);
            result.RowCount = rowCount;
            return result;
        }

        return null;
    }

    private SqlLimitExpression AcceptPgsqlLimitExpression()
    {
        var result = new SqlLimitExpression() { DbType = dbType };
        if (CheckNextToken(Token.Limit) && CheckNextNextToken(Token.NumberConstant))
        {
            Accept(Token.Limit);
            var rowCount = AcceptNestedComplexExpression();

            if (Accept(Token.Offset))
            {
                var offset = AcceptNestedComplexExpression();
                result.RowCount = rowCount;
                result.Offset = offset;
            }
            else
            {
                result.RowCount = rowCount;
            }

            return result;
        }
        else if (CheckNextToken(Token.Offset) && CheckNextNextToken(Token.NumberConstant))
        {
            Accept(Token.Offset);
            var offset = AcceptNestedComplexExpression();
            result.Offset = offset;
            return result;
        }

        return null;
    }

    private SqlLimitExpression AcceptMysqlLimitExpression()
    {
        if (CheckNextToken(Token.Limit) && CheckNextNextToken(Token.NumberConstant))
        {
            Accept(Token.Limit);
            var result = new SqlLimitExpression() { DbType = dbType };
            var first = AcceptNestedComplexExpression();

            if (Accept(Token.Comma))
            {
                result.Offset = first;
                var rowCount = AcceptNestedComplexExpression();
                result.RowCount = rowCount;
            }
            else
            {
                result.RowCount = first;
            }

            return result;
        }

        return null;
    }

    private SqlExpression AcceptSelectInto()
    {
        if (IsSqlServer)
        {
            if (Accept(Token.Into))
            {
                var table = AcceptTableExpression();
                return table;
            }
        }

        return null;
    }

    private SqlTopExpression AcceptTopN()
    {
        if (IsSqlServer)
        {
            if (Accept(Token.Top))
            {
                AcceptOrThrowException(Token.NumberConstant);
                var topCount = GetCurrentTokenNumberValue();
                var result = new SqlTopExpression()
                {
                    DbType = dbType,
                    Body = new SqlNumberExpression()
                    {
                        DbType = dbType,
                        Value = topCount
                    }
                };
                return result;
            }
        }

        return null;
    }

    private SqlResultSetReturnOption? AcceptResultSetReturnOption()
    {
        SqlResultSetReturnOption? resultSetReturnOption = null;
        if (Accept(Token.All))
        {
            resultSetReturnOption = SqlResultSetReturnOption.All;
        }
        else if (Accept(Token.Distinct))
        {
            resultSetReturnOption = SqlResultSetReturnOption.Distinct;
        }
        else if (IsOracle && Accept(Token.Unique))
        {
            resultSetReturnOption = SqlResultSetReturnOption.Unique;
        }

        return resultSetReturnOption;
    }

    private List<SqlWithSubQueryExpression> AcceptWithSubQueryExpression()
    {
        if (Accept(Token.With))
        {
            var subQueryExpressions = new List<SqlWithSubQueryExpression>();
            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                var subQueryExpression = new SqlWithSubQueryExpression()
                {
                    DbType = dbType,
                };
                Accept(Token.Comma);
                AcceptOrThrowException(Token.IdentifierString);
                var alias = GetCurrentTokenValue();
                subQueryExpression.Alias = new SqlIdentifierExpression()
                {
                    DbType = dbType,
                    Value = alias
                };
                if (Accept(Token.LeftParen))
                {
                    subQueryExpression.Columns = new List<SqlIdentifierExpression>();
                    var j = 0;
                    while (true)
                    {
                        if (j >= whileMaximumNumberOfLoops)
                        {
                            throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                        }

                        j++;
                        Accept(Token.Comma);
                        AcceptOrThrowException(Token.IdentifierString);
                        var columnName = GetCurrentTokenValue();
                        var columnExpression = new SqlIdentifierExpression()
                        {
                            DbType = dbType,
                            Value = columnName
                        };
                        subQueryExpression.Columns.Add(columnExpression);
                        if (CheckNextToken(Token.RightParen) || nextToken == null)
                        {
                            break;
                        }
                    }
                    AcceptOrThrowException(Token.RightParen);
                }

                AcceptOrThrowException(Token.As);
                AcceptOrThrowException(Token.LeftParen);
                var subQuery = AcceptSelectExpression();
                AcceptOrThrowException(Token.RightParen);
                subQueryExpression.FromSelect = subQuery;
                subQueryExpressions.Add(subQueryExpression);

                if (CheckNextToken(Token.Select) || nextToken == null)
                {
                    break;
                }
            }
            return subQueryExpressions;
        }

        return null;
    }


    /// <summary>
    /// Check if the next step is order by
    /// 检查接下来是否是order by
    /// </summary>
    /// <returns></returns>
    private bool CheckNextIsOrderBy()
    {
        return CheckNextToken(Token.Order) && CheckNextNextToken(Token.By);
    }

    /// <summary>
    /// Check if the next step is group by
    /// 检查接下来是否是group by
    /// </summary>
    /// <returns></returns>
    private bool CheckNextIsGroupBy()
    {
        return CheckNextToken(Token.Group) && CheckNextNextToken(Token.By);
    }

    private List<SqlSelectItemExpression> AcceptSelectItemsExpression()
    {
        var result = new List<SqlSelectItemExpression>();
        var item = new SqlSelectItemExpression()
        {
            DbType = dbType,
        };
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            if (CheckNextToken(Token.From)
                || nextToken == null
                || CheckNextToken(Token.RightParen)
                || (IsSqlServer && CheckNextToken(Token.Into))
                || ((IsPgsql || IsSqlServer || IsSqlite) && (CheckNextToken(Token.Union) || CheckNextToken(Token.Except) || CheckNextToken(Token.Intersect)))
                || (IsMySql && CheckNextToken(Token.Union))
                || ((IsPgsql || IsSqlite || IsMySql) && CheckNextIsGroupBy())
                || ((IsPgsql || IsSqlServer || IsSqlite || IsMySql) && CheckNextIsOrderBy())
                || (IsSqlite && CheckNextToken(Token.Where))
                || CheckNextIsLimit())
            {
                break;
            }

            if (IsOracle && (CheckNextIsGroupBy() || CheckNextIsOrderBy()))
            {
                ThrowSqlParsingErrorException();
            }

            var hasComma = false;
            if (Accept(Token.Comma))
            {
                hasComma = true;
                item = new SqlSelectItemExpression()
                {
                    DbType = dbType,
                };
            }

            if (!hasComma && i != 1)
            {
                throw new Exception("Missing symbol ',' between select options");
            }

            item.Body = AcceptNestedComplexExpression();

            var asStr = AcceptAsToken(true);
            if (!string.IsNullOrWhiteSpace(asStr))
            {
                item.Alias = new SqlIdentifierExpression()
                {
                    DbType = dbType,
                    LeftQualifiers = currentToken.HasValue ? currentToken.Value.LeftQualifiers : "",
                    RightQualifiers = currentToken.HasValue ? currentToken.Value.RightQualifiers : "",
                    Value = asStr
                };
            }
            result.Add(item);
        }

        return result;
    }


    /// <summary>
    /// Parsing at time zone expressions
    /// 解析at time zone表达式
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    private SqlExpression AcceptAtTimeZoneExpression(SqlExpression body)
    {
        if (IsPgsql)
        {
            if (Accept(Token.AtValue))
            {
                var value = GetCurrentTokenValue();

                if (Accept(Token.Time))
                {
                    value = GetCurrentTokenValue();

                    if (Accept(Token.Zone))
                    {
                        value = GetCurrentTokenValue();

                        if (Accept(Token.StringConstant))
                        {
                            value = GetCurrentTokenValue();
                            var result = new SqlAtTimeZoneExpression()
                            {
                                DbType = dbType,
                                Body = body,
                                TimeZone = new SqlStringExpression()
                                {
                                    DbType = dbType,
                                    Value = value
                                }
                            };
                            //Recursive call, such as SELECT order_date at TIME zone 'Asia/ShangHai' at TIME zone 'utc' as b FROM orders;
                            //递归调用，例如SELECT order_date at TIME zone 'Asia/ShangHai' at TIME zone 'utc' as b FROM orders;
                            var recursionResult = AcceptAtTimeZoneExpression(result);

                            return recursionResult ?? result;
                        }

                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Parsing nested complex expressions
    /// 解析嵌套的复杂表达式
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptNestedComplexExpression()
    {
        var result = AcceptLogicalExpression();
        var atTimeZoneResult = AcceptAtTimeZoneExpression(result);
        return atTimeZoneResult ?? result;
    }

    /// <summary>
    /// Parsing logical expressions,not、and、or,The order of logical operators is not, and, or
    /// 解析逻辑表达式，not、and、or,逻辑运算符的顺序为not、and、or
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptLogicalExpression()
    {
        var exp = AcceptLogicalExpressionForOr();
        return exp;
    }


    /// <summary>
    /// Parsing logical expressions or
    /// 解析逻辑表达式or
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private SqlExpression AcceptLogicalExpressionForOr()
    {
        var left = AcceptLogicalExpressionForAnd();
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            SqlBinaryOperator @operator;
            if (Accept(Token.Or))
            {
                @operator = SqlBinaryOperator.Or;
            }
            else
            {
                break;
            }

            var right = AcceptLogicalExpressionForAnd();

            left = new SqlBinaryExpression()
            {
                DbType = dbType,
                Left = left,
                Right = right,
                Operator = @operator
            };
        }

        return left;
    }

    /// <summary>
    /// Parsing logical expressions and
    /// 解析逻辑表达式and
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private SqlExpression AcceptLogicalExpressionForAnd()
    {
        var left = AcceptLogicalExpressionForNot();
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            SqlBinaryOperator @operator;
            if (Accept(Token.And))
            {
                @operator = SqlBinaryOperator.And;
            }
            else
            {
                break;
            }

            var right = AcceptLogicalExpressionForNot();

            left = new SqlBinaryExpression()
            {
                DbType = dbType,
                Left = left,
                Right = right,
                Operator = @operator
            };
        }

        return left;
    }

    /// <summary>
    /// Parsing logical expression not
    /// 解析逻辑表达式not
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptLogicalExpressionForNot()
    {
        var isNot = Accept(Token.Not);
        var exp = AcceptEquationOperationExpression();
        if (isNot)
        {
            if (exp is SqlExistsExpression sqlExistsExpression)
            {
                sqlExistsExpression.IsNot = true;
                return sqlExistsExpression;
            }

            return new SqlNotExpression()
            {
                DbType = dbType,
                Body = exp
            };
        }

        return exp;
    }

    /// <summary>
    /// 解析等式运算
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptEquationOperationExpression()
    {
        var left = AcceptRelationalExpression();
        AppendCollateExpression(left);
        SqlExpression right = null;
        // not in,not like,not between
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            SqlBinaryOperator @operator = null;
            if (Accept(Token.EqualTo))
            {
                @operator = SqlBinaryOperator.EqualTo;
            }
            else if (Accept(Token.NotEqualTo))
            {
                @operator = SqlBinaryOperator.NotEqualTo;
            }
            else if (Accept(Token.LessThen))
            {
                @operator = SqlBinaryOperator.LessThen;
            }
            else if (Accept(Token.LessThenOrEqualTo))
            {
                @operator = SqlBinaryOperator.LessThenOrEqualTo;
            }
            else if (Accept(Token.GreaterThen))
            {
                @operator = SqlBinaryOperator.GreaterThen;
            }
            else if (Accept(Token.GreaterThenOrEqualTo))
            {
                @operator = SqlBinaryOperator.GreaterThenOrEqualTo;
            }
            else
            {
                break;
            }

            if (right == null)
            {
                right = AcceptRelationalExpression();
                AppendCollateExpression(right);
            }

            left = new SqlBinaryExpression()
            {
                DbType = dbType,
                Left = left,
                Right = right,
                Operator = @operator
            };
            
            right = null;
        }

        return left;
    }
    /// <summary>
    /// Parse relational expressions, such as ||,is null,Between,like,in etc.
    /// 解析关系型表达式，比如||,is null,Between,like,in等
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptRelationalExpression()
    {
        var left = AcceptFourArithmeticOperationsAddOrSub();
        AppendCollateExpression(left);
        SqlExpression right = null;
        // not in,not like,not between
        var isNot = false;
        Token? not = null;
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            SqlBinaryOperator @operator = null;

            if (Accept(Token.Is))
            {

                if (Accept(Token.Not))
                {
                    @operator = SqlBinaryOperator.IsNot;
                }
                else
                {
                    @operator = SqlBinaryOperator.Is;
                }

                AcceptOrThrowException(Token.Null);
                right = new SqlNullExpression() { DbType = dbType };
            }
            else if (Accept(Token.Between))
            {
                var between = currentToken;
                this.isOnlyRecursiveFourArithmeticOperations = true;
                var begin = AcceptFourArithmeticOperationsAddOrSub();
                AcceptOrThrowException(Token.And);
                var and = currentToken;
                var end = AcceptFourArithmeticOperationsAddOrSub();
                this.isOnlyRecursiveFourArithmeticOperations = false;
                var result = new SqlBetweenAndExpression()
                {
                    DbType = dbType,
                    IsNot = isNot,
                    Begin = begin,
                    End = end,
                    Body = left,
                    TokenContext = new SqlBetweenAndExpressionTokenContext()
                    {
                        Between = between,
                        And = and,
                        Not = isNot ? not : null
                    }
                };
                return result;
            }
            else if (Accept(Token.In))
            {
                var result = new SqlInExpression()
                {
                    DbType = dbType,
                    Body = left,
                    IsNot = isNot
                };
                AcceptOrThrowException(Token.LeftParen);
                var subQuery = AcceptSelectExpression();
                if (subQuery != null)
                {
                    result.SubQuery = subQuery;
                }
                else
                {
                    var targetList = new List<SqlExpression>();
                    this.isOnlyRecursiveFourArithmeticOperations = true;
                    var j = 0;
                    while (true)
                    {
                        if (j >= whileMaximumNumberOfLoops)
                        {
                            throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                        }

                        j++;
                        Accept(Token.Comma);
                        var item = AcceptFourArithmeticOperationsAddOrSub();
                        targetList.Add(item);
                        if (nextToken == null || CheckNextToken(Token.RightParen))
                        {
                            break;
                        }
                    }
                    this.isOnlyRecursiveFourArithmeticOperations = false;
                    result.TargetList = targetList;
                }
                AcceptOrThrowException(Token.RightParen);
                return result;
            }
            else if (Accept(Token.Like))
            {
                @operator = SqlBinaryOperator.Like;
            }
            else if (IsPgsql && Accept(Token.ILike))
            {
                @operator = SqlBinaryOperator.ILike;
            }
            else if (IsPgsql && Accept(Token.RegexPForPg))
            {
                var result = new SqlRegexExpression()
                {
                    DbType = dbType,
                    IsCaseSensitive = true,
                    Body = left
                };
                if (Accept(Token.Star))
                {
                    result.IsCaseSensitive = false;
                }
                else if (CheckNextToken(Token.StringConstant))
                {
                   
                }
                else
                {
                    ThrowSqlParsingErrorException();
                }

                if (Accept(Token.StringConstant))
                {
                    var regex = GetCurrentTokenValue();
                    result.RegEx = new SqlStringExpression()
                    {
                        DbType = dbType,
                        Value = regex
                    };
                }

                AppendCollateExpression(result);

                return result;
            }
            else if (IsMySql && Accept(Token.RegexpForMysql))
            {
                var result = new SqlRegexExpression()
                {
                    DbType = dbType,
                    IsCaseSensitive = true,
                    Body = left
                };
                if (Accept(Token.StringConstant))
                {
                    var regEx = GetCurrentTokenValue();
                    result.RegEx = new SqlStringExpression()
                    {
                        DbType = dbType,
                        Value = regEx
                    };
                }
                else
                {
                    ThrowSqlParsingErrorException();
                }

                AppendCollateExpression(result);

                return result;
            }
            else if (Accept(Token.BarBar))
            {
                @operator = SqlBinaryOperator.Concat;
            }
            else if (Accept(Token.Not))
            {
                not = currentToken;
                if (Accept(Token.Like))
                {
                    @operator = SqlBinaryOperator.NotLike;
                }
                else if (IsPgsql && Accept(Token.ILike))
                {
                    @operator = SqlBinaryOperator.NotILike;
                }
                else
                {
                    isNot = true;
                    continue;
                }
            }
            else
            {
                break;
            }

            if (right == null)
            {
                right = AcceptFourArithmeticOperationsAddOrSub();
                AppendCollateExpression(right);
            }

            left = new SqlBinaryExpression()
            {
                DbType = dbType,
                Left = left,
                Right = right,
                Operator = @operator,
            };

            if (isNot)
            {
                isNot = false;
                if (left is SqlExistsExpression sqlExistsExpression)
                {
                    sqlExistsExpression.IsNot = true;
                }
                else if (left is SqlInExpression sqlInExpression)
                {
                    sqlInExpression.IsNot = true;
                }
                else if (left is SqlBetweenAndExpression sqlBetweenAndExpression)
                {
                    sqlBetweenAndExpression.IsNot = true;
                    if (sqlBetweenAndExpression.TokenContext != null)
                    {
                        sqlBetweenAndExpression.TokenContext.Not = not;
                    }
                }
                else
                {
                    var notResult = new SqlNotExpression()
                    {
                        DbType = dbType,
                        Body = left
                    };
                    return notResult;
                }

            }
            right = null;
        }

        return left;
    }
    /// <summary>
    /// Analyze addition and subtraction in the four arithmetic operations
    /// 解析四则运算中的加法和减法
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptFourArithmeticOperationsAddOrSub()
    {
        var left = AcceptFourArithmeticOperationsMultiplyOrDivide();
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            SqlBinaryOperator @operator;
            if (Accept(Token.Plus))
            {
                @operator = SqlBinaryOperator.Add;
            }
            else if (Accept(Token.Sub))
            {
                @operator = SqlBinaryOperator.Sub;
            }
            else
            {
                break;
            }

            var right = AcceptFourArithmeticOperationsMultiplyOrDivide();

            left = new SqlBinaryExpression()
            {
                DbType = dbType,
                Left = left,
                Right = right,
                Operator = @operator
            };
        }

        return left;
    }

    /// <summary>
    /// Analyze multiplication and division in the four arithmetic operations
    /// 解析四则运算中的乘法和除法
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptFourArithmeticOperationsMultiplyOrDivide()
    {
        var left = AcceptBitwiseOperations();
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            SqlBinaryOperator @operator;
            if (Accept(Token.Star))
            {
                @operator = SqlBinaryOperator.Multiply;
            }
            else if (Accept(Token.Slash))
            {
                @operator = SqlBinaryOperator.Divide;
            }
            else if (Accept(Token.Modulus))
            {
                @operator = SqlBinaryOperator.Mod;
            }
            else
            {
                break;
            }

            var right = AcceptBitwiseOperations();

            left = new SqlBinaryExpression()
            {
                DbType = dbType,
                Left = left,
                Right = right,
                Operator = @operator
            };
        }

        return left;
    }
    /// <summary>
    /// 解析位运算，位运算优先级高于加减乘除
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptBitwiseOperations()
    {
        var left = AcceptFourArithmeticOperationsBaseOperationUnit();
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            SqlBinaryOperator @operator;
            if (Accept(Token.Bar))
            {
                @operator = SqlBinaryOperator.BitwiseOr;
            }
            else if (Accept(Token.BitwiseAnd))
            {
                @operator = SqlBinaryOperator.BitwiseAnd;
            }
            else if (Accept(Token.BitwiseXor))
            {
                @operator = SqlBinaryOperator.BitwiseXor;
            }
            else if (IsPgsql && Accept(Token.BitwiseXorForPg))
            {
                @operator = SqlBinaryOperator.BitwiseXorForPg;
            }
            else
            {
                break;
            }

            var right = AcceptFourArithmeticOperationsBaseOperationUnit();

            left = new SqlBinaryExpression()
            {
                DbType = dbType,
                Left = left,
                Right = right,
                Operator = @operator
            };
        }

        return left;
    }

    private int savePoint = -1;
    private int savePoint2 = -1;
    private bool SavePoint()
    {
        if (pos == tokens.Count - 1)
        {
            savePoint = -1;
            return false;
        }
        savePoint = pos;
        return true;
    }
    private bool SavePoint2()
    {
        if (pos == tokens.Count - 1)
        {
            savePoint2 = -1;
            return false;
        }
        savePoint2 = pos;
        return true;
    }
    private void RestoreSavePoint()
    {
        if (savePoint != -1)
        {
            pos = savePoint;
            if (pos - 1 >= 0)
            {
                currentToken = tokens[pos - 1];
            }
            nextToken = tokens[pos];
            savePoint = -1;
        }
    }
    private void RestoreSavePoint2()
    {
        if (savePoint2 != -1)
        {
            pos = savePoint2;
            if (pos - 1 >= 0)
            {
                currentToken = tokens[pos - 1];
            }
            nextToken = tokens[pos];
            savePoint2 = -1;
        }

    }
    private bool AcceptKeywordAsIdentifier()
    {
        if (nextToken == null)
        {
            return false;
        }
        var notAllowTokens = new List<Token>()
        {
            Token.Values,
            Token.Comma,
            Token.When,
            Token.Where,
            Token.Group,
            Token.Order,
            Token.Limit,
            Token.And,
            Token.Or,
            Token.LeftParen,
            Token.RightParen,
            Token.From,
            Token.Join,
            Token.Dot,
            Token.Union,
            Token.Except,
            Token.Intersect,
            Token.With
        };

        if (notAllowTokens.Any(it => CheckNextToken(it)))
        {
            return false;
        }
        SavePoint();
        if (AcceptKeyword())
        {
            var name = GetCurrentTokenValue();
            SavePoint2();
            if (AcceptAnyOne())
            {

                if (currentToken?.IsToken(Token.Join) == true && (name.ToLowerInvariant() == "left" || name.ToLowerInvariant() == "right") || name.ToLowerInvariant() == "full" || name.ToLowerInvariant() == "cross" || name.ToLowerInvariant() == "inner")
                {
                    RestoreSavePoint();
                    return false;
                }

                if (notAllowTokens.Any(it => currentToken?.IsToken(it) == true) || currentToken?.TokenType == TokenType.Symbol || currentToken?.TokenType == TokenType.Operator)
                {
                    RestoreSavePoint2();
                    return true;
                }

                var joinTokens = new List<Token>()
                {
                    Token.Left,
                    Token.Right,
                    Token.Inner,
                    Token.Full,
                    Token.Cross
                };
                if (joinTokens.Any(it => currentToken?.IsToken(it) == true))
                {
                    RestoreSavePoint2();
                    return true;
                }
            }
            else
            {
                return true;
            }

        }
        RestoreSavePoint();
        return false;
    }

    private SqlExpression AcceptPgsqlSpecialCaseAs(SqlExpression body)
    {
        if (IsPgsql)
        {
            SqlExpression result;
            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                if (Accept(Token.ColonColon))
                {
                    var targetTypeNameStringBuilder = new StringBuilder();

                    void AppendCurrentTokenValue()
                    {
                        targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                        targetTypeNameStringBuilder.Append(" ");
                    }

                    if ((AcceptSpecifiedWord("TIMESTAMP") || Accept(Token.Time)))
                    {
                        AppendCurrentTokenValue();
                        if (Accept(Token.With))
                        {
                            AppendCurrentTokenValue();
                            if (Accept(Token.Time))
                            {
                                AppendCurrentTokenValue();
                                if (Accept(Token.Zone))
                                {
                                    targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                                }
                                else
                                {
                                    ThrowSqlParsingErrorException();
                                }
                            }
                            else
                            {
                                ThrowSqlParsingErrorException();
                            }
                        }
                    }
                    else
                    {
                        AcceptAnyOne();
                        targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                        if (AcceptSpecifiedWord("varying"))
                        {
                            targetTypeNameStringBuilder.Append(" ");
                            targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                        }
                    }

                    var isParentheses = false;
                    if (Accept(Token.LeftParen))
                    {
                        targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                        var k = 0;
                        while (true)
                        {
                            if (k >= whileMaximumNumberOfLoops)
                            {
                                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                            }

                            k++;
                            if (Accept(Token.RightParen))
                            {
                                targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                                isParentheses = true;
                                break;
                            }
                            AcceptAnyOne();
                            targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                        }
                    }

                    if (Accept(Token.LeftSquareBracket))
                    {
                        targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                        AcceptOrThrowException(Token.RightSquareBracket);
                        targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                        isParentheses = true;
                    }

                    var targetTypeName = targetTypeNameStringBuilder.ToString().TrimEnd();
                    body = new SqlFunctionCallExpression()
                    {
                        DbType = dbType,
                        Name = new SqlIdentifierExpression()
                        {
                            DbType = dbType,
                            Value = "cast"
                        },
                        Arguments = new List<SqlExpression>()
                        {
                            body
                        },
                        CaseAsTargetType = new SqlIdentifierExpression()
                        {
                            DbType = dbType,
                            Value = targetTypeName
                        }
                    };
                    if (CheckNextTokenIsSplitToken())
                    {
                        break;
                    }

                    if (isParentheses)
                    {
                        break;
                    }

                    if (!CheckNextToken(Token.ColonColon))
                    {
                        break;
                    }
                }
            }

            return body;
        }

        return null;
    }

    /// <summary>
    /// Analyze the basic units in the four arithmetic operations
    /// 解析四则运算中的基础单元
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptFourArithmeticOperationsBaseOperationUnit()
    {
        if (Accept(Token.LeftParen))
        {
            var expr = new SqlExpression();
            if (CheckNextToken(Token.Select))
            {
                expr = AcceptSelectExpression();
            }
            else
            {
                expr = isOnlyRecursiveFourArithmeticOperations ? AcceptFourArithmeticOperationsAddOrSub() : AcceptLogicalExpression();
            }

            expr = AcceptAtTimeZoneExpression(expr) ?? expr;
            AcceptOrThrowException(Token.RightParen);
            return expr;
        }

        SqlExpression body = new SqlExpression();

        if (CheckNextToken(Token.NumberConstant) || CheckNextToken(Token.Sub))
        {
            var isNegative = Accept(Token.Sub);
            if (Accept(Token.NumberConstant))
            {
                var number = GetCurrentTokenNumberValue();
                body = new SqlNumberExpression()
                {
                    DbType = dbType,
                    LeftQualifiers = currentToken.HasValue ? currentToken.Value.LeftQualifiers : "",
                    RightQualifiers = currentToken.HasValue ? currentToken.Value.RightQualifiers : "",
                    Value = (isNegative ? -1 : 1) * number
                };
            }

        }
        else if (Accept(Token.StringConstant))
        {
            var txt = GetCurrentTokenValue();
            body = new SqlStringExpression()
            {
                DbType = dbType,
                Value = txt
            };

        }
        else if (Accept(Token.Star))
        {
            return new SqlAllColumnExpression() { DbType = dbType, };
        }
        else if (Accept(Token.At) || Accept(Token.Colon))
        {
            var prefix = GetCurrentTokenValue();
            AcceptOrThrowException(Token.IdentifierString);
            var name = GetCurrentTokenValue();
            var result = new SqlVariableExpression()
            {
                DbType = dbType,
                Prefix = prefix,
                Name = name
            };
            return result;
        }
        else if (Accept(Token.Exists))
        {
            AcceptOrThrowException(Token.LeftParen);
            var result = new SqlExistsExpression()
            {
                DbType = dbType,
                Body = AcceptSelectExpression()
            };
            AcceptOrThrowException(Token.RightParen);
            return result;
        }
        else if (Accept(Token.Any))
        {
            var any = currentToken;
            AcceptOrThrowException(Token.LeftParen);
            var result = new SqlAnyExpression()
            {
                DbType = dbType,
                Body = AcceptSelectExpression(),
                TokenContext = new SqlAnyExpressionTokenContext()
                {
                    Any = any
                }
            };
            AcceptOrThrowException(Token.RightParen);
            return result;
        }
        else if (Accept(Token.All))
        {
            var all = currentToken;
            AcceptOrThrowException(Token.LeftParen);
            var result = new SqlAllExpression()
            {
                DbType = dbType,
                Body = AcceptSelectExpression(),
                TokenContext = new SqlAllExpressionTokenContext()
                {
                    All = all
                }
            };
            AcceptOrThrowException(Token.RightParen);
            return result;
        }
        else if (Accept(Token.Case))
        {
            var caseToken = currentToken;
            var tokenContext = new SqlCaseExpressionTokenContext()
            {
                Case = caseToken
            };
            var result = new SqlCaseExpression()
            {
                DbType = dbType,
                Items = new List<SqlCaseItemExpression>(),
                TokenContext = tokenContext
            };

            if (!CheckNextToken(Token.When))
            {
                var value = AcceptLogicalExpression();
                result.Value = value;
            }

            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                if (nextToken == null || CheckNextToken(Token.Else) || CheckNextToken(Token.End))
                {
                    break;
                }

                var item = new SqlCaseItemExpression() { DbType = dbType, };
                AcceptOrThrowException(Token.When);
                var whenToken = currentToken;
                item.Condition = AcceptNestedComplexExpression();
                AcceptOrThrowException(Token.Then);
                var thenToken = currentToken;
                item.Value = AcceptNestedComplexExpression();
                item.TokenContext = new SqlCaseItemExpressionTokenContext()
                {
                    When = whenToken,
                    Then = thenToken
                };
                result.Items.Add(item);
            }
            if (Accept(Token.Else))
            {
                var elseToken = currentToken;
                tokenContext.Else = elseToken;
                result.Else = AcceptNestedComplexExpression();
            }

            AcceptOrThrowException(Token.End);
            var endToken = currentToken;
            tokenContext.End = endToken;
            return result;
        }
        else if (Accept(Token.Null))
        {
            return new SqlNullExpression() { DbType = dbType, };
        }
        else if (Accept(Token.True))
        {
            var trueToken = currentToken;
            return new SqlBoolExpression()
            {
                DbType = dbType,
                Value = true,
                TokenContext = new SqlBoolExpressionTokenContext()
                {
                    Bool = trueToken
                }
            };
        }
        else if (Accept(Token.False))
        {
            var falseToken = currentToken;
            return new SqlBoolExpression()
            {
                DbType = dbType,
                Value = false,
                TokenContext = new SqlBoolExpressionTokenContext()
                {
                    Bool = falseToken
                }
            };
        }
        else if (nextToken == null)
        {
            return null;
        }
        else if (Accept(Token.IdentifierString))
        {
            var name = GetCurrentTokenValue();
            var mainToken = currentToken;
            if (Accept(Token.Dot))
            {
                if (Accept(Token.IdentifierString) || Accept(Token.Star))
                {
                    var propertyName = GetCurrentTokenValue();
                    if (CheckNextToken(Token.LeftParen))
                    {
                        body = AcceptFunctionCall(name + "." + propertyName);
                    }
                    else
                    {
                        var sqlPropertyExpression = new SqlPropertyExpression() { DbType = dbType, };
                        var sqlIdentifierExpression = new SqlIdentifierExpression()
                        {
                            DbType = dbType,
                            LeftQualifiers = mainToken.HasValue ? mainToken.Value.LeftQualifiers : "",
                            RightQualifiers = mainToken.HasValue ? mainToken.Value.RightQualifiers : "",
                            Value = name
                        };
                        var sqlPropertyNameIdentifierExpression = new SqlIdentifierExpression()
                        {
                            DbType = dbType,
                            LeftQualifiers = currentToken.HasValue ? currentToken.Value.LeftQualifiers : "",
                            RightQualifiers = currentToken.HasValue ? currentToken.Value.RightQualifiers : "",
                            Value = propertyName
                        };
                        sqlPropertyExpression.Name = sqlPropertyNameIdentifierExpression;
                        sqlPropertyExpression.Table = sqlIdentifierExpression;
                        body = sqlPropertyExpression;
                    }
                }
                else
                {
                    ThrowSqlParsingErrorException();
                }
            }
            else if (CheckNextToken(Token.LeftParen))
            {
                var result = AcceptFunctionCall(name);
                return result;
                //if (functions.Any(it => it.ToLower() == txt.ToLower()))
                //{

                //}
                //else
                //{
                //    throw new Exception($"无法识别函数{txt}");
                //}

            }
            else if (name.ToLowerInvariant() == "n" && Accept(Token.StringConstant))
            {
                //nchar
                var txt = GetCurrentTokenValue();
                body = new SqlStringExpression()
                {
                    DbType = dbType,
                    IsUniCode = true,
                    Value = txt
                };
            }
            else
            {
                var sqlIdentifierExpression = new SqlIdentifierExpression()
                {
                    DbType = dbType,
                    LeftQualifiers = mainToken.HasValue ? mainToken.Value.LeftQualifiers : "",
                    RightQualifiers = mainToken.HasValue ? mainToken.Value.RightQualifiers : "",
                    Value = name
                };
                body = sqlIdentifierExpression;
            }
        }
        else if ((IsPgsql || IsOracle || IsMySql) && CheckNextToken(Token.Interval))
        {
            var intervalExpression = AcceptIntervalExpression();
            return intervalExpression;
        }
        else
        {
            ThrowSqlParsingErrorException();
        }

        if (IsPgsql && CheckNextToken(Token.ColonColon))
        {
            var result = AcceptPgsqlSpecialCaseAs(body);
            return result;
        }

        return body;

    }

    private SqlIntervalExpression AcceptIntervalExpression()
    {
        if ((IsPgsql || IsOracle || IsMySql))
        {
            AcceptOrThrowException(Token.Interval);
            var result = new SqlIntervalExpression()
            {
                DbType = dbType
            };
            if (IsPgsql)
            {
                AcceptOrThrowException(Token.StringConstant);
                var value = GetCurrentTokenValue();
                result.Body = new SqlStringExpression()
                {
                    DbType = dbType,
                    Value = value
                };
                return result;
            }
            else if (IsOracle)
            {
                AcceptOrThrowException(Token.StringConstant);
                var body = GetCurrentTokenValue();
                var sb = new StringBuilder();
                if (AcceptTimeUnit())
                {
                    var value = GetCurrentTokenValue();
                    if (Accept(Token.LeftParen))
                    {
                        value += "(";
                        AcceptOrThrowException(Token.NumberConstant);
                        var numberValue = GetCurrentTokenValue();
                        value += numberValue;
                        AcceptOrThrowException(Token.RightParen);
                        value += ")";
                    }

                    sb.Append(value);

                    if (Accept(Token.To))
                    {
                        var toValue = GetCurrentTokenValue();
                        sb.Append(" " + toValue);
                        if (AcceptTimeUnit())
                        {
                            var twoValue = GetCurrentTokenValue();
                            if (Accept(Token.LeftParen))
                            {
                                twoValue += "(";
                                AcceptOrThrowException(Token.NumberConstant);
                                var numberValue = GetCurrentTokenValue();
                                twoValue += numberValue;
                                AcceptOrThrowException(Token.RightParen);
                                twoValue += ")";
                            }

                            sb.Append(" " + twoValue);
                        }
                    }
                }

                result.Body = new SqlStringExpression()
                {
                    DbType = dbType,
                    Value = body
                };
                result.Unit = new SqlTimeUnitExpression()
                {
                    DbType = dbType,
                    Unit = sb.ToString()
                };
                return result;
            }
            else if (IsMySql)
            {
                AcceptOrThrowException(Token.NumberConstant);
                var value = GetCurrentTokenNumberValue();
                var unit = "";
                if (AcceptTimeUnit())
                {
                    unit = GetCurrentTokenValue();
                }
                else
                {
                    ThrowSqlParsingErrorException();
                }

                result.Body = new SqlNumberExpression()
                {
                    DbType = dbType,
                    Value = value
                };

                result.Unit = new SqlTimeUnitExpression()
                {
                    DbType = dbType,
                    Unit = unit
                };
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Accepts time units；接受时间单位
    /// </summary>
    /// <returns></returns>
    private bool AcceptTimeUnit()
    {
        return AcceptSpecifiedWords(timeUnitSet);
    }

    private bool AcceptSpecifiedWords(HashSet<string> set)
    {
        if (nextToken.HasValue && !string.IsNullOrWhiteSpace(nextToken.Value.RawValue) && set.Contains(nextToken.Value.RawValue.ToLowerInvariant()))
        {
            GetNextToken();
            return true;
        }

        return false;
    }

    private bool AcceptSpecifiedWord(string word)
    {
        if (nextToken.HasValue && !string.IsNullOrWhiteSpace(nextToken.Value.RawValue) && nextToken.Value.RawValue.ToLowerInvariant() == word.ToLowerInvariant())
        {
            GetNextToken();
            return true;
        }

        return false;
    }

    private SqlFunctionCallExpression AcceptFunctionCall(string functionName)
    {
        AcceptOrThrowException(Token.LeftParen);
        var arguments = new List<SqlExpression>();
        var result = new SqlFunctionCallExpression()
        {
            DbType = dbType,
            Name = new SqlIdentifierExpression()
            {
                DbType = dbType,
                Value = functionName
            }
        };

        if (Accept(Token.Distinct))
        {
            result.IsDistinct = true;
        }

        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            if (CheckNextToken(Token.RightParen) || nextToken == null)
            {
                break;
            }

            Accept(Token.Comma);
            var argument = AcceptNestedComplexExpression();
            arguments.Add(argument);

            //SELECT CAST('123' AS INTEGER);
            if (Accept(Token.As))
            {
                var targetTypeNameStringBuilder = new StringBuilder();
                var j = 0;
                while (true)
                {
                    if (j >= whileMaximumNumberOfLoops)
                    {
                        throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                    }

                    j++;
                    if (CheckNextToken(Token.RightParen) || nextToken == null)
                    {
                        break;
                    }

                    AcceptAnyOne();
                    targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                    targetTypeNameStringBuilder.Append(" ");
                }

                var targetTypeName = targetTypeNameStringBuilder.ToString().TrimEnd();
                result.CaseAsTargetType = new SqlIdentifierExpression()
                {
                    DbType = dbType,
                    Value = targetTypeName
                };
                break;
            }
            // such as pgsql,EXTRACT(YEAR FROM order_date)
            if ((IsPgsql || IsOracle || IsMySql) && Accept(Token.From) && functionName.ToLowerInvariant() == "extract")
            {
                var fromSource = AcceptNestedComplexExpression();
                result.FromSource = fromSource;
            }
        }

        if (arguments.Count == 0)
        {
            arguments = null;
        }

        result.Arguments = arguments;

        AcceptOrThrowException(Token.RightParen);

        if (result.CaseAsTargetType == null)
        {
            result.WithinGroup = AcceptWithinGroup();

            if (CheckNextToken(Token.Over))
            {
                var over = AcceptOver();

                result.Over = over;
            }
        }

        AppendCollateExpression(result);

        return result;
    }

    private SqlWithinGroupExpression AcceptWithinGroup()
    {
        if (IsSqlServer || IsPgsql || (IsOracle))
        {
            if (Accept(Token.Within) && Accept(Token.Group))
            {
                AcceptOrThrowException(Token.LeftParen);
                var orderBy = AcceptOrderByExpression();
                AcceptOrThrowException(Token.RightParen);
                var result = new SqlWithinGroupExpression()
                {
                    DbType = dbType,
                    OrderBy = orderBy
                };
                return result;
            }
        }

        return null;
    }
    private SqlOverExpression AcceptOver()
    {
        if (Accept(Token.Over))
        {
            AcceptOrThrowException(Token.LeftParen);
            var partitionBy = AcceptPartitionByExpression();
            var orderBy = AcceptOrderByExpression();
            AcceptOrThrowException(Token.RightParen);
            var over = new SqlOverExpression()
            {
                DbType = dbType,
                PartitionBy = partitionBy,
                OrderBy = orderBy
            };
            return over;
        }

        return null;
    }

    private string AcceptAsToken(bool isSelectItem)
    {
        var asStr = "";
        if (Accept(Token.As))
        {
            if (isSelectItem && IsPgsql)
            {
                if (!(Accept(Token.IdentifierString) || AcceptKeyword()))
                {
                    ThrowSqlParsingErrorException();
                }
            }
            else
            {
                AcceptOrThrowException(Token.IdentifierString);
            }

            asStr = GetCurrentTokenValue();
        }
        else if (Accept(Token.IdentifierString))
        {
            asStr = GetCurrentTokenValue();
        }

        //sql server兼容单引号包裹列别名，例如select city 'b' from Address 
        if (IsSqlServer && Accept(Token.StringConstant))
        {
            asStr = GetCurrentTokenValue();
            currentToken = new Token()
            {
                LeftQualifiers = "'",
                RightQualifiers = "'",
            };
        }

        return asStr;
    }

    private SqlExpression AcceptTableSourceExpression()
    {
        //单表或者多表关联
        var tableOrJoinTable = AcceptTableOrJoinTableSourceExpression();
        var result = AcceptPivotTable(tableOrJoinTable);
        return result;
    }

    private SqlExpression AcceptPivotTable(SqlExpression source)
    {
        if ((IsOracle || IsSqlServer) && Accept(Token.Pivot))
        {
            AcceptOrThrowException(Token.LeftParen);
            AcceptOrThrowException(Token.IdentifierString);
            var name = GetCurrentTokenValue();
            var functionCall = AcceptFunctionCall(name);
            AcceptOrThrowException(Token.For);
            this.isOnlyRecursiveFourArithmeticOperations = true;
            var @for = AcceptFourArithmeticOperationsAddOrSub();
            this.isOnlyRecursiveFourArithmeticOperations = false;
            AcceptOrThrowException(Token.In);
            AcceptOrThrowException(Token.LeftParen);
            var @in = new List<SqlExpression>();
            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }
                i++;
                if (nextToken == null || CheckNextToken(Token.RightParen) || !(i == 1 || Accept(Token.Comma)))
                {
                    break;
                }

                var value = AcceptNestedComplexExpression();
                var item = new SqlSelectItemExpression()
                {
                    DbType = dbType,
                    Body = value
                };
                if (IsOracle && Accept(Token.As))
                {
                    AcceptOrThrowException(Token.IdentifierString);
                    var alias = GetCurrentTokenValue();
                    item.Alias = new SqlIdentifierExpression()
                    {
                        DbType = dbType,
                        LeftQualifiers = currentToken.HasValue ? currentToken.Value.LeftQualifiers : "",
                        RightQualifiers = currentToken.HasValue ? currentToken.Value.RightQualifiers : "",
                        Value = alias
                    };
                }
                @in.Add(item);
            }

            AcceptOrThrowException(Token.RightParen);
            AcceptOrThrowException(Token.RightParen);
            var result = new SqlPivotTableExpression()
            {
                DbType = dbType,
                SubQuery = source,
                FunctionCall = functionCall,
                For = @for,
                In = @in
            };
            if (IsSqlServer)
            {
                AcceptOrThrowException(Token.As);
                AcceptOrThrowException(Token.IdentifierString);
                var alias = GetCurrentTokenValue();
                result.Alias = new SqlIdentifierExpression()
                {
                    DbType = dbType,
                    LeftQualifiers = currentToken.HasValue ? currentToken.Value.LeftQualifiers : "",
                    RightQualifiers = currentToken.HasValue ? currentToken.Value.RightQualifiers : "",
                    Value = alias
                };
            }
            if (IsOracle && Accept((Token.IdentifierString)))
            {
                var alias = GetCurrentTokenValue();
                result.Alias = new SqlIdentifierExpression()
                {
                    DbType = dbType,
                    Value = alias
                };
            }

            return result;
        }

        return source;
    }

    private SqlExpression AcceptTableOrJoinTableSourceExpression()
    {
        SqlExpression left = AcceptTableExpression();
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
            SqlJoinType joinType;
            //Determine whether the on condition is required 
            //判断on条件是否是必须的
            var isRequireOnCondition = true;

            /// use comma split，eg. select * from test t,test11 t1 where t.name =t1.name
            /// 使用逗号分隔的join语法，select * from test t,test11 t1 where t.name =t1.name
            var isCommaJoin = false;
            if (Accept(Token.Inner))
            {
                joinType = SqlJoinType.InnerJoin;
            }
            else if (CheckNextToken(Token.Join))
            {
                joinType = SqlJoinType.InnerJoin;
            }
            else if (Accept(Token.Left))
            {
                joinType = SqlJoinType.LeftJoin;
            }
            else if (Accept(Token.Right))
            {
                joinType = SqlJoinType.RightJoin;
            }
            else if (Accept(Token.Full))
            {
                joinType = SqlJoinType.FullJoin;
            }
            else if (Accept(Token.Cross))
            {
                isRequireOnCondition = false;
                joinType = SqlJoinType.CrossJoin;
            }
            else if (Accept(Token.Comma))
            {
                isRequireOnCondition = false;
                isCommaJoin = true;
                joinType = SqlJoinType.CommaJoin;
            }
            else
            {
                break;
            }

            if (Accept(Token.Join) || (isCommaJoin))
            {

                var right = AcceptTableExpression();

                SqlExpression conditions = null;
                if (isRequireOnCondition || IsMySql)
                {
                    AcceptOrThrowException(Token.On);
                    conditions = AcceptNestedComplexExpression();
                }

                left = new SqlJoinTableExpression()
                {
                    DbType = dbType,
                    Left = left,
                    Right = right,
                    JoinType = joinType,
                    Conditions = conditions
                };

            }
        }

        return left;
    }

    private SqlOrderByExpression AcceptOrderByExpression()
    {
        var isSiblings = false;
        var isOrderBy = false;
        if (Accept(Token.Order))
        {
            if (Accept(Token.Siblings))
            {
                isSiblings = true;
            }
            if (Accept(Token.By))
            {
                isOrderBy = true;
            }
        }
        if (isOrderBy)
        {
            var items = new List<SqlOrderByItemExpression>();
            var result = new SqlOrderByExpression()
            {
                DbType = dbType,
                Items = items,
                IsSiblings = isSiblings
            };
            var i = 0;

            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                if (nextToken == null
                    || (IsMySql && nextToken.HasValue && nextToken.Value.IsToken(Token.Limit))
                    || (IsSqlServer && nextToken.HasValue && nextToken.Value.IsToken(Token.Offset))
                    || (IsOracle && nextToken.HasValue && nextToken.Value.IsToken(Token.Fetch))
                    || (IsPgsql && nextToken.HasValue && (nextToken.Value.IsToken(Token.Limit) || nextToken.Value.IsToken(Token.Offset)))
                    || (nextToken.HasValue && (nextToken.Value.IsToken(Token.RightParen))))
                {
                    break;
                }

                if (i == 1 || Accept(Token.Comma))
                {
                    var item = AcceptNestedComplexExpression();

                    SqlOrderByType? orderByType = null;
                    if (Accept(Token.Asc))
                    {
                        orderByType = SqlOrderByType.Asc;
                    }
                    else if (Accept(Token.Desc))
                    {
                        orderByType = SqlOrderByType.Desc;
                    }

                    SqlOrderByNullsType? nullsType = null;
                    if (IsOracle || IsPgsql || IsSqlite)
                    {
                        if (Accept(Token.Nulls))
                        {
                            if (Accept(Token.First))
                            {
                                nullsType = SqlOrderByNullsType.First;
                            }
                            else if (Accept(Token.Last))
                            {
                                nullsType = SqlOrderByNullsType.Last;
                            }
                            else
                            {
                                throw new Exception("Missing keywords");
                            }
                        }
                    }

                    var orderByItem = new SqlOrderByItemExpression()
                    {
                        DbType = dbType,
                        Body = item,
                        OrderByType = orderByType,
                        NullsType = nullsType
                    };
                    items.Add(orderByItem);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        return null;
    }

    /// <summary>
    /// The collate clause is mainly used to specify string comparison and sorting rules.
    /// collate子句主要用于指定字符串比较和排序的规则
    /// </summary>
    private SqlCollateExpression AcceptCollateExpression()
    {
        if (Accept(Token.Collate))
        {
            SqlExpression body = null;
            if (Accept(Token.IdentifierString))
            {
                var bodyToken = currentToken;
                body = new SqlIdentifierExpression()
                {
                    LeftQualifiers = bodyToken.HasValue ? bodyToken.Value.LeftQualifiers : "",
                    RightQualifiers = bodyToken.HasValue ? bodyToken.Value.RightQualifiers : "",
                    Value = GetTokenValue(bodyToken),
                    DbType = dbType
                };
            }
            else if (Accept(Token.StringConstant))
            {
                var value = GetCurrentTokenValue();
                body = new SqlStringExpression()
                {
                    DbType = dbType,
                    Value = value
                };
            }
            else
            {
                ThrowSqlParsingErrorException();
            }

            var result = new SqlCollateExpression()
            {
                DbType = dbType,
                Body = body
            };
            return result;
        }

        return null;
    }
    private SqlPartitionByExpression AcceptPartitionByExpression()
    {
        if (Accept(Token.Partition) && Accept(Token.By))
        {
            var items = new List<SqlExpression>();
            var result = new SqlPartitionByExpression()
            {
                DbType = dbType,
                Items = items
            };
            var i = 0;

            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                if (nextToken == null
                   || (nextToken.HasValue && (nextToken.Value.IsToken(Token.Order)))
                   || (nextToken.HasValue && (nextToken.Value.IsToken(Token.RightParen))))
                {
                    break;
                }

                if (i == 1 || Accept(Token.Comma))
                {
                    var item = AcceptNestedComplexExpression();
                    AppendCollateExpression(item);
                    items.Add(item);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        return null;
    }

    private SqlConnectByExpression AcceptConnectByExpression()
    {
        SqlExpression startWith = null;
        if (Accept(Token.Start) && Accept(Token.With))
        {
            startWith = AcceptNestedComplexExpression();
        }

        if (Accept(Token.Connect) && Accept(Token.By))
        {
            var isNocycle = Accept(Token.Nocycle);
            var isPrior = Accept(Token.Prior);
            var body = AcceptNestedComplexExpression();
            var orderBy = AcceptOrderByExpression();
            var result = new SqlConnectByExpression()
            {
                DbType = dbType,
                StartWith = startWith,
                OrderBy = orderBy,
                IsNocycle = isNocycle,
                IsPrior = isPrior,
                Body = body
            };

            return result;
        }

        return null;
    }

    private SqlGroupByExpression AcceptGroupByExpression()
    {
        if (Accept(Token.Group) && Accept(Token.By))
        {
            var items = new List<SqlExpression>();
            var result = new SqlGroupByExpression()
            {
                DbType = dbType,
                Items = items
            };
            var i = 0;

            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;

                if (i == 1 || Accept(Token.Comma))
                {
                    var item = AcceptNestedComplexExpression();
                    AppendCollateExpression(item);
                    items.Add(item);
                }
                else
                {
                    break;
                }

                if (CheckNextToken(Token.Having) || nextToken == null)
                {
                    break;
                }
            }

            if (Accept(Token.Having))
            {
                var having = AcceptNestedComplexExpression();
                result.Having = having;
            }

            return result;
        }

        return null;
    }

    private void AppendCollateExpression(SqlExpression item)
    {
        var collate = AcceptCollateExpression();
        if (collate != null)
        {
            if (item is ICollateExpression collateExpression)
            {
                collateExpression.Collate = collate;
            }
        }
    }

    private SqlExpression AcceptWhereExpression()
    {
        if (Accept(Token.Where))
        {
            var result = AcceptNestedComplexExpression();
            return result;

        }

        return null;
    }

    private string GetCurrentTokenValue()
    {
        if (currentToken.HasValue)
        {
            if (currentToken.Value.IsKeyWord || currentToken.Value.IsHints)
            {
                return currentToken.Value.RawValue;
            }
            return currentToken.Value.Value.ToString();
        }

        return "";
    }

    private string GetTokenValue(Token? token)
    {
        if (token.HasValue)
        {
            if (token.Value.TokenType == TokenType.Keyword)
            {
                return token.Value.RawValue;
            }
            return token.Value.Value.ToString();
        }

        return "";
    }

    private decimal GetCurrentTokenNumberValue()
    {
        if (currentToken.HasValue)
        {
            return (decimal)currentToken.Value.Value;
        }

        return 0;
    }
    private bool Accept(Token token)
    {
        if (nextToken.HasValue && nextToken.Value.IsToken(token))
        {
            GetNextToken();
            return true;
        }

        if (token.IsToken(Token.IdentifierString))
        {
            return AcceptKeywordAsIdentifier();
        }
        return false;
    }

    private bool AcceptOrThrowException(Token token)
    {
        if (Accept(token))
        {
            return true;
        }

        ThrowSqlParsingErrorException();
        return false;
    }

    private bool AcceptAnyOne()
    {
        if (nextToken.HasValue)
        {
            GetNextToken();
            return true;
        }

        return false;
    }
    private bool AcceptKeyword()
    {
        if (nextToken?.IsKeyWord == true)
        {
            GetNextToken();
            return true;
        }

        return false;
    }
    private bool CheckNextToken(Token token)
    {
        if (nextToken.HasValue && nextToken.Value.IsToken(token))
        {
            return true;
        }

        return false;
    }

    private bool CheckPreviewToken(Token token)
    {
        return pos - 1 >= 0 && tokens[pos - 1].IsToken(token);
    }

    private bool CheckNextNextToken(Token token)
    {
        if (pos + 1 <= tokens.Count - 1)
        {
            var nextNextToken = tokens[pos + 1];
            if (nextNextToken.IsToken(token))
            {
                return true;
            }
        }

        return false;
    }
    private bool CheckNextTokenIsSplitToken()
    {
        if (nextToken.HasValue && splitTokenDics.ContainsKey(nextToken.Value) || nextToken == null)
        {
            return true;
        }

        return false;
    }
    private bool CheckCurrentToken(Token token)
    {
        if (currentToken.HasValue && currentToken.Value.IsToken(token))
        {
            return true;
        }

        return false;
    }
    private void GetNextToken()
    {
        currentToken = nextToken;
        if (pos + 1 <= tokens.Count - 1)
        {
            pos++;
            nextToken = tokens[pos];
        }
        else
        {
            nextToken = null;
        }
    }
}