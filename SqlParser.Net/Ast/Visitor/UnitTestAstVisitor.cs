using System;
using System.Linq;
using System.Text;
using SqlParser.Net.Ast.Expression;

namespace SqlParser.Net.Ast.Visitor;

public class UnitTestAstVisitor : BaseAstVisitor
{
    private StringBuilder sb = new StringBuilder();
    private string fourSpace = "    ";
    private int numberOfLevels = 0;

    private bool addSpace = true;

    public string GetResult()
    {
        return sb.ToString();
    }
    public override SqlExpression VisitSqlAllColumnExpression(SqlAllColumnExpression sqlAllColumnExpression)
    {
        AppendLine("new SqlAllColumnExpression()");
        return sqlAllColumnExpression;
    }
    public override SqlExpression VisitSqlAllExpression(SqlAllExpression sqlAllExpression)
    {
        AppendLine("new SqlAllExpression()");
        AppendLine("{");


        if (sqlAllExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlAllExpression.Body.Accept(this);
            });
        }

        AppendLine("},");
        return sqlAllExpression;
    }
    public override SqlExpression VisitSqlAnyExpression(SqlAnyExpression sqlAnyExpression)
    {
        AppendLine("new SqlAnyExpression()");
        AppendLine("{");


        if (sqlAnyExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlAnyExpression.Body.Accept(this);
            });
        }

        AppendLine("},");

        return sqlAnyExpression;
    }
    public override SqlExpression VisitSqlBetweenAndExpression(SqlBetweenAndExpression sqlBetweenAndExpression)
    {
        AppendLine("new SqlBetweenAndExpression()");
        AppendLine("{");
        if (sqlBetweenAndExpression.IsNot)
        {
            AdvanceNext(() =>
            {
                AppendLine("IsNot = true,");
            });
        }
        if (sqlBetweenAndExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlBetweenAndExpression.Body.Accept(this);
            });
        }

        if (sqlBetweenAndExpression.Begin != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Begin = ");
                sqlBetweenAndExpression.Begin.Accept(this);
            });
        }
        if (sqlBetweenAndExpression.End != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("End = ");
                sqlBetweenAndExpression.End.Accept(this);
            });
        }
        AppendLine("},");
        return sqlBetweenAndExpression;
    }
    public override SqlExpression VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
    {
        AppendLine("new SqlBinaryExpression()");
        AppendLine("{");
        if (sqlBinaryExpression.Left != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Left = ");
                sqlBinaryExpression.Left?.Accept(this);
            });
        }
        if (sqlBinaryExpression.Operator != null)
        {
            AdvanceNext(() =>
            {
                AppendLine($"Operator = SqlBinaryOperator.{sqlBinaryExpression.Operator.Name},");
            });
        }
        if (sqlBinaryExpression.Right != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Right = ");
                sqlBinaryExpression.Right?.Accept(this);
            });
        }
        if (sqlBinaryExpression.Collate != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Collate = ");
                sqlBinaryExpression.Collate.Accept(this);
            });
        }

        AppendLine("},");
        return sqlBinaryExpression;
    }
    public override SqlExpression VisitSqlCaseExpression(SqlCaseExpression sqlCaseExpression)
    {
        AppendLine("new SqlCaseExpression()");
        AppendLine("{");

        if (sqlCaseExpression.Items.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Items = new List<SqlCaseItemExpression>()");
                AppendLine("{");
                foreach (var item in sqlCaseExpression.Items)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }

        if (sqlCaseExpression.Else != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Else = ");
                sqlCaseExpression.Else.Accept(this);
            });
        }
        if (sqlCaseExpression.Value != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Value = ");
                sqlCaseExpression.Value.Accept(this);
            });
        }

        AppendLine("},");
        return sqlCaseExpression;
    }
    public override SqlExpression VisitSqlCaseItemExpression(SqlCaseItemExpression sqlCaseItemExpression)
    {
        AppendLine("new SqlCaseItemExpression()");
        AppendLine("{");

        if (sqlCaseItemExpression.Condition != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Condition = ");
                sqlCaseItemExpression.Condition.Accept(this);
            });
        }
        if (sqlCaseItemExpression.Value != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Value = ");
                sqlCaseItemExpression.Value.Accept(this);
            });
        }

        AppendLine("},");
        return sqlCaseItemExpression;
    }
    public override SqlExpression VisitSqlDeleteExpression(SqlDeleteExpression sqlDeleteExpression)
    {
        var isFirst = numberOfLevels == 0;
        if (isFirst)
        {
            sb.Append("var expect = ");
        }
        AppendLine("new SqlDeleteExpression()");
        AppendLine("{");

        if (sqlDeleteExpression.WithSubQuerys.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("WithSubQuerys = new List<SqlWithSubQueryExpression>()");
                AppendLine("{");
                foreach (var item in sqlDeleteExpression.WithSubQuerys)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }

        if (sqlDeleteExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlDeleteExpression.Body.Accept(this);
            });
        }
        if (sqlDeleteExpression.Table != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Table = ");
                sqlDeleteExpression.Table.Accept(this);
            });
        }
        if (sqlDeleteExpression.Where != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Where = ");
                sqlDeleteExpression.Where.Accept(this);
            });
        }
        if (isFirst)
        {
            AppendLine("};");
        }
        else
        {
            AppendLine("},");
        }
        return sqlDeleteExpression;
    }
    public override SqlExpression VisitSqlExistsExpression(SqlExistsExpression sqlExistsExpression)
    {
        AppendLine("new SqlExistsExpression()");
        AppendLine("{");

        if (sqlExistsExpression.IsNot)
        {
            AdvanceNext(() =>
            {
                AppendLine("IsNot = true,");
            });
        }
        if (sqlExistsExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlExistsExpression.Body.Accept(this);
            });
        }

        AppendLine("},");
        return sqlExistsExpression;
    }

    public override SqlExpression VisitSqlFunctionCallExpression(SqlFunctionCallExpression sqlFunctionCallExpression)
    {
        AppendLine("new SqlFunctionCallExpression()");
        AppendLine("{");
        if (sqlFunctionCallExpression.Name != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Name = ");
                sqlFunctionCallExpression.Name?.Accept(this);
            });
        }
        if (sqlFunctionCallExpression.WithinGroup != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("WithinGroup = ");
                sqlFunctionCallExpression.WithinGroup?.Accept(this);
            });
        }
        if (sqlFunctionCallExpression.FromSource != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("FromSource = ");
                sqlFunctionCallExpression.FromSource?.Accept(this);
            });
        }
        if (sqlFunctionCallExpression.Over != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Over = ");
                sqlFunctionCallExpression.Over?.Accept(this);
            });
        }
        if (sqlFunctionCallExpression.Arguments != null)
        {
            AdvanceNext(() =>
            {
                AppendLine("Arguments = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var argument in sqlFunctionCallExpression.Arguments)
                {
                    AdvanceNext(() =>
                    {
                        argument.Accept(this);
                    });
                }
                AppendLine("},");
            });
        }
        if (sqlFunctionCallExpression.IsDistinct)
        {
            AdvanceNext(() =>
            {
                AppendLine($"IsDistinct = true,");
            });
        }
        if (sqlFunctionCallExpression.CaseAsTargetType != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("CaseAsTargetType = ");
                sqlFunctionCallExpression.CaseAsTargetType?.Accept(this);
            });
        }
        if (sqlFunctionCallExpression.Collate != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Collate = ");
                sqlFunctionCallExpression.Collate?.Accept(this);
            });
        }
        AppendLine("},");
        return sqlFunctionCallExpression;
    }
    public override SqlExpression VisitSqlGroupByExpression(SqlGroupByExpression sqlGroupByExpression)
    {
        if (!sqlGroupByExpression.HasValue())
        {
            return sqlGroupByExpression;
        }
        AppendLine("new SqlGroupByExpression()");
        AppendLine("{");
        if (sqlGroupByExpression.Items.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Items = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var item in sqlGroupByExpression.Items)
                {
                    item.Accept(this);
                }
                AppendLine("},");
            });
        }

        if (sqlGroupByExpression.Having != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Having = ");
                sqlGroupByExpression.Having.Accept(this);
            });
        }

        AppendLine("},");
        return sqlGroupByExpression;
    }
    public override SqlExpression VisitSqlIdentifierExpression(SqlIdentifierExpression sqlIdentifierExpression)
    {
        AppendLine("new SqlIdentifierExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            var value = sqlIdentifierExpression.Value;

            AppendLine($"Value = \"{value}\",");
            HandleQualifiers(sqlIdentifierExpression);
        });
        if (sqlIdentifierExpression.Collate != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Collate = ");
                sqlIdentifierExpression.Collate?.Accept(this);
            });
        }
        AppendLine("},");
        return sqlIdentifierExpression;
    }
    public override SqlExpression VisitSqlInExpression(SqlInExpression sqlInExpression)
    {
        AppendLine("new SqlInExpression()");
        AppendLine("{");

        if (sqlInExpression.IsNot)
        {
            AdvanceNext(() =>
            {
                AppendLine("IsNot = true,");
            });
        }
        if (sqlInExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlInExpression.Body.Accept(this);
            });
        }


        if (sqlInExpression.TargetList.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("TargetList = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var item in sqlInExpression.TargetList)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }

        if (sqlInExpression.SubQuery != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("SubQuery = ");
                sqlInExpression.SubQuery.Accept(this);
            });
        }
        AppendLine("},");
        return sqlInExpression;
    }
    public override SqlExpression VisitSqlInsertExpression(SqlInsertExpression sqlInsertExpression)
    {
        var isFirst = numberOfLevels == 0;
        if (isFirst)
        {
            sb.Append("var expect = ");
        }

        AppendLine("new SqlInsertExpression()");
        AppendLine("{");

        if (sqlInsertExpression.WithSubQuerys.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("WithSubQuerys = new List<SqlWithSubQueryExpression>()");
                AppendLine("{");
                foreach (var item in sqlInsertExpression.WithSubQuerys)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });
                }
                AppendLine("},");
            });
        }

        if (sqlInsertExpression.Columns.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Columns = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var item in sqlInsertExpression.Columns)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }
        if (sqlInsertExpression.ValuesList.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("ValuesList = new List<List<SqlExpression>>()");
                AppendLine("{");
                foreach (var items in sqlInsertExpression.ValuesList)
                {
                    AdvanceNext(() =>
                    {
                        AppendLine("new List<SqlExpression>()");
                        AppendLine("{");
                        foreach (var item in items)
                        {
                            AdvanceNext(() =>
                            {
                                item.Accept(this);
                            });

                        }
                        AppendLine("},");
                    });
                }
                AppendLine("},");
            });
        }
        if (sqlInsertExpression.Table != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Table = ");
                sqlInsertExpression.Table?.Accept(this);
            });
        }
        if (sqlInsertExpression.FromSelect != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("FromSelect = ");
                sqlInsertExpression.FromSelect?.Accept(this);
            });
        }
        if (sqlInsertExpression.Returning != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Returning = ");
                sqlInsertExpression.Returning?.Accept(this);
            });
        }

        if (isFirst)
        {
            AppendLine("};");
        }
        else
        {
            AppendLine("},");
        }

        return sqlInsertExpression;
    }
    public override SqlExpression VisitSqlJoinTableExpression(SqlJoinTableExpression sqlJoinTableExpression)
    {
        AppendLine("new SqlJoinTableExpression()");
        AppendLine("{");
        if (sqlJoinTableExpression.Left != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Left = ");
                sqlJoinTableExpression.Left.Accept(this);
            });
        }
        if (sqlJoinTableExpression.JoinType != null)
        {
            AdvanceNext(() =>
            {
                AppendLine($"JoinType = SqlJoinType.{sqlJoinTableExpression.JoinType.ToString()},");
            });
        }
        if (sqlJoinTableExpression.Right != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Right = ");
                sqlJoinTableExpression.Right.Accept(this);
            });
        }

        if (sqlJoinTableExpression.Conditions != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Conditions = ");
                sqlJoinTableExpression.Conditions.Accept(this);
            });
        }
        AppendLine("},");
        return sqlJoinTableExpression;
    }
    public override SqlExpression VisitSqlLimitExpression(SqlLimitExpression sqlLimitExpression)
    {
        AppendLine("new SqlLimitExpression()");
        AppendLine("{");

        if (sqlLimitExpression.Offset != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Offset = ");
                sqlLimitExpression.Offset.Accept(this);
            });
        }
        if (sqlLimitExpression.RowCount != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("RowCount = ");
                sqlLimitExpression.RowCount.Accept(this);
            });
        }

        AppendLine("},");
        return sqlLimitExpression;
    }
    public override SqlExpression VisitSqlNotExpression(SqlNotExpression sqlNotExpression)
    {
        AppendLine("new SqlNotExpression()");
        AppendLine("{");


        if (sqlNotExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlNotExpression.Body.Accept(this);
            });
        }

        AppendLine("},");
        return sqlNotExpression;
    }
    public override SqlExpression VisitSqlNullExpression(SqlNullExpression sqlNullExpression)
    {
        AppendLine("new SqlNullExpression()");
        return sqlNullExpression;
    }
    public override SqlExpression VisitSqlNumberExpression(SqlNumberExpression sqlNumberExpression)
    {
        AppendLine("new SqlNumberExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            AppendLine($"Value = {sqlNumberExpression.Value}M,");
            HandleQualifiers(sqlNumberExpression);

        });
        AppendLine("},");
        return sqlNumberExpression;
    }

    private void HandleQualifiers(IQualifierExpression qualifierExpression)
    {
        if (!string.IsNullOrWhiteSpace(qualifierExpression.LeftQualifiers))
        {
            var value = qualifierExpression.LeftQualifiers;
            if (value == "\"")
            {
                value = "\\\"";
            }
            AppendLine($"LeftQualifiers = \"{value}\",");
        }
        if (!string.IsNullOrWhiteSpace(qualifierExpression.RightQualifiers))
        {
            var value = qualifierExpression.RightQualifiers;
            if (value == "\"")
            {
                value = "\\\"";
            }
            AppendLine($"RightQualifiers = \"{value}\",");
        }
    }
    public override SqlExpression VisitSqlOrderByExpression(SqlOrderByExpression sqlOrderByExpression)
    {
        if (!sqlOrderByExpression.HasValue())
        {
            return sqlOrderByExpression;
        }
        AppendLine("new SqlOrderByExpression()");
        AppendLine("{");
        if (sqlOrderByExpression.Items.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Items = new List<SqlOrderByItemExpression>()");
                AppendLine("{");
                foreach (var item in sqlOrderByExpression.Items)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }

        if (sqlOrderByExpression.IsSiblings)
        {

            AdvanceNext(() =>
            {
                AppendLine($"IsSiblings = {sqlOrderByExpression.IsSiblings.ToString().ToLowerInvariant()},");

            });
        }


        AppendLine("},");
        return sqlOrderByExpression;
    }


    public override SqlExpression VisitSqlConnectByExpression(SqlConnectByExpression sqlConnectByExpression)
    {
        AppendLine("new SqlConnectByExpression()");
        AppendLine("{");

        if (sqlConnectByExpression.StartWith != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("StartWith = ");
                sqlConnectByExpression.StartWith?.Accept(this);
            });
        }
        if (sqlConnectByExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlConnectByExpression.Body?.Accept(this);
            });
        }
        if (sqlConnectByExpression.IsNocycle)
        {

            AdvanceNext(() =>
            {
                AppendLine($"IsNocycle = {sqlConnectByExpression.IsNocycle.ToString().ToLowerInvariant()},");

            });
        }
        if (sqlConnectByExpression.IsPrior)
        {

            AdvanceNext(() =>
            {
                AppendLine($"IsPrior = {sqlConnectByExpression.IsPrior.ToString().ToLowerInvariant()},");

            });
        }

        if (sqlConnectByExpression.OrderBy.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("OrderBy = ");
                sqlConnectByExpression.OrderBy?.Accept(this);
            });
        }

        AppendLine("},");
        return sqlConnectByExpression;
    }

    public override SqlExpression VisitSqlOrderByItemExpression(SqlOrderByItemExpression sqlOrderByItemExpression)
    {
        AppendLine("new SqlOrderByItemExpression()");
        AppendLine("{");
        if (sqlOrderByItemExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlOrderByItemExpression.Body?.Accept(this);
            });
        }

        if (sqlOrderByItemExpression.OrderByType != null)
        {

            AdvanceNext(() =>
            {
                AppendLine($"OrderByType = SqlOrderByType.{sqlOrderByItemExpression.OrderByType?.ToString()},");

            });
        }
        if (sqlOrderByItemExpression.NullsType != null)
        {

            AdvanceNext(() =>
            {
                AppendLine($"NullsType = SqlOrderByNullsType.{sqlOrderByItemExpression.NullsType?.ToString()},");

            });
        }

        AppendLine("},");
        return sqlOrderByItemExpression;
    }
    public override SqlExpression VisitSqlOverExpression(SqlOverExpression sqlOverExpression)
    {
        AppendLine("new SqlOverExpression()");
        AppendLine("{");


        if (sqlOverExpression.PartitionBy != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("PartitionBy = ");
                sqlOverExpression.PartitionBy.Accept(this);
            });
        }
        if (sqlOverExpression.OrderBy.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("OrderBy = ");
                sqlOverExpression.OrderBy.Accept(this);
            });
        }
        AppendLine("},");
        return sqlOverExpression;
    }
    public override SqlExpression VisitSqlPartitionByExpression(SqlPartitionByExpression sqlPartitionByExpression)
    {
        if (!(sqlPartitionByExpression.Items != null && sqlPartitionByExpression.Items.Count > 0))
        {
            return sqlPartitionByExpression;
        }
        AppendLine("new SqlPartitionByExpression()");
        AppendLine("{");


        if (sqlPartitionByExpression.Items.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Items = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var item in sqlPartitionByExpression.Items)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }
        AppendLine("},");
        return sqlPartitionByExpression;
    }
    public override SqlExpression VisitSqlPivotTableExpression(SqlPivotTableExpression sqlPivotTableExpression)
    {
        AppendLine("new SqlPivotTableExpression()");
        AppendLine("{");


        if (sqlPivotTableExpression.Alias != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Alias = ");
                sqlPivotTableExpression.Alias.Accept(this);
            });
        }
        if (sqlPivotTableExpression.For != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("For = ");
                sqlPivotTableExpression.For.Accept(this);
            });
        }
        if (sqlPivotTableExpression.FunctionCall != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("FunctionCall = ");
                sqlPivotTableExpression.FunctionCall.Accept(this);
            });
        }
        if (sqlPivotTableExpression.SubQuery != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("SubQuery = ");
                sqlPivotTableExpression.SubQuery.Accept(this);
            });
        }
        if (sqlPivotTableExpression.In != null)
        {
            AdvanceNext(() =>
            {
                AppendLine("In = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var item in sqlPivotTableExpression.In)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }
        AppendLine("},");
        return sqlPivotTableExpression;
    }
    public override SqlExpression VisitSqlPropertyExpression(SqlPropertyExpression sqlPropertyExpression)
    {
        AppendLine("new SqlPropertyExpression()");
        AppendLine("{");
        if (sqlPropertyExpression.Name != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Name = ");
                sqlPropertyExpression.Name?.Accept(this);
            });
        }
        if (sqlPropertyExpression.Table != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Table = ");
                sqlPropertyExpression.Table?.Accept(this);
            });
        }
        if (sqlPropertyExpression.Collate != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Collate = ");
                sqlPropertyExpression.Collate?.Accept(this);
            });
        }
        AppendLine("},");
        return sqlPropertyExpression;
    }
    public override SqlExpression VisitSqlReferenceTableExpression(SqlReferenceTableExpression sqlReferenceTableExpression)
    {
        AppendLine("new SqlReferenceTableExpression()");
        AppendLine("{");
        if (sqlReferenceTableExpression.FunctionCall != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("FunctionCall = ");
                sqlReferenceTableExpression.FunctionCall?.Accept(this);
            });
        }
        if (sqlReferenceTableExpression.Alias != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Alias = ");
                sqlReferenceTableExpression.Alias?.Accept(this);
            });
        }
        AppendLine("}");
        return sqlReferenceTableExpression;
    }
    public override SqlExpression VisitSqlSelectExpression(SqlSelectExpression sqlSelectExpression)
    {
        var isFirst = numberOfLevels == 0;
        if (isFirst)
        {
            sb.Append("var expect = ");
        }

        AppendLine("new SqlSelectExpression()");
        AppendLine("{");

        if (sqlSelectExpression.Alias != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Alias = ");
                sqlSelectExpression.Alias?.Accept(this);
            });
        }
        if (sqlSelectExpression.Query != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Query = ");
                sqlSelectExpression.Query?.Accept(this);
            });
        }

        if (sqlSelectExpression.OrderBy.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("OrderBy = ");
                sqlSelectExpression.OrderBy.Accept(this);
            });
        }

        if (sqlSelectExpression.Limit != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Limit = ");
                sqlSelectExpression.Limit.Accept(this);
            });
        }

        if (isFirst)
        {
            AppendLine("};");
        }
        else
        {
            AppendLine("},");
        }

        return sqlSelectExpression;
    }
    public override SqlExpression VisitSqlSelectItemExpression(SqlSelectItemExpression sqlSelectItemExpression)
    {
        AdvanceNext(() =>
        {
            AppendLine("new SqlSelectItemExpression()");
            AppendLine("{");
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlSelectItemExpression.Body?.Accept(this);
            });
            if (sqlSelectItemExpression.Alias != null)
            {
                AdvanceNext(() =>
                {
                    AppendAndNotRequiredNextSpace("Alias = ");
                    sqlSelectItemExpression.Alias?.Accept(this);
                });
            }
            AppendLine("},");
        });
        return sqlSelectItemExpression;
    }

    private void AdvanceNext(Action action)
    {
        numberOfLevels++;
        action();
        numberOfLevels--;
    }
    public override SqlExpression VisitSqlSelectQueryExpression(SqlSelectQueryExpression sqlSelectQueryExpression)
    {
        AppendLine("new SqlSelectQueryExpression()");
        AppendLine("{");

        if (sqlSelectQueryExpression.WithSubQuerys.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("WithSubQuerys = new List<SqlWithSubQueryExpression>()");
                AppendLine("{");
                foreach (var item in sqlSelectQueryExpression.WithSubQuerys)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }

        if (sqlSelectQueryExpression.Columns.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Columns = new List<SqlSelectItemExpression>()");
                AppendLine("{");
                foreach (var column in sqlSelectQueryExpression.Columns)
                {
                    column.Accept(this);
                }
                AppendLine("},");
            });
        }

        if (sqlSelectQueryExpression.ResultSetReturnOption != null)
        {
            AdvanceNext(() =>
            {
                AppendLine($"ResultSetReturnOption = SqlResultSetReturnOption.{sqlSelectQueryExpression.ResultSetReturnOption.ToString()},");
            });
        }



        if (sqlSelectQueryExpression.Into != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Into = ");
                sqlSelectQueryExpression.Into.Accept(this);
            });
        }

        if (sqlSelectQueryExpression.Top != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Top = ");
                sqlSelectQueryExpression.Top.Accept(this);
            });
        }

        if (sqlSelectQueryExpression.From != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("From = ");
                sqlSelectQueryExpression.From.Accept(this);
            });
        }
        if (sqlSelectQueryExpression.Where != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Where = ");
                sqlSelectQueryExpression.Where.Accept(this);
            });
        }
        if (sqlSelectQueryExpression.OrderBy.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("OrderBy = ");
                sqlSelectQueryExpression.OrderBy.Accept(this);
            });
        }
        if (sqlSelectQueryExpression.GroupBy.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("GroupBy = ");
                sqlSelectQueryExpression.GroupBy.Accept(this);
            });
        }
        if (sqlSelectQueryExpression.Limit != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Limit = ");
                sqlSelectQueryExpression.Limit.Accept(this);
            });
        }

        if (sqlSelectQueryExpression.ConnectBy != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("ConnectBy = ");
                sqlSelectQueryExpression.ConnectBy.Accept(this);
            });
        }

        if (sqlSelectQueryExpression.Hints.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Hints = new List<SqlHintExpression>()");
                AppendLine("{");
                foreach (var hint in sqlSelectQueryExpression.Hints)
                {
                    AdvanceNext(() =>
                    {
                        hint.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }

        AppendLine("},");
        return sqlSelectQueryExpression;
    }

    private void AppendLine(string str)
    {
        if (addSpace)
        {
            for (int i = 0; i < numberOfLevels; i++)
            {
                sb.Append(fourSpace);
            }
        }
        else
        {
            addSpace = true;
        }

        sb.Append(str + "\r\n");
    }

    private void AppendAndNotRequiredNextSpace(string str)
    {
        if (addSpace)
        {
            for (int i = 0; i < numberOfLevels; i++)
            {
                sb.Append(fourSpace);
            }
        }
        sb.Append(str);
        this.addSpace = false;
    }

    private void Append(string str)
    {
        if (addSpace)
        {
            for (int i = 0; i < numberOfLevels; i++)
            {
                sb.Append(fourSpace);
            }
        }
        sb.Append(str);
    }
    public override SqlExpression VisitSqlStringExpression(SqlStringExpression sqlStringExpression)
    {
        AppendLine("new SqlStringExpression()");
        AppendLine("{");
        if (sqlStringExpression.IsUniCode)
        {
            AdvanceNext(() =>
            {
                AppendLine($"IsUniCode = true,");
            });
        }
        AdvanceNext(() =>
        {
            AppendLine($"Value = \"{sqlStringExpression.Value}\",");
        });
        if (sqlStringExpression.Collate != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Collate = ");
                sqlStringExpression.Collate?.Accept(this);
            });
        }
        AppendLine("},");
        return sqlStringExpression;
    }

    public override SqlExpression VisitSqlBoolExpression(SqlBoolExpression sqlBoolExpression)
    {
        AppendLine("new SqlBoolExpression()");
        AppendLine("{");

        AdvanceNext(() =>
        {
            AppendLine($"Value = {sqlBoolExpression.Value.ToString().ToLowerInvariant()}");
        });
        AppendLine("},");
        return sqlBoolExpression;
    }

    public override SqlExpression VisitSqlTableExpression(SqlTableExpression sqlTableExpression)
    {
        AppendLine("new SqlTableExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            AppendAndNotRequiredNextSpace("Name = ");
            sqlTableExpression.Name?.Accept(this);
            if (sqlTableExpression.Database != null)
            {
                AppendAndNotRequiredNextSpace("Database = ");
                sqlTableExpression.Database?.Accept(this);
            }
            if (sqlTableExpression.Schema != null)
            {
                AppendAndNotRequiredNextSpace("Schema = ");
                sqlTableExpression.Schema?.Accept(this);
            }
            if (sqlTableExpression.DbLink != null)
            {
                AppendAndNotRequiredNextSpace("DbLink = ");
                sqlTableExpression.DbLink?.Accept(this);
            }
        });
        if (sqlTableExpression.Alias != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Alias = ");
                sqlTableExpression.Alias?.Accept(this);
            });
        }

        if (sqlTableExpression.Hints.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Hints = new List<SqlHintExpression>()");
                AppendLine("{");
                foreach (var hint in sqlTableExpression.Hints)
                {
                    AdvanceNext(() =>
                    {
                        hint.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }
        AppendLine("},");
        return sqlTableExpression;
    }
    public override SqlExpression VisitSqlUnionQueryExpression(SqlUnionQueryExpression sqlUnionQueryExpression)
    {
        AppendLine("new SqlUnionQueryExpression()");
        AppendLine("{");


        if (sqlUnionQueryExpression.Left != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Left = ");
                sqlUnionQueryExpression.Left.Accept(this);
            });
        }

        if (sqlUnionQueryExpression.UnionType != null)
        {
            AdvanceNext(() =>
            {
                AppendLine($"UnionType = SqlUnionType.{sqlUnionQueryExpression.UnionType.ToString()},");
            });
        }

        if (sqlUnionQueryExpression.Right != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Right = ");
                sqlUnionQueryExpression.Right.Accept(this);
            });
        }
        AppendLine("},");
        return sqlUnionQueryExpression;
    }
    public override SqlExpression VisitSqlUpdateExpression(SqlUpdateExpression sqlUpdateExpression)
    {
        var isFirst = numberOfLevels == 0;
        if (isFirst)
        {
            sb.Append("var expect = ");
        }
        AppendLine("new SqlUpdateExpression()");
        AppendLine("{");

        if (sqlUpdateExpression.WithSubQuerys.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("WithSubQuerys = new List<SqlWithSubQueryExpression>()");
                AppendLine("{");
                foreach (var item in sqlUpdateExpression.WithSubQuerys)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });
                }
                AppendLine("},");
            });
        }

        if (sqlUpdateExpression.Table != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Table = ");
                sqlUpdateExpression.Table.Accept(this);
            });
        }

        if (sqlUpdateExpression.From != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("From = ");
                sqlUpdateExpression.From.Accept(this);
            });
        }
        if (sqlUpdateExpression.Where != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Where = ");
                sqlUpdateExpression.Where.Accept(this);
            });
        }
        if (sqlUpdateExpression.Items.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Items = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var item in sqlUpdateExpression.Items)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }
        if (isFirst)
        {
            AppendLine("};");
        }
        else
        {
            AppendLine("},");
        }

        return sqlUpdateExpression;
    }
    public override SqlExpression VisitSqlVariableExpression(SqlVariableExpression sqlVariableExpression)
    {

        AppendLine("new SqlVariableExpression()");
        AppendLine("{");


        if (!string.IsNullOrWhiteSpace(sqlVariableExpression.Name))
        {
            AdvanceNext(() =>
            {
                AppendLine($"Name = \"{sqlVariableExpression.Name}\",");
            });
        }

        if (!string.IsNullOrWhiteSpace(sqlVariableExpression.Prefix))
        {
            AdvanceNext(() =>
            {
                AppendLine($"Prefix = \"{sqlVariableExpression.Prefix}\",");
            });
        }
        if (sqlVariableExpression.Collate != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Collate = ");
                sqlVariableExpression.Collate?.Accept(this);
            });
        }
        AppendLine("},");
        return sqlVariableExpression;
    }
    public override SqlExpression VisitSqlWithinGroupExpression(SqlWithinGroupExpression sqlWithinGroupExpression)
    {
        AppendLine("new SqlWithinGroupExpression()");
        AppendLine("{");


        if (sqlWithinGroupExpression.OrderBy.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("OrderBy = ");
                sqlWithinGroupExpression.OrderBy.Accept(this);
            });
        }

        AppendLine("},");
        return sqlWithinGroupExpression;
    }
    public override SqlExpression VisitSqlWithSubQueryExpression(SqlWithSubQueryExpression sqlWithSubQueryExpression)
    {

        AppendLine("new SqlWithSubQueryExpression()");
        AppendLine("{");


        if (sqlWithSubQueryExpression.Alias != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Alias = ");
                sqlWithSubQueryExpression.Alias.Accept(this);
            });
        }
        if (sqlWithSubQueryExpression.FromSelect != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("FromSelect = ");
                sqlWithSubQueryExpression.FromSelect.Accept(this);
            });
        }

        if (sqlWithSubQueryExpression.Columns.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Columns = new List<SqlIdentifierExpression>()");
                AppendLine("{");
                foreach (var item in sqlWithSubQueryExpression.Columns)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }
        AppendLine("},");
        return sqlWithSubQueryExpression;
    }

    public override SqlExpression VisitSqlTopExpression(SqlTopExpression sqlTopExpression)
    {
        AppendLine("new SqlTopExpression()");
        AppendLine("{");

        if (sqlTopExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlTopExpression.Body.Accept(this);
            });
        }

        AppendLine("},");
        return sqlTopExpression;
    }

    public override SqlExpression VisitSqlHintExpression(SqlHintExpression sqlHintExpression)
    {
        AppendLine("new SqlHintExpression()");
        AppendLine("{");

        if (sqlHintExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlHintExpression.Body.Accept(this);
            });
        }

        AppendLine("},");
        return sqlHintExpression;
    }

    public override SqlExpression VisitSqlAtTimeZoneExpression(SqlAtTimeZoneExpression sqlAtTimeZoneExpression)
    {
        AppendLine("new SqlAtTimeZoneExpression()");
        AppendLine("{");

        if (sqlAtTimeZoneExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlAtTimeZoneExpression.Body.Accept(this);
            });
        }

        if (sqlAtTimeZoneExpression.TimeZone != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("TimeZone = ");
                sqlAtTimeZoneExpression.TimeZone.Accept(this);
            });
        }

        AppendLine("},");
        return sqlAtTimeZoneExpression;
    }

    public override SqlExpression VisitSqlIntervalExpression(SqlIntervalExpression sqlIntervalExpression)
    {
        AppendLine("new SqlIntervalExpression()");
        AppendLine("{");

        if (sqlIntervalExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlIntervalExpression.Body.Accept(this);
            });
        }

        if (sqlIntervalExpression.Unit != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Unit = ");
                sqlIntervalExpression.Unit.Accept(this);
            });
        }

        AppendLine("},");
        return sqlIntervalExpression;
    }

    public override SqlExpression VisitSqlTimeUnitExpression(SqlTimeUnitExpression sqlTimeUnitExpression)
    {
        AppendLine("new SqlTimeUnitExpression()");
        AppendLine("{");

        if (!string.IsNullOrWhiteSpace(sqlTimeUnitExpression.Unit))
        {
            AdvanceNext(() =>
            {
                AppendLine($"Unit = \"{sqlTimeUnitExpression.Unit}\"");
            });
        }

        AppendLine("},");
        return sqlTimeUnitExpression;
    }

    public override SqlExpression VisitSqlCollateExpression(SqlCollateExpression sqlCollateExpression)
    {
        AppendLine("new SqlCollateExpression()");
        AppendLine("{");

        if (sqlCollateExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlCollateExpression.Body.Accept(this);
            });
        }

        AppendLine("},");
        return sqlCollateExpression;
    }

    public override SqlExpression VisitSqlRegexExpression(SqlRegexExpression sqlRegexExpression)
    {
        AppendLine("new SqlRegexExpression()");
        AppendLine("{");

        if (sqlRegexExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlRegexExpression.Body.Accept(this);
            });
        }

        if (sqlRegexExpression.RegEx != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("RegEx = ");
                sqlRegexExpression.RegEx.Accept(this);
            });
        }

        if (sqlRegexExpression.Collate != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Collate = ");
                sqlRegexExpression.Collate.Accept(this);
            });
        }


        AdvanceNext(() =>
        {
            var boolStr = sqlRegexExpression.IsCaseSensitive ? "true" : "false";
            AppendLine($"IsCaseSensitive = {boolStr},");
        });

        AppendLine("},");
        return sqlRegexExpression;
    }

    public override SqlExpression VisitSqlReturningExpression(SqlReturningExpression sqlReturningExpression)
    {
        if (!sqlReturningExpression.HasValue())
        {
            return sqlReturningExpression;
        }
        AppendLine("new SqlReturningExpression()");
        AppendLine("{");
        if (sqlReturningExpression.Items.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Items = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var item in sqlReturningExpression.Items)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }
        if (sqlReturningExpression.IntoVariables.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("IntoVariables = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var item in sqlReturningExpression.IntoVariables)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }
        AppendLine("},");
        return sqlReturningExpression;
    }

    public override SqlExpression VisitSqlArrayExpression(SqlArrayExpression sqlArrayExpression)
    {
        if (!sqlArrayExpression.HasValue())
        {
            return sqlArrayExpression;
        }
        AppendLine("new SqlArrayExpression()");
        AppendLine("{");
        if (sqlArrayExpression.Items.HasValue())
        {
            AdvanceNext(() =>
            {
                AppendLine("Items = new List<SqlExpression>()");
                AppendLine("{");
                foreach (var item in sqlArrayExpression.Items)
                {
                    AdvanceNext(() =>
                    {
                        item.Accept(this);
                    });

                }
                AppendLine("},");
            });
        }

        AppendLine("},");
        return sqlArrayExpression;
    }

    public override SqlExpression VisitSqlArrayIndexExpression(SqlArrayIndexExpression sqlArrayIndexExpression)
    {
        if (sqlArrayIndexExpression.Body == null || sqlArrayIndexExpression.Index == null)
        {
            return sqlArrayIndexExpression;
        }

        AppendLine("new SqlArrayIndexExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            AppendAndNotRequiredNextSpace("Body = ");
            sqlArrayIndexExpression.Body.Accept(this);
        });
        AdvanceNext(() =>
        {
            AppendAndNotRequiredNextSpace("Index = ");
            sqlArrayIndexExpression.Index.Accept(this);
        });
        AppendLine("},");
        return sqlArrayIndexExpression;
    }

    public override SqlExpression VisitSqlArraySliceExpression(SqlArraySliceExpression sqlArraySliceExpression)
    {
        if (sqlArraySliceExpression.Body == null)
        {
            return sqlArraySliceExpression;
        }

        AppendLine("new SqlArraySliceExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            AppendAndNotRequiredNextSpace("Body = ");
            sqlArraySliceExpression.Body.Accept(this);
        });
        if (sqlArraySliceExpression.StartIndex != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("StartIndex = ");
                sqlArraySliceExpression.StartIndex.Accept(this);
            });
        }
        if (sqlArraySliceExpression.EndIndex != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("EndIndex = ");
                sqlArraySliceExpression.EndIndex.Accept(this);
            });
        }
        AppendLine("},");
        return sqlArraySliceExpression;
    }
}