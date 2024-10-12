using System;
using System.Diagnostics;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Lexer;

namespace SqlParser.Net;

public class DbUtils
{
    public static SqlExpression Parse(string sql, DbType dbType)
    {
        var sw = new Stopwatch();
        sw.Start();
        var sqlLexer = new SqlLexer();
        var tokens = sqlLexer.Parse(sql, dbType);
        if (tokens.Count == 0)
        {
            return null;
        }
        sw.Stop();
        var t = sw.ElapsedMilliseconds;
        Trace.WriteLine("t:" + t);
        sw.Restart();
        var sqlParser = new Ast.SqlParser();
        var result = sqlParser.Parse(tokens, dbType);
        sw.Stop();
        var t2 = sw.ElapsedMilliseconds;
        sw.Restart();
        Trace.WriteLine("t2:" + t2);
        return result;
    }
}