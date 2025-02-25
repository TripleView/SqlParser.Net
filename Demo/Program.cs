using SqlParser.Net;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var c = "a;;;;".TrimEnd(';');
            var sql = @"SELECT *
FROM (SELECT * FROM s1 WHERE update_time >= '2025-02-25 09:03:16' and id >='1646756951' order by update_time asc, id ASC LIMIT 15000) t1
LEFT JOIN (SELECT * FROM s2 WHERE update_time >= '2025-02-25 09:03:16' and id >='1646756951' order by update_time asc, id ASC LIMIT 15000) t2 ON t1.id = t2.id - 1
LEFT JOIN (SELECT * FROM s3 WHERE update_time >= '2025-02-25 09:03:16' and id >='1646756951' order by update_time asc, id ASC LIMIT 15000) t3 ON t1.id = t3.id - 2
WHERE t1.id % 3 = 1 LIMIT 5000";
            var sqlAst = DbUtils.Parse(sql, DbType.MySql);
            var unitTestAstVisitor = new UnitTestAstVisitor();
            sqlAst.Accept(unitTestAstVisitor);
            var result = unitTestAstVisitor.GetResult();
        }
    }
}
