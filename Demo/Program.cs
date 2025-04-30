using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SqlParser.Net.Ast.Visitor;

namespace Demo
{
   
    internal class Program
    {
        static void Main(string[] args)
        {
            var sql = @"select 'Name' as Value,'姓名' as Text,1 as InxNbr union select 'IdentityId' as Value,'身份证号' as Text,2 as InxNbr union select 'PhoneNumber' as Value,'手机号' as Text,3 as InxNbr order by InxNbr";
            sql =
                "select 3 as pn  union select 2 as pn OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY";

//            sql = "update t3 set Id ='1' from  T4 where t3.id = t4.Pid ";//pgsql
//            sql = "update t set t.Id ='1' from T3 as t inner join T4 as t4 on t.id = t4.Pid where t.id = 'abc'";//sqlserver
//            sql = "update t3  join T4  t4 on t3.id = t4.Pid  set t3.Id ='1'  where 1=1";//mysql

//            sql = @"UPDATE t3 SET id=(SELECT pid FROM t4 WHERE t3.id=t4.PID) WHERE EXISTS (
//    SELECT 1
//    FROM t4
//    WHERE t3.id = t4.pid
//)";//oracle
//            sql = "update t3 set Id ='aa' from t4 where t3.Id =t4.Pid ";//sqlite

            var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
            var result = sqlAst.ToFormat();
            var newSql= sqlAst.ToSql();
        }
    }
}
