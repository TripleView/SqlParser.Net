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
    public override SqlExpression VisitSqlAllColumnExpression(SqlAllColumnExpression sqlAllColumnExpression)
    {
        AppendWithSpace("*");
        return sqlAllColumnExpression;
    }
    public override SqlExpression VisitSqlAllExpression(SqlAllExpression sqlAllExpression)
    {
        Append("all");
        if (sqlAllExpression.Body != null)
        {
            EnableParen(() => { sqlAllExpression.Body = sqlAllExpression.Body.Accept(this); });
        }
        return sqlAllExpression;
    }
    public override SqlExpression VisitSqlAnyExpression(SqlAnyExpression sqlAnyExpression)
    {
        Append("any");
        if (sqlAnyExpression.Body != null)
        {
            if (sqlAnyExpression.Body is SqlSelectExpression)
            {
                sqlAnyExpression.Body = sqlAnyExpression.Body.Accept(this);
            }
            else
            {
                EnableParen(() => { sqlAnyExpression.Body = sqlAnyExpression.Body.Accept(this); });
            }
            
        }
        return sqlAnyExpression;
    }
    public override SqlExpression VisitSqlBetweenAndExpression(SqlBetweenAndExpression sqlBetweenAndExpression)
    {
        if (sqlBetweenAndExpression.Body != null)
        {
            sqlBetweenAndExpression.Body = sqlBetweenAndExpression.Body.Accept(this);
        }

        if (sqlBetweenAndExpression.IsNot)
        {
            AppendWithSpace("not");
        }
        AppendWithSpace("between");
        if (sqlBetweenAndExpression.Begin != null)
        {
            sqlBetweenAndExpression.Begin = sqlBetweenAndExpression.Begin.Accept(this);
        }

        AppendWithSpace("and");

        if (sqlBetweenAndExpression.End != null)
        {
            sqlBetweenAndExpression.End = sqlBetweenAndExpression.End.Accept(this);
        }

        return sqlBetweenAndExpression;
    }
    public override SqlExpression VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
    {
        void internalAction()
        {
            if (sqlBinaryExpression.Left != null)
            {
                sqlBinaryExpression.Left = sqlBinaryExpression.Left.Accept(this);
            }

            if (sqlBinaryExpression.Operator != null)
            {
                AppendWithSpace(sqlBinaryExpression.Operator.Value.ToString().ToLowerInvariant());
            }
            if (sqlBinaryExpression.Right != null)
            {
                sqlBinaryExpression.Right = sqlBinaryExpression.Right.Accept(this);
            }

            if (sqlBinaryExpression.Collate != null)
            {
                sqlBinaryExpression.Collate = (SqlCollateExpression)sqlBinaryExpression.Collate.Accept(this);
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

        return sqlBinaryExpression;
    }
    public override SqlExpression VisitSqlCaseExpression(SqlCaseExpression sqlCaseExpression)
    {
        Append("case");

        if (sqlCaseExpression.Value != null)
        {
            sqlCaseExpression.Value = sqlCaseExpression.Value.Accept(this);
        }

        if (sqlCaseExpression.Items.HasValue())
        {
            var newItems = new List<SqlCaseItemExpression>();
            foreach (var item in sqlCaseExpression.Items)
            {
                var newItem = (SqlCaseItemExpression)item.Accept(this);
                newItems.Add(newItem);
            }

            sqlCaseExpression.Items = newItems;
        }

        if (sqlCaseExpression.Else != null)
        {
            AppendWithSpace("else");
            sqlCaseExpression.Else = sqlCaseExpression.Else.Accept(this);
        }

        AppendWithLeftSpace("end");

        return sqlCaseExpression;
    }
    public override SqlExpression VisitSqlCaseItemExpression(SqlCaseItemExpression sqlCaseItemExpression)
    {

        if (sqlCaseItemExpression.Condition != null)
        {
            AppendWithSpace("when");
            sqlCaseItemExpression.Condition = sqlCaseItemExpression.Condition.Accept(this);
        }

        if (sqlCaseItemExpression.Value != null)
        {
            AppendWithSpace("then");
            sqlCaseItemExpression.Value = sqlCaseItemExpression.Value.Accept(this);
        }
        return sqlCaseItemExpression;
    }
    public override SqlExpression VisitSqlDeleteExpression(SqlDeleteExpression sqlDeleteExpression)
    {
        if (sqlDeleteExpression.WithSubQuerys.HasValue())
        {
            AppendWithRightSpace("with");
            for (var i = 0; i < sqlDeleteExpression.WithSubQuerys.Count; i++)
            {
                var item = sqlDeleteExpression.WithSubQuerys[i];
                item.Accept(this);
                if (i < sqlDeleteExpression.WithSubQuerys.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
            AppendSpace();
        }

        AppendWithRightSpace("delete");
        if (sqlDeleteExpression.Body != null)
        {
            sqlDeleteExpression.Body = sqlDeleteExpression.Body.Accept(this);
        }

        AppendWithSpace("from");

        if (sqlDeleteExpression.Table != null)
        {
            sqlDeleteExpression.Table = sqlDeleteExpression.Table.Accept(this);
        }
        if (sqlDeleteExpression.Where != null)
        {
            AppendWithSpace("where");
            sqlDeleteExpression.Where = sqlDeleteExpression.Where.Accept(this);
        }

        return sqlDeleteExpression;
    }
    public override SqlExpression VisitSqlExistsExpression(SqlExistsExpression sqlExistsExpression)
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
                sqlExistsExpression.Body = (SqlSelectExpression)sqlExistsExpression.Body.Accept(this);
            });
        }

        return sqlExistsExpression;
    }

    public override SqlExpression VisitSqlFunctionCallExpression(SqlFunctionCallExpression sqlFunctionCallExpression)
    {

        if (sqlFunctionCallExpression.Name != null)
        {
            sqlFunctionCallExpression.Name = (SqlIdentifierExpression)sqlFunctionCallExpression.Name.Accept(this);
        }
        if (sqlFunctionCallExpression.Arguments != null)
        {

            EnableParen(() =>
            {
                var newArguments = new List<SqlExpression>();
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
                    var newArgument = argument.Accept(this);
                    newArguments.Add(newArgument);
                    if (sqlFunctionCallExpression.CaseAsTargetType != null)
                    {
                        AppendWithSpace("as");
                        sqlFunctionCallExpression.CaseAsTargetType =
                            (SqlIdentifierExpression)sqlFunctionCallExpression.CaseAsTargetType.Accept(this);
                        //Append($"{sqlFunctionCallExpression.CaseAsTargetType.Value}");
                    }

                    if (sqlFunctionCallExpression.FromSource != null)
                    {
                        AppendWithSpace($"from");
                        sqlFunctionCallExpression.FromSource = sqlFunctionCallExpression.FromSource.Accept(this);
                    }

                    if (i < sqlFunctionCallExpression.Arguments.Count - 1)
                    {
                        AppendWithoutSpaces(",");
                    }
                }

                sqlFunctionCallExpression.Arguments = newArguments;
            });
        }
        else
        {
            AppendWithoutSpaces("()");
        }

        if (sqlFunctionCallExpression.WithinGroup != null)
        {
            sqlFunctionCallExpression.WithinGroup = (SqlWithinGroupExpression)sqlFunctionCallExpression.WithinGroup.Accept(this);
        }

        if (sqlFunctionCallExpression.Over != null)
        {
            sqlFunctionCallExpression.Over = (SqlOverExpression)sqlFunctionCallExpression.Over.Accept(this);
        }

        if (sqlFunctionCallExpression.Collate != null)
        {
            sqlFunctionCallExpression.Collate = (SqlCollateExpression)sqlFunctionCallExpression.Collate.Accept(this);
        }
        return sqlFunctionCallExpression;
    }
    public override SqlExpression VisitSqlGroupByExpression(SqlGroupByExpression sqlGroupByExpression)
    {
        if (!sqlGroupByExpression.HasValue())
        {
            return sqlGroupByExpression;
        }
        AppendWithSpace("group by");

        if (sqlGroupByExpression.Items.HasValue())
        {
            var newItems = new List<SqlExpression>();
            for (var i = 0; i < sqlGroupByExpression.Items.Count; i++)
            {
                var item = sqlGroupByExpression.Items[i];
                var newItem = item.Accept(this);
                newItems.Add(newItem);
                if (i < sqlGroupByExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
            sqlGroupByExpression.Items = newItems;
        }

        if (sqlGroupByExpression.Having != null)
        {
            AppendWithSpace("having");
            sqlGroupByExpression.Having = sqlGroupByExpression.Having.Accept(this);
        }

        return sqlGroupByExpression;
    }
    public override SqlExpression VisitSqlIdentifierExpression(SqlIdentifierExpression sqlIdentifierExpression)
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
            sqlIdentifierExpression.Collate = (SqlCollateExpression)sqlIdentifierExpression.Collate.Accept(this);
        }
        return sqlIdentifierExpression;
    }
    public override SqlExpression VisitSqlInExpression(SqlInExpression sqlInExpression)
    {
        if (sqlInExpression.Body != null)
        {
            sqlInExpression.Body = sqlInExpression.Body.Accept(this);
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
                var newTargetList = new List<SqlExpression>();
                for (var i = 0; i < sqlInExpression.TargetList.Count; i++)
                {
                    var item = sqlInExpression.TargetList[i];
                    var newItem = item.Accept(this);
                    newTargetList.Add(newItem);
                    if (i < sqlInExpression.TargetList.Count - 1)
                    {
                        AppendWithoutSpaces(", ");
                    }
                }
                sqlInExpression.TargetList = newTargetList;
            });
        }

        if (sqlInExpression.SubQuery != null)
        {
            EnableParen(() =>
            {
                sqlInExpression.SubQuery = (SqlSelectExpression)sqlInExpression.SubQuery.Accept(this);
            });
        }
        return sqlInExpression;
    }
    public override SqlExpression VisitSqlInsertExpression(SqlInsertExpression sqlInsertExpression)
    {
        if (sqlInsertExpression.WithSubQuerys.HasValue())
        {
            AppendWithRightSpace("with");
            var newWithSubQueries = new List<SqlWithSubQueryExpression>();
            for (var i = 0; i < sqlInsertExpression.WithSubQuerys.Count; i++)
            {
                var item = sqlInsertExpression.WithSubQuerys[i];
                var newItem = (SqlWithSubQueryExpression)item.Accept(this);
                newWithSubQueries.Add(newItem);
                if (i < sqlInsertExpression.WithSubQuerys.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }

            sqlInsertExpression.WithSubQuerys = newWithSubQueries;
            AppendSpace();
        }

        AppendWithRightSpace("insert into");

        if (sqlInsertExpression.Table != null)
        {
            sqlInsertExpression.Table = sqlInsertExpression.Table.Accept(this);
        }

        if (sqlInsertExpression.Columns.HasValue())
        {
            EnableParen(() =>
            {
                var newColumns = new List<SqlExpression>();
                for (var i = 0; i < sqlInsertExpression.Columns.Count; i++)
                {
                    var item = sqlInsertExpression.Columns[i];
                    var newItem = item.Accept(this);
                    newColumns.Add(newItem);
                    if (i < sqlInsertExpression.Columns.Count - 1)
                    {
                        AppendWithoutSpaces(", ");
                    }
                }
                sqlInsertExpression.Columns = newColumns;
            });
        }
        if (sqlInsertExpression.ValuesList.HasValue())
        {
            AppendWithLeftSpace("values");
            var newValuesList = new List<List<SqlExpression>>();
            for (var i = 0; i < sqlInsertExpression.ValuesList.Count; i++)
            {
                var items = sqlInsertExpression.ValuesList[i];
                var newItems = new List<SqlExpression>();
                EnableParen((() =>
                {
                    for (var j = 0; j < items.Count; j++)
                    {
                        var item = items[j];
                        var newItem = item.Accept(this);
                        newItems.Add(newItem);
                        if (j < items.Count - 1)
                        {
                            AppendWithoutSpaces(", ");
                        }
                    }
                }));
                newValuesList.Add(newItems);

                if (i < sqlInsertExpression.ValuesList.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }
            sqlInsertExpression.ValuesList = newValuesList;

        }

        if (sqlInsertExpression.FromSelect != null)
        {
            AppendSpace();
            sqlInsertExpression.FromSelect = (SqlSelectExpression)sqlInsertExpression.FromSelect.Accept(this);
        }

        if (sqlInsertExpression.Returning != null)
        {
            sqlInsertExpression.Returning = (SqlReturningExpression)sqlInsertExpression.Returning.Accept(this);
        }

        return sqlInsertExpression;
    }
    public override SqlExpression VisitSqlJoinTableExpression(SqlJoinTableExpression sqlJoinTableExpression)
    {

        if (sqlJoinTableExpression.Left != null)
        {
            sqlJoinTableExpression.Left = sqlJoinTableExpression.Left.Accept(this);
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
            sqlJoinTableExpression.Right = sqlJoinTableExpression.Right.Accept(this);
        }

        if (sqlJoinTableExpression.Conditions != null)
        {
            AppendWithSpace("on");
            sqlJoinTableExpression.Conditions = sqlJoinTableExpression.Conditions.Accept(this);
        }

        return sqlJoinTableExpression;
    }
    public override SqlExpression VisitSqlLimitExpression(SqlLimitExpression sqlLimitExpression)
    {
        switch (dbType)
        {
            case DbType.Oracle:
                AppendWithSpace("fetch first");
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount = sqlLimitExpression.RowCount.Accept(this);
                }
                AppendWithLeftSpace("rows only");
                break;
            case DbType.MySql:
            case DbType.Sqlite:
                AppendWithSpace("limit");
                if (sqlLimitExpression.Offset != null)
                {
                    sqlLimitExpression.Offset = sqlLimitExpression.Offset.Accept(this);
                    AppendWithoutSpaces(", ");
                }
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount = sqlLimitExpression.RowCount.Accept(this);
                }

                break;
            case DbType.SqlServer:
                AppendWithSpace("OFFSET");
                if (sqlLimitExpression.Offset != null)
                {
                    sqlLimitExpression.Offset = sqlLimitExpression.Offset.Accept(this);
                }
                AppendWithSpace("ROWS FETCH NEXT");
                if (sqlLimitExpression.RowCount != null)
                {
                    sqlLimitExpression.RowCount = sqlLimitExpression.RowCount.Accept(this);
                }
                AppendWithSpace("ROWS ONLY");
                break;
            case DbType.Pgsql:

                if (sqlLimitExpression.RowCount != null)
                {
                    AppendWithSpace("limit");
                    sqlLimitExpression.RowCount = sqlLimitExpression.RowCount.Accept(this);
                }

                if (sqlLimitExpression.Offset != null)
                {
                    AppendWithSpace("offset");
                    sqlLimitExpression.Offset = sqlLimitExpression.Offset.Accept(this);
                }

                break;
        }

        return sqlLimitExpression;
    }
    public override SqlExpression VisitSqlNotExpression(SqlNotExpression sqlNotExpression)
    {
        AppendWithSpace("not");
        if (sqlNotExpression.Body != null)
        {
            EnableParen(() =>
            {
                sqlNotExpression.Body = sqlNotExpression.Body.Accept(this);
            });
        }

        return sqlNotExpression;
    }
    public override SqlExpression VisitSqlNullExpression(SqlNullExpression sqlNullExpression)
    {
        Append("null");
        return sqlNullExpression;
    }
    public override SqlExpression VisitSqlNumberExpression(SqlNumberExpression sqlNumberExpression)
    {
        if (!string.IsNullOrWhiteSpace(sqlNumberExpression.LeftQualifiers))
        {
            Append(sqlNumberExpression.LeftQualifiers + sqlNumberExpression.Value + sqlNumberExpression.RightQualifiers);
        }
        else
        {
            Append(sqlNumberExpression.Value.ToString());
        }
        return sqlNumberExpression;
    }
    public override SqlExpression VisitSqlOrderByExpression(SqlOrderByExpression sqlOrderByExpression)
    {
        if (!sqlOrderByExpression.HasValue())
        {
            return sqlOrderByExpression;
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
            var newItems = new List<SqlOrderByItemExpression>();
            for (var i = 0; i < sqlOrderByExpression.Items.Count; i++)
            {
                var item = sqlOrderByExpression.Items[i];
                var newItem = (SqlOrderByItemExpression)item.Accept(this);
                newItems.Add(newItem);
                if (i < sqlOrderByExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
            sqlOrderByExpression.Items = newItems;
        }

        return sqlOrderByExpression;
    }

    public override SqlExpression VisitSqlConnectByExpression(SqlConnectByExpression sqlConnectByExpression)
    {
        if (sqlConnectByExpression.StartWith != null)
        {
            AppendWithSpace("start with");
            sqlConnectByExpression.StartWith = sqlConnectByExpression.StartWith.Accept(this);
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

        sqlConnectByExpression.Body = sqlConnectByExpression.Body.Accept(this);

        if (sqlConnectByExpression.OrderBy.HasValue())
        {
            sqlConnectByExpression.OrderBy = (SqlOrderByExpression)sqlConnectByExpression.OrderBy.Accept(this);
        }

        return sqlConnectByExpression;
    }

    public override SqlExpression VisitSqlOrderByItemExpression(SqlOrderByItemExpression sqlOrderByItemExpression)
    {
        if (sqlOrderByItemExpression.Body != null)
        {
            sqlOrderByItemExpression.Body = sqlOrderByItemExpression.Body.Accept(this);
        }

        if (sqlOrderByItemExpression.OrderByType.HasValue)
        {
            AppendWithLeftSpace(sqlOrderByItemExpression.OrderByType == SqlOrderByType.Asc ? "asc" : "desc");
        }
        if (sqlOrderByItemExpression.NullsType.HasValue)
        {
            AppendWithLeftSpace(sqlOrderByItemExpression.NullsType == SqlOrderByNullsType.First ? "nulls first" : "nulls last");
        }

        return sqlOrderByItemExpression;
    }
    public override SqlExpression VisitSqlOverExpression(SqlOverExpression sqlOverExpression)
    {
        AppendWithSpace("over");
        EnableParen((() =>
        {
            if (sqlOverExpression.PartitionBy != null)
            {
                sqlOverExpression.PartitionBy = (SqlPartitionByExpression)sqlOverExpression.PartitionBy.Accept(this);
            }
            if (sqlOverExpression.OrderBy.HasValue())
            {
                sqlOverExpression.OrderBy = (SqlOrderByExpression)sqlOverExpression.OrderBy.Accept(this);
            }
        }));
        return sqlOverExpression;
    }
    public override SqlExpression VisitSqlPartitionByExpression(SqlPartitionByExpression sqlPartitionByExpression)
    {
        if (!(sqlPartitionByExpression.Items != null && sqlPartitionByExpression.Items.Count > 0))
        {
            return sqlPartitionByExpression;
        }
        AppendWithRightSpace("partition by");

        if (sqlPartitionByExpression.Items.HasValue())
        {
            var newItems = new List<SqlExpression>();
            for (var i = 0; i < sqlPartitionByExpression.Items.Count; i++)
            {
                var item = sqlPartitionByExpression.Items[i];
                var newItem = item.Accept(this);
                newItems.Add(newItem);
                if (i < sqlPartitionByExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }

            sqlPartitionByExpression.Items = newItems;
        }

        return sqlPartitionByExpression;
    }
    public override SqlExpression VisitSqlPivotTableExpression(SqlPivotTableExpression sqlPivotTableExpression)
    {
        if (sqlPivotTableExpression.SubQuery != null)
        {
            sqlPivotTableExpression.SubQuery = sqlPivotTableExpression.SubQuery.Accept(this);
        }

        AppendWithSpace("pivot");
        EnableParen(() =>
        {
            if (sqlPivotTableExpression.FunctionCall != null)
            {
                sqlPivotTableExpression.FunctionCall = (SqlFunctionCallExpression)sqlPivotTableExpression.FunctionCall.Accept(this);
            }
            if (sqlPivotTableExpression.For != null)
            {
                AppendWithSpace("for");
                sqlPivotTableExpression.For = sqlPivotTableExpression.For.Accept(this);
            }
            if (sqlPivotTableExpression.In != null)
            {
                AppendWithSpace("in");
                EnableParen(() =>
                {
                    var newItems = new List<SqlExpression>();
                    for (var i = 0; i < sqlPivotTableExpression.In.Count; i++)
                    {
                        var item = sqlPivotTableExpression.In[i];
                        var newItem = item.Accept(this);
                        newItems.Add(newItem);
                        if (i < sqlPivotTableExpression.In.Count - 1)
                        {
                            AppendWithoutSpaces(", ");
                        }
                    }
                    sqlPivotTableExpression.In = newItems;
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

            sqlPivotTableExpression.Alias = (SqlIdentifierExpression)sqlPivotTableExpression.Alias.Accept(this);
        }

        return sqlPivotTableExpression;
    }
    public override SqlExpression VisitSqlPropertyExpression(SqlPropertyExpression sqlPropertyExpression)
    {

        if (sqlPropertyExpression.Table != null)
        {
            sqlPropertyExpression.Table = sqlPropertyExpression.Table.Accept(this);
            Append(".");
        }

        if (sqlPropertyExpression.Name != null)
        {
            sqlPropertyExpression.Name = (SqlIdentifierExpression)sqlPropertyExpression.Name.Accept(this);
        }

        if (sqlPropertyExpression.Collate != null)
        {
            sqlPropertyExpression.Collate = (SqlCollateExpression)sqlPropertyExpression.Collate.Accept(this);
        }

        return sqlPropertyExpression;
    }
    public override SqlExpression VisitSqlReferenceTableExpression(SqlReferenceTableExpression sqlReferenceTableExpression)
    {
        if (sqlReferenceTableExpression.FunctionCall != null)
        {
            sqlReferenceTableExpression.FunctionCall = (SqlFunctionCallExpression)sqlReferenceTableExpression.FunctionCall.Accept(this);
        }

        if (sqlReferenceTableExpression.Alias != null)
        {
            if (!IsOracle)
            {
                AppendWithSpace("as");
            }
            sqlReferenceTableExpression.Alias = (SqlIdentifierExpression)sqlReferenceTableExpression.Alias.Accept(this);
        }

        return sqlReferenceTableExpression;
    }
    public override SqlExpression VisitSqlSelectExpression(SqlSelectExpression sqlSelectExpression)
    {
        if (sqlSelectExpression.Alias == null
            && (sb.Length == 0 || sqlSelectExpression.Parent is SqlInsertExpression
                               || sqlSelectExpression.Parent is SqlInExpression
                               || (IsSqlite && sqlSelectExpression.Parent is not SqlFunctionCallExpression)
                               || sqlSelectExpression.Query is SqlUnionQueryExpression
                               || sqlSelectExpression.Parent is SqlExistsExpression
                               || sqlSelectExpression.Parent is SqlWithSubQueryExpression))
        {
            if (sqlSelectExpression.Query != null)
            {
                sqlSelectExpression.Query = sqlSelectExpression.Query.Accept(this);
            }
        }
        else
        {
            EnableParen(() =>
            {
                if (sqlSelectExpression.Query != null)
                {
                    sqlSelectExpression.Query = sqlSelectExpression.Query.Accept(this);
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
            sqlSelectExpression.Alias = (SqlIdentifierExpression)sqlSelectExpression.Alias.Accept(this);
        }

        if (sqlSelectExpression.OrderBy.HasValue())
        {
            sqlSelectExpression.OrderBy = (SqlOrderByExpression)sqlSelectExpression.OrderBy.Accept(this);
        }

        if (sqlSelectExpression.Limit != null)
        {
            sqlSelectExpression.Limit = (SqlLimitExpression)sqlSelectExpression.Limit.Accept(this);
        }
        return sqlSelectExpression;
    }
    public override SqlExpression VisitSqlSelectItemExpression(SqlSelectItemExpression sqlSelectItemExpression)
    {
        sqlSelectItemExpression.Body = sqlSelectItemExpression.Body?.Accept(this);
        if (sqlSelectItemExpression.Alias != null)
        {
            AppendWithSpace("as");
            sqlSelectItemExpression.Alias = (SqlIdentifierExpression)sqlSelectItemExpression.Alias.Accept(this);
        }

        return sqlSelectItemExpression;
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

    public override SqlExpression VisitSqlSelectQueryExpression(SqlSelectQueryExpression sqlSelectQueryExpression)
    {
        if (sqlSelectQueryExpression.WithSubQuerys.HasValue())
        {
            AppendWithRightSpace(" with");
            var newWithSubQuerys = new List<SqlWithSubQueryExpression>();
            for (var i = 0; i < sqlSelectQueryExpression.WithSubQuerys.Count; i++)
            {
                var item = sqlSelectQueryExpression.WithSubQuerys[i];
                var newItem = (SqlWithSubQueryExpression)item.Accept(this);
                newWithSubQuerys.Add(newItem);
                if (i < sqlSelectQueryExpression.WithSubQuerys.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
            sqlSelectQueryExpression.WithSubQuerys = newWithSubQuerys;
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
            sqlSelectQueryExpression.Top = (SqlTopExpression)sqlSelectQueryExpression.Top.Accept(this);
        }

        if (sqlSelectQueryExpression.Columns.HasValue())
        {
            var newColumns = new List<SqlSelectItemExpression>();
            for (var i = 0; i < sqlSelectQueryExpression.Columns.Count; i++)
            {
                AppendSpace();
                var item = sqlSelectQueryExpression.Columns[i];
                var newItem = (SqlSelectItemExpression)item.Accept(this);
                newColumns.Add(newItem);
                if (i < sqlSelectQueryExpression.Columns.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }
            sqlSelectQueryExpression.Columns = newColumns;
        }

        if (sqlSelectQueryExpression.Into != null)
        {
            AppendWithSpace("into");
            sqlSelectQueryExpression.Into = sqlSelectQueryExpression.Into.Accept(this);
        }
        if (sqlSelectQueryExpression.From != null)
        {
            AppendWithSpace("from");
            sqlSelectQueryExpression.From = sqlSelectQueryExpression.From.Accept(this);
        }
        if (sqlSelectQueryExpression.Where != null)
        {
            AppendWithSpace("where");
            sqlSelectQueryExpression.Where = sqlSelectQueryExpression.Where.Accept(this);
        }

        if (sqlSelectQueryExpression.GroupBy.HasValue())
        {
            sqlSelectQueryExpression.GroupBy = (SqlGroupByExpression)sqlSelectQueryExpression.GroupBy.Accept(this);
        }

        if (sqlSelectQueryExpression.OrderBy.HasValue())
        {
            sqlSelectQueryExpression.OrderBy = (SqlOrderByExpression)sqlSelectQueryExpression.OrderBy.Accept(this);
        }

        if (dbType == DbType.Oracle && sqlSelectQueryExpression.ConnectBy != null)
        {
            sqlSelectQueryExpression.ConnectBy = (SqlConnectByExpression)sqlSelectQueryExpression.ConnectBy.Accept(this);
        }

        if (sqlSelectQueryExpression.Limit != null)
        {
            sqlSelectQueryExpression.Limit = (SqlLimitExpression)sqlSelectQueryExpression.Limit.Accept(this);
        }
        if (sqlSelectQueryExpression.Hints.HasValue())
        {
            foreach (var tableExpressionHint in sqlSelectQueryExpression.Hints)
            {
                AppendSpace();
                tableExpressionHint.Body = tableExpressionHint.Body?.Accept(this);
            }
        }

        return sqlSelectQueryExpression;
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

    public override SqlExpression VisitSqlBoolExpression(SqlBoolExpression sqlBoolExpression)
    {
        if (dbType == DbType.MySql || dbType == DbType.Pgsql || dbType == DbType.Sqlite)
        {
            Append($"{sqlBoolExpression.Value.ToString().ToLowerInvariant()}");
        }

        return sqlBoolExpression;
    }

    public override SqlExpression VisitSqlStringExpression(SqlStringExpression sqlStringExpression)
    {
        Append($"{(sqlStringExpression.IsUniCode ? "N" : "")}'{sqlStringExpression.Value.Replace("'", "''")}'");
        if (sqlStringExpression.Collate != null)
        {
            sqlStringExpression.Collate = (SqlCollateExpression)sqlStringExpression.Collate.Accept(this);
        }
        return sqlStringExpression;
    }
    public override SqlExpression VisitSqlTableExpression(SqlTableExpression sqlTableExpression)
    {
        if (sqlTableExpression.Database != null)
        {
            sqlTableExpression.Database = (SqlIdentifierExpression)sqlTableExpression.Database.Accept(this);
            Append(".");
        }

        if (sqlTableExpression.Schema != null)
        {
            sqlTableExpression.Schema = (SqlIdentifierExpression)sqlTableExpression.Schema.Accept(this);
            Append(".");
        }

        sqlTableExpression.Name = (SqlIdentifierExpression)sqlTableExpression.Name?.Accept(this);
        if ((IsOracle || IsPgsql) && sqlTableExpression.DbLink != null)
        {
            AppendWithoutSpaces("@");
            sqlTableExpression.DbLink = (SqlIdentifierExpression)sqlTableExpression.DbLink.Accept(this);
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

            sqlTableExpression.Alias = (SqlIdentifierExpression)sqlTableExpression.Alias.Accept(this);
        }

        if (sqlTableExpression.Hints.HasValue())
        {
            foreach (var tableExpressionHint in sqlTableExpression.Hints)
            {
                AppendSpace();
                tableExpressionHint.Body = tableExpressionHint.Body?.Accept(this);
            }
        }

        return sqlTableExpression;
    }
    public override SqlExpression VisitSqlUnionQueryExpression(SqlUnionQueryExpression sqlUnionQueryExpression)
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

        return sqlUnionQueryExpression;
    }

    private void VisitSqlUnionQueryExpressionInternal(SqlUnionQueryExpression sqlUnionQueryExpression)
    {
        if (sqlUnionQueryExpression.Left != null)
        {
            sqlUnionQueryExpression.Left = sqlUnionQueryExpression.Left.Accept(this);
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

        if (sqlUnionQueryExpression.Right != null)
        {
            sqlUnionQueryExpression.Right = sqlUnionQueryExpression.Right.Accept(this);
        }
    }

    public override SqlExpression VisitSqlUpdateExpression(SqlUpdateExpression sqlUpdateExpression)
    {
        sqlParseType = ParseType.Update;
        if (sqlUpdateExpression.WithSubQuerys.HasValue())
        {
            AppendWithRightSpace("with");
            var newWithSubQueries = new List<SqlWithSubQueryExpression>();
            for (var i = 0; i < sqlUpdateExpression.WithSubQuerys.Count; i++)
            {
                var item = sqlUpdateExpression.WithSubQuerys[i];
                var newItem = (SqlWithSubQueryExpression)item.Accept(this);
                newWithSubQueries.Add(newItem);
                if (i < sqlUpdateExpression.WithSubQuerys.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
            sqlUpdateExpression.WithSubQuerys = newWithSubQueries;
            AppendSpace();
        }

        AppendWithRightSpace("update");
        if (sqlUpdateExpression.Table != null)
        {
            sqlUpdateExpression.Table = sqlUpdateExpression.Table.Accept(this);
        }

        if (sqlUpdateExpression.Items.HasValue())
        {
            var newItems = new List<SqlExpression>();
            AppendWithSpace("set");
            for (var i = 0; i < sqlUpdateExpression.Items.Count; i++)
            {
                var item = sqlUpdateExpression.Items[i];
                //item.Accept(this);
                EnableUpdateSetContext(() =>
                {
                    var newItem = item.Accept(this);
                    newItems.Add(newItem);
                });

                if (i < sqlUpdateExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
            sqlUpdateExpression.Items = newItems;
        }

        if (IsSqlServer || IsPgsql || IsSqlite)
        {
            if (sqlUpdateExpression.From != null)
            {
                AppendWithSpace("from");
                sqlUpdateExpression.From = sqlUpdateExpression.From.Accept(this);
            }
        }

        if (sqlUpdateExpression.Where != null)
        {
            AppendWithSpace("where");
            sqlUpdateExpression.Where = sqlUpdateExpression.Where.Accept(this);
        }

        return sqlUpdateExpression;

    }
    public override SqlExpression VisitSqlVariableExpression(SqlVariableExpression sqlVariableExpression)
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
            sqlVariableExpression.Collate = (SqlCollateExpression)sqlVariableExpression.Collate.Accept(this);
        }

        return sqlVariableExpression;
    }
    public override SqlExpression VisitSqlWithinGroupExpression(SqlWithinGroupExpression sqlWithinGroupExpression)
    {
        AppendWithLeftSpace("within group");
        EnableParen(() =>
        {
            if (sqlWithinGroupExpression.OrderBy.HasValue())
            {
                sqlWithinGroupExpression.OrderBy = (SqlOrderByExpression)sqlWithinGroupExpression.OrderBy.Accept(this);
            }
        });

        return sqlWithinGroupExpression;
    }
    public override SqlExpression VisitSqlWithSubQueryExpression(SqlWithSubQueryExpression sqlWithSubQueryExpression)
    {
        if (sqlWithSubQueryExpression.Alias != null)
        {
            sqlWithSubQueryExpression.Alias = (SqlIdentifierExpression)sqlWithSubQueryExpression.Alias.Accept(this);
        }

        if (sqlWithSubQueryExpression.Columns.HasValue())
        {
            var newItems = new List<SqlIdentifierExpression>();
            EnableParen(() =>
            {
                for (var i = 0; i < sqlWithSubQueryExpression.Columns.Count; i++)
                {
                    var item = sqlWithSubQueryExpression.Columns[i];
                    var newItem = (SqlIdentifierExpression)item.Accept(this);
                    newItems.Add(newItem);
                    if (i < sqlWithSubQueryExpression.Columns.Count - 1)
                    {
                        AppendWithoutSpaces(",");
                    }
                }
            });
            sqlWithSubQueryExpression.Columns = newItems;
        }

        AppendWithSpace("as");
        if (sqlWithSubQueryExpression.FromSelect != null)
        {
            EnableParen(() =>
            {
                sqlWithSubQueryExpression.FromSelect = (SqlSelectExpression)sqlWithSubQueryExpression.FromSelect.Accept(this);
            });
        }

        return sqlWithSubQueryExpression;
    }

    public override SqlExpression VisitSqlTopExpression(SqlTopExpression sqlTopExpression)
    {
        AppendWithSpace("top");
        if (sqlTopExpression.Body != null)
        {
            sqlTopExpression.Body = (SqlNumberExpression)sqlTopExpression.Body.Accept(this);
        }

        return sqlTopExpression;
    }

    public override SqlExpression VisitSqlHintExpression(SqlHintExpression sqlHintExpression)
    {
        if (sqlHintExpression.Body != null)
        {
            AppendSpace();
            sqlHintExpression.Body = sqlHintExpression.Body.Accept(this);
        }
        return sqlHintExpression;
    }

    public override SqlExpression VisitSqlAtTimeZoneExpression(SqlAtTimeZoneExpression sqlAtTimeZoneExpression)
    {
        if (sqlAtTimeZoneExpression.Body != null)
        {
            sqlAtTimeZoneExpression.Body = sqlAtTimeZoneExpression.Body.Accept(this);
        }
        AppendWithSpace("at time zone");
        sqlAtTimeZoneExpression.TimeZone = (SqlStringExpression)sqlAtTimeZoneExpression.TimeZone.Accept(this);
        return sqlAtTimeZoneExpression;
    }

    public override SqlExpression VisitSqlIntervalExpression(SqlIntervalExpression sqlIntervalExpression)
    {
        AppendWithSpace("interval");
        if (sqlIntervalExpression.Body != null)
        {
            AppendSpace();
            sqlIntervalExpression.Body = sqlIntervalExpression.Body.Accept(this);
        }
        if (sqlIntervalExpression.Unit != null)
        {
            AppendSpace();
            sqlIntervalExpression.Unit = (SqlTimeUnitExpression)sqlIntervalExpression.Unit.Accept(this);
        }
        return sqlIntervalExpression;
    }

    public override SqlExpression VisitSqlTimeUnitExpression(SqlTimeUnitExpression sqlTimeUnitExpression)
    {
        if (!string.IsNullOrWhiteSpace(sqlTimeUnitExpression.Unit))
        {
            Append(sqlTimeUnitExpression.Unit);
        }
        return sqlTimeUnitExpression;
    }

    public override SqlExpression VisitSqlCollateExpression(SqlCollateExpression sqlCollateExpression)
    {
        AppendWithSpace("collate");
        if (sqlCollateExpression.Body != null)
        {
            sqlCollateExpression.Body = sqlCollateExpression.Body.Accept(this);
        }

        return sqlCollateExpression;
    }

    public override SqlExpression VisitSqlRegexExpression(SqlRegexExpression sqlRegexExpression)
    {
        if (sqlRegexExpression.Body != null)
        {
            sqlRegexExpression.Body = sqlRegexExpression.Body.Accept(this);
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
            sqlRegexExpression.RegEx = (SqlStringExpression)sqlRegexExpression.RegEx.Accept(this);
        }
        if (sqlRegexExpression.Collate != null)
        {
            sqlRegexExpression.Collate = (SqlCollateExpression)sqlRegexExpression.Collate.Accept(this);
        }

        return sqlRegexExpression;
    }

    public override SqlExpression VisitSqlReturningExpression(SqlReturningExpression sqlReturningExpression)
    {
        if (!sqlReturningExpression.HasValue())
        {
            return sqlReturningExpression;
        }

        if (sqlReturningExpression.Items.HasValue())
        {
            var newItems = new List<SqlExpression>();
            AppendWithSpace("returning");
            for (var i = 0; i < sqlReturningExpression.Items.Count; i++)
            {
                var item = sqlReturningExpression.Items[i];
                var newItem = item.Accept(this);
                newItems.Add(newItem);
                if (i < sqlReturningExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
            sqlReturningExpression.Items = newItems;
        }

        if (sqlReturningExpression.IntoVariables.HasValue())
        {
            AppendWithSpace("into");
            var newItems = new List<SqlExpression>();
            for (var i = 0; i < sqlReturningExpression.IntoVariables.Count; i++)
            {
                var item = sqlReturningExpression.IntoVariables[i];
                var newItem = item.Accept(this);
                newItems.Add(newItem);
                if (i < sqlReturningExpression.IntoVariables.Count - 1)
                {
                    AppendWithoutSpaces(", ");
                }
            }
            sqlReturningExpression.IntoVariables = newItems;
        }

        return sqlReturningExpression;
    }

    public override SqlExpression VisitSqlArrayExpression(SqlArrayExpression sqlArrayExpression)
    {
        if (!sqlArrayExpression.HasValue())
        {
            return sqlArrayExpression;
        }

        if (sqlArrayExpression.Items.HasValue())
        {
            Append("array[");
            var newItems = new List<SqlExpression>();
            for (var i = 0; i < sqlArrayExpression.Items.Count; i++)
            {
                var item = sqlArrayExpression.Items[i];
                var newItem = item.Accept(this);
                newItems.Add(newItem);
                if (i < sqlArrayExpression.Items.Count - 1)
                {
                    AppendWithoutSpaces(",");
                }
            }
            sqlArrayExpression.Items = newItems;
            AppendWithoutSpaces("]");
        }

        return sqlArrayExpression;
    }

    public override SqlExpression VisitSqlArrayIndexExpression(SqlArrayIndexExpression sqlArrayIndexExpression)
    {
        if (sqlArrayIndexExpression.Body == null || sqlArrayIndexExpression.Index == null)
        {
            return sqlArrayIndexExpression;
        }

        if (sqlArrayIndexExpression.Body is SqlArrayExpression)
        {
            Append("(");
            sqlArrayIndexExpression.Body = sqlArrayIndexExpression.Body.Accept(this);
            Append(")");
        }
        else
        {
            sqlArrayIndexExpression.Body = sqlArrayIndexExpression.Body.Accept(this);
        }

        Append("[");
        sqlArrayIndexExpression.Index = (SqlNumberExpression)sqlArrayIndexExpression.Index.Accept(this);
        Append("]");
        return sqlArrayIndexExpression;
    }

    public override SqlExpression VisitSqlArraySliceExpression(SqlArraySliceExpression sqlArraySliceExpression)
    {
        if (sqlArraySliceExpression.Body == null)
        {
            return sqlArraySliceExpression;
        }

        Append("(");
        sqlArraySliceExpression.Body = sqlArraySliceExpression.Body.Accept(this);
        Append(")");
        Append("[");
        sqlArraySliceExpression.StartIndex = (SqlNumberExpression)sqlArraySliceExpression.StartIndex?.Accept(this);
        Append(":");
        sqlArraySliceExpression.EndIndex = (SqlNumberExpression)sqlArraySliceExpression.EndIndex?.Accept(this);
        Append("]");
        return sqlArraySliceExpression;
    }
}