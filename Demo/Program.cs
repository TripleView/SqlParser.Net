using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = @" SELECT 
    EXTRACT(YEAR FROM order_date) AS order_year,
    COUNT(*) AS order_count
FROM 
    orders
GROUP BY 
    EXTRACT(YEAR FROM order_date);";

            sql = @"  SELECT order_date at TIME zone 'Asia/ShangHai' as b FROM orders ";
            //sql server SELECT GETDATE() AT TIME ZONE 'UTC' AT TIME ZONE 'Central Standard Time';
            //pgsql SELECT order_date at TIME zone 'Asia/ShangHai' at TIME zone 'utc' as b FROM orders
            //--创建示例表
            //CREATE TABLE orders(
            //    id serial,
            //    order_date date
            //);

            //--插入示例数据
            //INSERT INTO orders(order_date) VALUES('2025-01-01'), ('2025-02-15'), ('2024-03-20');

            //SELECT order_date at TIME zone 'Asia/ShangHai' at TIME zone 'utc' as b FROM orders


            //select(SELECT order_date as b FROM orders limit 1) at TIME zone 'Asia/ShangHai' from orders
            //SELECT order_date at TIME zone 'Asia/ShangHai' as b FROM orders where date_trunc('minute',(order_date at TIME zone 'Asia/ShangHai'))= '2023-04-19 03:11'::timestamp


            //SELECT date_trunc('minute',(order_date at TIME zone 'Asia/ShangHai')) at TIME zone 'Asia/ShangHai' as b FROM orders where date_trunc('minute',(order_date at TIME zone 'Asia/ShangHai'))= '2023-04-19 03:11'::timestamp


            var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
            var unitTestAstVisitor = new UnitTestAstVisitor();
            sqlAst.Accept(unitTestAstVisitor);
            var result = unitTestAstVisitor.GetResult();
            var newSql= sqlAst.ToSql();
        }
    }
}
