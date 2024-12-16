﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    private static Dictionary<Token, bool> splitTokenDics = new Dictionary<Token, bool>();
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
            Token.Xor,
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
        };
        splitTokenDics = splitTokens.ToDictionary(it => it, it => true);
    }

    private ParseType? parseType;

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

        //var t12 = TimeUtils.TestMicrosecond((() =>
        //{
        //    MergeEscapeCharacterCharAndRemoveAllComment();
        //}));

        if (CheckNextToken(Token.Select) || CheckNextToken(Token.With) || CheckNextToken(Token.LeftParen))
        {
            parseType = ParseType.Select;
            var result = new SqlSelectExpression();
            result = AcceptSelectExpression();
            result.Comments = comments;
            //var t = TimeUtils.TestMicrosecond((() =>
            //{


            //}));
            return result;
            //return result;
        }
        else if (CheckNextToken(Token.Update))
        {
            parseType = ParseType.Update;
            var result = AcceptUpdateExpression();
            result.Comments = comments;
            return result;
        }
        else if (CheckNextToken(Token.Delete))
        {
            parseType = ParseType.Delete;
            var result = AcceptDeleteExpression();
            result.Comments = comments;
            return result;
        }
        else if (CheckNextToken(Token.Insert))
        {
            parseType = ParseType.Insert;
            var result = AcceptInsertExpression();
            result.Comments = comments;
            return result;
        }

        throw new Exception("不识别该种解析类型");
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
        var result = new SqlInsertExpression();
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
        var result = new SqlDeleteExpression();
        AcceptOrThrowException(Token.From);
        result.Table = AcceptTableExpression();
        result.Where = AcceptWhereExpression();
        return result;
    }

    private SqlUpdateExpression AcceptUpdateExpression()
    {
        AcceptOrThrowException(Token.Update);
        var result = new SqlUpdateExpression();

        result.Table = AcceptTableExpression();

        result.Items = AcceptUpdateItemsExpression();

        result.Where = AcceptWhereExpression();
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
                    Value = subQueryAlias
                };
            }
            return subQuery;
        }

        var isFrom = CheckCurrentToken(Token.From);
        AcceptOrThrowException(Token.IdentifierString);

        var name = GetCurrentTokenValue();
        var mainToken = currentToken;
        //var nameList = new List<string>() { name };
        var nameTokenList = new List<Token?>() { mainToken };

        var dbLinkName = "";
        //such as:SELECT * FROM TABLE(splitstr('a;b',';'))
        if (isFrom && CheckNextToken(Token.LeftParen))
        {
            var functionCall = AcceptFunctionCall(name);
            var table = new SqlReferenceTableExpression()
            {
                FunctionCall = functionCall
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
                var isOracleDbLink = dbType == DbType.Oracle && Accept(Token.At);
                if (Accept(Token.Dot))
                {
                    if (Accept(Token.IdentifierString))
                    {
                        var value = GetCurrentTokenValue();
                        mainToken = currentToken;
                        //nameList.Add(value);
                        nameTokenList.Add(mainToken);
                    }
                    else
                    {
                        throw new Exception("sql error");
                    }
                }
                else if (isOracleDbLink)
                {
                    if (Accept(Token.IdentifierString))
                    {
                        dbLinkName = GetCurrentTokenValue();
                    }
                    else
                    {
                        throw new Exception("sql error");
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
                    Value = GetTokenValue(nameTokenList.Last())
                }
            };

            if (!string.IsNullOrWhiteSpace(dbLinkName))
            {
                table.DbLink = new SqlIdentifierExpression()
                {
                    Value = dbLinkName
                };
            }

            if (nameTokenList.Count > 1)
            {
                nameTokenList.RemoveAt(nameTokenList.Count - 1);
                var schema = string.Join(".", nameTokenList.Where(x => x != null)
                    .Select(y => y.Value.LeftQualifiers + y.Value.Value + y.Value.RightQualifiers));
                table.Schema = new SqlIdentifierExpression()
                {
                    Value = schema
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
                    Value = alias
                };
            }
            return table;
        }

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
            Accept(Token.Comma);
            var item = AcceptNestedComplexExpression();
            items.Add(item);
            if (CheckNextToken(Token.Where) || nextToken == null)
            {
                break;
            }
        }

        return items;
    }

    private SqlSelectExpression AcceptSelectExpression()
    {
        var left = AcceptSelectExpression2();
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
                var right = AcceptSelectExpression2();
                total = new SqlUnionQueryExpression()
                {
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
            Query = total
        };
        return sqlSelectExpression;
    }

    private SqlSelectExpression AcceptSelectExpression2()
    {
        if (Accept(Token.LeftParen))
        {
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
        var query = new SqlSelectQueryExpression();
        query.WithSubQuerys = AcceptWithSubQueryExpression();

        AcceptOrThrowException(Token.Select);

        query.ResultSetReturnOption = AcceptResultSetReturnOption();

        query.Top = AcceptTopN();

        query.Columns = AcceptSelectItemsExpression();

        query.Into = AcceptSelectInto();
        if (Accept(Token.From))
        {
            query.From = AcceptTableSourceExpression();
            query.Where = AcceptWhereExpression();
            query.GroupBy = AcceptGroupByExpression();
            query.OrderBy = AcceptOrderByExpression();
            if (dbType == DbType.Oracle)
            {
                query.ConnectBy = AcceptConnectByExpression();
            }
            query.Limit = AcceptLimitExpression();
        }

        var result = new SqlSelectExpression()
        {
            Query = query
        };
        return result;
    }

    private SqlLimitExpression AcceptLimitExpression()
    {
        if (dbType == DbType.MySql)
        {
            return AcceptMysqlLimitExpression();
        }
        else if (dbType == DbType.SqlServer)
        {
            return AcceptSqlServerLimitExpression();
        }
        else if (dbType == DbType.Pgsql)
        {
            return AcceptPgsqlLimitExpression();
        }
        else if (dbType == DbType.Oracle)
        {
            return AcceptOracleLimitExpression();
        }

        return null;
    }
    private SqlLimitExpression AcceptSqlServerLimitExpression()
    {
        if (Accept(Token.Offset))
        {
            var result = new SqlLimitExpression();
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
        if (Accept(Token.Fetch))
        {
            var result = new SqlLimitExpression();
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
        var result = new SqlLimitExpression();
        if (Accept(Token.Limit))
        {

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
        else if (Accept(Token.Offset))
        {
            var offset = AcceptNestedComplexExpression();
            result.Offset = offset;
            return result;
        }

        return null;
    }

    private SqlLimitExpression AcceptMysqlLimitExpression()
    {
        if (Accept(Token.Limit))
        {
            var result = new SqlLimitExpression();
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
        if (dbType == DbType.SqlServer)
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
        if (dbType == DbType.SqlServer)
        {
            if (Accept(Token.Top))
            {
                AcceptOrThrowException(Token.NumberConstant);
                var topCount = GetCurrentTokenNumberValue();
                var result = new SqlTopExpression()
                {
                    Body = new SqlNumberExpression()
                    {
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
        else if (dbType == DbType.Oracle && Accept(Token.Unique))
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
                };
                Accept(Token.Comma);
                AcceptOrThrowException(Token.IdentifierString);
                var alias = GetCurrentTokenValue();
                subQueryExpression.Alias = new SqlIdentifierExpression()
                {
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

    private List<SqlSelectItemExpression> AcceptSelectItemsExpression()
    {
        var result = new List<SqlSelectItemExpression>();
        var item = new SqlSelectItemExpression()
        {
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
                || (dbType == DbType.SqlServer && CheckNextToken(Token.Into))
                || nextToken == null)
            {
                break;
            }

            if (Accept(Token.Comma))
            {
                item = new SqlSelectItemExpression()
                {
                };
            }

            item.Body = AcceptNestedComplexExpression();
            var asStr = AcceptAsToken();
            if (!string.IsNullOrWhiteSpace(asStr))
            {
                item.Alias = new SqlIdentifierExpression()
                {
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
    /// Parsing nested complex expressions
    /// 解析嵌套的复杂表达式
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptNestedComplexExpression()
    {
        var result = AcceptLogicalExpression();
        return result;
    }

    /// <summary>
    /// Parsing logical expressions, and or xor
    /// 解析逻辑表达式，and or xor
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptLogicalExpression()
    {
        var left = AcceptEquationOperationExpression();
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
            else if (Accept(Token.And))
            {
                @operator = SqlBinaryOperator.And;
            }
            else if (Accept(Token.Xor))
            {
                @operator = SqlBinaryOperator.Xor;
            }
            else
            {
                break;
            }

            var right = AcceptEquationOperationExpression();

            left = new SqlBinaryExpression()
            {
                Left = left,
                Right = right,
                Operator = @operator
            };

        }

        return left;
    }
    /// <summary>
    /// 解析等式运算
    /// </summary>
    /// <returns></returns>
    private SqlExpression AcceptEquationOperationExpression()
    {
        var left = AcceptRelationalExpression();
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
            }

            left = new SqlBinaryExpression()
            {
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
        SqlExpression right = null;
        // not in,not like,not between
        var isNot = false;
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
                right = new SqlNullExpression();
            }
            else if (Accept(Token.Between))
            {
                this.isOnlyRecursiveFourArithmeticOperations = true;
                var begin = AcceptFourArithmeticOperationsAddOrSub();
                AcceptOrThrowException(Token.And);
                var end = AcceptFourArithmeticOperationsAddOrSub();
                this.isOnlyRecursiveFourArithmeticOperations = false;
                var result = new SqlBetweenAndExpression()
                {
                    IsNot = isNot,
                    Begin = begin,
                    End = end,
                    Body = left
                };
                return result;
            }
            else if (Accept(Token.In))
            {
                var result = new SqlInExpression()
                {
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
            else if (Accept(Token.BarBar))
            {
                @operator = SqlBinaryOperator.Concat;
            }
            else if (Accept(Token.Not))
            {
                if (Accept(Token.Like))
                {
                    @operator = SqlBinaryOperator.NotLike;
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
            }

            left = new SqlBinaryExpression()
            {
                Left = left,
                Right = right,
                Operator = @operator
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
                }
                else
                {
                    var notResult = new SqlNotExpression()
                    {
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
            else
            {
                break;
            }

            var right = AcceptBitwiseOperations();

            left = new SqlBinaryExpression()
            {
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
            else
            {
                break;
            }

            var right = AcceptFourArithmeticOperationsBaseOperationUnit();

            left = new SqlBinaryExpression()
            {
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
            Token.Comma,
            Token.When,
            Token.Where,
            Token.Group,
            Token.Order,
            Token.Limit,
            Token.Offset,
            Token.And,
            Token.Or,
            Token.Xor,
            Token.LeftParen,
            Token.RightParen,
            Token.From,
            Token.Join,
            Token.Dot,
            Token.Union,
            Token.Except,
            Token.Intersect
        };
        //if (CheckNextTokenIsOperatorOrSymbol())
        //{
        //    return false;
        //}
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
        if (dbType == DbType.Pgsql)
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
                    var j = 0;
                    while (true)
                    {
                        if (j >= whileMaximumNumberOfLoops)
                        {
                            throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                        }

                        j++;
                        if (CheckNextTokenIsSplitToken())
                        {
                            break;
                        }

                        if (CheckNextToken(Token.ColonColon))
                        {
                            break;
                        }
                        AcceptAnyOne();
                        targetTypeNameStringBuilder.Append(GetCurrentTokenValue());
                        targetTypeNameStringBuilder.Append(" ");
                    }

                    var targetTypeName = targetTypeNameStringBuilder.ToString().TrimEnd();
                    body = new SqlFunctionCallExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "cast"
                        },
                        Arguments = new List<SqlExpression>()
                        {
                            body
                        },
                        CaseAsTargetType = new SqlIdentifierExpression()
                        {
                            Value = targetTypeName
                        }
                    };
                    if (CheckNextTokenIsSplitToken())
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
            AcceptOrThrowException(Token.RightParen);
            return expr;
        }
        else if (CheckNextToken(Token.Not))
        {
            var result = AcceptNot();
            return result;
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
                Value = txt
            };

        }
        else if (Accept(Token.Star))
        {
            return new SqlAllColumnExpression();
        }
        else if (Accept(Token.At) || Accept(Token.Colon))
        {
            var prefix = GetCurrentTokenValue();
            AcceptOrThrowException(Token.IdentifierString);
            var name = GetCurrentTokenValue();
            var result = new SqlVariableExpression()
            {
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
                Body = AcceptSelectExpression()
            };
            AcceptOrThrowException(Token.RightParen);
            return result;
        }
        else if (Accept(Token.Any))
        {
            AcceptOrThrowException(Token.LeftParen);
            var result = new SqlAnyExpression()
            {
                Body = AcceptSelectExpression()
            };
            AcceptOrThrowException(Token.RightParen);
            return result;
        }
        else if (Accept(Token.All))
        {
            AcceptOrThrowException(Token.LeftParen);
            var result = new SqlAllExpression()
            {
                Body = AcceptSelectExpression()
            };
            AcceptOrThrowException(Token.RightParen);
            return result;
        }
        else if (Accept(Token.Case))
        {
            var result = new SqlCaseExpression()
            {
                Items = new List<SqlCaseItemExpression>()
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

                var item = new SqlCaseItemExpression();
                AcceptOrThrowException(Token.When);
                item.Condition = AcceptNestedComplexExpression();
                AcceptOrThrowException(Token.Then);
                item.Value = AcceptNestedComplexExpression();
                result.Items.Add(item);
            }
            if (Accept(Token.Else))
            {
                result.Else = AcceptNestedComplexExpression();
            }

            AcceptOrThrowException(Token.End);
            return result;
        }
        else if (Accept(Token.Null))
        {
            return new SqlNullExpression();
        }
        else if (Accept(Token.True))
        {
            return new SqlBoolExpression()
            {
                Value = true
            };
        }
        else if (Accept(Token.False))
        {
            return new SqlBoolExpression()
            {
                Value = false
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
                        var sqlPropertyExpression = new SqlPropertyExpression();
                        var sqlIdentifierExpression = new SqlIdentifierExpression()
                        {
                            LeftQualifiers = mainToken.HasValue ? mainToken.Value.LeftQualifiers : "",
                            RightQualifiers = mainToken.HasValue ? mainToken.Value.RightQualifiers : "",
                            Value = name
                        };
                        var sqlPropertyNameIdentifierExpression = new SqlIdentifierExpression()
                        {
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
                    throw new Exception("sql syntc error");
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
                    IsUniCode = true,
                    Value = txt
                };
            }
            else
            {
                var sqlIdentifierExpression = new SqlIdentifierExpression()
                {
                    LeftQualifiers = mainToken.HasValue ? mainToken.Value.LeftQualifiers : "",
                    RightQualifiers = mainToken.HasValue ? mainToken.Value.RightQualifiers : "",
                    Value = name
                };
                body = sqlIdentifierExpression;
            }
        }
        else
        {
            var startIndex = -1;
            if (pos - 2 >= 0)
            {
                startIndex = tokens[pos - 2].StartPositionIndex;
            }
            else if (pos - 1 >= 0)
            {
                startIndex = tokens[pos - 1].StartPositionIndex;
            }
            else
            {
                startIndex = tokens[pos].StartPositionIndex;
            }
            var endIndex = -1;
            if (pos + 2 <= tokens.Count - 1)
            {
                endIndex = tokens[pos + 2].EndPositionIndex;
            }
            else if (pos + 1 <= tokens.Count - 1)
            {
                endIndex = tokens[pos + 1].EndPositionIndex;
            }
            else
            {
                endIndex = tokens[pos].EndPositionIndex;
            }
            throw new Exception($"An error occurred at position :{nextToken?.StartPositionIndex},near sql is:{Sql.Substring(startIndex, endIndex - startIndex + 1)}");
        }

        if (dbType == DbType.Pgsql && CheckNextToken(Token.ColonColon))
        {
            var result = AcceptPgsqlSpecialCaseAs(body);
            return result;
        }

        return body;

    }

    private SqlExpression AcceptNot()
    {
        if (Accept(Token.Not))
        {
            var expression = AcceptLogicalExpression();
            if (expression is SqlExistsExpression sqlExistsExpression)
            {
                sqlExistsExpression.IsNot = true;
                return sqlExistsExpression;
            }
            else if (expression is SqlInExpression sqlInExpression)
            {
                sqlInExpression.IsNot = true;
                return sqlInExpression;
            }
            else if (expression is SqlBetweenAndExpression sqlBetweenAndExpression)
            {
                sqlBetweenAndExpression.IsNot = true;
                return sqlBetweenAndExpression;
            }
            else
            {
                var result = new SqlNotExpression()
                {
                    Body = expression
                };
                return result;
            }

        }

        return null;
    }

    private SqlFunctionCallExpression AcceptFunctionCall(string functionName)
    {
        AcceptOrThrowException(Token.LeftParen);
        var arguments = new List<SqlExpression>();
        var result = new SqlFunctionCallExpression()
        {
            Name = new SqlIdentifierExpression()
            {
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
                    Value = targetTypeName
                };
                break;
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

        return result;
    }

    private SqlWithinGroupExpression AcceptWithinGroup()
    {
        if (dbType == DbType.SqlServer || dbType == DbType.Pgsql || (dbType == DbType.Oracle))
        {
            if (Accept(Token.Within) && Accept(Token.Group))
            {
                AcceptOrThrowException(Token.LeftParen);
                var orderBy = AcceptOrderByExpression();
                AcceptOrThrowException(Token.RightParen);
                var result = new SqlWithinGroupExpression()
                {
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
                PartitionBy = partitionBy,
                OrderBy = orderBy
            };
            return over;
        }

        return null;
    }

    private string AcceptAsToken()
    {
        var asStr = "";
        if (Accept(Token.As))
        {
            AcceptOrThrowException(Token.IdentifierString);
            asStr = GetCurrentTokenValue();
        }
        else if (Accept(Token.IdentifierString))
        {
            asStr = GetCurrentTokenValue();
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
        if ((dbType == DbType.Oracle || dbType == DbType.SqlServer) && Accept(Token.Pivot))
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
                    Body = value
                };
                if (dbType == DbType.Oracle && Accept(Token.As))
                {
                    AcceptOrThrowException(Token.IdentifierString);
                    var alias = GetCurrentTokenValue();
                    item.Alias = new SqlIdentifierExpression()
                    {
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
                SubQuery = source,
                FunctionCall = functionCall,
                For = @for,
                In = @in
            };
            if (dbType == DbType.SqlServer)
            {
                AcceptOrThrowException(Token.As);
                AcceptOrThrowException(Token.IdentifierString);
                var alias = GetCurrentTokenValue();
                result.Alias = new SqlIdentifierExpression()
                {
                    LeftQualifiers = currentToken.HasValue ? currentToken.Value.LeftQualifiers : "",
                    RightQualifiers = currentToken.HasValue ? currentToken.Value.RightQualifiers : "",
                    Value = alias
                };
            }
            if (dbType == DbType.Oracle && Accept((Token.IdentifierString)))
            {
                var alias = GetCurrentTokenValue();
                result.Alias = new SqlIdentifierExpression()
                {
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
                if (isRequireOnCondition)
                {
                    AcceptOrThrowException(Token.On);
                    conditions = AcceptNestedComplexExpression();
                }

                left = new SqlJoinTableExpression()
                {
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
                    || (dbType == DbType.MySql && nextToken.HasValue && nextToken.Value.IsToken(Token.Limit))
                    || (dbType == DbType.SqlServer && nextToken.HasValue && nextToken.Value.IsToken(Token.Offset))
                    || (dbType == DbType.Oracle && nextToken.HasValue && nextToken.Value.IsToken(Token.Fetch))
                    || (dbType == DbType.Pgsql && nextToken.HasValue && (nextToken.Value.IsToken(Token.Limit) || nextToken.Value.IsToken(Token.Offset)))
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
                    if (dbType == DbType.Oracle || dbType == DbType.Pgsql || dbType == DbType.Sqlite)
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

    private SqlPartitionByExpression AcceptPartitionByExpression()
    {
        if (Accept(Token.Partition) && Accept(Token.By))
        {
            var items = new List<SqlExpression>();
            var result = new SqlPartitionByExpression()
            {
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
                StartWith = startWith,
                OrderBy = orderBy,
                IsNocycle = isNocycle,
                IsPrior = isPrior,
                Body = body
            };
            body.Parent = result;

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
            if (currentToken.Value.TokenType == TokenType.Keyword)
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

        if (parseType == ParseType.Select && token.IsToken(Token.IdentifierString))
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

        throw new Exception("sql语句有语意错误");
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