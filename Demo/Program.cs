using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = "SELECT * FROM (SELECT t.*, ROW_NUMBER() OVER (PARTITION BY bill_code ORDER BY seq_date DESC) as rn FROM bill_sequence t) ranked WHERE rn <= 4 ORDER BY bill_code, seq_date DESC;";
            //sql = "SELECT n FROM generate_series(1, 5) AS t(n);";
            //sql = "SELECT * FROM unnest(ARRAY[10, 20, 30])";
            var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
            if (sqlAst is SqlSelectExpression selectExpression &&
                selectExpression.Query
                    is SqlSelectQueryExpression a && a.From is SqlSelectExpression b && b.Query is SqlSelectQueryExpression c)
            {
                var d = b.ToSql();
            }
            var result = sqlAst.ToFormat();
            var newSql= sqlAst.ToSql();
        }
    }
}
