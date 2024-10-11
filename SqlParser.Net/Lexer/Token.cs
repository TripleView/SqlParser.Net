namespace SqlParser.Net.Lexer;

public struct Token
{
    public string Name { get; }


    public object Value { get; set; }
    public bool IsKeyWord { get; }
    public Token(string name, object value, bool isKeyWord = true)
    {
        Name = name;
        Value = value;
        IsKeyWord = isKeyWord;
    }

    //public override string ToString()
    //{
    //    return $"{Name}({Value})";
    //}
    public override string ToString()
    {

        return $"{(IsKeyWord ? "Keyword-"+ Name : Name),-20} | {Value,-5}";
    }


    public override bool Equals(object obj)
    {
        if (obj is Token otherToken)
        {
            return otherToken.Name == this.Name && otherToken.Value == this.Value;
        }

        return false;
    }

    public static readonly Token Select = new Token("Select", "Select");

    public static readonly Token Delete = new Token("Delete", "Delete");
    public static readonly Token Insert = new Token("Insert", "Insert");
    public static readonly Token Update = new Token("Update", "Update");
    public static readonly Token Having = new Token("Having", "Having");
    public static readonly Token Where = new Token("Where", "Where");
    public static readonly Token Order = new Token("Order", "Order");
    public static readonly Token By = new Token("By", "By");
    public static readonly Token Group = new Token("Group", "Group");
    public static readonly Token As = new Token("As", "As");
    public static readonly Token Null = new Token("Null", "Null");
    public static readonly Token Not = new Token("Not", "Not");
    public static readonly Token Distinct = new Token("Distinct", "Distinct");
    public static readonly Token From = new Token("From", "From");
    public static readonly Token Create = new Token("Create", "Create");
    public static readonly Token Alter = new Token("Alter", "Alter");
    public static readonly Token Drop = new Token("Drop", "Drop");
    public static readonly Token Set = new Token("Set", "Set");
    public static readonly Token Into = new Token("Into", "Into");
    
    public static readonly Token View = new Token("View", "View");
    public static readonly Token Index = new Token("Index", "Index");
    public static readonly Token Union = new Token("Union", "Union");
    public static readonly Token Left = new Token("Left", "Left");
    public static readonly Token Inner = new Token("Inner", "Inner");
    public static readonly Token Right = new Token("Right", "Right");
    public static readonly Token Full = new Token("Full", "Full");
    public static readonly Token Outer = new Token("Outer", "Outer");
    public static readonly Token Cross = new Token("Cross", "Cross");
    public static readonly Token Join = new Token("Join", "Join");


    public static readonly Token On = new Token("On", "On");
    public static readonly Token Cast = new Token("Cast", "Cast");
    public static readonly Token And = new Token("And", "And");
    public static readonly Token Or = new Token("Or", "Or");
    public static readonly Token Xor = new Token("Xor", "Xor");


    public static readonly Token Case = new Token("Case", "Case");
    public static readonly Token When = new Token("When", "When");
    public static readonly Token Then = new Token("Then", "Then");
    public static readonly Token Else = new Token("Else", "Else");
    public static readonly Token ElseIf = new Token("ElseIf", "ElseIf");

    public static readonly Token End = new Token("End", "End");
    public static readonly Token Asc = new Token("Asc", "Asc");
    public static readonly Token Desc = new Token("Desc", "Desc");
    public static readonly Token Is = new Token("Is", "Is");
    public static readonly Token Like = new Token("Like", "Like");
    public static readonly Token In = new Token("In", "In");
    public static readonly Token Between = new Token("Between", "Between");
    public static readonly Token Values = new Token("Values", "Values");
    public static readonly Token Over = new Token("Over", "Over");
    public static readonly Token Partition = new Token("Partition", "Partition");
    //mysql
    public static readonly Token True = new Token("True", "True");
    public static readonly Token False = new Token("False", "False");
    public static readonly Token Limit = new Token("Limit", "Limit");
    public static readonly Token Identified = new Token("Identified", "Identified");
    public static readonly Token Password = new Token("Password", "Password");
    public static readonly Token Dual = new Token("Dual", "Dual");
    public static readonly Token Unique = new Token("Unique", "Unique");
    //sql server
    public static readonly Token Offset = new Token("Offset", "Offset");
    public static readonly Token Rows = new Token("Rows", "Rows");
    public static readonly Token Fetch = new Token("Fetch", "Fetch");
    public static readonly Token Next = new Token("Next", "Next");
    public static readonly Token Only = new Token("Only", "Only");
    //operator
    public static readonly Token LeftParen = new Token("LeftParen", "(");
    public static readonly Token RightParen = new Token("RightParen", ")");
    public static readonly Token LeftCurlyBrackets = new Token("LeftCurlyBrackets", "{");
    public static readonly Token RightCurlyBrackets = new Token("RightCurlyBrackets", "}");
    public static readonly Token LeftSquareBracket = new Token("LeftSquareBracket", "[");
    public static readonly Token RightSquareBracket = new Token("RightSquareBracket", "]");
    public static readonly Token Semicolon = new Token("Semicolon", ";");
    public static readonly Token Comma = new Token("Comma", ",");

    public static readonly Token Dot = new Token("Dot", ".");
    public static readonly Token At = new Token("At", "@");
    public static readonly Token EqualTo = new Token("EqualTo", "=");
    public static readonly Token GreaterThen = new Token("GreaterThen", ">");
    public static readonly Token LessThen = new Token("LessThen", "<");
    public static readonly Token Bang = new Token("Bang", "!");
    public static readonly Token Colon = new Token("Colon", ":");
    public static readonly Token NotEqualTo = new Token("NotEqualTo", "!=");
    public static readonly Token GreaterThenOrEqualTo = new Token("GreaterThenOrEqualTo", ">=");
    public static readonly Token LessThenOrEqualTo = new Token("LessThenOrEqualTo", "<=");
    public static readonly Token Exists = new Token("Exists", "Exists");
    public static readonly Token Plus = new Token("Plus", "+");
    public static readonly Token Sub = new Token("Sub", "-");
    public static readonly Token Star = new Token("Star", "*");
    public static readonly Token Slash = new Token("Slash", "/");
    public static readonly Token With = new Token("With", "With");
    public static readonly Token All = new Token("All", "All");
    public static readonly Token Intersect = new Token("Intersect", "Intersect");
    public static readonly Token Except = new Token("Except", "Except");
    public static readonly Token Minus = new Token("Minus", "Minus");
    public static readonly Token Any = new Token("Any", "Any");
    public static readonly Token LineBreak = new Token("LineBreak", "\n");

    public static readonly Token LineComment = new Token("LineComment", "LineComment");
    public static readonly Token MultiLineComment = new Token("MultiLineComment", "MultiLineComment");
    /// <summary>
    /// 标识符
    /// </summary>
    public static readonly Token IdentifierString = new Token("IdentifierString", "IdentifierString", false);
    /// <summary>
    /// 数字常量
    /// </summary>
    public static readonly Token NumberConstant = new Token("NumberConstant", 0, false);
    /// <summary>
    /// 字符串常量
    /// </summary>
    public static readonly Token StringConstant = new Token("StringConstant", "StringConstant", false);


}