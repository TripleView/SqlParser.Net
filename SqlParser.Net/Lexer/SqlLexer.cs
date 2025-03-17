using SqlParser.Net.Ast.Expression;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SqlParser.Net.Lexer;

public class SqlLexer
{
    public Action<long, string> Logger { get; set; }
    private int pos = -1;

    private Token currentToken;
    private Token nextToken;
    private char? currentChar;
    private char? nextChar;
    private char? nextNextChar;
    private List<char> chars;
    /// <summary>
    /// while maximum number of loops, used to avoid infinite loops
    /// while最大循环次数，用来避免死循环
    /// </summary>
    private int whileMaximumNumberOfLoops = 100000;
    private Dictionary<char, bool> digitDic = new Dictionary<char, bool>();
    private ConcurrentDictionary<string, Token> tokenDic = new ConcurrentDictionary<string, Token>();
    /// <summary>
    /// Token dictionary for all database types
    /// 所有数据库类型的token字典
    /// </summary>
    public static ConcurrentDictionary<DbType, ConcurrentDictionary<string, Token>> AllDbTypeTokenDic = new ConcurrentDictionary<DbType, ConcurrentDictionary<string, Token>>();
    private List<Token> tokens = new List<Token>();
    private DbType dbType;
    /// <summary>
    /// right Qualifiers char
    /// 右限定符
    /// </summary>
    private char leftQualifierChar;
    /// <summary>
    /// left Qualifiers char
    /// 左限定符
    /// </summary>
    private char rightQualifierChar;
    //static SqlLexer()
    //{
    //    InitTokenDic();
    //}
    public List<Token> Parse(string sql, DbType dbType)
    {
        this.dbType = dbType;
        //tokens.Clear();
        InitQualifierChar();
        InitTokenDic();
        //Only recognize line breaks \n
        //仅识别换行符\n
        sql = sql.Replace("\r\n", "\n");
        chars = sql.Select(it => it).ToList();

        InitDigitDic();

        GetNextChar();

        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;

            if (nextChar == null)
            {
                break;
            }

            var isHit = false;
            isHit = AcceptQualifiersIdentifier();
            if (isHit)
            {
                continue;
            }
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
                AcceptHints();
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



    /// <summary>
    /// Initialize the Qualifiers character, such as [System] in sql:select * from RouteData rd where rd.[System] ='a';
    /// 初始化限定字符，比如sql:select * from RouteData rd where rd.[System] ='a'中的[System]
    /// </summary>
    private void InitQualifierChar()
    {
        switch (this.dbType)
        {
            case DbType.MySql:
            case DbType.Sqlite:
                leftQualifierChar = '`';
                rightQualifierChar = '`';
                break;
            case DbType.SqlServer:
                leftQualifierChar = '[';
                rightQualifierChar = ']';
                break;
            case DbType.Pgsql:
            case DbType.Oracle:
                leftQualifierChar = '"';
                rightQualifierChar = '"';
                break;
        }
    }

    /// <summary>
    /// 获取限定标识符，比如select * from [System]中的[System]，又比如SELECT "first name" FROM "user data"中的first name和user data
    /// </summary>
    /// <returns></returns>
    private bool AcceptQualifiersIdentifier()
    {
        if (Accept(leftQualifierChar))
        {
            var startIndex = pos - 1;
            var sb = new StringBuilder();
            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                if (Accept(rightQualifierChar))
                {
                    var txt = sb.ToString();
                    var token = Token.IdentifierString;
                    token.Value = txt;
                    var endIndex = pos - 1;
                    token.StartPositionIndex = startIndex;
                    token.EndPositionIndex = endIndex;
                    token.LeftQualifiers = leftQualifierChar.ToString();
                    token.RightQualifiers = rightQualifierChar.ToString();
                    tokens.Add(token);
                    return true;
                }

                AcceptAnyOneChar();
                var ch = GetCurrentCharValue();
                sb.Append(ch);
            }
        }

        return false;
    }


    private bool AcceptHints()
    {
        if (dbType == DbType.SqlServer)
        {
            var lastToken = tokens.Last();
            var startPositionIndex = lastToken.StartPositionIndex;
            if ((lastToken.IsToken(Token.With) || lastToken.IsToken(Token.Option)))
            {
                var sb = new StringBuilder();
                var start = lastToken.RawValue;
                sb.Append(start);
                while (AcceptEmptyChar())
                {
                    var value = GetCurrentCharValue();
                    sb.Append(value);
                }

                if (Accept('('))
                {
                    var ch = GetCurrentCharValue();
                    sb.Append(ch);
                    var i = 0;
                    var leftParenCount = 1;

                    while (leftParenCount != 0)
                    {
                        if (i >= whileMaximumNumberOfLoops)
                        {
                            throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                        }

                        i++;
                        if (nextChar == null)
                        {
                            break;
                        }

                        if (Accept('('))
                        {
                            leftParenCount++;
                        }
                        else if (Accept(')'))
                        {
                            leftParenCount--;
                        }
                        else
                        {
                            AcceptAnyOneChar();
                        }
                        var value = GetCurrentCharValue();
                        sb.Append(value);
                    }

                    var token = Token.HintsConstant;
                    //token.Value = sb.ToString();
                    token.RawValue = sb.ToString();
                    token.StartPositionIndex = startPositionIndex;
                    token.EndPositionIndex = pos - 1;
                    tokens.Remove(lastToken);
                    tokens.Add(token);
                }
                else
                {
                    return false;
                }
            }
        }

        return false;
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
            var startIndex = pos;
            Accept('/');
            Accept('*');
            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
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
                    var endIndex = pos - 1;
                    token.StartPositionIndex = startIndex;
                    token.EndPositionIndex = endIndex;
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
            var startIndex = pos;
            Accept('-');
            Accept('-');
            var i = 0;
            while (true)
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
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
                    var endIndex = pos - 1;
                    token.StartPositionIndex = startIndex;
                    token.EndPositionIndex = endIndex;
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
        if (Accept('\n') || Accept('\t') || Accept(' ') || AcceptNextEmptyChar())
        {
            return true;
        }

        return false;
    }

    private bool AcceptNumber()
    {
        if (AcceptDigits())
        {
            var startIndex = pos - 1;
            var sb = new StringBuilder();
            var ch = GetCurrentCharValue();
            sb.Append(ch);
            var i = 0;
            while (Accept('.') || AcceptDigits())
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                //if (currentChar == '.' && txt.IndexOf(".", StringComparison.InvariantCulture) != -1)
                //{
                //    throw new Exception("数字里不允许出现多个.");
                //}
                ch = GetCurrentCharValue();
                sb.Append(ch);
            }

            var token = AcceptNumberToken(sb.ToString());
            var endIndex = pos - 1;
            token.StartPositionIndex = startIndex;
            token.EndPositionIndex = endIndex;
            tokens.Add(token);
            return true;
        }

        return false;
    }

    private char GetCurrentCharValue()
    {
        if (currentChar.HasValue)
        {
            return currentChar.Value;
        }

        return default;
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
            var startIndex = pos - 1;
            var sb = new StringBuilder();
            var ch = GetCurrentCharValue();
            sb.Append(ch);
            var i = 0;
            while (AcceptLetters() || Accept('_') || AcceptDigits())
            {
                if (i >= whileMaximumNumberOfLoops)
                {
                    throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
                }

                i++;
                ch = GetCurrentCharValue();
                sb.Append(ch);
            }

            var txt = sb.ToString();
            var token = AcceptIdentifierOrKeywordToken(txt);
            token.RawValue = txt;
            var endIndex = pos - 1;
            token.StartPositionIndex = startIndex;
            token.EndPositionIndex = endIndex;
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
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }
        if (Accept('-'))
        {
            var token = Token.Sub;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }
        if (Accept('*'))
        {
            var token = Token.Star;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }
        if (Accept('/'))
        {
            var token = Token.Slash;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }
        if (Accept('%'))
        {
            var token = Token.Modulus;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }
        return false;
    }

    private void UpdateTokenPosition(ref Token token, int numberOf = 1)
    {
        var endIndex = pos - 1;
        token.StartPositionIndex = pos - numberOf;
        token.EndPositionIndex = endIndex;
    }

    /// <summary>
    /// 接受普通符号
    /// </summary>
    private bool AcceptSymbol()
    {
        if (Accept(',') || (dbType == DbType.Oracle && Accept('，')))
        {
            var token = Token.Comma;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept('.'))
        {
            var token = Token.Dot;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept('(') || (dbType == DbType.Oracle && Accept('（')))
        {
            var token = Token.LeftParen;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept(')') || (dbType == DbType.Oracle && Accept('）')))
        {
            var token = Token.RightParen;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept('['))
        {
            var token = Token.LeftSquareBracket;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept(']'))
        {
            var token = Token.RightSquareBracket;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept('{'))
        {
            var token = Token.LeftCurlyBrackets;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept('}'))
        {
            var token = Token.RightCurlyBrackets;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept('='))
        {
            var token = Token.EqualTo;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept('!'))
        {
            AcceptOrThrowException('=');
            var token = Token.NotEqualTo;
            UpdateTokenPosition(ref token, 2);
            tokens.Add(token);
            return true;
        }

        if (Accept(';'))
        {
            var token = Token.Semicolon;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept('>'))
        {
            if (Accept('='))
            {
                var token = Token.GreaterThenOrEqualTo;
                UpdateTokenPosition(ref token, 2);
                tokens.Add(token);
            }
            else
            {
                var token = Token.GreaterThen;
                UpdateTokenPosition(ref token);
                tokens.Add(token);
            }
            return true;
        }

        if (Accept('<'))
        {
            if (Accept('='))
            {
                var token = Token.LessThenOrEqualTo;
                UpdateTokenPosition(ref token, 2);
                tokens.Add(token);
            }
            else if (Accept('>'))
            {
                var token = Token.NotEqualTo;
                UpdateTokenPosition(ref token, 2);
                tokens.Add(token);
            }
            else
            {
                var token = Token.LessThen;
                UpdateTokenPosition(ref token);
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
            if (Accept(':'))
            {
                var token = Token.ColonColon;
                UpdateTokenPosition(ref token, 2);
                tokens.Add(token);
                return true;
            }
            else
            {
                var token = Token.Colon;
                UpdateTokenPosition(ref token);
                tokens.Add(token);
                return true;
            }
        }
        if (Accept('&'))
        {
            var token = Token.BitwiseAnd;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }
        if (Accept('^'))
        {
            var token = Token.BitwiseXor;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }
        if (Accept('@'))
        {
            var token = Token.At;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }
        if (Accept('"'))
        {
            var token = Token.DoubleQuotes;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }
        if (Accept('`'))
        {
            var token = Token.Backtick;
            UpdateTokenPosition(ref token);
            tokens.Add(token);
            return true;
        }

        if (Accept('|'))
        {
            if (Accept('|'))
            {
                var token = Token.BarBar;
                UpdateTokenPosition(ref token, 2);
                tokens.Add(token);
            }
            else
            {
                var token = Token.Bar;
                UpdateTokenPosition(ref token);
                tokens.Add(token);
            }
            return true;
        }
        return false;
    }

    private Token AcceptStringToken()
    {
        var startIndex = pos - 1;
        var buffer = new List<char>();
        var i = 0;
        while (true)
        {
            if (i >= whileMaximumNumberOfLoops)
            {
                throw new Exception($"The number of SQL parsing times exceeds {whileMaximumNumberOfLoops}");
            }

            i++;
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
                    var endIndex = pos - 1;
                    token.StartPositionIndex = startIndex;
                    token.EndPositionIndex = endIndex;
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
        //var sw = new Stopwatch();
        //sw.Start();
        if (decimal.TryParse(txt, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var number))
        {
            //sw.Stop();
            //var t = sw.ElapsedMilliseconds;
            //if (Logger != null)
            //{
            //    Logger(t, "dd" + txt);
            //}
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
    /// 初始化token字典集合
    /// </summary>
    private void InitTokenDic()
    {
        if (AllDbTypeTokenDic.TryGetValue(dbType, out tokenDic))
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
        tokenDic.TryAdd("BitwiseAnd".ToLowerInvariant(), Token.BitwiseAnd);
        tokenDic.TryAdd("BitwiseXor".ToLowerInvariant(), Token.BitwiseXor);

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
        if (dbType == DbType.MySql || dbType == DbType.Pgsql || dbType == DbType.Sqlite)
        {
            tokenDic.TryAdd("True".ToLowerInvariant(), Token.True);
            tokenDic.TryAdd("False".ToLowerInvariant(), Token.False);
        }

        tokenDic.TryAdd("Identified".ToLowerInvariant(), Token.Identified);
        //tokenDic.TryAdd("Password".ToLowerInvariant(), Token.Password);

        tokenDic.TryAdd("Exists".ToLowerInvariant(), Token.Exists);
        tokenDic.TryAdd("With".ToLowerInvariant(), Token.With);
        tokenDic.TryAdd("All".ToLowerInvariant(), Token.All);
        tokenDic.TryAdd("Intersect".ToLowerInvariant(), Token.Intersect);
        tokenDic.TryAdd("Except".ToLowerInvariant(), Token.Except);
        tokenDic.TryAdd("Minus".ToLowerInvariant(), Token.Minus);
        tokenDic.TryAdd("Any".ToLowerInvariant(), Token.Any);

        //oracle
        if (dbType == DbType.Oracle)
        {
            tokenDic.TryAdd("Unique".ToLowerInvariant(), Token.Unique);
            tokenDic.TryAdd("Siblings".ToLowerInvariant(), Token.Siblings);
            tokenDic.TryAdd("Connect".ToLowerInvariant(), Token.Connect);
            tokenDic.TryAdd("Start".ToLowerInvariant(), Token.Start);
            tokenDic.TryAdd("Nocycle".ToLowerInvariant(), Token.Nocycle);
            tokenDic.TryAdd("Prior".ToLowerInvariant(), Token.Prior);
        }
        //oracle
        if (dbType == DbType.Oracle || dbType == DbType.Pgsql || dbType == DbType.Sqlite)
        {
            tokenDic.TryAdd("First".ToLowerInvariant(), Token.First);
            tokenDic.TryAdd("Last".ToLowerInvariant(), Token.Last);
            tokenDic.TryAdd("Nulls".ToLowerInvariant(), Token.Nulls);
        }
        if (dbType == DbType.MySql || dbType == DbType.Pgsql)
        {
            tokenDic.TryAdd("Limit".ToLowerInvariant(), Token.Limit);
        }

        if (dbType == DbType.SqlServer || dbType == DbType.Pgsql)
        {
            tokenDic.TryAdd("Offset".ToLowerInvariant(), Token.Offset);
        }

        if (dbType == DbType.SqlServer || dbType == DbType.Pgsql || (dbType == DbType.Oracle))
        {
            tokenDic.TryAdd("Within".ToLowerInvariant(), Token.Within);
        }

        if (dbType == DbType.SqlServer || dbType == DbType.Oracle)
        {
            tokenDic.TryAdd("Rows".ToLowerInvariant(), Token.Rows);
            tokenDic.TryAdd("Fetch".ToLowerInvariant(), Token.Fetch);
            tokenDic.TryAdd("Only".ToLowerInvariant(), Token.Only);
            tokenDic.TryAdd("Pivot".ToLowerInvariant(), Token.Pivot);
            tokenDic.TryAdd("For".ToLowerInvariant(), Token.For);
        }
        //sql server
        if (dbType == DbType.SqlServer)
        {
            tokenDic.TryAdd("Next".ToLowerInvariant(), Token.Next);
            tokenDic.TryAdd("Top".ToLowerInvariant(), Token.Top);
            tokenDic.TryAdd("Option".ToLowerInvariant(), Token.Option);

        }

        //if (dbType == DbType.Pgsql)
        //{
        //    tokenDic.TryAdd("ColonColon".ToLowerInvariant(), Token.ColonColon);
        //}
        AllDbTypeTokenDic.TryAdd(dbType, tokenDic);
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
        if (nextChar.HasValue && char.IsLetter(nextChar.Value))
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

    private bool AcceptNextEmptyChar()
    {
        if (nextChar != null && char.IsWhiteSpace(nextChar.Value))
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