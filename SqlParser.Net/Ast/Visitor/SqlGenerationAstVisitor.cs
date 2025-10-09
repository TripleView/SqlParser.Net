using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using SqlParser.Net.Ast.AnalyzeContext;
using SqlParser.Net.Ast.Expression;

namespace SqlParser.Net.Ast.Visitor;

public class SqlGenerationAstVisitor : BaseAstVisitor
{
    private StringBuilder sb = new StringBuilder();
    private string fourSpace = "    ";
    private int numberOfLevels = 0;

    private DbType dbType = DbType.MySql;
    private bool addParen = true;
    private bool IsOracle => this.dbType == DbType.Oracle;
    private bool IsSqlServer => this.dbType == DbType.SqlServer;
    private bool IsPgsql => this.dbType == DbType.Pgsql;

    private bool IsMySql => this.dbType == DbType.MySql;
    private bool IsSqlite => this.dbType == DbType.Sqlite;

    private ParseType sqlParseType;

    private bool isInUpdateSetContext = false;

    private CommonContext commonContext = new CommonContext();

    //private HashSet<char> delimiters = new HashSet<char>() { '.',',','(',')'};
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
        AppendWithSpace("*");
    }
    public override void VisitSqlAllExpression(SqlAllExpression sqlAllExpression)
    {
        Append("all");
        if (sqlAllExpression.Body != null)
        {
            EnableParen(() => { sqlAllExpression.Body.Accept(this); });
        }
    }
    public override void VisitSqlAnyExpression(SqlAnyExpression sqlAnyExpression)
    {
        Append("any");
        if (sqlAnyExpression.Body != null)
        {
            EnableParen(() => { sqlAnyExpression.Body.Accept(this); });
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
            AppendWithSpace("not");
        }
        AppendWithSpace("between");
        if (sqlBetweenAndExpression.Begin != null)
        {
            sqlBetweenAndExpression.Begin.Accept(this);
        }

        AppendWithSpace("and");

        if (sqlBetweenAndExpression.End != null)
        {
            sqlBetweenAndExpression.End.Accept(this);
        }

    }
    public override void VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
    {
        void internalAction()
        {
            if (sqlBinaryExpression.Left != null)
            {
                sqlBinaryExpression.Left.Accept(this);
            }

            if (sqlBinaryExpression.Operator != null)
            {
                AppendWithSpace(sqlBinaryExpression.Operator.Value.ToString().ToLowerInvariant());
            }
            if (sqlBinaryExpression.Right != null)
            {
                sqlBinaryExpression.Right.Accept(this);
            }

            if (sqlBinaryExpression.Collate != null)
            {
                sqlBinaryExpression.Collate.Accept(this);
            }
        }

        if (sqlBinaryExpression.Parent is SqlConnectByExpression || (sqlBinaryExpression.Parent is SqlUpdateExpression && isInUpdateSetContext))
        {
            internalAction();
        }
        else
        {
            EnableParen(internalAction);
        }

    }
    public override void VisitSqlCaseExpression(SqlCaseExpression sqlCaseExpression)
    {
        Append("case");

        if (sqlCaseExpression.Value != null)
        {
            sqlCaseExpression.Value.Accept(this);
        }

        if (sqlCaseExpression.Items.HasValue())
        {
            foreach (var item in sqlCaseExpression.Items)
            {
                item.Accept(this);
            }
        }

        if (sqlCaseExpression.Else != null)
        {
            AppendWithSpace("else");
            sqlCaseExpression.Else.Accept(this);
        }

        AppendWithLeftSpace("end");
    }
    public override void VisitSqlCaseItemExpression(SqlCaseItemExpression sqlCaseItemExpression)
    {

        if (sqlCaseItemExpression.Condition != null)
        {
            AppendWithSpace("when");
            sqlCaseItemExpression.Condition.Accept(this);
        }

        if (sqlCaseItemExpression.Value != null)
        {
            AppendWithSpace("then");
            sqlCaseItemExpression.Value.Accept(this);
        }

    }
    public override void VisitSqlDeleteExpression(SqlDeleteExpression sqlDeleteExpression)
    {
        AppendWithRightSpace("delete");
        if (sqlDeleteExpression.Body != null)
        {
            sqlDeleteExpression.Body.Accept(this);
        }

        AppendWithSpace("from");

        if (sqlDeleteExpression.Table != null)
        {
            sqlDeleteExpression.Table.Accept(this);
        }
        if (sqlDeleteExpression.Where != null)
        {
            AppendWithSpace("where");
            sqlDeleteExpression.Where.Accept(this);
        }

    }
    public override void VisitSqlExistsExpression(SqlExistsExpression sqlExistsExpression)
    {
        if (sqlExistsExpression.IsNot)
        {
            AppendWithSpace("not");
        }

        Append("exists");

        if (sqlExistsExpression.Body != null)
        {
            EnableParen(() =>
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
            EnableParen(() =>
            {
                for (var i = 0; i < sqlFunctionCallExpression.Arguments.Count; i++)
                {
                    if (i == 0)
                    {
                        if (sqlFunctionCallExpression.IsDistinct)
                        {
                            AppendWithSpace("distinct");
                        }
                    }
                    var argument = sqlFunctionCallExpression.Arguments[i];
                    argument.Accept(this);

                    if (sqlFunctionCallExpression.CaseAsTargetType != null)
                    {
                        AppendWithSpace("as");
                        Append($"{sqlFunctionCallExpression.CaseAsTargetType.Value}");
                    }

                    if (sqlFunctionCallExpression.FromSource != null)
                    {
                        AppendWithSpace($"from");
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
            sqlFunctionCallExpression.Over.Accept(this);
        }

        if (sqlFunctionCallExpression.Collate != null)
        {
            sqlFunctionCallExpression.Collate?.Accept(this);
        }
    }
    public override void VisitSqlGroupByExpression(SqlGroupByExpression sqlGroupByExpression)
    {
        if (!sqlGroupByExpression.HasValue())
        {
            return;
        }
        AppendWithSpace("group by");
        if (sqlGroupByExpression.Items.HasValue())
        {
            for (var i = 0; i < sqlGroupByExpression.Items.Count; i++)
            {
                var item = sqlGroupByExpression.Items[i];
                item.Accept(this);
                if (i < sqlGroupByExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
        }

        if (sqlGroupByExpression.Having != null)
        {
            AppendWithSpace("having");
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

        if (sqlIdentifierExpression.Collate != null)
        {
            sqlIdentifierExpression.Collate?.Accept(this);
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
            AppendWithSpace("not");
        }
        AppendWithSpace("in");

        if (sqlInExpression.TargetList.HasValue())
        {
            EnableParen(() =>
            {
                for (var i = 0; i < sqlInExpression.TargetList.Count; i++)
                {
                    var item = sqlInExpression.TargetList[i];
                    item.Accept(this);
                    if (i < sqlInExpression.TargetList.Count - 1)
                    {
                        AppendWithoutSpaces(", ");
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
        AppendWithRightSpace("insert into");

        if (sqlInsertExpression.Table != null)
        {
            sqlInsertExpression.Table?.Accept(this);
        }

        if (sqlInsertExpression.Columns.HasValue())
        {
            EnableParen(() =>
            {
                for (var i = 0; i < sqlInsertExpression.Columns.Count; i++)
                {
                    var item = sqlInsertExpression.Columns[i];
                    item.Accept(this);
                    if (i < sqlInsertExpression.Columns.Count - 1)
                    {
                        AppendWithoutSpaces(", ");
                    }
                }
            });
        }
        if (sqlInsertExpression.ValuesList.HasValue())
        {
            AppendWithLeftSpace("values");
            for (var i = 0; i < sqlInsertExpression.ValuesList.Count; i++)
            {
                var items = sqlInsertExpression.ValuesList[i];
                EnableParen((() =>
                {
                    for (var j = 0; j < items.Count; j++)
                    {
                        var item = items[j];
                        item.Accept(this);
                        if (j < items.Count - 1)
                        {
                            AppendWithoutSpaces(", ");
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
            AppendSpace();
            sqlInsertExpression.FromSelect?.Accept(this);
        }

        if (sqlInsertExpression.Returning != null)
        {
            sqlInsertExpression.Returning?.Accept(this);
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
            AppendWithSpace(joinType);

        }
        if (sqlJoinTableExpression.Right != null)
        {
            sqlJoinTableExpression.Right.Accept(this);
        }

        if (sqlJoinTableExpression.Conditions != null)
        {
            AppendWithSpace("on");
            sqlJoinTableExpression.Conditions.Accept(this);
        }

    }
    public override void VisitSqlLimitExpression(SqlLimitExpression sqlLimitExpression)
    {
        switch (dbType)
        {
            case DbType.Oracle:
                AppendWithSpace("fetch first");
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount.Accept(this);
                }
                AppendWithLeftSpace("rows only");
                break;
            case DbType.MySql:
            case DbType.Sqlite:
                AppendWithSpace("limit");
                if (sqlLimitExpression.Offset != null)
                {
                    sqlLimitExpression.Offset.Accept(this);
                    AppendWithoutSpaces(", ");
                }
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount.Accept(this);
                }

                break;
            case DbType.SqlServer:
                AppendWithSpace("OFFSET");
                if (sqlLimitExpression.Offset != null)
                {
                    sqlLimitExpression.Offset.Accept(this);
                }
                AppendWithSpace("ROWS FETCH NEXT");
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount.Accept(this);
                }
                AppendWithSpace("ROWS ONLY");
                break;
            case DbType.Pgsql:

                if (sqlLimitExpression.RowCount != null)
                {
                    AppendWithSpace("limit");
                    sqlLimitExpression.RowCount.Accept(this);
                }

                if (sqlLimitExpression.Offset != null)
                {
                    AppendWithSpace("offset");
                    sqlLimitExpression.Offset.Accept(this);
                }

                break;
        }



    }
    public override void VisitSqlNotExpression(SqlNotExpression sqlNotExpression)
    {
        AppendWithSpace("not");
        if (sqlNotExpression.Body != null)
        {
            EnableParen(() =>
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
        if (!sqlOrderByExpression.HasValue())
        {
            return;
        }
        if (sqlOrderByExpression.IsSiblings)
        {
            AppendWithSpace("order siblings by");
        }
        else
        {
            AppendWithSpace("order by");
        }

        if (sqlOrderByExpression.Items.HasValue())
        {
            for (var i = 0; i < sqlOrderByExpression.Items.Count; i++)
            {
                var item = sqlOrderByExpression.Items[i];
                item.Accept(this);
                if (i < sqlOrderByExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }

        }
    }

    public override void VisitSqlConnectByExpression(SqlConnectByExpression sqlConnectByExpression)
    {
        if (sqlConnectByExpression.StartWith != null)
        {
            AppendWithSpace("start with");
            sqlConnectByExpression.StartWith.Accept(this);
        }

        AppendWithSpace("connect by");
        if (sqlConnectByExpression.IsNocycle)
        {
            AppendWithSpace("nocycle");
        }
        if (sqlConnectByExpression.IsPrior)
        {
            AppendWithSpace("prior");
        }

        sqlConnectByExpression.Body.Accept(this);

        if (sqlConnectByExpression.OrderBy.HasValue())
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
            AppendWithLeftSpace(sqlOrderByItemExpression.OrderByType == SqlOrderByType.Asc ? "asc" : "desc");
        }
        if (sqlOrderByItemExpression.NullsType.HasValue)
        {
            AppendWithLeftSpace(sqlOrderByItemExpression.NullsType == SqlOrderByNullsType.First ? "nulls first" : "nulls last");
        }
    }
    public override void VisitSqlOverExpression(SqlOverExpression sqlOverExpression)
    {
        AppendWithSpace("over");
        EnableParen((() =>
        {
            if (sqlOverExpression.PartitionBy != null)
            {
                sqlOverExpression.PartitionBy.Accept(this);
            }
            if (sqlOverExpression.OrderBy.HasValue())
            {
                sqlOverExpression.OrderBy.Accept(this);
            }
        }));

    }
    public override void VisitSqlPartitionByExpression(SqlPartitionByExpression sqlPartitionByExpression)
    {
        if (!(sqlPartitionByExpression.Items != null && sqlPartitionByExpression.Items.Count > 0))
        {
            return;
        }
        AppendWithRightSpace("partition by");

        if (sqlPartitionByExpression.Items.HasValue())
        {
            for (var i = 0; i < sqlPartitionByExpression.Items.Count; i++)
            {
                var item = sqlPartitionByExpression.Items[i];
                item.Accept(this);
                if (i < sqlPartitionByExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
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

        AppendWithSpace("pivot");
        EnableParen(() =>
        {
            if (sqlPivotTableExpression.FunctionCall != null)
            {
                sqlPivotTableExpression.FunctionCall.Accept(this);
            }
            if (sqlPivotTableExpression.For != null)
            {
                AppendWithSpace("for");
                sqlPivotTableExpression.For.Accept(this);
            }
            if (sqlPivotTableExpression.In != null)
            {
                AppendWithSpace("in");
                EnableParen(() =>
                {
                    for (var i = 0; i < sqlPivotTableExpression.In.Count; i++)
                    {
                        var item = sqlPivotTableExpression.In[i];
                        item.Accept(this);
                        if (i < sqlPivotTableExpression.In.Count - 1)
                        {
                            AppendWithoutSpaces(", ");
                        }
                    }
                });
            }
        });


        if (sqlPivotTableExpression.Alias != null)
        {
            if (dbType != DbType.Oracle)
            {
                AppendWithSpace("as");
            }
            else
            {
                AppendSpace();
            }

            sqlPivotTableExpression.Alias.Accept(this);
        }
    }
    public override void VisitSqlPropertyExpression(SqlPropertyExpression sqlPropertyExpression)
    {

        if (sqlPropertyExpression.Table != null)
        {
            sqlPropertyExpression.Table?.Accept(this);
            Append(".");
        }

        if (sqlPropertyExpression.Name != null)
        {
            sqlPropertyExpression.Name?.Accept(this);
        }

        if (sqlPropertyExpression.Collate != null)
        {
            sqlPropertyExpression.Collate?.Accept(this);
        }

    }
    public override void VisitSqlReferenceTableExpression(SqlReferenceTableExpression sqlReferenceTableExpression)
    {
        if (sqlReferenceTableExpression.FunctionCall != null)
        {
            sqlReferenceTableExpression.FunctionCall?.Accept(this);
        }

        if (sqlReferenceTableExpression.Alias != null)
        {
            if (!IsOracle)
            {
                AppendWithSpace("as");
            }
            sqlReferenceTableExpression.Alias?.Accept(this);
        }
    }
    public override void VisitSqlSelectExpression(SqlSelectExpression sqlSelectExpression)
    {
        if (sqlSelectExpression.Alias == null && (sb.Length == 0
            || sqlSelectExpression.Parent is SqlInsertExpression
            || (IsSqlite && sqlSelectExpression.Parent is not SqlFunctionCallExpression)
            || sqlSelectExpression.Query is SqlUnionQueryExpression
            || sqlSelectExpression.Parent is SqlExistsExpression))
        {
            if (sqlSelectExpression.Query != null)
            {
                sqlSelectExpression.Query?.Accept(this);
            }
        }
        else
        {
            EnableParen(() =>
            {
                if (sqlSelectExpression.Query != null)
                {
                    sqlSelectExpression.Query?.Accept(this);
                }
            }, isInUpdateSetContext || sqlSelectExpression.Alias != null);
        }

        if (sqlSelectExpression.Alias != null)
        {
            if (dbType != DbType.Oracle)
            {
                AppendWithSpace("as");
            }
            else
            {
                AppendSpace();
            }
            sqlSelectExpression.Alias.Accept(this);
        }

        if (sqlSelectExpression.OrderBy.HasValue())
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
            AppendWithSpace("as");
            sqlSelectItemExpression.Alias?.Accept(this);
        }
    }

    private void EnableParen(Action action, bool isForce = false)
    {
        Append("(");
        action();
        Append(")");
    }

    private void EnableUpdateSetContext(Action action)
    {
        this.isInUpdateSetContext = true;
        action();
        this.isInUpdateSetContext = false;
    }

    public override void VisitSqlSelectQueryExpression(SqlSelectQueryExpression sqlSelectQueryExpression)
    {
        if (sqlSelectQueryExpression.WithSubQuerys.HasValue())
        {
            AppendWithRightSpace(" with");
            for (var i = 0; i < sqlSelectQueryExpression.WithSubQuerys.Count; i++)
            {
                var item = sqlSelectQueryExpression.WithSubQuerys[i];
                item.Accept(this);
                if (i < sqlSelectQueryExpression.WithSubQuerys.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
            AppendSpace();
        }

        AppendWithRightSpace("select");

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
            AppendWithSpace(resultSetReturnOption);
        }

        if (sqlSelectQueryExpression.Top != null)
        {
            sqlSelectQueryExpression.Top.Accept(this);
        }

        if (sqlSelectQueryExpression.Columns.HasValue())
        {
            for (var i = 0; i < sqlSelectQueryExpression.Columns.Count; i++)
            {
                AppendSpace();
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
            AppendWithSpace("into");
            sqlSelectQueryExpression.Into.Accept(this);
        }
        if (sqlSelectQueryExpression.From != null)
        {
            AppendWithSpace("from");
            sqlSelectQueryExpression.From.Accept(this);
        }
        if (sqlSelectQueryExpression.Where != null)
        {
            AppendWithSpace("where");
            sqlSelectQueryExpression.Where.Accept(this);
        }

        if (sqlSelectQueryExpression.GroupBy.HasValue())
        {
            sqlSelectQueryExpression.GroupBy.Accept(this);
        }

        if (sqlSelectQueryExpression.OrderBy.HasValue())
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
        if (sqlSelectQueryExpression.Hints.HasValue())
        {
            foreach (var tableExpressionHint in sqlSelectQueryExpression.Hints)
            {
                AppendSpace();
                tableExpressionHint.Body?.Accept(this);
            }
        }
    }

    private void AppendWithSpace(Action action)
    {
        AppendSpace();
        action();
        AppendSpace();
    }

    private void AppendWithSpace(string str)
    {
        Append(str, true, true);
    }

    private void AppendWithRightSpace(string str)
    {
        Append(str, false, true);
    }
    private void AppendWithLeftSpace(string str)
    {
        Append(str, true, false);
    }
    private void Append(string str, bool hasLeftSpace = false, bool hasRightSpace = false)
    {
        var isLastCharSpace = sb.Length > 0 && sb[sb.Length - 1] == ' ';
        var isLastCharLeftParen = sb.Length > 0 && sb[sb.Length - 1] == '(';
        if (hasLeftSpace && !(isLastCharSpace || isLastCharLeftParen))
        {
            sb.Append(" ");
        }
        sb.Append(str);
        if (hasRightSpace)
        {
            sb.Append(" ");
        }
    }

    private void AppendSpace()
    {
        Append($"", true);
    }

    private void AppendWithoutSpaces(string str)
    {
        Append($"{str}");
    }

    public override void VisitSqlBoolExpression(SqlBoolExpression sqlBoolExpression)
    {
        if (dbType == DbType.MySql || dbType == DbType.Pgsql || dbType == DbType.Sqlite)
        {
            Append($"{sqlBoolExpression.Value.ToString().ToLowerInvariant()}");
        }

    }

    public override void VisitSqlStringExpression(SqlStringExpression sqlStringExpression)
    {
        Append($"{(sqlStringExpression.IsUniCode ? "N" : "")}'{sqlStringExpression.Value.Replace("'", "''")}'");
        if (sqlStringExpression.Collate != null)
        {
            sqlStringExpression.Collate?.Accept(this);
        }
    }
    public override void VisitSqlTableExpression(SqlTableExpression sqlTableExpression)
    {
        if (sqlTableExpression.Database != null)
        {
            sqlTableExpression.Database?.Accept(this);
            Append(".");
        }

        if (sqlTableExpression.Schema != null)
        {
            sqlTableExpression.Schema?.Accept(this);
            Append(".");
        }

        sqlTableExpression.Name?.Accept(this);
        if ((IsOracle || IsPgsql) && sqlTableExpression.DbLink != null)
        {
            AppendWithoutSpaces("@");
            sqlTableExpression.DbLink?.Accept(this);
        }
        if (sqlTableExpression.Alias != null)
        {
            if (!IsOracle)
            {
                AppendWithSpace("as");
            }
            else
            {
                AppendSpace();
            }

            sqlTableExpression.Alias?.Accept(this);
        }

        if (sqlTableExpression.Hints.HasValue())
        {
            foreach (var tableExpressionHint in sqlTableExpression.Hints)
            {
                AppendSpace();
                tableExpressionHint.Body?.Accept(this);
            }
        }

    }
    public override void VisitSqlUnionQueryExpression(SqlUnionQueryExpression sqlUnionQueryExpression)
    {
        if (sb.Length > 0 && sb[sb.Length - 1] == '(')
        {
            VisitSqlUnionQueryExpressionInternal(sqlUnionQueryExpression);
        }
        else
        {
            if (IsSqlite)
            {
                VisitSqlUnionQueryExpressionInternal(sqlUnionQueryExpression);
            }
            else
            {
                EnableParen(() =>
                {
                    VisitSqlUnionQueryExpressionInternal(sqlUnionQueryExpression);
                });
            }
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
            AppendWithSpace(unionType);
        }
        var right = sqlUnionQueryExpression.Right;
        if (right != null)
        {
            right.Accept(this);
        }
    }

    public override void VisitSqlUpdateExpression(SqlUpdateExpression sqlUpdateExpression)
    {
        sqlParseType = ParseType.Update;
        AppendWithRightSpace("update");
        if (sqlUpdateExpression.Table != null)
        {
            sqlUpdateExpression.Table.Accept(this);
        }

        if (sqlUpdateExpression.Items.HasValue())
        {
            AppendWithSpace("set");
            for (var i = 0; i < sqlUpdateExpression.Items.Count; i++)
            {
                var item = sqlUpdateExpression.Items[i];
                //item.Accept(this);
                EnableUpdateSetContext(() =>
                {
                    item.Accept(this);
                });

                if (i < sqlUpdateExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
        }

        if (IsSqlServer || IsPgsql || IsSqlite)
        {
            if (sqlUpdateExpression.From != null)
            {
                AppendWithSpace("from");
                sqlUpdateExpression.From.Accept(this);
            }
        }

        if (sqlUpdateExpression.Where != null)
        {
            AppendWithSpace("where");
            sqlUpdateExpression.Where.Accept(this);
        }


    }
    public override void VisitSqlVariableExpression(SqlVariableExpression sqlVariableExpression)
    {
        if (sb.Length > 0)
        {
            var lastChar = sb[sb.Length - 1];
            if (lastChar != ' ' && lastChar != '(' && lastChar != ',')
            {
                Append(" ");
            }
        }
        if (!string.IsNullOrWhiteSpace(sqlVariableExpression.Prefix))
        {
            Append(sqlVariableExpression.Prefix);
        }
        if (!string.IsNullOrWhiteSpace(sqlVariableExpression.Name))
        {
            Append(sqlVariableExpression.Name);
        }
        if (sqlVariableExpression.Collate != null)
        {
            sqlVariableExpression.Collate?.Accept(this);
        }
    }
    public override void VisitSqlWithinGroupExpression(SqlWithinGroupExpression sqlWithinGroupExpression)
    {
        AppendWithLeftSpace("within group");
        EnableParen((() =>
        {
            if (sqlWithinGroupExpression.OrderBy.HasValue())
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

        if (sqlWithSubQueryExpression.Columns.HasValue())
        {
            EnableParen((() =>
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

        AppendWithSpace("as");
        if (sqlWithSubQueryExpression.FromSelect != null)
        {
            EnableParen(() =>
            {
                sqlWithSubQueryExpression.FromSelect.Accept(this);
            });
        }
    }

    public override void VisitSqlTopExpression(SqlTopExpression sqlTopExpression)
    {
        AppendWithSpace("top");
        if (sqlTopExpression.Body != null)
        {
            sqlTopExpression.Body.Accept(this);
        }
    }

    public override void VisitSqlHintExpression(SqlHintExpression sqlHintExpression)
    {
        if (sqlHintExpression.Body != null)
        {
            AppendSpace();
            sqlHintExpression.Body.Accept(this);
        }
    }

    public override void VisitSqlAtTimeZoneExpression(SqlAtTimeZoneExpression sqlAtTimeZoneExpression)
    {
        if (sqlAtTimeZoneExpression.Body != null)
        {
            sqlAtTimeZoneExpression.Body.Accept(this);
        }
        AppendWithSpace("at time zone");
        sqlAtTimeZoneExpression.TimeZone.Accept(this);
    }

    public override void VisitSqlIntervalExpression(SqlIntervalExpression sqlIntervalExpression)
    {
        AppendWithSpace("interval");
        if (sqlIntervalExpression.Body != null)
        {
            AppendSpace();
            sqlIntervalExpression.Body.Accept(this);
        }
        if (sqlIntervalExpression.Unit != null)
        {
            AppendSpace();
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

    public override void VisitSqlCollateExpression(SqlCollateExpression sqlCollateExpression)
    {
        AppendWithSpace("collate");
        if (sqlCollateExpression.Body != null)
        {
            sqlCollateExpression.Body.Accept(this);
        }
    }

    public override void VisitSqlRegexExpression(SqlRegexExpression sqlRegexExpression)
    {
        if (sqlRegexExpression.Body != null)
        {
            sqlRegexExpression.Body.Accept(this);
        }
        if (IsPgsql)
        {
            if (sqlRegexExpression.IsCaseSensitive)
            {
                Append("~");
            }
            else
            {
                Append("~*");
            }
        }
        else if (IsMySql)
        {
            AppendWithSpace("regexp");
        }

        if (sqlRegexExpression.RegEx != null)
        {
            sqlRegexExpression.RegEx.Accept(this);
        }
        if (sqlRegexExpression.Collate != null)
        {
            sqlRegexExpression.Collate.Accept(this);
        }

    }

    public override void VisitSqlReturningExpression(SqlReturningExpression sqlReturningExpression)
    {
        if (!sqlReturningExpression.HasValue())
        {
            return;
        }

        if (sqlReturningExpression.Items.HasValue())
        {
            AppendWithSpace("returning");
            for (var i = 0; i < sqlReturningExpression.Items.Count; i++)
            {
                var item = sqlReturningExpression.Items[i];
                item.Accept(this);
                if (i < sqlReturningExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }

        }

        if (sqlReturningExpression.IntoVariables.HasValue())
        {
            AppendWithSpace("into");
            for (var i = 0; i < sqlReturningExpression.IntoVariables.Count; i++)
            {
                var item = sqlReturningExpression.IntoVariables[i];
                item.Accept(this);
                if (i < sqlReturningExpression.IntoVariables.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
        }
    }

    public override void VisitSqlArrayExpression(SqlArrayExpression sqlArrayExpression)
    {
        if (!sqlArrayExpression.HasValue())
        {
            return;
        }

        if (sqlArrayExpression.Items.HasValue())
        {
            Append("array[");

            for (var i = 0; i < sqlArrayExpression.Items.Count; i++)
            {
                var item = sqlArrayExpression.Items[i];
                item.Accept(this);
                if (i < sqlArrayExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }
            AppendWithoutSpaces("]");
        }
    }

    public override void VisitSqlArrayIndexExpression(SqlArrayIndexExpression sqlArrayIndexExpression)
    {
        if (sqlArrayIndexExpression.Body == null || sqlArrayIndexExpression.Index == null)
        {
            return;
        }

        if (sqlArrayIndexExpression.Body is SqlArrayExpression)
        {
            Append("(");
            sqlArrayIndexExpression.Body.Accept(this);
            Append(")");
        }
        else
        {
            sqlArrayIndexExpression.Body.Accept(this);
        }
        
        Append("[");
        sqlArrayIndexExpression.Index.Accept(this);
        Append("]");
    }

    public override void VisitSqlArraySliceExpression(SqlArraySliceExpression sqlArraySliceExpression)
    {
        if (sqlArraySliceExpression.Body == null)
        {
            return;
        }

        Append("(");
        sqlArraySliceExpression.Body.Accept(this);
        Append(")");
        Append("[");
        sqlArraySliceExpression.StartIndex?.Accept(this);
        Append(":");
        sqlArraySliceExpression.EndIndex?.Accept(this);
        Append("]");
    }
}