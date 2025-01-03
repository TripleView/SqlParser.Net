using System;
using System.Diagnostics;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Lexer;

namespace SqlParser.Net;

public class DbUtils
{
    public static SqlExpression Parse(string sql, DbType dbType, Action<long, string> logger = null)
    {
        sql = sql.TrimEnd(';');
        var sw = new Stopwatch();
        sw.Start();
        var sqlLexer = new SqlLexer()
        {
            Logger = logger
        };
        var tokens = sqlLexer.Parse(sql, dbType);
        if (tokens.Count == 0)
        {
            return null;
        }
        sw.Stop();
        var t = sw.ElapsedMilliseconds;
        if (logger != null)
        {
            logger(t, "lexer");
        }
        sw.Restart();
        var sqlParser = new Ast.SqlParser();
        var result = sqlParser.Parse(tokens, sql, dbType);
        sw.Stop();
        var t2 = sw.ElapsedMilliseconds;
        if (logger != null)
        {
            logger(t2, "parser");
        }
        sw.Restart();
        result.DbType = dbType;
        return result;
    }
}