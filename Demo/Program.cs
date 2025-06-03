using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
     
            var sql = "select \"Id\", LTRIM((((\"Id\" || '(') || \"TableName\") || ')')) as ProgramId, \"Name\", \"TableName\", \"Icon\", \"Descr\" from \"winter.system\".\"Program\" as \"sr\" where((1 = 1) and((1 = 1) and((\"Id\" COLLATE \"C\" like '%FX%'))))";

            var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
            var result = sqlAst.ToFormat();
            var newSql= sqlAst.ToSql();
        }
    }
}
