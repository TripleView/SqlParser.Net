# 序列序言
开源项目应该是出自我自己的需求，我自己解决后，分享出来给大家，那么必须附上前因后果，不然看的人就容易晕，同时要附上整个项目的代码的大概思路，免得读者在代码的海洋里调试的头痛欲裂，我想要添加一个软删除的功能，那么如果能在sql上为每张表动态添加active=1这个条件，不就可以了，同时比如单租户应用变成多租户，动态检查where条件等等。
antlr
就像我们数学刚入门的时候不能上来就学微积分一样，我们得先从简单的四则运算-加减乘除学起，打好基础，再由易入难。

在ast树命名的部分，参考了阿里巴巴开源的druid

c#中所有数字都可以用decimal表示
解决嵌套问题的唯一方案，就是用递归
为了帮助各位有兴趣的靓仔更快的融入本项目，解决思路比解决方法更重要，授人以渔

单元测试的重要性，几百种案例，靠自己手工一个一个去验证那得累死，添加一个需求，新增1到N个该需求的单元测试，然后跑一遍所有单元测试，确保新增功能的时候不影响旧功能

字母构成单词，单词构成句子，句子构成文章，有个层层递进的过程。
因为要解析各种各样的情况，所以对sql的了解就更加深入了

自然语言和形式语言的一个重要区别是，自然语言的一个语句，可能有多重含义，而形式语言的一个语句，只能有一个语义;形式语言的语法是人为规定的，有了一定的语法规则，语法解析器就能根据语法规则，解析出一个语句的一个唯一含义

visit 负数  select 5|3  TestNot3 oracle表别名 select * from ATL_Login.dbo.[ATLReportingRelationship] ar 
行转列的SQL操作通常称为“透视”。不同的数据库有不同的实现方式
在 Oracle 中，(+) 是用于表示外连接（Outer Join）的旧式语法符号。它是特有于 Oracle 的非标准 SQL 语法。以下是它的用法：
用法
在 Oracle 中，(+) 被放在需要右外连接的表的列旁边

极致的速度
文档最完善，代码最简洁易懂
字符串比较比数字比较慢10倍以上
发现ToLowerInvariant()耗时比ToLower()少非常多，初步估计是内部初始化文化需要很长时间
txt.IndexOf(".") 与文化相关的都非常耗时
double.tryparse 文化相关，存在预热问题

在 SQL 中，FROM 关键字后面可以有多种形式来指定数据来源。主要有以下几种
1，表名或者视图：
```sql
select * from test
```
2.子查询（子表）：
```sql
select * from (select * from test) t
```
3，联接（JOIN）：
```sql
select * from test t join test2 t2 on t1.id=t2.id
```
4.公用表表达式（CTE）：：
```sql
with c1 as (select name from test t) , c2(name) AS (SELECT name FROM test3 t3 ) select *from c1 JOIN c2 ON c1.name=c2.name
```
5，函数返回的结果集：
特定数据库支持从返回结果集的函数中查询：
```sql
SELECT * FROM TABLE(function_name(parameters));
```
对应的表达式为SqlReferenceTableExpression

//var sw = new Stopwatch();
//sw.Start();
//sw.Stop();
//var t = sw.ElapsedMilliseconds;
//if (Logger != null)
//{
//    Logger(t, "AcceptNumber");
//}

//sw.Restart();
 //sw.Stop();
 //t = sw.ElapsedMilliseconds;
 //if (Logger != null)
 //{
 //    Logger(t, "AcceptNumber");
 //}

手动写了100个单元测试以后，瑞了瑞了，所以赶紧写了一个自动生成单元测试的visitor压压惊，即UnitTestAstVisitor.cs

visitor 结构与算法分离，你可以根据结构自己任意解析这颗语法树

sql格式化工具


如何加入本项目
1.在token.cs中添加新的token，注意CompareIndex是token的唯一标识符，要逐渐递增，不能与原有的冲突，
2.如果新的token是关键字，需要在SqlLexer.cs的InitTokenDic()方法中添加关键字字典
3.如果是新的语法类型，可能需要定义新的sqlexpression，定义的新的sqlExpression里,需要参考已有其他sqlExpression重写Equals和Accept方法
定义后需要在SqlParser.cs里添加处理逻辑
4.在SqlParser.Net.Test项目里添加新的单元测试，使用TestHelper.cs自动生成IAcceptVisitor.cs和BaseAstVisitor.cs的代码，然后更新SqlParser.Net项目里的IAcceptVisitor.cs和BaseAstVisitor.cs

Quote Symbol