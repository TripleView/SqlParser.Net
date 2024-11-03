namespace SqlParser.Net.Lexer;

public struct Token
{
    public string Name { get; }

    public object Value { get; set; }
    /// <summary>
    /// Used to determine whether the token is a keyword
    /// 用来判断token是否为关键字
    /// </summary>
    public bool IsKeyWord { get; }
    /// <summary>
    /// Used to indicate that the token is removed during the parsing process and does not participate in the parsing
    /// 用来表示token在解析过程中被移除，不参与解析
    /// </summary>
    public bool IsRemove { get; set; }
    /// <summary>
    /// Used for fast comparison between two tokens, because comparison between numbers is 10 times faster than comparison between strings.
    /// 用来进行2个token之间的快速比较，因为数字之间的比较比字符串之间的比较快10倍
    /// </summary>
    public int CompareIndex { get; set; }
    /// <summary>
    /// Token start position index
    /// token起始位置索引
    /// </summary>
    public int StartPositionIndex { get; set; }
    /// <summary>
    /// Token end position index
    /// token结束位置索引
    /// </summary>
    public int EndPositionIndex { get; set; }
    /// <summary>
    /// Left Qualifiers
    /// 左限定符
    /// </summary>
    public string LeftQualifiers { get; set; }
    /// <summary>
    /// right Qualifiers
    /// 右限定符
    /// </summary>
    public string RightQualifiers { get; set; }
    public Token(string name, object value, int compareIndex, bool isKeyWord = true)
    {
        Name = name;
        Value = value;
        IsKeyWord = isKeyWord;
        CompareIndex = compareIndex;
    }

    public bool IsToken(Token otherToken)
    {
        return this.CompareIndex == otherToken.CompareIndex;
    }

    public override string ToString()
    {
        return $"{(IsKeyWord ? "Keyword-" + Name : Name),-20} | {Value,-5}";
    }


    public override bool Equals(object obj)
    {
        if (obj is Token otherToken)
        {
            return otherToken.CompareIndex == this.CompareIndex && otherToken.Value == this.Value;
        }

        return false;
    }

    public static readonly Token Select = new Token("Select", "Select", 1);
    public static readonly Token Delete = new Token("Delete", "Delete", 2);
    public static readonly Token Insert = new Token("Insert", "Insert", 3);
    public static readonly Token Update = new Token("Update", "Update", 4);
    public static readonly Token Having = new Token("Having", "Having", 5);
    public static readonly Token Where = new Token("Where", "Where", 6);
    public static readonly Token Order = new Token("Order", "Order", 7);
    public static readonly Token By = new Token("By", "By", 8);
    public static readonly Token Group = new Token("Group", "Group", 9);
    public static readonly Token As = new Token("As", "As", 10);
    public static readonly Token Null = new Token("Null", "Null", 11);
    public static readonly Token Not = new Token("Not", "Not", 12);
    public static readonly Token Distinct = new Token("Distinct", "Distinct", 13);
    public static readonly Token From = new Token("From", "From", 14);
    public static readonly Token Create = new Token("Create", "Create", 15);
    public static readonly Token Alter = new Token("Alter", "Alter", 16);
    public static readonly Token Drop = new Token("Drop", "Drop", 17);
    public static readonly Token Set = new Token("Set", "Set", 18);
    public static readonly Token Into = new Token("Into", "Into", 19);
    public static readonly Token View = new Token("View", "View", 20);
    public static readonly Token Index = new Token("Index", "Index", 21);
    public static readonly Token Union = new Token("Union", "Union", 22);
    public static readonly Token Left = new Token("Left", "Left", 23);
    public static readonly Token Inner = new Token("Inner", "Inner", 24);
    public static readonly Token Right = new Token("Right", "Right", 25);
    public static readonly Token Full = new Token("Full", "Full", 26);
    public static readonly Token Outer = new Token("Outer", "Outer", 27);
    public static readonly Token Cross = new Token("Cross", "Cross", 28);
    public static readonly Token Join = new Token("Join", "Join", 29);
    public static readonly Token On = new Token("On", "On", 30);
    public static readonly Token Cast = new Token("Cast", "Cast", 31);
    public static readonly Token And = new Token("And", "And", 32);
    public static readonly Token Or = new Token("Or", "Or", 33);
    public static readonly Token Xor = new Token("Xor", "Xor", 34);
    public static readonly Token Case = new Token("Case", "Case", 35);
    public static readonly Token When = new Token("When", "When", 36);
    public static readonly Token Then = new Token("Then", "Then", 37);
    public static readonly Token Else = new Token("Else", "Else", 38);
    public static readonly Token ElseIf = new Token("ElseIf", "ElseIf", 39);
    public static readonly Token End = new Token("End", "End", 40);
    public static readonly Token Asc = new Token("Asc", "Asc", 41);
    public static readonly Token Desc = new Token("Desc", "Desc", 42);
    public static readonly Token Is = new Token("Is", "Is", 43);
    public static readonly Token Like = new Token("Like", "Like", 44);
    public static readonly Token In = new Token("In", "In", 45);
    public static readonly Token Between = new Token("Between", "Between", 46);
    public static readonly Token Values = new Token("Values", "Values", 47);
    public static readonly Token Over = new Token("Over", "Over", 48);
    public static readonly Token Partition = new Token("Partition", "Partition", 49);
    //mysql
    public static readonly Token True = new Token("True", "True", 50);
    public static readonly Token False = new Token("False", "False", 51);
    public static readonly Token Limit = new Token("Limit", "Limit", 52);
    public static readonly Token Identified = new Token("Identified", "Identified", 53);
    public static readonly Token Password = new Token("Password", "Password", 54);
    //oracle
    public static readonly Token Unique = new Token("Unique", "Unique", 56);
    public static readonly Token First = new Token("First", "First", 57);
    //sql server
    public static readonly Token Offset = new Token("Offset", "Offset", 58);
    public static readonly Token Rows = new Token("Rows", "Rows", 59);
    public static readonly Token Fetch = new Token("Fetch", "Fetch", 60);
    public static readonly Token Next = new Token("Next", "Next", 61);
    public static readonly Token Only = new Token("Only", "Only", 62);
    //operator
    public static readonly Token LeftParen = new Token("LeftParen", "(", 63);
    public static readonly Token RightParen = new Token("RightParen", ")", 64);
    public static readonly Token LeftCurlyBrackets = new Token("LeftCurlyBrackets", "{", 65);
    public static readonly Token RightCurlyBrackets = new Token("RightCurlyBrackets", "}", 66);
    public static readonly Token LeftSquareBracket = new Token("LeftSquareBracket", "[", 67);
    public static readonly Token RightSquareBracket = new Token("RightSquareBracket", "]", 68);
    public static readonly Token Semicolon = new Token("Semicolon", ";", 69);
    public static readonly Token Comma = new Token("Comma", ",", 70);

    public static readonly Token Dot = new Token("Dot", ".", 71);
    public static readonly Token At = new Token("At", "@", 72);
    public static readonly Token EqualTo = new Token("EqualTo", "=", 73);
    public static readonly Token GreaterThen = new Token("GreaterThen", ">", 74);
    public static readonly Token LessThen = new Token("LessThen", "<", 75);
    public static readonly Token Bang = new Token("Bang", "!", 76);
    public static readonly Token Colon = new Token("Colon", ":", 77);
    public static readonly Token NotEqualTo = new Token("NotEqualTo", "!=", 78);
    public static readonly Token GreaterThenOrEqualTo = new Token("GreaterThenOrEqualTo", ">=", 79);
    public static readonly Token LessThenOrEqualTo = new Token("LessThenOrEqualTo", "<=", 80);
    public static readonly Token Exists = new Token("Exists", "Exists", 81);
    public static readonly Token Plus = new Token("Plus", "+", 82);
    public static readonly Token Sub = new Token("Sub", "-", 83);
    public static readonly Token Star = new Token("Star", "*", 84);
    public static readonly Token Slash = new Token("Slash", "/", 85);
    public static readonly Token With = new Token("With", "With", 86);
    public static readonly Token All = new Token("All", "All", 87);
    public static readonly Token Intersect = new Token("Intersect", "Intersect", 88);
    public static readonly Token Except = new Token("Except", "Except", 89);
    public static readonly Token Minus = new Token("Minus", "Minus", 90);
    public static readonly Token Any = new Token("Any", "Any", 91);
    public static readonly Token LineBreak = new Token("LineBreak", "\n", 92);
    public static readonly Token DoubleQuotes = new Token("DoubleQuotes", "\"", 93);

    public static readonly Token LineComment = new Token("LineComment", "LineComment", 94);
    public static readonly Token MultiLineComment = new Token("MultiLineComment", "MultiLineComment", 95);
    /// <summary>
    /// Identifier
    /// 标识符
    /// </summary>
    public static readonly Token IdentifierString = new Token("IdentifierString", "IdentifierString", 96, false);
    /// <summary>
    /// Number Constant
    /// 数字常量
    /// </summary>
    public static readonly Token NumberConstant = new Token("NumberConstant", 0, 97, false);
    /// <summary>
    /// String Constant
    /// 字符串常量
    /// </summary>
    public static readonly Token StringConstant = new Token("StringConstant", "StringConstant", 98, false);
    /// <summary>
    /// Backtick
    /// 单反引号
    /// </summary>
    public static readonly Token Backtick = new Token("Backtick", "`", 99);

    /// <summary>
    /// BarBar,||
    /// 连接符||
    /// </summary>
    public static readonly Token BarBar = new Token("BarBar", "||", 100);
    /// <summary>
    ///Bitwise OR |
    /// 按位或|
    /// </summary>
    public static readonly Token Bar = new Token("Bar", "|", 101);

    public static readonly Token Within = new Token("Within", "Within", 102);

    public static readonly Token Pivot = new Token("Pivot", "Pivot", 103);

    public static readonly Token For = new Token("For", "For", 104);

    public static readonly Token BitwiseAnd = new Token("BitwiseAnd", "&", 105);
    public static readonly Token BitwiseXor = new Token("BitwiseXor", "^", 106);
    



}