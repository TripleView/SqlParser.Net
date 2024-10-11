using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Lexer;
using System.Diagnostics;

namespace SqlParser.Net;

public class DbUtils
{
    public static SqlExpression Parse(string sql, DbType dbType)
    {
        var sqlLexer = new SqlLexer();
        var tokens = sqlLexer.Parse(sql, dbType);
        if (tokens.Count == 0)
        {
            return null;
        }
        var sqlParser = new Ast.SqlParser();
        var result = sqlParser.Parse(tokens, dbType);
        return result;
    }
}