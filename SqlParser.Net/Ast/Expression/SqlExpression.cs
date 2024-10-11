namespace SqlParser.Net.Ast.Expression;

public  class SqlExpression
{
    public virtual SqlExpressionType Type { get; protected set; }

    public SqlExpression()
    {
        
    }

}