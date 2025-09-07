
# 什么是sql解析
正如我们拿到一个json字符串，我们在程序里是无法处理的，只有将他反序列化为类，我们的程序才能正确处理这个类，这个反序列化的过程，也可以称之为json解析，sql解析也是一样的，帮助我们将sql字符串解析为结构化的类

# 背景
hi 大家好，我是三合，在过往的岁月中，我曾经想过要写以下这些工具
1. 写一个通过拦截业务系统所有sql，然后根据这些sql自动分析表与表,字段与字段之间是如何关联的工具，即sql血缘分析工具
2. 想着动态改写sql，比如给where动态添加一个条件。
3. 写一个sql格式化工具
4. 写一个像mycat那样的分库分表中间件
5. 写一个sql防火墙，防止出现where 1=1后没有其他条件导致查询全表
6. 写一个数据库之间的sql翻译工具，比如把sqlserver的sql自动翻译为oracle的sql

但是无一例外，都失败了，因为要实现以上这些需求，都需要一个核心的类库，即sql解析引擎，遗憾的是，我没有找到合适的，这是我当初寻找的轨迹
1. 我发现了[tsql-parser](https://github.com/bruce-dunwiddie/tsql-parser),但他只支持sql server，所以只能pass。
2. 然后我又发现了[SqlParser-cs](https://github.com/TylerBrinks/SqlParser-cs),
他的语法树解析出来像这样，
````csharp
JsonConvert.SerializeObject(statements.First(), Formatting.Indented)
// Elided for readability
{
   "Query": {
      "Body": {
         "Select": {
            "Projection": [
               {
                  "Expression": {
                     "Ident": {
                        "Value": "a",
                        "QuoteStyle": null
                     }
                  }
               }
	...
````
额，怎么说呢，这语法树也太丑了点吧，同时非常难以理解，跟我想象中的完全不一样啊，于是也只能pass。

3. 接下来我又发现了另外一些基于antlr来解析sql的类库，比如[SQLParser](https://github.com/JaCraig/SQLParser),因为代码是antlr自动生成的，比较难以进行手动优化，所以还是pass。

4. 最后我还发现了另外一个[gsp的sqlparser](https://www.sqlparser.com/sql-parser-pricelist.php),但它是收费的，而且巨贵无比，也pass。

找了一圈下来，我发现符合我要求的类库并不存在，所以我上面的那些想法，也一度搁浅了，但每一次的搁浅，都会使我内心的不甘加重一分，终于有一天，我下定决心，自己动手，丰衣足食，所以最近花了大概3个月时间，从头开始写了一个sql解析引擎，包括词法解析器到语法分析器，不依赖任何第三方组件，纯c#代码，在通过了360个各种各样场景的单元测试以及各种真实的业务环境验证后，今天它[SqlParser.Net](https://github.com/TripleView/SqlParser.Net)1.0.0正式发布了，本项目基于MIT协议开源，有以下优点，
1. 支持5大数据库，oracle，sqlserver，mysql，pgsql以及sqlite。
2. 极致的速度，解析普通sql，时间基本在0.3毫秒以下，当然了,sql越长，解析需要的时间就越长。
3. 文档完善，众所周知，我三合的开源项目，一向是文档齐全且简单易懂，做到看完就能上手，同时，我也会根据用户的反馈不断的补充以及完善文档。
4. 代码简洁易懂



# SqlParser.Net存在的意义
SqlParser.Net是一个免费，功能全面且高性能的sql解析引擎类库，它可以帮助你简单快速高效的解析和处理sql。如果你使用本项目实现了哪些有意思的功能，欢迎提交issue告诉我，我会把这些案例放在readme里，以供其他用户借此产生灵感。

# Getting Started

接下来，我将介绍[SqlParser.Net](https://github.com/TripleView/SqlParser.Net)的用法

## 通过Nuget安装
你可以运行以下命令在你的项目中安装 SqlParser.Net 。
 
 ```PM> Install-Package SqlParser.Net ```
### 支持框架

netstandard2.0

## 从最简单的demo开始
让我们一起看一个最简单的select语句是如何解析的
````csharp
var sql = "select * from test";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            }
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test"
            }
        }
    }
};
````
以上面为例子，抽象语法树的所有叶子节点均为sqlExpression的子类，且各种sqlExpression节点可以互相嵌套，组成一颗复杂无比的树，其他sql解析引擎还分为statement和expression，我认为过于复杂，所以全部统一为sqlExpression，顶级的sqlExpression总共分为4种，
1. 查询语句(SqlSelectExpression)
2. 插入语句(SqlInsertExpression)
3. 删除语句(SqlDeleteExpression)
4. 更新语句(SqlUpdateExpression)

这4种顶级语句中，我认为最复杂的是查询语句，因为查询组合非常多，要兼容各种各样的情况，其他3种反而非常简单。现阶段，sqlExpression的子类一共有38种，我将在下面的演示中，结合例子给大家讲解一下。

## 1. Select查询语句
如上例子，SqlSelectExpression代表一个查询语句，SqlSelectQueryExpression则是真正具体的查询语句，他包括了
1. 要查询的所有列（Columns字段）
2. 数据源(From字段)
3. 条件过滤语句(Where字段)
4. 分组语句（GroupBy字段）
5. 排序语句(OrderBy字段)
6. 分页语句(Limit字段)
7. Into语句(sql server专用，如SELECT id,name into test14 from TEST t)
8. ConnectBy语句（oracle专用，如SELECT LEVEL l FROM DUAL  CONNECT BY NOCYCLE LEVEL<=100）
9. WithSubQuerys语句，公用表表达式，即CTE

其中Columns是一个列表，他的每一个子项都是一个SqlSelectItemExpression，他的body代表一个逻辑子句，逻辑子句的值，可以为以下这些
1. 字段，如name，
2. 二元表达式，如t.age+3
3. 函数调用，如LOWER(t.NAME)
4. 一个完整的查询语句，如SELECT name FROM TEST2 t2

包括order by，partition by，group by,between,in,case when后面跟着的都是逻辑子句，这个稍后会演示，在这个例子中，因为是要查询所有列，所以仅有一个SqlSelectItemExpression，他的body是SqlAllColumnExpression（代表所有列），From代表要查询的数据源，在这里仅单表查询，所以From的值为SqlTableExpression（代表单表），表名是一个SqlIdentifierExpression，即标识符表达式，表示这是一个标识符，在SQL中，标识符（Identifier）是用于命名数据库对象的名称。这些对象可以包括表、列、索引、视图、模式、数据库等。标识符使得我们能够引用和操作这些对象，在这里，标识符的值为test，表示表名为test。

### 1.1 查询返回列的各种情形
#### 1.1.1 查询指定字段
````csharp
var sql = "select id AS bid,t.NAME testName  from test t";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "id",
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "bid",
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "NAME",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "t",
                    },
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "testName",
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
    },
};


````
在上面这个例子中，我们指定了要查询2个字段，分别是id和t.NAME，此时Columns列表里有2个值，
第一个SqlSelectItemExpression包含了
1. 主体，即body字段，在本例子中他的值是一个SqlIdentifierExpression表达式，值为id，表示列名为id，
2. 别名，即Alias字段，在本例子中他也是一个SqlIdentifierExpression，值为bid，代表列别名为bid，

第二个SqlSelectItemExpression的body里是一个SqlPropertyExpression，代表这是一个属性表达式，SqlPropertyExpression它包含了
1. 表名，即Table字段，值为t，即表名为t
2. 属性名，即Name字段，值为Name，即属性名为name

合起来则代表t表的name字段，而第二个SqlSelectItemExpression也有列别名，即testName，这个查询也是单表查询，但SqlTableExpression他多了一个Alias别名字段，即表示，表别名为t。

#### 1.1.2 查询列为二元表达式的情况
````csharp
var sql = "select 1+2 from test";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlBinaryExpression()
                {
                    Left = new SqlNumberExpression()
                    {
                        Value = 1M,
                    },
                    Operator = SqlBinaryOperator.Add,
                    Right = new SqlNumberExpression()
                    {
                        Value = 2M,
                    },
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
        },
    },
};
````

在这个例子中，要查询的字段的值为一个二元表达式SqlBinaryExpression，他包含了
1. 左边部分，即Left字段，值为一个SqlNumberExpression，即数字表达式，它的值为1
2. 右边部分，即Right字段，值为一个SqlNumberExpression，即数字表达式，它的值为2
3. 中间符号，即Operator字段，值为add，即加法

这个例子证明了，SqlSelectItemExpression代表一个逻辑子句，而不仅仅是某个字段。

#### 1.1.3 查询列为字符串/数字/布尔值的情况
````csharp
var sql = "select ''' ''',3,true FROM test";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlStringExpression()
                {
                    Value = "' '"
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlNumberExpression()
                {
                    Value = 3M,
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlBoolExpression()
                {
                    Value = true
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
        },
    },
};


````
在这个例子中，要查询的3个字段为字符串，数字和布尔值，字符串表达式即SqlStringExpression，body里即字符串的值' '，数字表达式即SqlNumberExpression，值为3，布尔表达式即SqlBoolExpression，值为true；

#### 1.1.4 查询列为函数调用的情况
##### 1.1.4.1 简单的函数调用
````csharp
var sql = "select LOWER(name)  FROM test";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlFunctionCallExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "LOWER",
                    },
                    Arguments = new List<SqlExpression>()
                    {
                        new SqlIdentifierExpression()
                        {
                            Value = "name",
                        },
                    },
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
        },
    },
};


````
在这个例子中，要查询的表达式是一个函数调用，函数调用表达式即SqlFunctionCallExpression，它包含了
1. 函数名，即Name字段，值为LOWER，
2. 函数参数列表，即Arguments字段，列表里只有一个值，即函数只有一个参数，且参数的值为name

##### 1.1.4.2 带有over子句的函数调用
````csharp
var sql = "SELECT t.*, ROW_NUMBER() OVER ( PARTITION BY t.ID  ORDER BY t.NAME,t.ID) as rnum FROM TEST t";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "*",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "t",
                    },
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlFunctionCallExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "ROW_NUMBER",
                    },
                    Over = new SqlOverExpression()
                    {
                        PartitionBy = new SqlPartitionByExpression()
                        {
                            Items = new List<SqlExpression>()
                            {
                                new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "ID",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "t",
                                    },
                                },
                            },
                        },
                        OrderBy = new SqlOrderByExpression()
                        {
                            Items = new List<SqlOrderByItemExpression>()
                            {
                                new SqlOrderByItemExpression()
                                {
                                    Body = new SqlPropertyExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "NAME",
                                        },
                                        Table = new SqlIdentifierExpression()
                                        {
                                            Value = "t",
                                        },
                                    },
                                },
                                new SqlOrderByItemExpression()
                                {
                                    Body = new SqlPropertyExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "ID",
                                        },
                                        Table = new SqlIdentifierExpression()
                                        {
                                            Value = "t",
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "rnum",
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
    },
};



````

在这个例子中，SqlFunctionCallExpression它除了常规字段外，还包含了Over子句，具体有以下这些
1. 函数名，即Name字段，值为ROW_NUMBER，
2. 函数参数列表，即Arguments字段，值为null，即无参数
3. Over子句，即Over字段，他的值为一个SqlOverExpression表达式，SqlOverExpression本身又包含了以下内容
    1. PartitionBy分区子句，值为一个SqlPartitionByExpression表达式，表达式的内容也非常简单，只有一个Items，即一个分区表达式的列表，在这个例子中，列表里只有一个值SqlPropertyExpression，即根据t.id分区
    2. OrderBy排序子句，值为SqlOrderByExpression表达式，表达式的内容也非常简单，只有一个Items，即一个排序表达式的列表，列表里的值为SqlOrderByItemExpression，即排序子项表达式，排序子项表达式里又包含了以下内容
        1. 排序依据，即Body字段，在这个例子中，排序依据是2个SqlPropertyExpression表达式，即根据t.NAME,t.ID排序
        2. 排序类型，即OrderByType字段，值为Asc或者Desc，默认为asc，在这2个例子中，默认排序类型都是asc

##### 1.1.4.3 带有within group子句的函数调用
````csharp
var sql = "select name,PERCENTILE_CONT(0.5) within group(order by \"number\") from TEST5 group by name";
var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "name",
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlFunctionCallExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "PERCENTILE_CONT",
                    },
                    WithinGroup = new SqlWithinGroupExpression()
                    {
                        OrderBy = new SqlOrderByExpression()
                        {
                            Items = new List<SqlOrderByItemExpression>()
                            {
                                new SqlOrderByItemExpression()
                                {
                                    Body = new SqlIdentifierExpression()
                                    {
                                        Value = "number",
                                        LeftQualifiers = "\"",
                                        RightQualifiers = "\"",
                                    },
                                },
                            },
                        },
                    },
                    Arguments = new List<SqlExpression>()
                    {
                        new SqlNumberExpression()
                        {
                            Value = 0.5M,
                        },
                    },
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST5",
            },
        },
        GroupBy = new SqlGroupByExpression()
        {
            Items = new List<SqlExpression>()
            {
            new SqlIdentifierExpression()
            {
                Value = "name",
            },
            },
        },
    },
};

````

在这个例子中，SqlFunctionCallExpression它除了常规字段外，还包含了within group子句，具体有以下这些
1. 函数名，即Name字段，值为PERCENTILE_CONT，
2. 函数参数列表，即Arguments字段，列表里只有一项，表示只有1个参数，参数是SqlNumberExpression表达式，值为0.5
3. within group子句，即WithinGroup字段，他的值为一个SqlWithinGroupExpression表达式，SqlWithinGroupExpression又包含了OrderBy排序子句，这里根据number字段排序


#### 1.1.5 查询列为子查询的情况
````csharp
var sql = "select c.*, (select a.name as province_name from portal_area a where a.id = c.province_id) as province_name, (select a.name as city_name from portal_area a where a.id = c.city_id) as city_name, (CASE WHEN c.area_id IS NULL THEN NULL ELSE (select a.name as area_name from portal_area a where a.id = c.area_id)  END )as area_name  from portal.portal_company c";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "*",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "c",
                    },
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlSelectExpression()
                {
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "name",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "a",
                                    },
                                },
                                Alias = new SqlIdentifierExpression()
                                {
                                    Value = "province_name",
                                },
                            },
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "portal_area",
                            },
                            Alias = new SqlIdentifierExpression()
                            {
                                Value = "a",
                            },
                        },
                        Where = new SqlBinaryExpression()
                        {
                            Left = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "id",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "a",
                                },
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "province_id",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "c",
                                },
                            },
                        },
                    },
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "province_name",
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlSelectExpression()
                {
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "name",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "a",
                                    },
                                },
                                Alias = new SqlIdentifierExpression()
                                {
                                    Value = "city_name",
                                },
                            },
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "portal_area",
                            },
                            Alias = new SqlIdentifierExpression()
                            {
                                Value = "a",
                            },
                        },
                        Where = new SqlBinaryExpression()
                        {
                            Left = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "id",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "a",
                                },
                            },
                            Operator = SqlBinaryOperator.EqualTo,
                            Right = new SqlPropertyExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "city_id",
                                },
                                Table = new SqlIdentifierExpression()
                                {
                                    Value = "c",
                                },
                            },
                        },
                    },
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "city_name",
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlCaseExpression()
                {
                    Items = new List<SqlCaseItemExpression>()
                    {
                        new SqlCaseItemExpression()
                        {
                            Condition = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "area_id",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "c",
                                    },
                                },
                                Operator = SqlBinaryOperator.Is,
                                Right = new SqlNullExpression()
                            },
                            Value = new SqlNullExpression()
                        },
                    },
                    Else = new SqlSelectExpression()
                    {
                        Query = new SqlSelectQueryExpression()
                        {
                            Columns = new List<SqlSelectItemExpression>()
                            {
                                new SqlSelectItemExpression()
                                {
                                    Body = new SqlPropertyExpression()
                                    {
                                        Name = new SqlIdentifierExpression()
                                        {
                                            Value = "name",
                                        },
                                        Table = new SqlIdentifierExpression()
                                        {
                                            Value = "a",
                                        },
                                    },
                                    Alias = new SqlIdentifierExpression()
                                    {
                                        Value = "area_name",
                                    },
                                },
                            },
                            From = new SqlTableExpression()
                            {
                                Name = new SqlIdentifierExpression()
                                {
                                    Value = "portal_area",
                                },
                                Alias = new SqlIdentifierExpression()
                                {
                                    Value = "a",
                                },
                            },
                            Where = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "id",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "a",
                                    },
                                },
                                Operator = SqlBinaryOperator.EqualTo,
                                Right = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "area_id",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "c",
                                    },
                                },
                            },
                        },
                    },
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "area_name",
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "portal_company",
            },
            Schema = new SqlIdentifierExpression()
            {
                Value = "portal",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "c",
            },
        },
    },
};


````
在这个例子中，要查询的列值为一个SqlSelectExpression表达式，即要查询的列是一个子查询


### 1.2 Where条件过滤语句
#### 1.2.1 二元表达式
````csharp
var sql = "SELECT * FROM test WHERE ID =1";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
        },
        Where = new SqlBinaryExpression()
        {
            Left = new SqlIdentifierExpression()
            {
                Value = "ID",
            },
            Operator = SqlBinaryOperator.EqualTo,
            Right = new SqlNumberExpression()
            {
                Value = 1M,
            },
        },
    },
};


````
在这个例子中，where字段的值是一个二元表达式SqlBinaryExpression，他包含了
1. 左边部分，即Left字段，值为一个SqlIdentifierExpression，即标识符表达式，它的值为ID
2. 右边部分，即Right字段，值为一个SqlNumberExpression，即数字表达式，它的值为1
3. 中间符号，即Operator字段，值为EqualTo，即等号，当然了，还可以是大于号，小于号，大于等于号，小于等于号，不等号等

二元表达式的两边可以非常灵活，可以是各种其他表达式，同时也可以自我嵌套另一个二元表达式，组成一个非常复杂的二元表达式

#### 1.2.2 between/not between子句
````csharp
var sql = "SELECT * FROM test WHERE ID BETWEEN 1 AND 2";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
        },
        Where = new SqlBetweenAndExpression()
        {
            Body = new SqlIdentifierExpression()
            {
                Value = "ID",
            },
            Begin = new SqlNumberExpression()
            {
                Value = 1M,
            },
            End = new SqlNumberExpression()
            {
                Value = 2M,
            },
        },
    },
};


````
between子句包含了
1. Begin部分，即Begin字段，在这个例子中，值为一个SqlNumberExpression，，它的值为1
2. End部分，即End字段，在这个例子中，值为一个SqlNumberExpression，它的值为2
3. Body主体部分，即Body字段，值为SqlIdentifierExpression，即标识符表达式，值为id
4. 取反部分，即IsNot字段，如果是not between，则IsNot=true

#### 1.2.3 is null/is not null子句
````csharp
var sql = "select * from test rd where rd.name is null";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "rd",
            },
        },
        Where = new SqlBinaryExpression()
        {
            Left = new SqlPropertyExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "name",
                },
                Table = new SqlIdentifierExpression()
                {
                    Value = "rd",
                },
            },
            Operator = SqlBinaryOperator.Is,
            Right = new SqlNullExpression()
        },
    },
};



````

is null/is not null子句主要体现在二元表达式里，Operator字段为Is/IsNot，right字段为SqlNullExpression，即null表达式，代表值为null

#### 1.2.4 exists/not exists子句
````csharp
var sql = "select * from TEST t where EXISTS(select * from TEST2 t2)";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
        Where = new SqlExistsExpression()
        {
            Body = new SqlSelectExpression()
            {
                Query = new SqlSelectQueryExpression()
                {
                    Columns = new List<SqlSelectItemExpression>()
                    {
                        new SqlSelectItemExpression()
                        {
                            Body = new SqlAllColumnExpression()
                        },
                    },
                    From = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "TEST2",
                        },
                        Alias = new SqlIdentifierExpression()
                        {
                            Value = "t2",
                        },
                    },
                },
            },
        },
    },
};




````

exists/not exists子句,主要体现为SqlExistsExpression表达式，
1. 主体，即body字段，本例子中值为一个SqlSelectExpression表达式
2. 取反部分，即IsNot字段，如果是not exists，则IsNot=true

#### 1.2.5 like/not like子句
````csharp
var sql = "SELECT * from TEST t WHERE name LIKE '%a%'";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
        Where = new SqlBinaryExpression()
        {
            Left = new SqlIdentifierExpression()
            {
                Value = "name",
            },
            Operator = SqlBinaryOperator.Like,
            Right = new SqlStringExpression()
            {
                Value = "%a%"
            },
        },
    },
};


````

like子句,主要体现在二元表达式里，Operator字段为Like/NotLike，本例子中right字段为字符串表达式，即SqlStringExpression表达式，值为%a%。

#### 1.2.6 all/any子句
````csharp
var sql = "select * from customer c where c.Age >all(select o.Quantity  from orderdetail o)";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "customer",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "c",
            },
        },
        Where = new SqlBinaryExpression()
        {
            Left = new SqlPropertyExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "Age",
                },
                Table = new SqlIdentifierExpression()
                {
                    Value = "c",
                },
            },
            Operator = SqlBinaryOperator.GreaterThen,
            Right = new SqlAllExpression()
            {
                Body = new SqlSelectExpression()
                {
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "Quantity",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "o",
                                    },
                                },
                            },
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "orderdetail",
                            },
                            Alias = new SqlIdentifierExpression()
                            {
                                Value = "o",
                            },
                        },
                    },
                },
            },
        },
    },
};



````

all/any子句,主要体现在SqlAllExpression/SqlAnyExpression表达式，它的body里是另一个SqlSelectExpression表达式

#### 1.2.7 in/ not in子句
````csharp
var sql = "SELECT  * from TEST t WHERE t.NAME IN ('a','b','c')";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
        Where = new SqlInExpression()
        {
            Body = new SqlPropertyExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "NAME",
                },
                Table = new SqlIdentifierExpression()
                {
                    Value = "t",
                },
            },
            TargetList = new List<SqlExpression>()
            {
                new SqlStringExpression()
                {
                    Value = "a"
                },
                new SqlStringExpression()
                {
                    Value = "b"
                },
                new SqlStringExpression()
                {
                    Value = "c"
                },
            },
        },
    },
};



````

in/not in子句,主要体现在SqlInExpression表达式，它包含了
1. body字段，即in的主体，在这里是SqlPropertyExpression，值为t.NAME
2. TargetList字段，即in的目标列表，在这里是一个SqlExpression的列表，里面包括3个SqlStringExpression，即字符串表达式，a,b,c
3. 取反部分，即IsNot字段，如果是not in，则IsNot=true

当然了，in也有另一种子查询的类型，即
````csharp
var sql = "select * from TEST5  WHERE NAME IN (SELECT NAME  FROM TEST3)";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST5",
            },
        },
        Where = new SqlInExpression()
        {
            Body = new SqlIdentifierExpression()
            {
                Value = "NAME",
            },
            SubQuery = new SqlSelectExpression()
            {
                Query = new SqlSelectQueryExpression()
                {
                    Columns = new List<SqlSelectItemExpression>()
                    {
                        new SqlSelectItemExpression()
                        {
                            Body = new SqlIdentifierExpression()
                            {
                                Value = "NAME",
                            },
                        },
                    },
                    From = new SqlTableExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "TEST3",
                        },
                    },
                },
            },
        },
    },
};
````

在这里的SqlInExpression表达式中，它包含了
1. body字段，即in的主体，在这里是SqlIdentifierExpression，值为NAME
2. SubQuery字段，即子查询，值为一个SqlSelectExpression
3. IsNot字段，如果是not in，则IsNot=true


#### 1.2.8 case when子句
````csharp
var sql = "SELECT CASE WHEN t.name='1' THEN 'a' WHEN t.name='2' THEN 'b' ELSE 'c' END AS v from TEST t";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlCaseExpression()
                {
                    Items = new List<SqlCaseItemExpression>()
                    {
                        new SqlCaseItemExpression()
                        {
                            Condition = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "name",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "t",
                                    },
                                },
                                Operator = SqlBinaryOperator.EqualTo,
                                Right = new SqlStringExpression()
                                {
                                    Value = "1"
                                },
                            },
                            Value = new SqlStringExpression()
                            {
                                Value = "a"
                            },
                        },
                        new SqlCaseItemExpression()
                        {
                            Condition = new SqlBinaryExpression()
                            {
                                Left = new SqlPropertyExpression()
                                {
                                    Name = new SqlIdentifierExpression()
                                    {
                                        Value = "name",
                                    },
                                    Table = new SqlIdentifierExpression()
                                    {
                                        Value = "t",
                                    },
                                },
                                Operator = SqlBinaryOperator.EqualTo,
                                Right = new SqlStringExpression()
                                {
                                    Value = "2"
                                },
                            },
                            Value = new SqlStringExpression()
                            {
                                Value = "b"
                            },
                        },
                    },
                    Else = new SqlStringExpression()
                    {
                        Value = "c"
                    },
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "v",
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
    },
};



````

case when子句,主要体现在SqlCaseExpression表达式里，他包含了
1. 各种case when键值对的列表，即Items字段，列表里的每一个元素都是SqlCaseItemExpression表达式，SqlCaseItemExpression表达式，又包含了
    1. 条件，即Condition字段，在本例子中是二元表达式，即SqlBinaryExpression表达式，值为t.name ='1'
    2. 值，即value字段,在本例子中值为字符串a
2. Else字段，即默认值，本例子中为字符串c

case when还有另外一种句式，如下：
````csharp
var sql = "select case t.name when 'a' then 1 else 2 end  from test t ";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlCaseExpression()
                {
                    Items = new List<SqlCaseItemExpression>()
                    {
                        new SqlCaseItemExpression()
                        {
                            Condition = new SqlStringExpression()
                            {
                                Value = "a"
                            },
                            Value = new SqlNumberExpression()
                            {
                                Value = 1M,
                            },
                        },
                    },
                    Else = new SqlNumberExpression()
                    {
                        Value = 2M,
                    },
                    Value = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "name",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t",
                        },
                    },
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
    },
};



````

在这种SqlCaseExpression表达式里，他包含了
1. case条件的主体变量，即Value字段，本例子中值为SqlPropertyExpression，它的值为t.name
2. 各种when then键值对的列表，即Items字段，列表里的每一个元素都是SqlCaseItemExpression表达式，SqlCaseItemExpression表达式，又包含了
    1. 条件，即Condition字段，在本例子中是字符串表达式SqlStringExpression，它的值为a
    2. 值，即Value字段,在本例子中值为SqlNumberExpression，它的值为1
3. Else字段，即默认值，本例子中为数字2

#### 1.2.9 not子句
````csharp
var sql = "select * from TEST t WHERE not t.NAME ='abc'";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
        Where = new SqlNotExpression()
        {
            Body = new SqlBinaryExpression()
            {
                Left = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "NAME",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "t",
                    },
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlStringExpression()
                {
                    Value = "abc"
                },
            },
        },
    },
};



````

not子句,主要体现在SqlNotExpression表达式里，它只有一个body字段，即代表否定的部分

#### 1.2.10 变量子句
````csharp
var sql = "select * from TEST t WHERE not t.NAME =:name";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
        Where = new SqlNotExpression()
        {
            Body = new SqlBinaryExpression()
            {
                Left = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "NAME",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "t",
                    },
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlVariableExpression()
                {
                    Name = "name",
                    Prefix = ":",
                },
            },
        },
    },
};


````

变量子句,主要体现在SqlVariableExpression表达式里，它包括以下部分:
1. 变量名，即字段Name,这里值为name
2. 变量前缀，这里值为:


### 1.3 From数据源
在sql中，From关键字后面有多种形式来指定数据源。主要有以下几种
#### 1.3.1 表名或者视图
```sql
select * from test
```
这个解析结果上面已经演示了。SqlTableExpression表达式主要包含了以下部分
1. 表名部分，即Name字段
2. 数据库模式部分，即Schema字段
3. 数据库名部分，即Database字段，如select * from [EPF].[dbo].[test]里的epf
4. DbLink部分，即DbLink字段。常用于oracle

#### 1.3.2 子查询（子表）
````csharp
var sql = "select * from (select * from test) t";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlSelectExpression()
        {
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
            Query = new SqlSelectQueryExpression()
            {
                Columns = new List<SqlSelectItemExpression>()
                {
                    new SqlSelectItemExpression()
                    {
                        Body = new SqlAllColumnExpression()
                    },
                },
                From = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test",
                    },
                },
            },
        },
    },
};

````
在这个例子中，数据源From的值为一个SqlSelectExpression，即SqlSelectExpression中可以嵌套SqlSelectExpression，同时我们注意到内部的SqlSelectExpression有一个表别名的字段Alias，标识符的值为t，表示表别名为t；

#### 1.3.3 连表查询（JOIN）
````csharp
var sql = "select t1.id from test t1 left join test2 t2 on t1.id=t2.id right join test3 t3 on t2.id=t3.id";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "id",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "t1",
                    },
                },
            },
        },
        From = new SqlJoinTableExpression()
        {
            Left = new SqlJoinTableExpression()
            {
                Left = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t1",
                    },
                },
                JoinType = SqlJoinType.LeftJoin,
                Right = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "test2",
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "t2",
                    },
                },
                Conditions = new SqlBinaryExpression()
                {
                    Left = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "id",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t1",
                        },
                    },
                    Operator = SqlBinaryOperator.EqualTo,
                    Right = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "id",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t2",
                        },
                    },
                },
            },
            JoinType = SqlJoinType.RightJoin,
            Right = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "test3",
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "t3",
                },
            },
            Conditions = new SqlBinaryExpression()
            {
                Left = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "id",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "t2",
                    },
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "id",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "t3",
                    },
                },
            },
        },
    },
};

````
在上面这个例子中，我们演示了连表查询是如何解析的，From字段的值为一个SqlJoinTableExpression，即连表查询表达式，他包含了
1. 左边部分，即Left字段
2. 右边部分，即Right字段
3. 连接方式，即JoinType字段，值包括InnerJoin,LeftJoin,RightJoin,FullJoin,CrossJoin,CommaJoin
4. 表关联条件，即Conditions字段。在这里，Conditions字段的值为一个二元表达式SqlBinaryExpression

在这个例子中，总共3张表联查，SqlJoinTableExpression中得left字段又是一个SqlJoinTableExpression，即SqlJoinTableExpression中可以嵌套SqlJoinTableExpression，无限套娃。

#### 1.3.4 公用表表达式（CTE）
````csharp
var sql = "with c1 as (select name from test t) , c2(name) AS (SELECT name FROM TEST2 t3 ) select *from c1 JOIN c2 ON c1.name=c2.name";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        WithSubQuerys = new List<SqlWithSubQueryExpression>()
        {
            new SqlWithSubQueryExpression()
            {
                Alias = new SqlIdentifierExpression()
                {
                    Value = "c1",
                },
                FromSelect = new SqlSelectExpression()
                {
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlIdentifierExpression()
                                {
                                    Value = "name",
                                },
                            },
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "test",
                            },
                            Alias = new SqlIdentifierExpression()
                            {
                                Value = "t",
                            },
                        },
                    },
                },
            },
            new SqlWithSubQueryExpression()
            {
                Alias = new SqlIdentifierExpression()
                {
                    Value = "c2",
                },
                FromSelect = new SqlSelectExpression()
                {
                    Query = new SqlSelectQueryExpression()
                    {
                        Columns = new List<SqlSelectItemExpression>()
                        {
                            new SqlSelectItemExpression()
                            {
                                Body = new SqlIdentifierExpression()
                                {
                                    Value = "name",
                                },
                            },
                        },
                        From = new SqlTableExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "TEST2",
                            },
                            Alias = new SqlIdentifierExpression()
                            {
                                Value = "t3",
                            },
                        },
                    },
                },
                Columns = new List<SqlIdentifierExpression>()
                {
                    new SqlIdentifierExpression()
                    {
                        Value = "name",
                    },
                },
            },
        },
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlJoinTableExpression()
        {
            Left = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "c1",
                },
            },
            JoinType = SqlJoinType.InnerJoin,
            Right = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "c2",
                },
            },
            Conditions = new SqlBinaryExpression()
            {
                Left = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "name",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "c1",
                    },
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "name",
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "c2",
                    },
                },
            },
        },
    },
};

````

公用表表达式（CTE），主要体现在SqlSelectQueryExpression的WithSubQuerys字段，他是一个SqlWithSubQueryExpression表达式列表，即公用表列表，它里面的每一个元素都是SqlWithSubQueryExpression表达式，此表达式，包含了
1. 公共表的来源部分，即FromSelect字段，在本例子中，他的值是一个SqlSelectExpression表达式，即一个查询
2. 公共表的表别名，即Alias字段，在本例子中，他的值是c1
3. 公共表的列部分，即Columns字段，在本例子中只有一个列名，即name

#### 1.3.5 函数返回的结果集
特定数据库支持从返回结果集的函数中查询，比如oracle中添加一个自定义函数splitstr，他的作用是将一个字符串根据;号进行分割，返回多行数据
````csharp
var sql = "SELECT * FROM TABLE(splitstr('a;b',';'))";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlReferenceTableExpression()
        {
            FunctionCall = new SqlFunctionCallExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "TABLE",
                },
                Arguments = new List<SqlExpression>()
                {
                    new SqlFunctionCallExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "splitstr",
                        },
                        Arguments = new List<SqlExpression>()
                        {
                            new SqlStringExpression()
                            {
                                Value = "a;b"
                            },
                            new SqlStringExpression()
                            {
                                Value = ";"
                            },
                        },
                    },
                },
            },
        }
    },
};

````

函数返回的结果集主要体现在SqlReferenceTableExpression表达式，他的内部包含了一个FunctionCall字段，值为SqlFunctionCallExpression表达式，代表从函数调用的结果集中进行查询。



### 1.4 OrderBy排序语句

````csharp
var sql = "select fa.FlowId  from FlowActivity fa order by fa.FlowId desc,fa.Id asc";
var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowId"
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "FlowActivity"
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "fa"
            },
        },
        OrderBy = new SqlOrderByExpression()
        {
            Items = new List<SqlOrderByItemExpression>()
            {
                new SqlOrderByItemExpression()
                {
                    Body =
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "FlowId"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    OrderByType = SqlOrderByType.Desc
                },
                new SqlOrderByItemExpression()
                {
                    Body =
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "Id"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    OrderByType = SqlOrderByType.Asc
                },
            },
        },
    },
};


````

OrderBy排序子句，值为SqlOrderByExpression表达式，表达式的内容也非常简单，只有一个Items，即一个排序子项表达式的列表，列表里的值为SqlOrderByItemExpression，即排序子项表达式，排序子项表达式里又包含了以下内容
1. 排序依据，即Body字段，在这个例子中，排序依据是2个SqlPropertyExpression表达式，即根据fa.FlowId,fa.Id排序
2. 排序类型，即OrderByType字段，值为Asc或者Desc，默认为asc，在这2个例子中，有asc和Desc
3. 决定null排在前面或后面的NullsType字段，在oracle，pgsql，sqlite中我们可以指定null在排序中的位置，如以下sql
````sql
select * from TEST5 t order by t.NAME  desc nulls FIRST,t.AGE ASC NULLS  last 
````
那么我们的NullsType字段，他的值有SqlOrderByNullsType.First和SqlOrderByNullsType.Last,与之对应。




### 1.5 GroupBy分组语句

````csharp
var sql = "select fa.FlowId  from FlowActivity fa group by fa.FlowId,fa.Id HAVING count(fa.Id)>1";
var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowId"
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "FlowActivity"
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "fa"
            },
        },
        GroupBy = new SqlGroupByExpression()
        {
            Items = new List<SqlExpression>()
            {
                new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "FlowId"
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
                new SqlPropertyExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "Id"
                    },
                    Table = new SqlIdentifierExpression()
                    {
                        Value = "fa"
                    },
                },
            },
            Having = new SqlBinaryExpression()
            {
                Left = new SqlFunctionCallExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "count"
                    },
                    Arguments = new List<SqlExpression>()
                    {
                        new SqlPropertyExpression()
                        {
                            Name = new SqlIdentifierExpression()
                            {
                                Value = "Id"
                            },
                            Table = new SqlIdentifierExpression()
                            {
                                Value = "fa"
                            },
                        },
                    },
                },
                Operator = SqlBinaryOperator.GreaterThen,
                Right = new SqlNumberExpression()
                {
                    Value = 1M
                },
            },
        },
    },
};


````

GroupBy分组语句，值为SqlGroupByExpression表达式，他的内容如下
1. 分组子项表达式的列表，即Items字段，列表里的值为SqlExpression，他的值是一个逻辑子句
2. 分组过滤子句，即Having字段，他的值是一个逻辑子句，在本例子中，逻辑子句的值为一个SqlBinaryExpression

### 1.5 Limit分页子句

#### 1.5.1 mysql，sqlite
````csharp
var sql = "select * from test t limit 1,5";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
        Limit = new SqlLimitExpression()
        {
            Offset = new SqlNumberExpression()
            {
                Value = 1M,
            },
            RowCount = new SqlNumberExpression()
            {
                Value = 5M,
            },
        },
    },
};


````

Limit分页子句，值为SqlLimitExpression表达式，他的内容如下
1. 每页数量，即RowCount字段，这本例子中，值为5
2. 跳过数量，即Offset字段,本例子中，值为1

#### 1.5.2 oracle
````csharp
var sql = "SELECT * FROM TEST3 t  ORDER BY t.NAME  DESC FETCH FIRST 2 rows ONLY";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            }
        },
        From = new SqlTableExpression()
        {
            Alias = new SqlIdentifierExpression()
            {
                Value = "t"
            },
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST3"
            }
        },

        OrderBy = new SqlOrderByExpression()
        {
            Items = new List<SqlOrderByItemExpression>()
            {
                new SqlOrderByItemExpression()
                {
                    OrderByType = SqlOrderByType.Desc,
                    Body = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Value = "NAME" },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t"
                        }
                    }
                }
            }
        },
        Limit = new SqlLimitExpression()
        {
            RowCount = new SqlNumberExpression()
            {
                Value = 2
            }
        }
    }
};

````

#### 1.5.3 pgsql
````csharp
var sql = "select * from test5   t order by t.name limit 1 offset 10;";
var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test5",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
        OrderBy = new SqlOrderByExpression()
        {
            Items = new List<SqlOrderByItemExpression>()
            {
                new SqlOrderByItemExpression()
                {
                    Body = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "name",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t",
                        },
                    },
                },
            },
        },
        Limit = new SqlLimitExpression()
        {
            Offset = new SqlNumberExpression()
            {
                Value = 10M,
            },
            RowCount = new SqlNumberExpression()
            {
                Value = 1M,
            },
        },
    },
};


````

#### 1.5.4 sqlServer
````csharp
var sql = "select * from test t order by t.name OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY";
var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            }
        },
        From = new SqlTableExpression()
        {
            Alias = new SqlIdentifierExpression()
            {
                Value = "t"
            },
            Name = new SqlIdentifierExpression()
            {
                Value = "test"
            }
        },

        OrderBy = new SqlOrderByExpression()
        {
            Items = new List<SqlOrderByItemExpression>()
            {
                new SqlOrderByItemExpression()
                {
                    Body = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression() { Value = "name" },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t"
                        }
                    }
                }
            }
        },
        Limit = new SqlLimitExpression()
        {
            Offset = new SqlNumberExpression()
            {
                Value = 5
            },
            RowCount = new SqlNumberExpression()
            {
                Value = 10
            }
        }
    }
};


````

### 1.6 ConnectBy层次查询语句（oracle专用）

````csharp
var sql = "SELECT EMPLOYEEID , MANAGERID , LEVEL FROM EMPLOYEE e START WITH MANAGERID IS NULL CONNECT BY NOCYCLE PRIOR EMPLOYEEID = MANAGERID ORDER SIBLINGS BY EMPLOYEEID ";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "EMPLOYEEID",
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "MANAGERID",
                },
            },
            new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "LEVEL",
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "EMPLOYEE",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "e",
            },
        },
        ConnectBy = new SqlConnectByExpression()
        {
            StartWith = new SqlBinaryExpression()
            {
                Left = new SqlIdentifierExpression()
                {
                    Value = "MANAGERID",
                },
                Operator = SqlBinaryOperator.Is,
                Right = new SqlNullExpression()
            },
            Body = new SqlBinaryExpression()
            {
                Left = new SqlIdentifierExpression()
                {
                    Value = "EMPLOYEEID",
                },
                Operator = SqlBinaryOperator.EqualTo,
                Right = new SqlIdentifierExpression()
                {
                    Value = "MANAGERID",
                },
            },
            IsNocycle = true,
            IsPrior = true,
            OrderBy = new SqlOrderByExpression()
            {
                Items = new List<SqlOrderByItemExpression>()
                {
                    new SqlOrderByItemExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "EMPLOYEEID",
                        },
                    },
                },
                IsSiblings = true,
            },
        },
    },
};

````

ConnectBy层次查询子句，值为SqlConnectByExpression表达式，他的内容如下
1. 指定层次查询的根节点条件，即StartWith字段，本例子中他的值为SqlBinaryExpression二元表达式
2. 主体关联条件子句，即Body字段，本例子中他的值是一个SqlBinaryExpression二元表达式
3. IsPrior字段用来指示层次结构中哪个列是父节点，如果sql中存在Prior关键字，则值为true
4. IsNocycle字段，用来防止循环引用导致无限递归，如果sql中存在Nocycle则为true
5. order by子句，用于排序

### 1.7 Into子句(sql server专用）

````csharp
var sql = "SELECT name into test14 from TEST as t ";
var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlIdentifierExpression()
                {
                    Value = "name"
                },
            },
        },
        Into = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test14"
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "TEST"
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t"
            },
        },
    },
};

````

into子句，在本例子中值为SqlTableExpression，即into到某张表里。

### 1.8 At Time Zone子句

````csharp
var sql = "select order_date at time zone  'Asia/ShangHai' as b from orders";
var sqlAst = DbUtils.Parse(sql, DbType.Pgsql);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAtTimeZoneExpression()
                {
                    Body = new SqlIdentifierExpression()
                    {
                        Value = "order_date",
                    },
                    TimeZone = new SqlStringExpression()
                    {
                        Value = "Asia/ShangHai"
                    },
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "b",
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "orders",
            },
        },
    },
};


````

At Time Zone子句，在本例子中值为SqlAtTimeZoneExpression，代表这是一个At Time Zone表达式，SqlAtTimeZoneExpression它包含了
1. 时区，即TimeZone字段，值为Asia/ShangHai
2. 主体关联条件子句，即Body字段，值为order_date

### 1.9 Interval子句

````csharp
var sql = "SELECT DATE_ADD(order_date, INTERVAL 3 HOUR) AS b FROM orders;";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlFunctionCallExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = "DATE_ADD",
                    },
                    Arguments = new List<SqlExpression>()
                    {
                        new SqlIdentifierExpression()
                        {
                            Value = "order_date",
                        },
                        new SqlIntervalExpression()
                        {
                            Body = new SqlNumberExpression()
                            {
                                Value = 3M,
                            },
                            Unit = new SqlTimeUnitExpression()
                            {
                                Unit = "HOUR"
                            },
                        },
                    },
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "b",
                },
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "orders",
            },
        },
    },
};


````

Interval子句，在本例子中值为SqlIntervalExpression，代表这是一个Interval表达式，SqlIntervalExpression它包含了
1. 单位，即Unit字段，值为SqlTimeUnitExpression表达式，SqlTimeUnitExpression表达式代表了这是用来存储时间单位的表达式，在这里SqlTimeUnitExpression里的值为HOUR
2. 时间间隔数值，即Body字段，值为3

### 1.10 Collate子句

````csharp
var sql = "SELECT * FROM test3 t  ORDER BY t.a collate utf8_general_ci;";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test3",
            },
            Alias = new SqlIdentifierExpression()
            {
                Value = "t",
            },
        },
        OrderBy = new SqlOrderByExpression()
        {
            Items = new List<SqlOrderByItemExpression>()
            {
                new SqlOrderByItemExpression()
                {
                    Body = new SqlPropertyExpression()
                    {
                        Name = new SqlIdentifierExpression()
                        {
                            Value = "a",
                        },
                        Table = new SqlIdentifierExpression()
                        {
                            Value = "t",
                        },
                    },
                    Collate = new SqlCollateExpression()
                    {
                        Body = new SqlIdentifierExpression()
                        {
                            Value = "utf8_general_ci",
                        },
                    },
                },
            },
        },
    },
};

````

Collate子句，即字符串比较和排序规则子句，在本例子中值为SqlCollateExpression，代表这是一个Collate表达式，SqlCollateExpression它包含了

1. 排序规则，即Body字段，值为utf8_general_ci

### 1.11 RegEx子句

````csharp
var sql = "SELECT * FROM test3 WHERE a regexp 'a'";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test3",
            },
        },
        Where = new SqlRegexExpression()
        {
            Body = new SqlIdentifierExpression()
            {
                Value = "a",
            },
            RegEx = new SqlStringExpression()
            {
                Value = "a"
            },
            IsCaseSensitive = true,
        },
    },
};


````

### 1.12 Returning子句

````csharp
var sql = "INSERT INTO CUSTOMER (name,age,TOTALCONSUMPTIONAMOUNT) VALUES (:name,:age,:TOTALCONSUMPTIONAMOUNT) RETURNING id,age INTO :id,:age2";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlInsertExpression()
{
    Columns = new List<SqlExpression>()
    {
        new SqlIdentifierExpression()
        {
            Value = "name",
        },
        new SqlIdentifierExpression()
        {
            Value = "age",
        },
        new SqlIdentifierExpression()
        {
            Value = "TOTALCONSUMPTIONAMOUNT",
        },
    },
    ValuesList = new List<List<SqlExpression>>()
    {
        new List<SqlExpression>()
        {
            new SqlVariableExpression()
            {
                Name = "name",
                Prefix = ":",
            },
            new SqlVariableExpression()
            {
                Name = "age",
                Prefix = ":",
            },
            new SqlVariableExpression()
            {
                Name = "TOTALCONSUMPTIONAMOUNT",
                Prefix = ":",
            },
        },
    },
    Table = new SqlTableExpression()
    {
        Name = new SqlIdentifierExpression()
        {
            Value = "CUSTOMER",
        },
    },
    Returning = new SqlReturningExpression()
    {
        Items = new List<SqlExpression>()
        {
                new SqlSelectItemExpression()
                {
                    Body = new SqlIdentifierExpression()
                    {
                        Value = "id",
                    },
                },
                new SqlSelectItemExpression()
                {
                    Body = new SqlIdentifierExpression()
                    {
                        Value = "age",
                    },
                },
        },
        IntoVariables = new List<SqlExpression>()
        {
            new SqlVariableExpression()
            {
                Name = "id",
                Prefix = ":",
            },
            new SqlVariableExpression()
            {
                Name = "age2",
                Prefix = ":",
            },
        },
    },
};

````

RegEx子句，即正则表达式子句，仅在mysql和pgsql中出现，在本例子中值为SqlRegexExpression，代表这是一个Regex表达式，SqlRegexExpression它包含了

1. 要进行正则匹配的字段，即Body字段，值为a
2. 正则表达式，即RegEx字段，值为a
3. 是否大小写敏感，即IsCaseSensitive字段，值为true

## 2. Insert插入语句

### 2.1 插入单个值

````csharp
var sql = "insert into test11(name,id) values('a1','a2')";
var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
````
解析结果如下：
````csharp
var expect = new SqlInsertExpression()
{
    Columns = new List<SqlExpression>()
    {
        new SqlIdentifierExpression()
        {
            Value = "name"
        },
        new SqlIdentifierExpression()
        {
            Value = "id"
        },
    },
    ValuesList = new List<List<SqlExpression>>()
    {
        new List<SqlExpression>()
        {
            new SqlStringExpression()
            {
                Value = "a1"
            },
            new SqlStringExpression()
            {
                Value = "a2"
            },
        },
    },
    Table = new SqlTableExpression()
    {
        Name = new SqlIdentifierExpression()
        {
            Value = "test11"
        },
    },
};

````

如上例子，插入语句表现为一个SqlInsertExpression，他包含了
1. 要插入的字段列表，即Columns字段，值为一个SqlExpression的列表，本例子中值为2个SqlIdentifierExpression，它们的值为name和id，即插入name和id字段
2. 值列表，即ValuesList字段，值为一个List\<SqlExpression\>的列表，即列表里的元素是一个列表，每个元素代表一组待插入的数据，本例子中列表里只有一个List<SqlExpression>，并且子列表里的值为a1和a2，即待插入的值为a1和a2。
3. 待插入数据的表，即Table字段，本例子中值为test11

为什么ValuesList字段是列表里嵌套列表呢？主要是因为可以插入多个值列表，让我们继续往下看

### 2.2 插入多个值

````csharp
var sql = "insert into test11(name,id) values('a1','a2'),('a3','a4')";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlInsertExpression()
{
    Columns = new List<SqlExpression>()
    {
        new SqlIdentifierExpression()
        {
            Value = "name"
        },
        new SqlIdentifierExpression()
        {
            Value = "id"
        },
    },
    ValuesList = new List<List<SqlExpression>>()
    {
        new List<SqlExpression>()
        {
            new SqlStringExpression()
            {
                Value = "a1"
            },
            new SqlStringExpression()
            {
                Value = "a2"
            },
        },
        new List<SqlExpression>()
        {
            new SqlStringExpression()
            {
                Value = "a3"
            },
            new SqlStringExpression()
            {
                Value = "a4"
            },
        },
    },
    Table = new SqlTableExpression()
    {
        Name = new SqlIdentifierExpression()
        {
            Value = "test11"
        },
    },
};

````

在本例子中，ValuesList字段中有2个子元素，即2个List<SqlExpression>列表，代表插入2组数据，值分别为('a1','a2')和('a3','a4')


### 2.3 待插入的值为一个子查询

````csharp
var sql = "INSERT INTO TEST2(name) SELECT name AS name2 FROM TEST t";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlInsertExpression()
{
    Columns = new List<SqlExpression>()
    {
        new SqlIdentifierExpression()
        {
            Value = "name"
        },
    },
    Table = new SqlTableExpression()
    {
        Name = new SqlIdentifierExpression()
        {
            Value = "TEST2"
        },
    },
    FromSelect = new SqlSelectExpression()
    {
        Query = new SqlSelectQueryExpression()
        {
            Columns = new List<SqlSelectItemExpression>()
            {
                new SqlSelectItemExpression()
                {
                    Body = new SqlIdentifierExpression()
                    {
                        Value = "name"
                    },
                    Alias = new SqlIdentifierExpression()
                    {
                        Value = "name2"
                    },
                },
            },
            From = new SqlTableExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = "TEST"
                },
                Alias = new SqlIdentifierExpression()
                {
                    Value = "t"
                },
            },
        },
    },
};

````

如上例子，插入语句表现为一个SqlInsertExpression，他包含了
1. 要插入的字段列表，即Columns字段，值为一个SqlExpression的列表，本例子中值为name
2. 子查询来源，即FromSelect字段，值为一个SqlSelectExpression，即一个子查询
3. 待插入数据的表，即Table字段，本例子中值为TEST2


## 3. Update更新语句

````csharp
var sql = "update test set name ='4',d='2024-11-22 08:19:47.243' where name ='1'";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlUpdateExpression()
{
    Table = new SqlTableExpression()
    {
        Name = new SqlIdentifierExpression()
        {
            Value = "test"
        },
    },
    Where = new SqlBinaryExpression()
    {
        Left = new SqlIdentifierExpression()
        {
            Value = "name"
        },
        Operator = SqlBinaryOperator.EqualTo,
        Right = new SqlStringExpression()
        {
            Value = "1"
        },
    },
    Items = new List<SqlExpression>()
    {
        new SqlBinaryExpression()
        {
            Left = new SqlIdentifierExpression()
            {
                Value = "name"
            },
            Operator = SqlBinaryOperator.EqualTo,
            Right = new SqlStringExpression()
            {
                Value = "4"
            },
        },
        new SqlBinaryExpression()
        {
            Left = new SqlIdentifierExpression()
            {
                Value = "d"
            },
            Operator = SqlBinaryOperator.EqualTo,
            Right = new SqlStringExpression()
            {
                Value = "2024-11-22 08:19:47.243"
            },
        },
    },
};
````

如上例子，更新语句表现为一个SqlUpdateExpression，他包含了
1. 要更新的（字段-值）的列表，即Items字段，值为一个SqlExpression的列表，本例子中值为2个SqlBinaryExpression，即name='4'和d='2024-11-22 08:19:47.243'
2. 条件过滤子句，即Where字段，代表过滤条件，本例子中值为一个SqlBinaryExpression，即name ='1'
3. 待更新数据的表，即Table字段，本例子中值为test


## 4. Delete删除语句

````csharp
var sql = "delete from test where name=4";
var sqlAst = DbUtils.Parse(sql, DbType.MySql);
````
解析结果如下：
````csharp
var expect = new SqlDeleteExpression()
{
    Table = new SqlTableExpression()
    {
        Name = new SqlIdentifierExpression()
        {
            Value = "test"
        },
    },
    Where = new SqlBinaryExpression()
    {
        Left = new SqlIdentifierExpression()
        {
            Value = "name"
        },
        Operator = SqlBinaryOperator.EqualTo,
        Right = new SqlNumberExpression()
        {
            Value = 4M
        },
    },
};
````

如上例子，删除语句表现为一个SqlDeleteExpression，他包含了
1. 条件过滤子句，即Where字段，代表过滤条件，本例子中值为一个SqlBinaryExpression，即name=4
2. 表源头，即Table字段，本例子中值为test
3. 删除的主体，即Body字段，比如DELETE t from T3 t join T4 t4 on t.id=t4.Pid where t.id='abc'中的t


## 5. 注释处理

### 5.1 单行注释
````csharp
var sql = @"select *--abc from test lbu WHERE a ='1'--aaaaaa
FROM test";
var sqlAst = DbUtils.Parse(sql, DbType.SqlServer);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
        },
    },
};
````

如上例子，单行注释被正确忽视，解析正确。

### 5.2 多行注释
````csharp
var sql = @"/*这
            是
            顶部*/
            select *--abc
            FROM test/*这
            是
            底部*/";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析结果如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
        },
    },
};

````

如上例子，多行注释被正确忽视，解析正确。

## 6. 如何解析ast抽象语法树

当我们通过
````csharp
var sql = @"select * from test";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````
解析sql获取到抽象语法树以后，我们就要对这颗抽象语法树进行解析，获取我们想要的数据，此时就要用上访问者模式(visitor) .

### 6.1 访问者模式
访问者模式最大的特点就是结构与算法分离，结合本项目理解，就是ast抽象语法树这个结构已经解析出来了，你可以根据自己的需要写算法去任意解析这颗语法树。这是一个1-N的操作，即一个抽象语法树，可以对应N个解析算法，当我们要自定义算法去解析抽象语法树时，我们需要自定义一个Visitor类，并且实现IAstVisitor接口
````csharp
public class CustomVisitor : IAcceptVisitor
{
    
}
````
但是实现这个接口要实现接口里的很多个方法，并且有些数据并不是我们关心的，所以我提供了一个实现了IAcceptVisitor接口的抽象类BaseAstVisitor用来简化操作，我们只需要继承这个抽象类，然后重写我们感兴趣的方法即可
````csharp
public class CustomVisitor : BaseAstVisitor
{
    
}
````
在本项目中，我提供了2个基本的vistor供大家使用，UnitTestAstVisitor和SqlGenerationAstVisitor，大家可以参考这2个visitor去写自己的算法来解析抽象语法树。接下来，我将介绍这2个visitor的用法。

### 6.2 UnitTestAstVisitor

当我们通过

````csharp
var sql = @"select * from test";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
````

解析sql获取到抽象语法树之后，sqlAst其实还是一个数据结构，我们可以通过vs监视这个变量来查看内部的结构，但是如果是非常复杂的sql，那这颗树会巨大无比，要靠我们手动去慢慢点开查看得累死，没错！写单元测试的时候，我刚开始都是用手写的结果去对比引擎解析出来的结果，后来我就被累死了，该说不说，这活真不是人干的，所以痛定思痛之后我就写了这个UnitTestAstVisitor来替我生成ast的结构字符串，接下来让我们看看用法
````csharp
var sql = @"select * from test";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
var unitTestAstVisitor = new UnitTestAstVisitor();
sqlAst.Accept(unitTestAstVisitor);
var result = unitTestAstVisitor.GetResult();
````
其中的result就是解析抽象语法树生成的字符串，如下：
````csharp
var expect = new SqlSelectExpression()
{
    Query = new SqlSelectQueryExpression()
    {
        Columns = new List<SqlSelectItemExpression>()
        {
            new SqlSelectItemExpression()
            {
                Body = new SqlAllColumnExpression()
            },
        },
        From = new SqlTableExpression()
        {
            Name = new SqlIdentifierExpression()
            {
                Value = "test",
            },
        },
    },
};
````
然后把这个生成的字符串黏贴到vs里去和引擎生成的结果进行对比，
````csharp
Assert.True(sqlAst.Equals(expect));
````
至此，我写单元测试的工作量大大减轻，同时对于生成的sqlAst语法树的结构也更加一目了然了。


### 6.3 SqlGenerationAstVisitor

我们通过解析sql生成了抽象语法树之后，如果我们想要给这颗抽象语法树添加一个where条件,比如添加test.name ='a'
````csharp
var sql = @"select * from test";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
if (sqlAst is SqlSelectExpression sqlSelectExpression && sqlSelectExpression.Query is SqlSelectQueryExpression sqlSelectQueryExpression)
{
    sqlSelectQueryExpression.Where = new SqlBinaryExpression()
    {
        Left = new SqlPropertyExpression()
        {
            Table = new SqlIdentifierExpression()
            {
                Value = "test"
            },
            Name = new SqlIdentifierExpression()
            {
                Value = "name"
            }
        },
        Operator = SqlBinaryOperator.EqualTo,
        Right = new SqlStringExpression()
        {
            Value = "a"
        }
    };
}
````

好了，现在我们添加完了，接下来我们肯定是想着把抽象语法树转化为sql语句，此时，就需要用上SqlGenerationAstVisitor了，他就是负责把抽象语法树转化为sql的
````csharp
var sqlGenerationAstVisitor = new SqlGenerationAstVisitor(DbType.Oracle);
sqlAst.Accept(sqlGenerationAstVisitor);
var newSql = sqlGenerationAstVisitor.GetResult();
````
我们获取到的newSql就是新的sql了，他的值为
````sql
select * from test where(test.name = 'a')
````
至此，我们的目的就达到了。

### 6.4 打印抽象语法树信息和生成sql

基于UnitTestAstVisitor，抽象语法树包装了sqlAst.ToFormat()方法用来打印抽象语法树的信息，基于SqlGenerationAstVisitor，抽象语法树包装了sqlAst.ToSql()方法用来打印生成的sql。

### 6.5 抽象语法树的克隆

在我们的实际使用中，我们有时候需要克隆整个抽象语法树，在克隆出来的抽象语法树中修改任何东西，都不会影响到原有的抽象语法树，框架也提供了对应的Clone方法，如下:

````csharp
var sql = @"select * from test";
var sqlAst = DbUtils.Parse(sql, DbType.Oracle);
var cloneAst = sqlAst.Clone();
````

## 7. sql解析的理论基础
sql之所以能被我们解析出来，主要是因为sql是一种形式语言，自然语言和形式语言的一个重要区别是，自然语言的一个语句，可能有多重含义，而形式语言的一个语句，只能有一个语义;形式语言的语法是人为规定的，有了一定的语法规则，语法解析器就能根据语法规则，解析出一个语句的唯一含义。

# 项目感悟
1. 解决嵌套问题的唯一方案，就是用递归
2. 对于基础项目，单元测试非常非常重要，因为开发的过程中可能会不断地重构，那以前跑过的测试案例就有可能失败，如果此时需要靠人手工去回归测试验证的话，那工作量是天量的，做不完，根本做不完，所以正确的解决方案是写单元测试，新添加一个功能后，为这个功能写1-N个单元测试，确保新功能对各种情况都有覆盖到，然后再跑一遍所有单元测试，确保没有影响到旧的功能。当然了，跑单元测试最让我崩溃的是，跑一遍所有单元测试，红了（即失败）几十个，天都塌了。


# 开源地址，欢迎star
本项目基于MIT协议开源，地址为
[https://github.com/TripleView/SqlParser.Net](https://github.com/TripleView/SqlParser.Net)

同时感谢以下项目

1. [阿里巴巴开源的druid](https://github.com/alibaba/druid)

# 写在最后
如果各位靓仔觉得这个项目不错，欢迎一键三连（推荐，star，关注）,同时欢迎加入三合的开源交流群，QQ群号：799648362

![QQ群799648362](https://img2024.cnblogs.com/blog/1323385/202412/1323385-20241216025914908-1726484063.jpg)