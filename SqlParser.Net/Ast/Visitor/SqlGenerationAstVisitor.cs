using System;
using System.Linq;
using System.Text;
using SqlParser.Net.Ast.Expression;

namespace SqlParser.Net.Ast.Visitor;

public class SqlGenerationAstVisitor : BaseAstVisitor
{
    private StringBuilder sb = new StringBuilder();
    private string fourSpace = "    ";
    private int numberOfLevels = 0;

    private bool addSpace = true;
    private DbType dbType = DbType.MySql;
    private bool addParen = true;
    private bool isFirstVisit = true;
    private bool IsOracle => this.dbType == DbType.Oracle;
    private bool IsSqlServer => this.dbType == DbType.SqlServer;
    private bool IsPgsql => this.dbType == DbType.Pgsql;

    private bool IsMySql => this.dbType == DbType.MySql;
    private bool IsSqlite => this.dbType == DbType.Sqlite;
    public SqlGenerationAstVisitor(DbType dbType)
    {
        this.dbType = dbType;
    }

    public string GetResult()
    {
        return sb.ToString().Trim(' ');
    }
    public override void VisitSqlAllColumnExpression(SqlAllColumnExpression sqlAllColumnExpression)
    {
        Append("*");
    }
    public override void VisitSqlAllExpression(SqlAllExpression sqlAllExpression)
    {
        Append("all");
        if (sqlAllExpression.Body != null)
        {
            WithBrackets(() => { sqlAllExpression.Body.Accept(this); });
        }
    }
    public override void VisitSqlAnyExpression(SqlAnyExpression sqlAnyExpression)
    {
        Append("any");
        if (sqlAnyExpression.Body != null)
        {
            WithBrackets(() => { sqlAnyExpression.Body.Accept(this); });
        }
    }
    public override void VisitSqlBetweenAndExpression(SqlBetweenAndExpression sqlBetweenAndExpression)
    {
        if (sqlBetweenAndExpression.Body != null)
        {
            sqlBetweenAndExpression.Body.Accept(this);
        }

        if (sqlBetweenAndExpression.IsNot)
        {
            Append("not");
        }
        Append("between");
        if (sqlBetweenAndExpression.Begin != null)
        {
            sqlBetweenAndExpression.Begin.Accept(this);
        }

        Append("and");

        if (sqlBetweenAndExpression.End != null)
        {
            sqlBetweenAndExpression.End.Accept(this);
        }

    }
    public override void VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
    {
        void action()
        {
            if (sqlBinaryExpression.Left != null)
            {
                sqlBinaryExpression.Left?.Accept(this);
            }

            if (sqlBinaryExpression.Operator != null)
            {
                Append(sqlBinaryExpression.Operator.Value.ToString().ToLowerInvariant());
            }

            if (sqlBinaryExpression.Right != null)
            {
                sqlBinaryExpression.Right?.Accept(this);
            }
        }

        if (sqlBinaryExpression.Parent is SqlConnectByExpression)
        {
            action();
        }
        else
        {
            WithBrackets(() =>
            {
                action();

            });
        }
    }
    public override void VisitSqlCaseExpression(SqlCaseExpression sqlCaseExpression)
    {
        Append("case");

        if (sqlCaseExpression.Value != null)
        {
            sqlCaseExpression.Value.Accept(this);
        }

        if (sqlCaseExpression.Items != null)
        {
            foreach (var item in sqlCaseExpression.Items)
            {
                item.Accept(this);
            }
        }

        if (sqlCaseExpression.Else != null)
        {
            Append("else");
            sqlCaseExpression.Else.Accept(this);
        }

        Append("end");
    }
    public override void VisitSqlCaseItemExpression(SqlCaseItemExpression sqlCaseItemExpression)
    {

        if (sqlCaseItemExpression.Condition != null)
        {
            Append("when");
            sqlCaseItemExpression.Condition.Accept(this);
        }

        if (sqlCaseItemExpression.Value != null)
        {
            Append("then");
            sqlCaseItemExpression.Value.Accept(this);
        }

    }
    public override void VisitSqlDeleteExpression(SqlDeleteExpression sqlDeleteExpression)
    {
        Append("delete");
        if (sqlDeleteExpression.Body != null)
        {
            sqlDeleteExpression.Body.Accept(this);
        }

        Append("from");

        if (sqlDeleteExpression.Table != null)
        {
            sqlDeleteExpression.Table.Accept(this);
        }
        if (sqlDeleteExpression.Where != null)
        {
            Append("where");
            sqlDeleteExpression.Where.Accept(this);
        }

    }
    public override void VisitSqlExistsExpression(SqlExistsExpression sqlExistsExpression)
    {
        if (sqlExistsExpression.IsNot)
        {
            Append("not");
        }

        Append("exists");

        if (sqlExistsExpression.Body != null)
        {
            WithBrackets(() =>
            {
                sqlExistsExpression.Body.Accept(this);
            });
        }

    }

    public override void VisitSqlFunctionCallExpression(SqlFunctionCallExpression sqlFunctionCallExpression)
    {

        if (sqlFunctionCallExpression.Name != null)
        {
            sqlFunctionCallExpression.Name?.Accept(this);
        }
        if (sqlFunctionCallExpression.Arguments != null)
        {
            WithBrackets(() =>
            {
                for (var i = 0; i < sqlFunctionCallExpression.Arguments.Count; i++)
                {
                    if (i == 0)
                    {
                        if (sqlFunctionCallExpression.IsDistinct)
                        {
                            Append("distinct");
                        }
                    }
                    var argument = sqlFunctionCallExpression.Arguments[i];
                    argument.Accept(this);

                    if (sqlFunctionCallExpression.CaseAsTargetType != null)
                    {
                        Append($"as {sqlFunctionCallExpression.CaseAsTargetType.Value}");
                    }

                    if (sqlFunctionCallExpression.FromSource != null)
                    {
                        Append($"from ");
                        sqlFunctionCallExpression.FromSource.Accept(this);
                    }

                    if (i < sqlFunctionCallExpression.Arguments.Count - 1)
                    {
                        AppendWithoutSpaces(",");
                    }
                }
            });
        }
        else
        {
            AppendWithoutSpaces("()");
        }

        if (sqlFunctionCallExpression.WithinGroup != null)
        {
            sqlFunctionCallExpression.WithinGroup?.Accept(this);
        }

        if (sqlFunctionCallExpression.Over != null)
        {
            sqlFunctionCallExpression.Over?.Accept(this);
        }

    }
    public override void VisitSqlGroupByExpression(SqlGroupByExpression sqlGroupByExpression)
    {
        Append("group by");
        if (sqlGroupByExpression.Items != null)
        {
            for (var i = 0; i < sqlGroupByExpression.Items.Count; i++)
            {
                var item = sqlGroupByExpression.Items[i];
                item.Accept(this);
                if (i < sqlGroupByExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }
        }

        if (sqlGroupByExpression.Having != null)
        {
            Append("having");
            sqlGroupByExpression.Having.Accept(this);
        }

    }
    public override void VisitSqlIdentifierExpression(SqlIdentifierExpression sqlIdentifierExpression)
    {
        if (!string.IsNullOrWhiteSpace(sqlIdentifierExpression.LeftQualifiers))
        {
            Append(sqlIdentifierExpression.LeftQualifiers + sqlIdentifierExpression.Value + sqlIdentifierExpression.RightQualifiers);
        }
        else
        {
            Append(sqlIdentifierExpression.Value);
        }
    }
    public override void VisitSqlInExpression(SqlInExpression sqlInExpression)
    {
        if (sqlInExpression.Body != null)
        {
            sqlInExpression.Body.Accept(this);
        }

        if (sqlInExpression.IsNot)
        {
            Append("not");
        }
        Append("in");

        if (sqlInExpression.TargetList != null)
        {
            WithBrackets(() =>
            {
                for (var i = 0; i < sqlInExpression.TargetList.Count; i++)
                {
                    var item = sqlInExpression.TargetList[i];
                    item.Accept(this);
                    if (i < sqlInExpression.TargetList.Count - 1)
                    {
                        AppendWithoutSpaces(",");
                    }
                }
            });
        }

        if (sqlInExpression.SubQuery != null)
        {
            sqlInExpression.SubQuery.Accept(this);
        }
    }
    public override void VisitSqlInsertExpression(SqlInsertExpression sqlInsertExpression)
    {
        Append("insert into");

        if (sqlInsertExpression.Table != null)
        {
            sqlInsertExpression.Table?.Accept(this);
        }

        if (sqlInsertExpression.Columns != null)
        {
            WithBrackets(() =>
            {
                for (var i = 0; i < sqlInsertExpression.Columns.Count; i++)
                {
                    var item = sqlInsertExpression.Columns[i];
                    item.Accept(this);
                    if (i < sqlInsertExpression.Columns.Count - 1)
                    {
                        AppendWithoutSpaces(",");
                    }
                }
            });
        }
        if (sqlInsertExpression.ValuesList != null)
        {
            Append("values");
            for (var i = 0; i < sqlInsertExpression.ValuesList.Count; i++)
            {
                var items = sqlInsertExpression.ValuesList[i];
                WithBrackets((() =>
                {
                    for (var j = 0; j < items.Count; j++)
                    {
                        var item = items[j];
                        item.Accept(this);
                        if (j < items.Count - 1)
                        {
                            AppendWithoutSpaces(",");
                        }
                    }
                }));


                if (i < sqlInsertExpression.ValuesList.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }

        }

        if (sqlInsertExpression.FromSelect != null)
        {
            sqlInsertExpression.FromSelect?.Accept(this);
        }
    }
    public override void VisitSqlJoinTableExpression(SqlJoinTableExpression sqlJoinTableExpression)
    {

        if (sqlJoinTableExpression.Left != null)
        {
            sqlJoinTableExpression.Left.Accept(this);
        }
        if (sqlJoinTableExpression.JoinType != null)
        {
            var joinType = "";
            switch (sqlJoinTableExpression.JoinType)
            {
                case SqlJoinType.InnerJoin:
                    joinType = "inner join";
                    break;
                case SqlJoinType.LeftJoin:
                    joinType = "left join";
                    break;
                case SqlJoinType.RightJoin:
                    joinType = "right join";
                    break;
                case SqlJoinType.FullJoin:
                    joinType = "full join";
                    break;
                case SqlJoinType.CrossJoin:
                    joinType = "cross join";
                    break;
                case SqlJoinType.CommaJoin:
                    joinType = ",";
                    break;
            }
            Append(joinType);

        }
        if (sqlJoinTableExpression.Right != null)
        {
            sqlJoinTableExpression.Right.Accept(this);
        }

        if (sqlJoinTableExpression.Conditions != null)
        {
            Append("on");
            sqlJoinTableExpression.Conditions.Accept(this);
        }

    }
    public override void VisitSqlLimitExpression(SqlLimitExpression sqlLimitExpression)
    {
        switch (dbType)
        {
            case DbType.Oracle:
                Append("fetch first");
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount.Accept(this);
                }
                Append("rows only");
                break;
            case DbType.MySql:
                Append("limit");
                if (sqlLimitExpression.Offset != null)
                {
                    sqlLimitExpression.Offset.Accept(this);
                    AppendWithoutSpaces(",");
                }
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount.Accept(this);
                }

                break;
            case DbType.SqlServer:
                Append("OFFSET");
                if (sqlLimitExpression.Offset != null)
                {
                    sqlLimitExpression.Offset.Accept(this);
                }
                Append("ROWS FETCH NEXT");
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount.Accept(this);
                }
                Append("ROWS ONLY");
                break;
            case DbType.Pgsql:

                if (sqlLimitExpression.RowCount != null)
                {
                    Append("limit");
                    sqlLimitExpression.RowCount.Accept(this);
                }

                if (sqlLimitExpression.Offset != null)
                {
                    Append("offset");
                    sqlLimitExpression.Offset.Accept(this);
                }

                break;
        }



    }
    public override void VisitSqlNotExpression(SqlNotExpression sqlNotExpression)
    {
        Append("not");
        if (sqlNotExpression.Body != null)
        {
            WithBrackets(() =>
            {
                sqlNotExpression.Body.Accept(this);
            });
        }
    }
    public override void VisitSqlNullExpression(SqlNullExpression sqlNullExpression)
    {
        Append("null");
    }
    public override void VisitSqlNumberExpression(SqlNumberExpression sqlNumberExpression)
    {
        if (!string.IsNullOrWhiteSpace(sqlNumberExpression.LeftQualifiers))
        {
            Append(sqlNumberExpression.LeftQualifiers + sqlNumberExpression.Value + sqlNumberExpression.RightQualifiers);
        }
        else
        {
            Append(sqlNumberExpression.Value.ToString());
        }
    }
    public override void VisitSqlOrderByExpression(SqlOrderByExpression sqlOrderByExpression)
    {
        if (sqlOrderByExpression.IsSiblings)
        {
            Append("order siblings by");
        }
        else
        {
            Append("order by");
        }

        if (sqlOrderByExpression.Items != null)
        {
            for (var i = 0; i < sqlOrderByExpression.Items.Count; i++)
            {
                var item = sqlOrderByExpression.Items[i];
                item.Accept(this);
                if (i < sqlOrderByExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }

        }
    }

    public override void VisitSqlConnectByExpression(SqlConnectByExpression sqlConnectByExpression)
    {
        if (sqlConnectByExpression.StartWith != null)
        {
            Append("start with");
            sqlConnectByExpression.StartWith.Accept(this);
        }

        Append("connect by");
        if (sqlConnectByExpression.IsNocycle)
        {
            Append("nocycle");
        }
        if (sqlConnectByExpression.IsPrior)
        {
            Append("prior");
        }

        sqlConnectByExpression.Body.Accept(this);

        if (sqlConnectByExpression.OrderBy != null)
        {
            sqlConnectByExpression.OrderBy.Accept(this);
        }
    }

    public override void VisitSqlOrderByItemExpression(SqlOrderByItemExpression sqlOrderByItemExpression)
    {
        if (sqlOrderByItemExpression.Body != null)
        {
            sqlOrderByItemExpression.Body?.Accept(this);
        }
        if (sqlOrderByItemExpression.OrderByType.HasValue)
        {
            Append(sqlOrderByItemExpression.OrderByType == SqlOrderByType.Asc ? "asc" : "desc");
        }
        if (sqlOrderByItemExpression.NullsType.HasValue)
        {
            Append(sqlOrderByItemExpression.NullsType == SqlOrderByNullsType.First ? "nulls first" : "nulls last");
        }
    }
    public override void VisitSqlOverExpression(SqlOverExpression sqlOverExpression)
    {
        Append("over");
        WithBrackets((() =>
        {
            if (sqlOverExpression.PartitionBy != null)
            {
                sqlOverExpression.PartitionBy.Accept(this);
            }
            if (sqlOverExpression.OrderBy != null)
            {
                sqlOverExpression.OrderBy.Accept(this);
            }
        }));

    }
    public override void VisitSqlPartitionByExpression(SqlPartitionByExpression sqlPartitionByExpression)
    {
        Append("partition by");

        if (sqlPartitionByExpression.Items != null)
        {
            for (var i = 0; i < sqlPartitionByExpression.Items.Count; i++)
            {
                var item = sqlPartitionByExpression.Items[i];
                item.Accept(this);
                if (i < sqlPartitionByExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }
        }
    }
    public override void VisitSqlPivotTableExpression(SqlPivotTableExpression sqlPivotTableExpression)
    {
        if (sqlPivotTableExpression.SubQuery != null)
        {
            sqlPivotTableExpression.SubQuery.Accept(this);
        }

        Append("pivot");
        WithBrackets(() =>
        {
            if (sqlPivotTableExpression.FunctionCall != null)
            {
                sqlPivotTableExpression.FunctionCall.Accept(this);
            }
            if (sqlPivotTableExpression.For != null)
            {
                Append("for");
                sqlPivotTableExpression.For.Accept(this);
            }
            if (sqlPivotTableExpression.In != null)
            {
                Append("in");
                WithBrackets(() =>
                {
                    for (var i = 0; i < sqlPivotTableExpression.In.Count; i++)
                    {
                        var item = sqlPivotTableExpression.In[i];
                        item.Accept(this);
                        if (i < sqlPivotTableExpression.In.Count - 1)
                        {
                            AppendWithoutSpaces(",");
                        }
                    }
                });
            }
        });


        if (sqlPivotTableExpression.Alias != null)
        {
            if (dbType != DbType.Oracle)
            {
                Append("as");
            }

            sqlPivotTableExpression.Alias.Accept(this);
        }
    }
    public override void VisitSqlPropertyExpression(SqlPropertyExpression sqlPropertyExpression)
    {

        if (sqlPropertyExpression.Table != null)
        {
            sqlPropertyExpression.Table?.Accept(this);
            sb.Append(".");
            this.addSpace = false;
        }

        if (sqlPropertyExpression.Name != null)
        {
            sqlPropertyExpression.Name?.Accept(this);
        }

    }
    public override void VisitSqlReferenceTableExpression(SqlReferenceTableExpression sqlReferenceTableExpression)
    {
        if (sqlReferenceTableExpression.FunctionCall != null)
        {
            sqlReferenceTableExpression.FunctionCall?.Accept(this);
        }
    }
    public override void VisitSqlSelectExpression(SqlSelectExpression sqlSelectExpression)
    {
        if (isFirstVisit)
        {
            isFirstVisit = false;
            if (sqlSelectExpression.Query != null)
            {
                sqlSelectExpression.Query?.Accept(this);
            }
        }
        else
        {
            var checkChildrenIsUnionQuery= sqlSelectExpression.Query is SqlUnionQueryExpression sqlUnionQueryExpression;
            if (IsSqlite|| checkChildrenIsUnionQuery)
            {
                if (sqlSelectExpression.Query != null)
                {
                    sqlSelectExpression.Query?.Accept(this);
                }
            }
            else
            {
                WithBrackets((() =>
                {
                    if (sqlSelectExpression.Query != null)
                    {
                        sqlSelectExpression.Query?.Accept(this);
                    }
                }));
            }
        }

        if (sqlSelectExpression.Alias != null)
        {
            if (dbType != DbType.Oracle)
            {
                Append("as");
            }
            sqlSelectExpression.Alias.Accept(this);
        }

        if (sqlSelectExpression.OrderBy != null)
        {
            sqlSelectExpression.OrderBy.Accept(this);
        }

        if (sqlSelectExpression.Limit != null)
        {
            sqlSelectExpression.Limit.Accept(this);
        }
    }
    public override void VisitSqlSelectItemExpression(SqlSelectItemExpression sqlSelectItemExpression)
    {
        sqlSelectItemExpression.Body?.Accept(this);
        if (sqlSelectItemExpression.Alias != null)
        {
            Append("as");
            sqlSelectItemExpression.Alias?.Accept(this);
        }
    }

    private void WithBrackets(Action action)
    {
        if (sb.Length == 0)
        {
            action();
            return;
        }
        if (addParen)
        {
            sb.Append("(");
        }

        this.addSpace = false;
        action();

        if (addParen)
        {
            sb.Append(")");
        }

    }

    private void DisableBrackets(Action action)
    {
        this.addParen = false;
        action();
        this.addParen = true;
    }

    public override void VisitSqlSelectQueryExpression(SqlSelectQueryExpression sqlSelectQueryExpression)
    {
        if (sqlSelectQueryExpression.WithSubQuerys != null)
        {
            Append(" with");
            for (var i = 0; i < sqlSelectQueryExpression.WithSubQuerys.Count; i++)
            {
                var item = sqlSelectQueryExpression.WithSubQuerys[i];
                item.Accept(this);
                if (i < sqlSelectQueryExpression.WithSubQuerys.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }
        }

        Append("select");

        if (sqlSelectQueryExpression.ResultSetReturnOption.HasValue)
        {
            var resultSetReturnOption = "";
            switch (sqlSelectQueryExpression.ResultSetReturnOption.Value)
            {
                case SqlResultSetReturnOption.Distinct:
                    resultSetReturnOption = "distinct";
                    break;
                case SqlResultSetReturnOption.All:
                    resultSetReturnOption = "all";
                    break;
                case SqlResultSetReturnOption.Unique:
                    resultSetReturnOption = "unique";
                    break;
            }
            Append(resultSetReturnOption);
        }

        if (sqlSelectQueryExpression.Top != null)
        {
            sqlSelectQueryExpression.Top.Accept(this);
        }

        if (sqlSelectQueryExpression.Columns != null)
        {
            for (var i = 0; i < sqlSelectQueryExpression.Columns.Count; i++)
            {
                var item = sqlSelectQueryExpression.Columns[i];
                item.Accept(this);
                if (i < sqlSelectQueryExpression.Columns.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }
        }

        if (sqlSelectQueryExpression.Into != null)
        {
            Append("into");
            sqlSelectQueryExpression.Into.Accept(this);
        }
        if (sqlSelectQueryExpression.From != null)
        {
            Append("from");
            sqlSelectQueryExpression.From.Accept(this);
        }
        if (sqlSelectQueryExpression.Where != null)
        {
            Append("where");
            sqlSelectQueryExpression.Where.Accept(this);
        }

        if (sqlSelectQueryExpression.GroupBy != null)
        {
            sqlSelectQueryExpression.GroupBy.Accept(this);
        }

        if (sqlSelectQueryExpression.OrderBy != null)
        {
            sqlSelectQueryExpression.OrderBy.Accept(this);
        }

        if (dbType == DbType.Oracle && sqlSelectQueryExpression.ConnectBy != null)
        {
            sqlSelectQueryExpression.ConnectBy.Accept(this);
        }

        if (sqlSelectQueryExpression.Limit != null)
        {
            sqlSelectQueryExpression.Limit.Accept(this);
        }
        if (sqlSelectQueryExpression.Hints != null && sqlSelectQueryExpression.Hints.Any())
        {
            foreach (var tableExpressionHint in sqlSelectQueryExpression.Hints)
            {
                tableExpressionHint.Body?.Accept(this);
            }
        }
    }

    private void Append(string str)
    {
        if (addSpace)
        {
            sb.Append($" {str}");
        }
        else
        {
            sb.Append($"{str}");
            addSpace = true;
        }

    }

    private void AppendSpace()
    {
        sb.Append($" ");
    }

    private void AppendWithoutSpaces(string str)
    {
        sb.Append($"{str}");
    }

    public override void VisitSqlBoolExpression(SqlBoolExpression sqlBoolExpression)
    {
        if (dbType == DbType.MySql || dbType == DbType.Pgsql || dbType == DbType.Sqlite)
        {
            sb.Append($" {sqlBoolExpression.Value.ToString().ToLowerInvariant()} ");
        }

    }

    public override void VisitSqlStringExpression(SqlStringExpression sqlStringExpression)
    {
        Append($"{(sqlStringExpression.IsUniCode ? "N" : "")}'{sqlStringExpression.Value.Replace("'", "''")}'");
    }
    public override void VisitSqlTableExpression(SqlTableExpression sqlTableExpression)
    {
        if (sqlTableExpression.Database != null)
        {
            sqlTableExpression.Database?.Accept(this);
            AppendWithoutSpaces(".");
            this.addSpace = false;
        }
        if (sqlTableExpression.Schema != null)
        {
            sqlTableExpression.Schema?.Accept(this);
            AppendWithoutSpaces(".");
            this.addSpace = false;
        }

        sqlTableExpression.Name?.Accept(this);
        if (dbType == DbType.Oracle && sqlTableExpression.DbLink != null)
        {
            AppendWithoutSpaces("@");
            this.addSpace = false;
            sqlTableExpression.DbLink?.Accept(this);
            this.addSpace = true;
        }
        if (sqlTableExpression.Alias != null)
        {
            if (dbType != DbType.Oracle)
            {
                Append("as");
            }
            sqlTableExpression.Alias?.Accept(this);
        }

        if (sqlTableExpression.Hints != null && sqlTableExpression.Hints.Any())
        {
            foreach (var tableExpressionHint in sqlTableExpression.Hints)
            {
                tableExpressionHint.Body?.Accept(this);
            }
        }

    }
    public override void VisitSqlUnionQueryExpression(SqlUnionQueryExpression sqlUnionQueryExpression)
    {
        WithBrackets(() =>
        {
            VisitSqlUnionQueryExpressionInternal(sqlUnionQueryExpression);
        });
        return;
        if (sb.Length == 0|| sb[sb.Length-1]=='(')
        {
            VisitSqlUnionQueryExpressionInternal(sqlUnionQueryExpression);
        }
        else
        {
            AppendSpace();
            WithBrackets(() =>
            {
                VisitSqlUnionQueryExpressionInternal(sqlUnionQueryExpression);
            });
        }
        
    }

    private void VisitSqlUnionQueryExpressionInternal(SqlUnionQueryExpression sqlUnionQueryExpression)
    {
        if (sqlUnionQueryExpression.Left != null)
        {
            sqlUnionQueryExpression.Left.Accept(this);
        }

        if (sqlUnionQueryExpression.UnionType != null)
        {
            var unionType = "";
            switch (sqlUnionQueryExpression.UnionType)
            {
                case SqlUnionType.Except:
                    unionType = "except";
                    break;
                case SqlUnionType.ExceptAll:
                    unionType = "except all";
                    break;
                case SqlUnionType.Intersect:
                    unionType = "intersect";
                    break;
                case SqlUnionType.IntersectAll:
                    unionType = "intersect all";
                    break;
                case SqlUnionType.Minus:
                    unionType = "minus";
                    break;
                case SqlUnionType.Union:
                    unionType = "union";
                    break;
                case SqlUnionType.UnionAll:
                    unionType = "union all";
                    break;
            }
            Append(unionType);
            AppendSpace();
        }
        var right = sqlUnionQueryExpression.Right;
        if (right != null)
        {
            right.Accept(this);
        }
    }

    public override void VisitSqlUpdateExpression(SqlUpdateExpression sqlUpdateExpression)
    {
        Append("update");
        if (sqlUpdateExpression.Table != null)
        {
            sqlUpdateExpression.Table.Accept(this);
        }

        if (sqlUpdateExpression.Items != null)
        {
            Append("set ");
            for (var i = 0; i < sqlUpdateExpression.Items.Count; i++)
            {
                var item = sqlUpdateExpression.Items[i];
                DisableBrackets(() =>
                {
                    item.Accept(this);
                });

                if (i < sqlUpdateExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }
        }
        if (sqlUpdateExpression.Where != null)
        {
            sb.Append(" where");
            sqlUpdateExpression.Where.Accept(this);
        }


    }
    public override void VisitSqlVariableExpression(SqlVariableExpression sqlVariableExpression)
    {
        if (!string.IsNullOrWhiteSpace(sqlVariableExpression.Prefix))
        {
            sb.Append(sqlVariableExpression.Prefix);
        }
        if (!string.IsNullOrWhiteSpace(sqlVariableExpression.Name))
        {
            sb.Append(sqlVariableExpression.Name);
        }
    }
    public override void VisitSqlWithinGroupExpression(SqlWithinGroupExpression sqlWithinGroupExpression)
    {
        Append("within group");
        WithBrackets((() =>
        {
            if (sqlWithinGroupExpression.OrderBy != null)
            {
                sqlWithinGroupExpression.OrderBy.Accept(this);
            }
        }));
    }
    public override void VisitSqlWithSubQueryExpression(SqlWithSubQueryExpression sqlWithSubQueryExpression)
    {
        if (sqlWithSubQueryExpression.Alias != null)
        {
            sqlWithSubQueryExpression.Alias.Accept(this);
        }

        if (sqlWithSubQueryExpression.Columns != null)
        {
            WithBrackets((() =>
            {
                for (var i = 0; i < sqlWithSubQueryExpression.Columns.Count; i++)
                {
                    var item = sqlWithSubQueryExpression.Columns[i];
                    item.Accept(this);
                    if (i < sqlWithSubQueryExpression.Columns.Count - 1)
                    {
                        AppendWithoutSpaces(",");
                    }
                }
            }));

        }

        Append("as");
        if (sqlWithSubQueryExpression.FromSelect != null)
        {
            WithBrackets(() =>
            {
                sqlWithSubQueryExpression.FromSelect.Accept(this);
            });
        }
    }

    public override void VisitSqlTopExpression(SqlTopExpression sqlTopExpression)
    {
        Append("top");
        if (sqlTopExpression.Body != null)
        {
            sqlTopExpression.Body.Accept(this);
        }
    }

    public override void VisitSqlHintExpression(SqlHintExpression sqlHintExpression)
    {
        if (sqlHintExpression.Body != null)
        {
            sqlHintExpression.Body.Accept(this);
        }
    }

    public override void VisitSqlAtTimeZoneExpression(SqlAtTimeZoneExpression sqlAtTimeZoneExpression)
    {
        if (sqlAtTimeZoneExpression.Body != null)
        {
            sqlAtTimeZoneExpression.Body.Accept(this);
        }
        Append("at time zone");
        sqlAtTimeZoneExpression.TimeZone.Accept(this);
    }

    public override void VisitSqlIntervalExpression(SqlIntervalExpression sqlIntervalExpression)
    {
        Append("interval");
        if (sqlIntervalExpression.Body != null)
        {
            sqlIntervalExpression.Body.Accept(this);
        }
        if (sqlIntervalExpression.Unit != null)
        {
            sqlIntervalExpression.Unit.Accept(this);
        }
    }

    public override void VisitSqlTimeUnitExpression(SqlTimeUnitExpression sqlTimeUnitExpression)
    {
        if (!string.IsNullOrWhiteSpace(sqlTimeUnitExpression.Unit))
        {
            Append(sqlTimeUnitExpression.Unit);
        }
    }
}