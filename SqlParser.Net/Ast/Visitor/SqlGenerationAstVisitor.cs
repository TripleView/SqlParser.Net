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

    private DbType dbType = DbType.MySql;
    private bool IsOracle => this.dbType == DbType.Oracle;
    private bool IsSqlServer => this.dbType == DbType.SqlServer;
    private bool IsPgsql => this.dbType == DbType.Pgsql;

    private bool IsMySql => this.dbType == DbType.MySql;
    private bool IsSqlite => this.dbType == DbType.Sqlite;


    private bool isInUpdateSetContext = false;
    /// <summary>
    /// 调用链
    /// </summary>
    private List<SqlExpressionType> callStack = new List<SqlExpressionType>();

    //private HashSet<char> delimiters = new HashSet<char>() { '.',',','(',')'};
    public SqlGenerationAstVisitor(DbType dbType)
    {
        this.dbType = dbType;
    }

    private void AddCallStackItem(SqlExpressionType sqlExpressionType)
    {
        callStack.Add(sqlExpressionType);
    }

    private void RemoveCallStackLastItem()
    {
        if (callStack.Count > 0)
        {
            callStack.RemoveAt(callStack.Count - 1);
        }
    }

    private bool SearchFromBottom(SqlExpressionType sqlExpressionType)
    {
        if (callStack.Count > 0)
        {
            for (var i = callStack.Count - 1; i >= 0; i--)
            {
                if (callStack[i] == sqlExpressionType)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool isDelete => CheckFirstItem(SqlExpressionType.Delete);
    private bool isUpdate => CheckFirstItem(SqlExpressionType.Update);
    private bool isInsert => CheckFirstItem(SqlExpressionType.Insert);
    private bool CheckFirstItem(SqlExpressionType sqlExpressionType)
    {
        if (callStack.Count > 0)
        {
            return callStack[0] == sqlExpressionType;
        }
        return false;
    }

    public string GetResult()
    {
        return sb.ToString().Trim(' ');
    }
    public override SqlExpression VisitSqlAllColumnExpression(SqlAllColumnExpression sqlAllColumnExpression, VisitContext context = null)
    {
        AppendWithSpace("*");
        return sqlAllColumnExpression;
    }
    public override SqlExpression VisitSqlAllExpression(SqlAllExpression sqlAllExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.All);
        Append("all");
        if (sqlAllExpression.Body != null)
        {
            EnableParen(() => { sqlAllExpression.Body = sqlAllExpression.Body.Accept(this); });
        }

        RemoveCallStackLastItem();
        return sqlAllExpression;
    }
    public override SqlExpression VisitSqlAnyExpression(SqlAnyExpression sqlAnyExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Any);
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

        RemoveCallStackLastItem();
        return sqlAnyExpression;
    }
    public override SqlExpression VisitSqlBetweenAndExpression(SqlBetweenAndExpression sqlBetweenAndExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.BetweenAnd);
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

        RemoveCallStackLastItem();
        return sqlBetweenAndExpression;
    }
    public override SqlExpression VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Binary);
        void internalAction()
        {
            if (context != null && context.Parent is not SqlDeleteExpression)
            {
                context = null;
            }
            if (sqlBinaryExpression.Left != null)
            {
                sqlBinaryExpression.Left = sqlBinaryExpression.Left.Accept(this, context);
            }

            if (sqlBinaryExpression.Operator != null)
            {
                AppendWithSpace(sqlBinaryExpression.Operator.Value.ToString().ToLowerInvariant());
            }
            if (sqlBinaryExpression.Right != null)
            {
                sqlBinaryExpression.Right = sqlBinaryExpression.Right.Accept(this, context);
            }

            if (sqlBinaryExpression.Collate != null)
            {
                sqlBinaryExpression.Collate = (SqlCollateExpression)sqlBinaryExpression.Collate.Accept(this);
            }
        }

        if (context?.Parent is SqlConnectByExpression || (context?.Parent is SqlUpdateExpression && isInUpdateSetContext))
        {
            internalAction();
        }
        else
        {
            EnableParen(internalAction);
        }

        RemoveCallStackLastItem();
        return sqlBinaryExpression;
    }
    public override SqlExpression VisitSqlCaseExpression(SqlCaseExpression sqlCaseExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Case);
        Append("case");

        if (sqlCaseExpression.Value != null)
        {
            AppendSpace();
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

        RemoveCallStackLastItem();
        return sqlCaseExpression;
    }
    public override SqlExpression VisitSqlCaseItemExpression(SqlCaseItemExpression sqlCaseItemExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.CaseItem);
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

        RemoveCallStackLastItem();
        return sqlCaseItemExpression;
    }
    public override SqlExpression VisitSqlDeleteExpression(SqlDeleteExpression sqlDeleteExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Delete);
        context = new VisitContext()
        {
            Parent = sqlDeleteExpression
        };
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
            RemoveCallStackLastItem();
            sqlDeleteExpression.Body = sqlDeleteExpression.Body.Accept(this, context);
        }

        AppendWithSpace("from");

        if (sqlDeleteExpression.Table != null)
        {
            sqlDeleteExpression.Table = sqlDeleteExpression.Table.Accept(this, context);
        }
        if (sqlDeleteExpression.Where != null)
        {
            AppendWithSpace("where");
            sqlDeleteExpression.Where = sqlDeleteExpression.Where.Accept(this, context);
        }

        RemoveCallStackLastItem();
        return sqlDeleteExpression;
    }
    public override SqlExpression VisitSqlExistsExpression(SqlExistsExpression sqlExistsExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Exists);
        context = new VisitContext()
        {
            Parent = sqlExistsExpression
        };
        if (sqlExistsExpression.IsNot)
        {
            AppendWithSpace("not");
        }

        Append("exists");

        if (sqlExistsExpression.Body != null)
        {
            EnableParen(() =>
            {
                sqlExistsExpression.Body = (SqlSelectExpression)sqlExistsExpression.Body.Accept(this, context);
            });
        }

        RemoveCallStackLastItem();
        return sqlExistsExpression;
    }

    public override SqlExpression VisitSqlFunctionCallExpression(SqlFunctionCallExpression sqlFunctionCallExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.FunctionCall);
        context = new VisitContext()
        {
            Parent = sqlFunctionCallExpression
        };
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
                    var newArgument = argument.Accept(this, context);
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
        RemoveCallStackLastItem();
        return sqlFunctionCallExpression;
    }
    public override SqlExpression VisitSqlGroupByExpression(SqlGroupByExpression sqlGroupByExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.GroupBy);
        if (!sqlGroupByExpression.HasValue())
        {
            RemoveCallStackLastItem();
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

        RemoveCallStackLastItem();
        return sqlGroupByExpression;
    }
    public override SqlExpression VisitSqlIdentifierExpression(SqlIdentifierExpression sqlIdentifierExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Identifier);
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
        RemoveCallStackLastItem();
        return sqlIdentifierExpression;
    }
    public override SqlExpression VisitSqlInExpression(SqlInExpression sqlInExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.In);
        context = new VisitContext()
        {
            Parent = sqlInExpression
        };
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
                sqlInExpression.SubQuery = (SqlSelectExpression)sqlInExpression.SubQuery.Accept(this, context);
            });
        }
        RemoveCallStackLastItem();
        return sqlInExpression;
    }
    public override SqlExpression VisitSqlInsertExpression(SqlInsertExpression sqlInsertExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Insert);
        context = new VisitContext()
        {
            Parent = sqlInsertExpression
        };
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
            sqlInsertExpression.FromSelect = (SqlSelectExpression)sqlInsertExpression.FromSelect.Accept(this, context);
        }

        if (sqlInsertExpression.Returning != null)
        {
            sqlInsertExpression.Returning = (SqlReturningExpression)sqlInsertExpression.Returning.Accept(this);
        }

        RemoveCallStackLastItem();
        return sqlInsertExpression;
    }
    public override SqlExpression VisitSqlJoinTableExpression(SqlJoinTableExpression sqlJoinTableExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.JoinTable);

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

        RemoveCallStackLastItem();
        return sqlJoinTableExpression;
    }
    public override SqlExpression VisitSqlLimitExpression(SqlLimitExpression sqlLimitExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Limit);
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

        RemoveCallStackLastItem();
        return sqlLimitExpression;
    }
    public override SqlExpression VisitSqlNotExpression(SqlNotExpression sqlNotExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Not);

        AppendWithSpace("not");
        if (sqlNotExpression.Body != null)
        {
            EnableParen(() =>
            {
                sqlNotExpression.Body = sqlNotExpression.Body.Accept(this);
            });
        }

        RemoveCallStackLastItem();
        return sqlNotExpression;
    }
    public override SqlExpression VisitSqlNullExpression(SqlNullExpression sqlNullExpression, VisitContext context = null)
    {
        Append("null");
        return sqlNullExpression;
    }
    public override SqlExpression VisitSqlNumberExpression(SqlNumberExpression sqlNumberExpression, VisitContext context = null)
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
    public override SqlExpression VisitSqlOrderByExpression(SqlOrderByExpression sqlOrderByExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.OrderBy);

        if (!sqlOrderByExpression.HasValue())
        {
            RemoveCallStackLastItem();
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

        RemoveCallStackLastItem();
        return sqlOrderByExpression;
    }

    public override SqlExpression VisitSqlConnectByExpression(SqlConnectByExpression sqlConnectByExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.ConnectBy);
        context = new VisitContext()
        {
            Parent = sqlConnectByExpression
        };
        if (sqlConnectByExpression.StartWith != null)
        {
            AppendWithSpace("start with");
            sqlConnectByExpression.StartWith = sqlConnectByExpression.StartWith.Accept(this, context);
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

        sqlConnectByExpression.Body = sqlConnectByExpression.Body.Accept(this, context);

        if (sqlConnectByExpression.OrderBy.HasValue())
        {
            sqlConnectByExpression.OrderBy = (SqlOrderByExpression)sqlConnectByExpression.OrderBy.Accept(this, context);
        }

        RemoveCallStackLastItem();
        return sqlConnectByExpression;
    }

    public override SqlExpression VisitSqlOrderByItemExpression(SqlOrderByItemExpression sqlOrderByItemExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.OrderByItem);

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

        RemoveCallStackLastItem();
        return sqlOrderByItemExpression;
    }
    public override SqlExpression VisitSqlOverExpression(SqlOverExpression sqlOverExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Over);

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
        RemoveCallStackLastItem();
        return sqlOverExpression;
    }
    public override SqlExpression VisitSqlPartitionByExpression(SqlPartitionByExpression sqlPartitionByExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.PartitionBy);

        if (!(sqlPartitionByExpression.Items != null && sqlPartitionByExpression.Items.Count > 0))
        {
            RemoveCallStackLastItem();
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

        RemoveCallStackLastItem();
        return sqlPartitionByExpression;
    }
    public override SqlExpression VisitSqlPivotTableExpression(SqlPivotTableExpression sqlPivotTableExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.PivotTable);

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

        RemoveCallStackLastItem();
        return sqlPivotTableExpression;
    }
    public override SqlExpression VisitSqlPropertyExpression(SqlPropertyExpression sqlPropertyExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Property);

        if (sqlPropertyExpression.Table != null && CheckTableOrPropertyNeedTableAlias())
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

        RemoveCallStackLastItem();
        return sqlPropertyExpression;
    }
    public override SqlExpression VisitSqlReferenceTableExpression(SqlReferenceTableExpression sqlReferenceTableExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.ReferenceTable);

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

        RemoveCallStackLastItem();
        return sqlReferenceTableExpression;
    }
    public override SqlExpression VisitSqlSelectExpression(SqlSelectExpression sqlSelectExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Select);

        if (sqlSelectExpression.Alias == null
            && (sb.Length == 0 || context?.Parent is SqlInsertExpression
                               || context?.Parent is SqlInExpression
                               || (IsSqlite && context?.Parent is not SqlFunctionCallExpression)
                               || sqlSelectExpression.Query is SqlUnionQueryExpression
                               || context?.Parent is SqlExistsExpression
                               || context?.Parent is SqlWithSubQueryExpression))
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
        RemoveCallStackLastItem();
        return sqlSelectExpression;
    }
    public override SqlExpression VisitSqlSelectItemExpression(SqlSelectItemExpression sqlSelectItemExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.SelectItem);

        sqlSelectItemExpression.Body = sqlSelectItemExpression.Body?.Accept(this);
        if (sqlSelectItemExpression.Alias != null)
        {
            AppendWithSpace("as");
            sqlSelectItemExpression.Alias = (SqlIdentifierExpression)sqlSelectItemExpression.Alias.Accept(this);
        }

        RemoveCallStackLastItem();
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

    public override SqlExpression VisitSqlSelectQueryExpression(SqlSelectQueryExpression sqlSelectQueryExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.SelectQuery);

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

        RemoveCallStackLastItem();
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

    public override SqlExpression VisitSqlBoolExpression(SqlBoolExpression sqlBoolExpression, VisitContext context = null)
    {
        if (dbType == DbType.MySql || dbType == DbType.Pgsql || dbType == DbType.Sqlite)
        {
            Append($"{sqlBoolExpression.Value.ToString().ToLowerInvariant()}");
        }

        return sqlBoolExpression;
    }

    public override SqlExpression VisitSqlStringExpression(SqlStringExpression sqlStringExpression, VisitContext context = null)
    {
        Append($"{(sqlStringExpression.IsUniCode ? "N" : "")}'{sqlStringExpression.Value.Replace("'", "''")}'");
        if (sqlStringExpression.Collate != null)
        {
            sqlStringExpression.Collate = (SqlCollateExpression)sqlStringExpression.Collate.Accept(this);
        }
        return sqlStringExpression;
    }

    /// <summary>
    /// 判断表和字段是否需要别名
    /// </summary>
    /// <returns></returns>
    private bool CheckTableOrPropertyNeedTableAlias()
    {
        return !isDelete || (isDelete && SearchFromBottom(SqlExpressionType.Select));
    }

    public override SqlExpression VisitSqlTableExpression(SqlTableExpression sqlTableExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Table);

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

        //delete语句中无需别名
        if (sqlTableExpression.Alias != null && CheckTableOrPropertyNeedTableAlias())
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

        RemoveCallStackLastItem();
        return sqlTableExpression;
    }
    public override SqlExpression VisitSqlUnionQueryExpression(SqlUnionQueryExpression sqlUnionQueryExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.UnionQuery);

        context = new VisitContext()
        {
            Parent = sqlUnionQueryExpression
        };
        if (sb.Length > 0 && sb[sb.Length - 1] == '(')
        {
            VisitSqlUnionQueryExpressionInternal(sqlUnionQueryExpression, context);
        }
        else
        {
            if (IsSqlite)
            {
                VisitSqlUnionQueryExpressionInternal(sqlUnionQueryExpression, context);
            }
            else
            {
                EnableParen(() =>
                {
                    VisitSqlUnionQueryExpressionInternal(sqlUnionQueryExpression, context);
                });
            }
        }

        RemoveCallStackLastItem();
        return sqlUnionQueryExpression;
    }

    private void VisitSqlUnionQueryExpressionInternal(SqlUnionQueryExpression sqlUnionQueryExpression, VisitContext context = null)
    {

        if (sqlUnionQueryExpression.Left != null)
        {
            sqlUnionQueryExpression.Left = sqlUnionQueryExpression.Left.Accept(this, context);
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
            sqlUnionQueryExpression.Right = sqlUnionQueryExpression.Right.Accept(this, context);
        }
    }

    public override SqlExpression VisitSqlUpdateExpression(SqlUpdateExpression sqlUpdateExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Update);

        context = new VisitContext()
        {
            Parent = sqlUpdateExpression
        };
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
                    var newItem = item.Accept(this, context);
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

        RemoveCallStackLastItem();
        return sqlUpdateExpression;

    }
    public override SqlExpression VisitSqlVariableExpression(SqlVariableExpression sqlVariableExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Variable);

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

        RemoveCallStackLastItem();
        return sqlVariableExpression;
    }
    public override SqlExpression VisitSqlWithinGroupExpression(SqlWithinGroupExpression sqlWithinGroupExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.WithinGroup);

        AppendWithLeftSpace("within group");
        EnableParen(() =>
        {
            if (sqlWithinGroupExpression.OrderBy.HasValue())
            {
                sqlWithinGroupExpression.OrderBy = (SqlOrderByExpression)sqlWithinGroupExpression.OrderBy.Accept(this);
            }
        });

        RemoveCallStackLastItem();
        return sqlWithinGroupExpression;
    }
    public override SqlExpression VisitSqlWithSubQueryExpression(SqlWithSubQueryExpression sqlWithSubQueryExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.WithSubQuery);

        context = new VisitContext()
        {
            Parent = sqlWithSubQueryExpression
        };
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
                sqlWithSubQueryExpression.FromSelect = (SqlSelectExpression)sqlWithSubQueryExpression.FromSelect.Accept(this, context);
            });
        }

        RemoveCallStackLastItem();
        return sqlWithSubQueryExpression;
    }

    public override SqlExpression VisitSqlTopExpression(SqlTopExpression sqlTopExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Top);

        AppendWithSpace("top");
        if (sqlTopExpression.Body != null)
        {
            sqlTopExpression.Body = (SqlNumberExpression)sqlTopExpression.Body.Accept(this);
        }

        RemoveCallStackLastItem();
        return sqlTopExpression;
    }

    public override SqlExpression VisitSqlHintExpression(SqlHintExpression sqlHintExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Hint);

        if (sqlHintExpression.Body != null)
        {
            AppendSpace();
            sqlHintExpression.Body = sqlHintExpression.Body.Accept(this);
        }
        RemoveCallStackLastItem();
        return sqlHintExpression;
    }

    public override SqlExpression VisitSqlAtTimeZoneExpression(SqlAtTimeZoneExpression sqlAtTimeZoneExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.AtTimeZone);

        if (sqlAtTimeZoneExpression.Body != null)
        {
            sqlAtTimeZoneExpression.Body = sqlAtTimeZoneExpression.Body.Accept(this);
        }
        AppendWithSpace("at time zone");
        sqlAtTimeZoneExpression.TimeZone = (SqlStringExpression)sqlAtTimeZoneExpression.TimeZone.Accept(this);
        RemoveCallStackLastItem();
        return sqlAtTimeZoneExpression;
    }

    public override SqlExpression VisitSqlIntervalExpression(SqlIntervalExpression sqlIntervalExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Interval);

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
        RemoveCallStackLastItem();
        return sqlIntervalExpression;
    }

    public override SqlExpression VisitSqlTimeUnitExpression(SqlTimeUnitExpression sqlTimeUnitExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.TimeUnit);

        if (!string.IsNullOrWhiteSpace(sqlTimeUnitExpression.Unit))
        {
            Append(sqlTimeUnitExpression.Unit);
        }
        RemoveCallStackLastItem();
        return sqlTimeUnitExpression;
    }

    public override SqlExpression VisitSqlCollateExpression(SqlCollateExpression sqlCollateExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Collate);

        AppendWithSpace("collate");
        if (sqlCollateExpression.Body != null)
        {
            sqlCollateExpression.Body = sqlCollateExpression.Body.Accept(this);
        }

        RemoveCallStackLastItem();
        return sqlCollateExpression;
    }

    public override SqlExpression VisitSqlRegexExpression(SqlRegexExpression sqlRegexExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Regex);

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

        RemoveCallStackLastItem();
        return sqlRegexExpression;
    }

    public override SqlExpression VisitSqlReturningExpression(SqlReturningExpression sqlReturningExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Returning);

        if (!sqlReturningExpression.HasValue())
        {
            RemoveCallStackLastItem();
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

        RemoveCallStackLastItem();
        return sqlReturningExpression;
    }

    public override SqlExpression VisitSqlArrayExpression(SqlArrayExpression sqlArrayExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.Array);

        if (!sqlArrayExpression.HasValue())
        {
            RemoveCallStackLastItem();
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

        RemoveCallStackLastItem();
        return sqlArrayExpression;
    }

    public override SqlExpression VisitSqlArrayIndexExpression(SqlArrayIndexExpression sqlArrayIndexExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.ArrayIndex);

        if (sqlArrayIndexExpression.Body == null || sqlArrayIndexExpression.Index == null)
        {
            RemoveCallStackLastItem();
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
        RemoveCallStackLastItem();
        return sqlArrayIndexExpression;
    }

    public override SqlExpression VisitSqlArraySliceExpression(SqlArraySliceExpression sqlArraySliceExpression, VisitContext context = null)
    {
        callStack.Add(SqlExpressionType.ArraySlice);

        if (sqlArraySliceExpression.Body == null)
        {
            RemoveCallStackLastItem();
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
        RemoveCallStackLastItem();
        return sqlArraySliceExpression;
    }
}