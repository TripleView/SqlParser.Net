﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Net.Lexer;

public class SqlLexer
{
    private int pos = -1;

    private Token currentToken;
    private Token nextToken;
    private char? currentChar;
    private char? nextChar;
    private char? nextNextChar;
    private List<char> chars;
    /// <summary>
    /// 标识符首字母列表
    /// </summary>
    private Dictionary<char, bool> charDic = new Dictionary<char, bool>();
    private Dictionary<char, bool> digitDic = new Dictionary<char, bool>();
    private ConcurrentDictionary<string, Token> tokenDic = new ConcurrentDictionary<string, Token>();
    /// <summary>
    /// Token dictionary for all database types
    /// 所有数据库类型的token字典
    /// </summary>
    private static ConcurrentDictionary<DbType, ConcurrentDictionary<string, Token>> allDbTypeTokenDic = new ConcurrentDictionary<DbType, ConcurrentDictionary<string, Token>>();
    private List<Token> tokens = new List<Token>();
    private DbType dbType
        ;
    //static SqlLexer()
    //{
    //    InitTokenDic();
    //}
    public List<Token> Parse(string sql, DbType dbType)
    {
        this.dbType = dbType;
        //tokens.Clear();
        InitTokenDic();
        //Only recognize line breaks \n
        //仅识别换行符\n
        sql = sql.Replace("\r\n", "\n");

        chars = sql.Select(it => it).ToList();

        InitCharDic();
        InitDigitDic();

        GetNextChar();

        //最大循环次数，避免死循环
        var maxCount = 100000;
        var i = 0;
        while (true)
        {
            if (i >= maxCount)
            {
                throw new Exception($"The number of SQL parsing times exceeds {maxCount}");
            }

            i++;

            if (nextChar == null)
            {
                break;
            }

            var isHit = false;
            isHit = AcceptComments();
            if (isHit)
            {
                continue;
            }
            isHit = AcceptEmptyChar();
            if (isHit)
            {
                continue;
            }

            isHit = AcceptIdentifierOrKeyword();
            if (isHit)
            {
                continue;
            }
            isHit = AcceptNumber();
            if (isHit)
            {
                continue;
            }
            isHit = AcceptOperators();
            if (isHit)
            {
                continue;
            }
            isHit = AcceptSymbol();
            if (isHit)
            {
                continue;
            }
            if (!isHit)
            {
                throw new NotSupportedException("not support char:" + GetNextCharValue());
            }
        }

        return tokens;
    }

    private bool AcceptComments()
    {
        var comments = new List<char>();
        //remove multi-line comments,such as 
        // /*aaa
        // this is a multi-line
        // comments
        // */
        //select * --abc
        //FROM test
        if (CheckNextChar('/') && CheckNextNextChar('*'))
        {
            Accept('/');
            Accept('*');
            while (true)
            {
                AcceptAnyOneChar();
                if (currentChar.HasValue)
                {
                    comments.Add(currentChar.Value);
                }

                var isHit = false;
                if (CheckNextChar('*') && CheckNextNextChar('/'))
                {
                    isHit = true;
                    Accept('*');
                    Accept('/');
                }
                else if (nextChar == null)
                {
                    isHit = true;
                }

                if (isHit)
                {
                    var comment = new string(comments.ToArray());
                    var token = Token.MultiLineComment;
                    token.Value = comment;
                    this.tokens.Add(token);
                    return true;
                }
            }
        }

        //remove single-line comments,such as
        //SELECT--
        //*--abc
        //FROM--ccd
        //TEST t----
        if (CheckNextChar('-') && CheckNextNextChar('-'))
        {
            Accept('-');
            Accept('-');
            while (true)
            {
                var isHit = false;
                if (CheckNextChar('\n'))
                {
                    isHit = true;
                    Accept('\n');

                }
                else if (nextChar == null)
                {
                    isHit = true;
                }

                if (isHit)
                {
                    var comment = new string(comments.ToArray());
                    var token = Token.LineComment;
                    token.Value = comment;
                    this.tokens.Add(token);
                    return true;
                }
                else
                {
                    AcceptAnyOneChar();
                    if (currentChar.HasValue)
                    {
                        comments.Add(currentChar.Value);
                    }
                }
            }
        }

        return false;
    }

    private bool CheckNextChar(char ch)
    {
        if (nextChar != null && nextChar == ch)
        {
            return true;
        }

        return false;
    }

    private bool CheckNextNextChar(char ch)
    {
        if (nextNextChar != null && nextNextChar == ch)
        {
            return true;
        }

        return false;
    }

    private bool CheckCurrentChar(char ch)
    {
        if (currentChar != null && currentChar == ch)
        {
            return true;
        }

        return false;
    }

    private bool AcceptEmptyChar()
    {
        if (Accept('\n') || Accept('\t') || Accept(' '))
        {
            return true;
        }

        return false;
    }

    private bool AcceptNumber()
    {
        if (AcceptDigits())
        {
            var txt = GetCurrentCharValue();
            while (Accept('.') || AcceptDigits())
            {
                if (currentChar == '.' && txt.IndexOf(".") != -1)
                {
                    throw new Exception("数字里不允许出现多个.");
                }
                txt += GetCurrentCharValue();
            }
            var token = AcceptNumberToken(txt);
            tokens.Add(token);
            return true;
        }

        return false;
    }

    private string GetCurrentCharValue()
    {
        if (currentChar.HasValue)
        {
            return currentChar.Value.ToString();
        }

        return "";
    }
    private string GetNextCharValue()
    {
        if (nextChar.HasValue)
        {
            return nextChar.Value.ToString();
        }

        return "";
    }
    private bool AcceptIdentifierOrKeyword()
    {
        if (AcceptLetters())
        {
            var txt = GetCurrentCharValue();
            while (AcceptLetters() || Accept('_') || AcceptDigits())
            {
                txt += GetCurrentCharValue();
            }

            var token = AcceptIdentifierOrKeywordToken(txt);
            tokens.Add(token);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 接受操作符
    /// </summary>
    private bool AcceptOperators()
    {
        if (Accept('+'))
        {
            var token = Token.Plus;
            tokens.Add(token);
            return true;
        }
        if (Accept('-'))
        {
            var token = Token.Sub;
            tokens.Add(token);
            return true;
        }
        if (Accept('*'))
        {
            var token = Token.Star;
            tokens.Add(token);
            return true;
        }
        if (Accept('/'))
        {
            var token = Token.Slash;
            tokens.Add(token);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 接受普通符号
    /// </summary>
    private bool AcceptSymbol()
    {
        if (Accept(','))
        {
            var token = Token.Comma;
            tokens.Add(token);
            return true;
        }

        if (Accept('.'))
        {
            var token = Token.Dot;
            tokens.Add(token);
            return true;
        }

        if (Accept('('))
        {
            var token = Token.LeftParen;
            tokens.Add(token);
            return true;
        }

        if (Accept(')'))
        {
            var token = Token.RightParen;
            tokens.Add(token);
            return true;
        }

        if (Accept('['))
        {
            var token = Token.LeftSquareBracket;
            tokens.Add(token);
            return true;
        }

        if (Accept(']'))
        {
            var token = Token.RightSquareBracket;
            tokens.Add(token);
            return true;
        }

        if (Accept('{'))
        {
            var token = Token.LeftCurlyBrackets;
            tokens.Add(token);
            return true;
        }

        if (Accept('}'))
        {
            var token = Token.RightCurlyBrackets;
            tokens.Add(token);
            return true;
        }

        if (Accept('='))
        {
            var token = Token.EqualTo;
            tokens.Add(token);
            return true;
        }

        if (Accept('!'))
        {
            AcceptOrThrowException('=');
            var token = Token.NotEqualTo;
            tokens.Add(token);
            return true;
        }

        if (Accept(';'))
        {
            var token = Token.Semicolon;
            tokens.Add(token);
            return true;
        }

        if (Accept('>'))
        {
            if (Accept('='))
            {
                var token = Token.GreaterThenOrEqualTo;
                tokens.Add(token);
            }
            else
            {
                var token = Token.GreaterThen;
                tokens.Add(token);
            }
            return true;
        }

        if (Accept('<'))
        {
            if (Accept('='))
            {
                var token = Token.LessThenOrEqualTo;
                tokens.Add(token);
            }
            else if (Accept('>'))
            {
                var token = Token.NotEqualTo;
                tokens.Add(token);
            }
            else
            {
                var token = Token.LessThen;
                tokens.Add(token);
            }
            return true;
        }

        if (Accept('\''))
        {
            var token = AcceptStringToken();
            tokens.Add(token);
            return true;
        }

        if (Accept(':'))
        {
            var token = Token.Colon;
            tokens.Add(token);
            return true;
        }
        if (Accept('@'))
        {
            var token = Token.At;
            tokens.Add(token);
            return true;
        }
        if (Accept('"'))
        {
            var token = Token.DoubleQuotes;
            tokens.Add(token);
            return true;
        }
        return false;
    }

    private Token AcceptStringToken()
    {
        var buffer = new List<char>();
        while (true)
        {
            if (Accept('\''))
            {
                //在sql中，''表示单个'
                if (Accept('\''))
                {
                    buffer.Add('\'');
                }
                else
                {
                    var token = Token.StringConstant;
                    token.Value = new string(buffer.ToArray());
                    return token;
                }
            }
            else
            {
                if (AcceptAnyOneChar() && currentChar.HasValue)
                {
                    buffer.Add(currentChar.Value);
                }
            }

        }
    }

    private Token AcceptNumberToken(string txt)
    {
        if (double.TryParse(txt, out var number))
        {
            var token = Token.NumberConstant;
            token.Value = number;
            return token;
        }

        throw new Exception("数字格式错误");
    }

    /// <summary>
    /// 这里判断是否为关键字或者标识符
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    private Token AcceptIdentifierOrKeywordToken(string txt)
    {
        if (!string.IsNullOrWhiteSpace(txt) && tokenDic.ContainsKey(txt.ToLowerInvariant()))
        {
            return tokenDic[txt.ToLowerInvariant()];
        }

        var token = Token.IdentifierString;
        token.Value = txt;
        return token;
    }


    /// <summary>
    /// 初始化token名称字典
    /// </summary>
    //private static void InitTokenDic()
    //{
    //    if (isInitTokenDic)
    //    {
    //        return;
    //    }

    //    isInitTokenDic = true;

    //    var fields = typeof(Token).GetFields(BindingFlags.Static | BindingFlags.Public);
    //    var i = 0;
    //    foreach (var fieldInfo in fields)
    //    {
    //        var token = (Token)fieldInfo.GetValue(null);
    //        try
    //        {
    //            tokenDic.Add(token.Value.ToString().ToLower(), token);
    //        }
    //        catch (Exception e)
    //        {
    //            var c = token;
    //            Console.WriteLine(e);
    //            throw;
    //        }

    //        i++;
    //    }
    //}

    /// <summary>
    /// 初始化允许的字符集合
    /// </summary>
    private void InitCharDic()
    {
        for (char i = 'a'; i < 'z'; i++)
        {
            charDic[i] = true;
        }
        for (char i = 'A'; i < 'Z'; i++)
        {
            charDic[i] = true;
        }
    }

    /// <summary>
    /// 初始化token字典集合
    /// </summary>
    private void InitTokenDic()
    {
        if (allDbTypeTokenDic.TryGetValue(dbType, out tokenDic))
        {
            return;
        }
        if (tokenDic == null)
        {
            tokenDic = new ConcurrentDictionary<string, Token>();
        }

        tokenDic.TryAdd("Select".ToLowerInvariant(), Token.Select);

        tokenDic.TryAdd("Delete".ToLowerInvariant(), Token.Delete);
        tokenDic.TryAdd("Insert".ToLowerInvariant(), Token.Insert);
        tokenDic.TryAdd("Update".ToLowerInvariant(), Token.Update);
        tokenDic.TryAdd("Having".ToLowerInvariant(), Token.Having);
        tokenDic.TryAdd("Where".ToLowerInvariant(), Token.Where);
        tokenDic.TryAdd("Order".ToLowerInvariant(), Token.Order);
        tokenDic.TryAdd("By".ToLowerInvariant(), Token.By);
        tokenDic.TryAdd("Group".ToLowerInvariant(), Token.Group);
        tokenDic.TryAdd("As".ToLowerInvariant(), Token.As);
        tokenDic.TryAdd("Null".ToLowerInvariant(), Token.Null);
        tokenDic.TryAdd("Not".ToLowerInvariant(), Token.Not);
        tokenDic.TryAdd("Distinct".ToLowerInvariant(), Token.Distinct);
        tokenDic.TryAdd("From".ToLowerInvariant(), Token.From);
        tokenDic.TryAdd("Create".ToLowerInvariant(), Token.Create);
        tokenDic.TryAdd("Alter".ToLowerInvariant(), Token.Alter);
        tokenDic.TryAdd("Drop".ToLowerInvariant(), Token.Drop);
        tokenDic.TryAdd("Set".ToLowerInvariant(), Token.Set);
        tokenDic.TryAdd("Into".ToLowerInvariant(), Token.Into);

        tokenDic.TryAdd("View".ToLowerInvariant(), Token.View);
        tokenDic.TryAdd("Index".ToLowerInvariant(), Token.Index);
        tokenDic.TryAdd("Union".ToLowerInvariant(), Token.Union);
        tokenDic.TryAdd("Left".ToLowerInvariant(), Token.Left);
        tokenDic.TryAdd("Inner".ToLowerInvariant(), Token.Inner);
        tokenDic.TryAdd("Right".ToLowerInvariant(), Token.Right);
        tokenDic.TryAdd("Full".ToLowerInvariant(), Token.Full);
        tokenDic.TryAdd("Outer".ToLowerInvariant(), Token.Outer);
        tokenDic.TryAdd("Cross".ToLowerInvariant(), Token.Cross);
        tokenDic.TryAdd("Join".ToLowerInvariant(), Token.Join);


        tokenDic.TryAdd("On".ToLowerInvariant(), Token.On);
        tokenDic.TryAdd("Cast".ToLowerInvariant(), Token.Cast);
        tokenDic.TryAdd("And".ToLowerInvariant(), Token.And);
        tokenDic.TryAdd("Or".ToLowerInvariant(), Token.Or);
        tokenDic.TryAdd("Xor".ToLowerInvariant(), Token.Xor);


        tokenDic.TryAdd("Case".ToLowerInvariant(), Token.Case);
        tokenDic.TryAdd("When".ToLowerInvariant(), Token.When);
        tokenDic.TryAdd("Then".ToLowerInvariant(), Token.Then);
        tokenDic.TryAdd("Else".ToLowerInvariant(), Token.Else);
        tokenDic.TryAdd("ElseIf".ToLowerInvariant(), Token.ElseIf);

        tokenDic.TryAdd("End".ToLowerInvariant(), Token.End);
        tokenDic.TryAdd("Asc".ToLowerInvariant(), Token.Asc);
        tokenDic.TryAdd("Desc".ToLowerInvariant(), Token.Desc);
        tokenDic.TryAdd("Is".ToLowerInvariant(), Token.Is);
        tokenDic.TryAdd("Like".ToLowerInvariant(), Token.Like);
        tokenDic.TryAdd("In".ToLowerInvariant(), Token.In);
        tokenDic.TryAdd("Between".ToLowerInvariant(), Token.Between);
        tokenDic.TryAdd("Values".ToLowerInvariant(), Token.Values);
        tokenDic.TryAdd("Over".ToLowerInvariant(), Token.Over);
        tokenDic.TryAdd("Partition".ToLowerInvariant(), Token.Partition);
        //mysql
        tokenDic.TryAdd("True".ToLowerInvariant(), Token.True);
        tokenDic.TryAdd("False".ToLowerInvariant(), Token.False);

        tokenDic.TryAdd("Identified".ToLowerInvariant(), Token.Identified);
        tokenDic.TryAdd("Password".ToLowerInvariant(), Token.Password);
        //operator
        tokenDic.TryAdd("LeftParen".ToLowerInvariant(), Token.LeftParen);
        tokenDic.TryAdd("RightParen".ToLowerInvariant(), Token.RightParen);
        tokenDic.TryAdd("LeftCurlyBrackets".ToLowerInvariant(), Token.LeftCurlyBrackets);
        tokenDic.TryAdd("RightCurlyBrackets".ToLowerInvariant(), Token.RightCurlyBrackets);
        tokenDic.TryAdd("LeftSquareBracket".ToLowerInvariant(), Token.LeftSquareBracket);
        tokenDic.TryAdd("RightSquareBracket".ToLowerInvariant(), Token.RightSquareBracket);
        tokenDic.TryAdd("Semicolon".ToLowerInvariant(), Token.Semicolon);
        tokenDic.TryAdd("Comma".ToLowerInvariant(), Token.Comma);

        tokenDic.TryAdd("Dot".ToLowerInvariant(), Token.Dot);
        tokenDic.TryAdd("At".ToLowerInvariant(), Token.At);
        tokenDic.TryAdd("EqualTo".ToLowerInvariant(), Token.EqualTo);
        tokenDic.TryAdd("GreaterThen".ToLowerInvariant(), Token.GreaterThen);
        tokenDic.TryAdd("LessThen".ToLowerInvariant(), Token.LessThen);
        tokenDic.TryAdd("Bang".ToLowerInvariant(), Token.Bang);
        tokenDic.TryAdd("Colon".ToLowerInvariant(), Token.Colon);
        tokenDic.TryAdd("NotEqualTo".ToLowerInvariant(), Token.NotEqualTo);
        tokenDic.TryAdd("GreaterThenOrEqualTo".ToLowerInvariant(), Token.GreaterThenOrEqualTo);
        tokenDic.TryAdd("LessThenOrEqualTo".ToLowerInvariant(), Token.LessThenOrEqualTo);
        tokenDic.TryAdd("Exists".ToLowerInvariant(), Token.Exists);
        tokenDic.TryAdd("Plus".ToLowerInvariant(), Token.Plus);
        tokenDic.TryAdd("Sub".ToLowerInvariant(), Token.Sub);
        tokenDic.TryAdd("Star".ToLowerInvariant(), Token.Star);
        tokenDic.TryAdd("Slash".ToLowerInvariant(), Token.Slash);
        tokenDic.TryAdd("With".ToLowerInvariant(), Token.With);
        tokenDic.TryAdd("All".ToLowerInvariant(), Token.All);
        tokenDic.TryAdd("Intersect".ToLowerInvariant(), Token.Intersect);
        tokenDic.TryAdd("Except".ToLowerInvariant(), Token.Except);
        tokenDic.TryAdd("Minus".ToLowerInvariant(), Token.Minus);
        tokenDic.TryAdd("Any".ToLowerInvariant(), Token.Any);
        tokenDic.TryAdd("LineBreak".ToLowerInvariant(), Token.LineBreak);
        tokenDic.TryAdd("DoubleQuotes".ToLowerInvariant(), Token.DoubleQuotes);

        tokenDic.TryAdd("LineComment".ToLowerInvariant(), Token.LineComment);
        tokenDic.TryAdd("MultiLineComment".ToLowerInvariant(), Token.MultiLineComment);
        //Identifier|标识符
        tokenDic.TryAdd("IdentifierString".ToLowerInvariant(), Token.IdentifierString);
        //NumberConstant|数字常量
        tokenDic.TryAdd("NumberConstant".ToLowerInvariant(), Token.NumberConstant);
        //StringConstant|字符串常量
        tokenDic.TryAdd("StringConstant".ToLowerInvariant(), Token.StringConstant);

        //oracle
        if (dbType == DbType.Oracle)
        {
            tokenDic.TryAdd("Dual".ToLowerInvariant(), Token.Dual);
            tokenDic.TryAdd("Unique".ToLowerInvariant(), Token.Unique);
            tokenDic.TryAdd("First".ToLowerInvariant(), Token.First);
        }

        if (dbType == DbType.MySql || dbType == DbType.Pgsql)
        {
            tokenDic.TryAdd("Limit".ToLowerInvariant(), Token.Limit);
        }

        if (dbType == DbType.SqlServer || dbType == DbType.Pgsql)
        {
            tokenDic.TryAdd("Offset".ToLowerInvariant(), Token.Offset);
        }

        if (dbType == DbType.SqlServer || dbType == DbType.Oracle)
        {
            tokenDic.TryAdd("Rows".ToLowerInvariant(), Token.Rows);
            tokenDic.TryAdd("Fetch".ToLowerInvariant(), Token.Fetch);
            tokenDic.TryAdd("Only".ToLowerInvariant(), Token.Only);
        }
        //sql server
        if (dbType == DbType.SqlServer)
        {
            tokenDic.TryAdd("Next".ToLowerInvariant(), Token.Next);
        }

        allDbTypeTokenDic.TryAdd(dbType, tokenDic);
    }
    /// <summary>
    /// 初始化允许的数字集合
    /// </summary>
    private void InitDigitDic()
    {
        for (int i = 0; i <= 9; i++)
        {
            var ch = char.Parse(i.ToString());
            digitDic[ch] = true;
        }

    }
    /// <summary>
    /// 接受数字集合
    /// </summary>
    /// <returns></returns>
    private bool AcceptDigits()
    {
        if (nextChar.HasValue && digitDic.ContainsKey(nextChar.Value))
        {
            GetNextChar();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 接受字母
    /// </summary>
    /// <returns></returns>
    private bool AcceptLetters()
    {
        if (nextChar.HasValue && charDic.ContainsKey(nextChar.Value))
        {
            GetNextChar();
            return true;
        }

        return false;
    }

    private void GetNextChar()
    {
        currentChar = nextChar;

        if (pos + 2 <= chars.Count - 1)
        {
            nextNextChar = chars[pos + 2];
        }
        else
        {
            nextNextChar = null;
        }

        if (pos + 1 <= chars.Count - 1)
        {
            pos++;
            nextChar = chars[pos];
        }
        else
        {
            nextChar = null;
        }

    }

    private bool Accept(char ch)
    {
        if (nextChar != null && nextChar == ch)
        {
            GetNextChar();
            return true;
        }

        return false;
    }
    private bool AcceptAnyOneChar()
    {
        if (nextChar != null)
        {
            GetNextChar();
            return true;
        }

        return false;
    }
    private bool AcceptOrThrowException(char ch)
    {
        if (Accept(ch))
        {
            return true;
        }

        throw new Exception($"{pos}位置语意错误，请确认");
    }
}