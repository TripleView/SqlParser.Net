using System;
using System.Text;
using SqlParser.Net.Ast.Expression;

namespace SqlParser.Net.Ast.Visitor;

public class UnitTestAstVisitor : BaseAstVisitor
{
    private StringBuilder sb = new StringBuilder();
    private string fourSpace = "    ";
    private int numberOfLevels = -1;

    private bool addSpace = true;

    public string GetAst()
    {
        return sb.ToString();
    }
    public override void VisitSqlAllColumnExpression(SqlAllColumnExpression sqlAllColumnExpression)
    {
        AppendLine("new SqlAllColumnExpression()");
    }
    public override void VisitSqlAllExpression(SqlAllExpression sqlAllExpression)
    {

    }
    public override void VisitSqlAnyExpression(SqlAnyExpression sqlAnyExpression)
    {

    }
    public override void VisitSqlBetweenAndExpression(SqlBetweenAndExpression sqlBetweenAndExpression)
    {

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

    }
    public override void VisitSqlCaseItemExpression(SqlCaseItemExpression sqlCaseItemExpression)
    {

    }
    public override void VisitSqlDeleteExpression(SqlDeleteExpression sqlDeleteExpression)
    {

    }
    public override void VisitSqlExistsExpression(SqlExistsExpression sqlExistsExpression)
    {

    }
    public override void VisitSqlExpression(SqlExpression sqlExpression)
    {

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
        AppendLine("}");
    }
    public override void VisitSqlGroupByExpression(SqlGroupByExpression sqlGroupByExpression)
    {

    }
    public override void VisitSqlIdentifierExpression(SqlIdentifierExpression sqlIdentifierExpression)
    {
        AppendLine("new SqlIdentifierExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            AppendLine($"Value = \"{sqlIdentifierExpression.Value}\"");
        });
        AppendLine("},");
    }
    public override void VisitSqlInExpression(SqlInExpression sqlInExpression)
    {

    }
    public override void VisitSqlInsertExpression(SqlInsertExpression sqlInsertExpression)
    {

    }
    public override void VisitSqlJoinTableExpression(SqlJoinTableExpression sqlJoinTableExpression)
    {

    }
    public override void VisitSqlLimitExpression(SqlLimitExpression sqlLimitExpression)
    {

    }
    public override void VisitSqlNotExpression(SqlNotExpression sqlNotExpression)
    {

    }
    public override void VisitSqlNullExpression(SqlNullExpression sqlNullExpression)
    {

    }
    public override void VisitSqlNumberExpression(SqlNumberExpression sqlNumberExpression)
    {
        AppendLine("new SqlNumberExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            AppendLine($"Value = {sqlNumberExpression.Value}");
        });
        AppendLine("},");
    }
    public override void VisitSqlOrderByExpression(SqlOrderByExpression sqlOrderByExpression)
    {

    }
    public override void VisitSqlOrderByItemExpression(SqlOrderByItemExpression sqlOrderByItemExpression)
    {

    }
    public override void VisitSqlOverExpression(SqlOverExpression sqlOverExpression)
    {

    }
    public override void VisitSqlPartitionByExpression(SqlPartitionByExpression sqlPartitionByExpression)
    {

    }
    public override void VisitSqlPivotTableExpression(SqlPivotTableExpression sqlPivotTableExpression)
    {

    }
    public override void VisitSqlPropertyExpression(SqlPropertyExpression sqlPropertyExpression)
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
        AppendLine("},");
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
        var isFirst = numberOfLevels == -1;
        if (isFirst)
        {
            sb.Append("var sqlSelect = ");
        }

        AdvanceNext(() =>
        {
            AppendLine("new SqlSelectExpression()");
            AppendLine("{");
            sqlSelectExpression.Alias?.Accept(this);
            sqlSelectExpression.Query?.Accept(this);
            if (isFirst)
            {
                AppendLine("};");
            }
            else
            {
                AppendLine("}");
            }
        });
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
        numberOfLevels++;
        action();
        numberOfLevels--;
    }
    public override void VisitSqlSelectQueryExpression(SqlSelectQueryExpression sqlSelectQueryExpression)
    {
        AdvanceNext(() =>
        {
            AppendLine("Query = new SqlSelectQueryExpression()");
            AppendLine("{");
            if (sqlSelectQueryExpression.Columns != null)
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
            AppendLine("}");
        });

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
        AppendLine("new SqlStringExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            AppendLine($"Value = \"{sqlStringExpression.Value}\"");
        });
        AppendLine("},");
    }
    public override void VisitSqlTableExpression(SqlTableExpression sqlTableExpression)
    {
        AppendLine("new SqlTableExpression()");
        AppendLine("{");
        AdvanceNext(() =>
        {
            AppendAndNotRequiredNextSpace("Value = ");
            sqlTableExpression.Name?.Accept(this);
        });
        if (sqlTableExpression.Alias != null)
        {
            AdvanceNext(() =>
            {
                AppendAndNotRequiredNextSpace("Alias = ");
                sqlTableExpression.Alias?.Accept(this);
            });
        }
        AppendLine("}");
    }
    public override void VisitSqlUnionQueryExpression(SqlUnionQueryExpression sqlUnionQueryExpression)
    {

    }
    public override void VisitSqlUpdateExpression(SqlUpdateExpression sqlUpdateExpression)
    {

    }
    public override void VisitSqlVariableExpression(SqlVariableExpression sqlVariableExpression)
    {

    }
    public override void VisitSqlWithinGroupExpression(SqlWithinGroupExpression sqlWithinGroupExpression)
    {

    }
    public override void VisitSqlWithSubQueryExpression(SqlWithSubQueryExpression sqlWithSubQueryExpression)
    {

    }
}