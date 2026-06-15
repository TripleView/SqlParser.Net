using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = "SELECT ARRAY(SELECT generate_series(1, 10))";
            
            sql = "select * from test where :f is null";
            sql = "select * from test3 c WHERE a='a' AND CASE WHEN to_char(c.b) ='b' THEN TO_NUMBER( c.c ) END >=240";
            //SELECT ARRAY(SELECT generate_series(1, 10));
            //SELECT ARRAY[[1, 2], [3, 4]] AS matrix;

            //SELECT ARRAY[[[1, 2], [3, 4]], [[5, 6], [7, 8]]];

            //SELECT* FROM unnest(ARRAY[10, 20, 30])


            //SELECT ARRAY(SELECT a FROM test3 t);

            //SELECT ARRAY[1, 2, 3] AS int_array;
            //sql = "SELECT * FROM unnest(ARRAY[10, 20, 30])";
            sql = "WITH older_users AS (\r\n    SELECT id FROM users WHERE age > 25\r\n)\r\nUPDATE users\r\nSET age = age + 1\r\nFROM users\r\nJOIN older_users ON users.id = older_users.id;";
            sql =
                @"WITH c AS 
(SELECT DISTINCT rtax001,rtaxl003
  FROM rtax_t LEFT  JOIN rtaxl_t ON rtaxent = rtaxlent AND rtax001 = rtaxl001 AND rtaxl002 = 'zh_CN'
  WHERE rtaxent = 100 AND rtax005 = 0 AND rtaxstus = 'Y'
  ORDER BY rtax001) 
,c1 AS (SELECT DISTINCT oocq002,oocql004
  FROM oocq_t LEFT  JOIN oocql_t ON oocq001 = oocql001 AND oocq002 = oocql002 AND oocqent = oocqlent AND oocql003 = 'zh_CN'
 WHERE oocqent = 100 AND oocq001= 210  AND oocqstus = 'Y' )
SELECT 
BMAA001, --主料件号
BMAA004, --生产单位
c.rtaxl003,--分类
mit.imaal003, --品名
mit.imaal004, --规格参数
CASE mi.IMAA004
        WHEN 'A' THEN '组合/加工品'
        WHEN 'E' THEN '费用/软件'
        WHEN 'F' THEN '事务用品'
        WHEN 'M' THEN '材料/零件/商品'
        WHEN 'T' THEN '范本'
        WHEN 'X' THEN '虚拟品'
        ELSE '未知类型'
    END AS IMAA004_DESC , --料件类别 
	c1.OOCQL004  --生命周期状态 
FROM bmaa_t  m
INNER JOIN imaa_t mi ON mi.IMAAENT =m.BMAAENT  AND mi.IMAA001 =m.BMAA001 
INNER JOIN IMAAL_T mit ON mit.IMAALENT =m.BMAAENT  AND mit.IMAAL001 =mi.IMAA001  AND mit.IMAAL002 ='zh_CN'
INNER JOIN c ON c.RTAX001 =mi.IMAA009 
INNER JOIN c1 ON c1.OOCQ002 =mi.IMAA010
WHERE BMAASITE ='YYDS' AND BMAAENT =100 AND bmaastus='Y'
AND m.BMAA001 ='23.05.0000065'";
            var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
           
            var result = sqlAst.ToFormat();

            var newSql= sqlAst.ToSql();


        }
    }
}
