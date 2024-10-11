using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    private static Dictionary<string, Token> tokenDic = new Dictionary<string, Token>();
    private List<Token> tokens = new List<Token>();
    private DbType dbType
        ;
    static SqlLexer()
    {
        InitTokenDic();
    }
    public List<Token> Parse(string sql,DbType dbType)
    {
        this.dbType = dbType;
        tokens.Clear();
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
            isHit = AcceptLineBreak();
            if (isHit)
            {
                continue;
            }
            isHit = AcceptWhiteSpace();
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
                throw new NotSupportedException("not support char:" + GetCurrentCharValue());
            }
        }

        return tokens;
    }

    private bool AcceptWhiteSpace()
    {
        if (Accept(' '))
        {
            return true;
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

    private bool AcceptLineBreak()
    {
        if (Accept('\n'))
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
        if (!string.IsNullOrWhiteSpace(txt) && tokenDic.ContainsKey(txt.ToLower()))
        {
            return tokenDic[txt.ToLower()];
        }

        var token = Token.IdentifierString;
        token.Value = txt;
        return token;
    }

    /// <summary>
    /// 初始化token名称字典
    /// </summary>
    private static void InitTokenDic()
    {
        var fields = typeof(Token).GetFields(BindingFlags.Static | BindingFlags.Public);
        var i = 0;
        foreach (var fieldInfo in fields)
        {
            var token = (Token)fieldInfo.GetValue(null);
            try
            {
                tokenDic.Add(token.Value.ToString().ToLower(), token);
            }
            catch (Exception e)
            {
                var c = token;
                Console.WriteLine(e);
                throw;
            }

            i++;
        }
    }

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