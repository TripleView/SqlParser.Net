namespace SqlParser.Net.Lexer;


/// <summary>
/// Whether it is in the context of a merge result set operation
/// 是否在合并结果集操作里的上下文
/// </summary>
public class InTheMergeResultSetOperationContext
{
    /// <summary>
    /// Is it in the merge result set operation
    /// 是否在合并结果集操作里
    /// </summary>
    public bool IsInTheMergeResultSetOperation
    {
        get
        {
            return isInTheMergeResultSetOperation;
        }
        set
        {
            this.isInTheMergeResultSetOperation = value;
            if (value == false)
            {
                InitParenDepth();
            }
        }
    }

    private bool isInTheMergeResultSetOperation = false;
    /// <summary>
    /// Parentheses depth
    /// 圆括号深度
    /// </summary>
    public int ParenDepth { get; private set; }

    public void IncreaseParenDepth()
    {
        this.ParenDepth++;
    }

    private void InitParenDepth()
    {
        this.ParenDepth = 0;
    }

    public bool HasParenDepth => this.ParenDepth > 0;
}