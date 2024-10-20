﻿using System;
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

    public string GetResult()
    {
        return sb.ToString();
    }
    public override void VisitSqlAllColumnExpression(SqlAllColumnExpression sqlAllColumnExpression)
    {
        AppendLine("new SqlAllColumnExpression()");
    }
    public override void VisitSqlAllExpression(SqlAllExpression sqlAllExpression)
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
    }
    public override void VisitSqlAnyExpression(SqlAnyExpression sqlAnyExpression)
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
    }
    public override void VisitSqlBetweenAndExpression(SqlBetweenAndExpression sqlBetweenAndExpression)
    {
        AppendLine("new SqlBetweenAndExpression()");
        AppendLine("{");
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
    }
    public override void VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
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
        AppendLine("},");
    }
    public override void VisitSqlCaseExpression(SqlCaseExpression sqlCaseExpression)
    {
        AppendLine("new SqlCaseExpression()");
        AppendLine("{");

        if (sqlCaseExpression.Items != null)
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
    }
    public override void VisitSqlCaseItemExpression(SqlCaseItemExpression sqlCaseItemExpression)
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
    }
    public override void VisitSqlDeleteExpression(SqlDeleteExpression sqlDeleteExpression)
    {
        var isFirst = numberOfLevels == 0;
        if (isFirst)
        {
            sb.Append("var expect = ");
        }
        AppendLine("new SqlDeleteExpression()");
        AppendLine("{");


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
    }
    public override void VisitSqlExistsExpression(SqlExistsExpression sqlExistsExpression)
    {
        AppendLine("new SqlExistsExpression()");
        AppendLine("{");


        if (sqlExistsExpression.Body != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Body = ");
                sqlExistsExpression.Body.Accept(this);
            });
        }

        AppendLine("},");
    }

    public override void VisitSqlFunctionCallExpression(SqlFunctionCallExpression sqlFunctionCallExpression)
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
        AppendLine("},");
    }
    public override void VisitSqlGroupByExpression(SqlGroupByExpression sqlGroupByExpression)
    {
        AppendLine("new SqlGroupByExpression()");
        AppendLine("{");
        if (sqlGroupByExpression.Items != null)
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
    }
    public override void VisitSqlIdentifierExpression(SqlIdentifierExpression sqlIdentifierExpression)
    {
       Append(sqlIdentifierExpression.Value);
    }
    public override void VisitSqlInExpression(SqlInExpression sqlInExpression)
    {
        AppendLine("new SqlInExpression()");
        AppendLine("{");


        if (sqlInExpression.Field != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Field = ");
                sqlInExpression.Field.Accept(this);
            });
        }


        if (sqlInExpression.TargetList != null)
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
        AppendLine("},");
    }
    public override void VisitSqlInsertExpression(SqlInsertExpression sqlInsertExpression)
    {
        var isFirst = numberOfLevels == 0;
        if (isFirst)
        {
            sb.Append("var expect = ");
        }

        AppendLine("new SqlInsertExpression()");
        AppendLine("{");
        if (sqlInsertExpression.Columns != null)
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
        if (sqlInsertExpression.ValuesList != null)
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


        if (isFirst)
        {
            AppendLine("};");
        }
        else
        {
            AppendLine("},");
        }
    }
    public override void VisitSqlJoinTableExpression(SqlJoinTableExpression sqlJoinTableExpression)
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
    }
    public override void VisitSqlLimitExpression(SqlLimitExpression sqlLimitExpression)
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
    }
    public override void VisitSqlNotExpression(SqlNotExpression sqlNotExpression)
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
    }
    public override void VisitSqlNullExpression(SqlNullExpression sqlNullExpression)
    {
        AppendLine("new SqlNullExpression()");
    }
    public override void VisitSqlNumberExpression(SqlNumberExpression sqlNumberExpression)
    {
        AppendLine("new SqlNumberExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            AppendLine($"Value = {sqlNumberExpression.Value}M");
        });
        AppendLine("},");
    }
    public override void VisitSqlOrderByExpression(SqlOrderByExpression sqlOrderByExpression)
    {
        AppendLine("new SqlOrderByExpression()");
        AppendLine("{");
        if (sqlOrderByExpression.Items != null)
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

        AppendLine("},");
    }
    public override void VisitSqlOrderByItemExpression(SqlOrderByItemExpression sqlOrderByItemExpression)
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
                AppendLine($"OrderByType = SqlOrderByType.{sqlOrderByItemExpression.OrderByType?.ToString()}");

            });
        }
        AppendLine("},");
    }
    public override void VisitSqlOverExpression(SqlOverExpression sqlOverExpression)
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
        if (sqlOverExpression.OrderBy != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("OrderBy = ");
                sqlOverExpression.OrderBy.Accept(this);
            });
        }
        AppendLine("},");
    }
    public override void VisitSqlPartitionByExpression(SqlPartitionByExpression sqlPartitionByExpression)
    {
        AppendLine("new SqlPartitionByExpression()");
        AppendLine("{");


        if (sqlPartitionByExpression.Items != null)
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
    }
    public override void VisitSqlPivotTableExpression(SqlPivotTableExpression sqlPivotTableExpression)
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
    }
    public override void VisitSqlPropertyExpression(SqlPropertyExpression sqlPropertyExpression)
    {

        if (sqlPropertyExpression.Table != null)
        {
            sqlPropertyExpression.Table?.Accept(this);
            sb.Append(".");
        }

        if (sqlPropertyExpression.Name != null)
        {
            sqlPropertyExpression.Name?.Accept(this);
        }
      
    }
    public override void VisitSqlReferenceTableExpression(SqlReferenceTableExpression sqlReferenceTableExpression)
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
        AppendLine("}");
    }
    public override void VisitSqlSelectExpression(SqlSelectExpression sqlSelectExpression)
    {
        var isFirst = numberOfLevels == 0;
        if (isFirst)
        {
            sb.Append("var sql = ");
        }

       

        if (sqlSelectExpression.Alias != null)
        {
         
        }

        if (sqlSelectExpression.Query != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Query = ");
                sqlSelectExpression.Query?.Accept(this);
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
    }
    public override void VisitSqlSelectItemExpression(SqlSelectItemExpression sqlSelectItemExpression)
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
    }

    private void AdvanceNext(Action action)
    {
        Append(" ( ");
        action();
        Append(" ) ");
    }
    public override void VisitSqlSelectQueryExpression(SqlSelectQueryExpression sqlSelectQueryExpression)
    {
      

        if (sqlSelectQueryExpression.WithSubQuerys != null)
        {
            foreach (var item in sqlSelectQueryExpression.WithSubQuerys)
            {
                item.Accept(this);
            }
        }

        Append("select ");

        if (sqlSelectQueryExpression.ResultSetReturnOption != null)
        {
            Append(sqlSelectQueryExpression.ResultSetReturnOption.ToString());
        }

        if (sqlSelectQueryExpression.Columns != null)
        {
            foreach (var column in sqlSelectQueryExpression.Columns)
            {
                column.Accept(this);
            }
        }

        if (sqlSelectQueryExpression.Into != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Into = ");
                sqlSelectQueryExpression.Into.Accept(this);
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
        if (sqlSelectQueryExpression.OrderBy != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("OrderBy = ");
                sqlSelectQueryExpression.OrderBy.Accept(this);
            });
        }
        if (sqlSelectQueryExpression.GroupBy != null)
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
        AppendLine("},");

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
    public override void VisitSqlStringExpression(SqlStringExpression sqlStringExpression)
    {
        Append($"'{sqlStringExpression.Value}'");
    }
    public override void VisitSqlTableExpression(SqlTableExpression sqlTableExpression)
    {
        sqlTableExpression.Name?.Accept(this);
        if (sqlTableExpression.Alias != null)
        {
            Append(" as ");
            sqlTableExpression.Alias?.Accept(this);
        }
  
    }
    public override void VisitSqlUnionQueryExpression(SqlUnionQueryExpression sqlUnionQueryExpression)
    {
    
        AppendLine("(");


        if (sqlUnionQueryExpression.Left != null)
        {
            sqlUnionQueryExpression.Left.Accept(this);
        }

        if (sqlUnionQueryExpression.UnionType != null)
        {
          Append($" {sqlUnionQueryExpression.UnionType.ToString()} ");
        }

        if (sqlUnionQueryExpression.Right != null)
        {
            sqlUnionQueryExpression.Right.Accept(this);
        }
        AppendLine(")");
    }
    public override void VisitSqlUpdateExpression(SqlUpdateExpression sqlUpdateExpression)
    {
        var isFirst = numberOfLevels == 0;
        if (isFirst)
        {
            sb.Append("var expect = ");
        }
        AppendLine("new SqlUpdateExpression()");
        AppendLine("{");


        if (sqlUpdateExpression.Table != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Table = ");
                sqlUpdateExpression.Table.Accept(this);
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
        if (sqlUpdateExpression.Items != null)
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
    }
    public override void VisitSqlVariableExpression(SqlVariableExpression sqlVariableExpression)
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
        AppendLine("},");
    }
    public override void VisitSqlWithinGroupExpression(SqlWithinGroupExpression sqlWithinGroupExpression)
    {
        
        if (sqlWithinGroupExpression.OrderBy != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("OrderBy = ");
                sqlWithinGroupExpression.OrderBy.Accept(this);
            });
        }

        AppendLine("},");
    }
    public override void VisitSqlWithSubQueryExpression(SqlWithSubQueryExpression sqlWithSubQueryExpression)
    {
        Append(" with ");
        if (sqlWithSubQueryExpression.Alias != null)
        {
            sqlWithSubQueryExpression.Alias.Accept(this);
        }

        if (sqlWithSubQueryExpression.Columns != null)
        {
            Append("( ");
            foreach (var item in sqlWithSubQueryExpression.Columns)
            {
                item.Accept(this);

            }
            Append(" ) ");
        }

        Append(" as ");
        if (sqlWithSubQueryExpression.FromSelect != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("FromSelect = ");
              
            });
        }


        AppendLine("},");
    }
}