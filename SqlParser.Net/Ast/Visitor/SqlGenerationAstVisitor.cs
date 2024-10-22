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

    private bool isFirst = true;

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
        WithBrackets(() =>
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

        });

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
        Append("delete from");


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
                    if (i < sqlFunctionCallExpression.Arguments.Count - 1)
                    {
                        AppendWithSpace(",");
                    }
                }
            });
        }
        else
        {
            AppendWithSpace("()");
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
                    AppendWithSpace(",");
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
        Append(sqlIdentifierExpression.Value);
    }
    public override void VisitSqlInExpression(SqlInExpression sqlInExpression)
    {
        if (sqlInExpression.Field != null)
        {
            sqlInExpression.Field.Accept(this);
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
                        AppendWithSpace(",");
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
                        AppendWithSpace(",");
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
                            AppendWithSpace(",");
                        }
                    }
                }));


                if (i < sqlInsertExpression.ValuesList.Count - 1)
                {
                    AppendWithSpace(",");
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
                Append("FETCH FIRST");
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount.Accept(this);
                }
                Append("rows ONLY");
                break;
            case DbType.MySql:
                Append("limit");
                if (sqlLimitExpression.Offset != null)
                {
                    sqlLimitExpression.Offset.Accept(this);
                    AppendWithSpace(",");
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
        Append(sqlNumberExpression.Value.ToString());
    }
    public override void VisitSqlOrderByExpression(SqlOrderByExpression sqlOrderByExpression)
    {
        Append("order by");
        if (sqlOrderByExpression.Items != null)
        {
            for (var i = 0; i < sqlOrderByExpression.Items.Count; i++)
            {
                var item = sqlOrderByExpression.Items[i];
                item.Accept(this);
                if (i < sqlOrderByExpression.Items.Count - 1)
                {
                    AppendWithSpace(",");
                }
            }

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
                    AppendWithSpace(",");
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
                            AppendWithSpace(",");
                        }
                    }
                });
            }
        });


        if (sqlPivotTableExpression.Alias != null)
        {
            //if (dbType == DbType.SqlServer)
            //{
            //    Append("as");
            //}

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
        if (isFirst)
        {
            isFirst = false;
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

        if (sqlSelectExpression.Alias != null)
        {
            sqlSelectExpression.Alias.Accept(this);
        }
    }
    public override void VisitSqlSelectItemExpression(SqlSelectItemExpression sqlSelectItemExpression)
    {
        sqlSelectItemExpression.Body?.Accept(this);
        if (sqlSelectItemExpression.Alias != null)
        {
            //Append("as");
            sqlSelectItemExpression.Alias?.Accept(this);
        }
    }

    private void WithBrackets(Action action)
    {
        sb.Append("(");
        this.addSpace = false;
        action();
        sb.Append(")");
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
                    AppendWithSpace(",");
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

        if (sqlSelectQueryExpression.Columns != null)
        {
            for (var i = 0; i < sqlSelectQueryExpression.Columns.Count; i++)
            {
                var item = sqlSelectQueryExpression.Columns[i];
                item.Accept(this);
                if (i < sqlSelectQueryExpression.Columns.Count - 1)
                {
                    AppendWithSpace(",");
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
        if (sqlSelectQueryExpression.OrderBy != null)
        {
            sqlSelectQueryExpression.OrderBy.Accept(this);
        }
        if (sqlSelectQueryExpression.GroupBy != null)
        {
            sqlSelectQueryExpression.GroupBy.Accept(this);
        }
        if (sqlSelectQueryExpression.Limit != null)
        {
            sqlSelectQueryExpression.Limit.Accept(this);
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
    private void AppendWithSpace(string str)
    {
        sb.Append($"{str}");
    }

    public override void VisitSqlStringExpression(SqlStringExpression sqlStringExpression)
    {
        Append($"'{sqlStringExpression.Value.Replace("'", "''")}'");
    }
    public override void VisitSqlTableExpression(SqlTableExpression sqlTableExpression)
    {
        sqlTableExpression.Name?.Accept(this);
        if (sqlTableExpression.Alias != null)
        {
            //Append("as");
            sqlTableExpression.Alias?.Accept(this);
        }

    }
    public override void VisitSqlUnionQueryExpression(SqlUnionQueryExpression sqlUnionQueryExpression)
    {

        Append("(");


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
        }

        if (sqlUnionQueryExpression.Right != null)
        {
            sqlUnionQueryExpression.Right.Accept(this);
        }
        Append(")");
    }
    public override void VisitSqlUpdateExpression(SqlUpdateExpression sqlUpdateExpression)
    {
        if (sqlUpdateExpression.Table != null)
        {
            sqlUpdateExpression.Table.Accept(this);
        }

        if (sqlUpdateExpression.Items != null)
        {
            Append("set");
            for (var i = 0; i < sqlUpdateExpression.Items.Count; i++)
            {
                var item = sqlUpdateExpression.Items[i];
                item.Accept(this);
                if (i < sqlUpdateExpression.Items.Count - 1)
                {
                    AppendWithSpace(",");
                }
            }
        }
        if (sqlUpdateExpression.Where != null)
        {
            sb.Append("where");
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
                        AppendWithSpace(",");
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
}