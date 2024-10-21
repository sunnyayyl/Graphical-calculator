namespace Calculator;

public static class Global
{
    public static Dictionary<string, Operators> OperatorsMap { get; } = new Dictionary<string, Operators>
    {
        { "+", Operators.Plus },
        { "-", Operators.Minus },
        { "*", Operators.Multiply },
        { "/", Operators.Divide },
        { "^", Operators.Power },
    };

    public static Dictionary<Operators, uint> PrecedenceMap { get; } = new Dictionary<Operators, uint>
    {
        { Operators.Plus, 1 },
        { Operators.Minus, 1 },
        { Operators.Multiply, 2 },
        { Operators.Divide, 2 },
        { Operators.Power, 3 }
    };

    public static Dictionary<Operators, Direction> AssociativeMap { get; } = new Dictionary<Operators, Direction>
    {
        { Operators.Plus, Direction.Left },
        { Operators.Minus, Direction.Left },
        { Operators.Multiply, Direction.Left },
        { Operators.Divide, Direction.Left },
        { Operators.Power, Direction.Right }
    };
}

public enum Direction
{
    Left,
    Right
}

public enum ErrorType
{
    Syntax,
    InvalidToken,
}

public enum Operators
{
    Plus,
    Minus,
    Multiply,
    Divide,
    Power,
}

public enum TokenType
{
    Invalid,
    Eof,
    Number,
    Operator,
    Parenthesis,
    Variable,
}

public interface IToken
{
    string Literal { get; }
    TokenType Type { get; }

    int Start { get; }
    int End { get; }
}

public class EofToken(int start, int end) : IToken
{
    public string Literal { get; private set; } = "\0";
    public TokenType Type { get; private set; } = TokenType.Eof;
    public int Start { get; private set; } = start;
    public int End { get; private set; } = end;

    public override string ToString()
    {
        return "EOFToken{}";
    }
}

public class ParenthesisToken : IToken
{
    public string Literal { get; private set; }
    public Direction Associative { get; private set; }
    public uint Precedence { get; private set; } = 5;
    public TokenType Type { get; private set; } = TokenType.Parenthesis;
    public int Start { get; private set; }
    public int End { get; private set; }

    public ParenthesisToken(string tokenLiteral, int start, int end)
    {
        this.Start = start;
        this.End = end;
        this.Literal = tokenLiteral;
        switch (this.Literal)
        {
            case "(":
                this.Associative = Direction.Left;
                break;
            case ")":
                this.Associative = Direction.Right;
                break;
            default:
                throw new Exception("Invalid parenthesis token");
        }
    }
}

public class VariableToken(string tokenLiteral, int start, int end) : IToken
{
    public string Literal { get; private set; } = tokenLiteral;
    public TokenType Type { get; private set; } = TokenType.Variable;
    public int Start { get; private set; } = start;
    public int End { get; private set; } = end;
}

public class InvalidToken(string literal, int start, int end) : IToken
{
    public string Literal { get; private set; } = literal;
    public TokenType Type { get; private set; } = TokenType.Invalid;
    public int Start { get; private set; } = start;
    public int End { get; private set; } = end;

    public override string ToString()
    {
        return "InvalidToken{Literal=" + this.Literal + "}";
    }
}

public class OperatorToken : IToken
{
    public string Literal { get; private set; }
    public Operators Operator { get; private set; }
    public uint Precedence { get; private set; }
    public Direction Associative { get; private set; }
    public TokenType Type { get; private set; } = TokenType.Operator;
    public int Start { get; private set; }
    public int End { get; private set; }

    public OperatorToken(string tokenLiteral, int start, int end)
    {
        this.Start = start;
        this.End = end;
        this.Literal = tokenLiteral;
        this.Operator = Global.OperatorsMap[this.Literal];
        this.Precedence = Global.PrecedenceMap[this.Operator];
        this.Associative = Global.AssociativeMap[this.Operator];
    }

    public override string ToString()
    {
        return $"OperatorToken{{Operator={this.Operator.ToString()}, Precedence={this.Precedence.ToString()}}}";
    }
}

public class NumberToken(string tokenLiteral, int start, int end) : IToken
{
    public string Literal { get; private set; } = tokenLiteral;
    public TokenType Type { get; private set; } = TokenType.Number;
    public int Start { get; private set; } = start;
    public int End { get; private set; } = end;

    public override string ToString()
    {
        return "NumberToken{Literal = " + Literal + "}";
    }
}

public interface IExpression
{
    double Eval(Dictionary<char, double> variables);
    List<Variable> ListVariables(List<Variable> variables);
}

public class Number : IExpression
{
    public double Value { get; private set; }

    public Number(double value)
    {
        this.Value = value;
    }

    public override string ToString()
    {
        return this.Value.ToString();
    }

    public double Eval(Dictionary<char, double> variables)
    {
        return this.Value;
    }

    public List<Variable> ListVariables(List<Variable> variables)
    {
        return variables;
    }
}

public class Variable : IExpression
{
    public char Name { get; private set; }

    public Variable(char name)
    {
        this.Name = name;
    }

    public double Eval(Dictionary<char, double> variables)
    {
        return variables[this.Name];
    }

    public override string ToString()
    {
        return this.Name.ToString();
    }

    public List<Variable> ListVariables(List<Variable> variables)
    {
        variables.Add(this);
        return variables;
    }
}

public class Infix : IExpression
{
    public IExpression Lhs { get; private set; }
    public Operators Op { get; private set; }
    public IExpression Rhs { get; private set; }

    public Infix(IExpression lhs, Operators op, IExpression rhs)
    {
        this.Lhs = lhs;
        this.Op = op;
        this.Rhs = rhs;
    }

    public override string ToString()
    {
        switch (this.Op)
        {
            case (Operators.Plus):
                return "(" + Lhs + " + " + Rhs + ")";
            case (Operators.Minus):
                return "(" + Lhs + " - " + Rhs + ")";
            case (Operators.Multiply):
                return "(" + Lhs + " * " + Rhs + ")";
            case (Operators.Divide):
                return "(" + Lhs + " / " + Rhs + ")";
            case (Operators.Power):
                return "(" + Lhs + " ^ " + Rhs + ")";
            default:
                throw new NotSupportedException($"Operator {this.Op} is not supported");
        }
    }

    public double Eval(Dictionary<char, double> variables)
    {
        switch (this.Op)
        {
            case (Operators.Plus):
                return this.Lhs.Eval(variables) + this.Rhs.Eval(variables);
            case (Operators.Minus):
                return this.Lhs.Eval(variables) - this.Rhs.Eval(variables);
            case (Operators.Multiply):
                return this.Lhs.Eval(variables) * this.Rhs.Eval(variables);
            case (Operators.Divide):
                return this.Lhs.Eval(variables) / this.Rhs.Eval(variables);
            case (Operators.Power):
                return Math.Pow(this.Lhs.Eval(variables), this.Rhs.Eval(variables));
            default:
                throw new NotSupportedException($"Operator {this.Op} is not supported");
        }
    }

    public List<Variable> ListVariables(List<Variable> variables)
    {
        return this.Lhs.ListVariables(new List<Variable>()).Concat(this.Rhs.ListVariables(new List<Variable>()))
            .Concat(variables).ToList();
    }
}

public interface IErrorMessage
{
    public int Start { get; }
    public int End { get; }
    string Message { get; }
    public ErrorType ErrorType { get; }
}

[Serializable]
public class InvalidTokenError : Exception, IErrorMessage
{
    public int Start { get; private set; }
    public int End { get; private set; }
    public ErrorType ErrorType { get; private set; } = ErrorType.InvalidToken;
    public string StringLiteral { get; private set; }

    public InvalidTokenError(int start, int end, string stringLiteral)
    {
        this.Start = start;
        this.End = end;
        this.StringLiteral = stringLiteral;
    }

    public override string Message
    {
        get
        {
            if (this.Start == this.End)
            {
                return $"Invalid token at character {this.Start}";
            }
            else
            {
                return $"Invalid token at character {this.Start} to {this.End}";
            }
        }
    }

    protected InvalidTokenError(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context)
    {
    }
}

[Serializable]
public class SyntaxError : Exception, IErrorMessage
{
    public List<TokenType> Expected { get; private set; }
    public int Start { get; private set; }
    public int End { get; private set; }
    public TokenType? Type { get; private set; }
    public ErrorType ErrorType { get; private set; } = ErrorType.Syntax;

    public SyntaxError(TokenType expected, IToken got)
    {
        this.Expected = new List<TokenType> { expected };
        this.Start = got.Start;
        this.End = got.End;
        this.Type = got.Type;
    }

    public SyntaxError(List<TokenType> expected, IToken got)
    {
        this.Expected = expected;
        this.Start = got.Start;
        this.End = got.End;
        this.Type = got.Type;
    }

    public SyntaxError(TokenType expected, int start, int end, TokenType got)
    {
        this.Expected = new List<TokenType> { expected };
        this.Start = start;
        this.End = end;
        this.Type = got;
    }

    public SyntaxError(List<TokenType> expected, int start, int end, TokenType got)
    {
        this.Expected = expected;
        this.Start = start;
        this.End = end;
        this.Type = got;
    }

    public override string Message
    {
        get
        {
            if (this.Start == this.End)
            {
                return
                    $"Syntax error: Expected {string.Join(", ", Expected)}, got {this.Type} at character {this.Start}";
            }
            else
            {
                return
                    $"Syntax error: Expected {string.Join(", ", Expected)}, got {this.Type} at character {this.Start} to {this.End}";
            }
        }
    }

    protected SyntaxError(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context)
    {
    }
}