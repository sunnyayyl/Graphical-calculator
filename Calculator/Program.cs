namespace Calculator
{
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

    public class Lexer // FIXME: Cannot be generalized as there is neither Union nor interface Indexable type? 
    {
        public string Input { get; private set; }
        public char CurrentCharacter { get; private set; }
        public int CurrentIndex { get; private set; }
        private char _peek;

        public Lexer(string input)
        {
            this.Input = input;
            this.CurrentIndex = -1;
            this.NextToken();
        }

        void ConsumeWhitespaces()
        {
            while (this.CurrentCharacter == ' ' || this.CurrentCharacter == '\t')
            {
                this.NextToken();
            }
        }

        public bool NextToken()
        {
            if (this.CurrentIndex + 1 < this.Input.Length)
            {
                this.CurrentIndex++;
                this.CurrentCharacter = this.Input[this.CurrentIndex];
                if (this.CurrentIndex + 1 < this.Input.Length)
                {
                    this._peek = this.Input[this.CurrentIndex + 1];
                }

                return true;
            }
            else
            {
                this.CurrentIndex++;
                this.CurrentCharacter = '\0';
                return false;
            }
        }

        public char Peek()
        {
            if (this.CurrentIndex + 1 < this.Input.Length)
            {
                return this._peek;
            }
            else
            {
                return '\0';
            }
        }

        private static bool _is(string obj, TokenType tt)
        {
            if (obj == "\0")
            {
                return false;
            }
            else if (int.TryParse(obj, out _))
            {
                return tt == TokenType.Number;
            }
            else if (Global.OperatorsMap.ContainsKey(obj))
            {
                return tt == TokenType.Operator;
            }
            else if (obj == "(" || obj == ")")
            {
                return tt == TokenType.Parenthesis;
            }
            else
            {
                return false;
            }
        }

        public bool Is(TokenType tt)
        {
            return _is(this.CurrentCharacter.ToString(), tt);
        }

        public bool PeekIs(TokenType tt)
        {
            return _is(this.Peek().ToString(), tt);
        }

        public IToken ParseToken()
        {
            this.ConsumeWhitespaces();
            if (this.Is(TokenType.Number))
            {
                var start = this.CurrentIndex;
                var result = this.CurrentCharacter.ToString();
                this.NextToken();
                while (this.Is(TokenType.Number))
                {
                    result += this.CurrentCharacter;
                    this.NextToken();
                }

                if (this.CurrentCharacter == '.')
                {
                    result += this.CurrentCharacter;
                    this.NextToken(); // FIXME: Duplicate code
                    while (this.Is(TokenType.Number))
                    {
                        result += this.CurrentCharacter;
                        this.NextToken();
                    }
                }

                return new NumberToken(result, start, this.CurrentIndex - 1);
            }
            else if (this.Is(TokenType.Operator))
            {
                var op = this.CurrentCharacter;
                this.NextToken();
                return new OperatorToken(op.ToString(), this.CurrentIndex - 2, this.CurrentIndex - 1);
            }
            else if (this.CurrentIndex >= this.Input.Length)
            {
                return new EofToken(this.CurrentIndex, this.CurrentIndex);
            }
            else if (this.CurrentCharacter == '(' || this.CurrentCharacter == ')')
            {
                var token = this.CurrentCharacter.ToString();
                this.NextToken();
                return new ParenthesisToken(token, this.CurrentIndex - 1, this.CurrentIndex - 1);
            }
            else if (char.IsAsciiLetter(this.CurrentCharacter) &&
                     !char.IsAsciiLetter(this.Peek())) // only single letter variable allowed
            {
                var name = this.CurrentCharacter.ToString();
                this.NextToken();
                return new VariableToken(name, this.CurrentIndex - 1, this.CurrentIndex - 1);
            }
            else
            {
                var literal = this.CurrentCharacter;
                this.NextToken();
                return new InvalidToken(literal.ToString(), this.CurrentIndex - 1, this.CurrentIndex - 1);
            }
        }

        public List<IToken> ParseAll()
        {
            // Reset
            this.CurrentIndex = -1;
            this.NextToken();
            var tokens = new List<IToken>();
            tokens.Add(this.ParseToken());
            while (tokens[^1].Type != TokenType.Eof)
            {
                tokens.Add(this.ParseToken());
            }

            return tokens;
        }
    }

    public interface IExpression
    {
        double Eval(Dictionary<char, int> variables);
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

        public double Eval(Dictionary<char, int> variables)
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

        public double Eval(Dictionary<char, int> variables)
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

        public double Eval(Dictionary<char, int> variables)
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

    public class Parser // FIXME: A lot of duplicate code
    {
        public List<IToken> Tokens { get; private set; }
        public int CurrentIndex { get; private set; }
        public IToken CurrentToken { get; private set; }
        private int _parenthesisLevel;
        private IToken _peek;
        private string _literal;

        public Parser(List<IToken> tokens, string literal)
        {
            _literal = literal;
            this.CurrentToken = new InvalidToken("\0", 0, 0); // To make my IDE stop shouting at me about null
            this.Tokens = tokens;
            CurrentIndex = -1;
            this.NextToken();
        }

        public Parser(Lexer lexer)
        {
            _literal = lexer.Input;
            this.CurrentToken = new InvalidToken("\0", 0, 0); // To make my IDE stop shouting at me about null
            this.Tokens = lexer.ParseAll();
            CurrentIndex = -1;
            this.NextToken();
        }

        public void NextToken()
        {
            if (this.CurrentIndex + 1 < this.Tokens.Count)
            {
                this.CurrentIndex++;
                this.CurrentToken = this.Tokens[this.CurrentIndex];
                if (this.CurrentIndex + 1 < this.Tokens.Count)
                {
                    this._peek = this.Tokens[this.CurrentIndex + 1];
                }

                if (this.CurrentToken.Type == TokenType.Invalid)
                {
                    throw new InvalidTokenError(this.CurrentToken.Start, this.CurrentToken.End,
                        ((InvalidToken)this.CurrentToken).Literal);
                }
                else if (this._peek.Type == TokenType.Invalid)
                {
                    throw new InvalidTokenError(this._peek.Start, this._peek.End, ((InvalidToken)this._peek).Literal);
                }
            }
            else
            {
                throw new Exception("Unexpected end of file");
            }
        }

        public IToken Peek()
        {
            if (this.CurrentIndex + 1 < this.Tokens.Count)
            {
                return this._peek;
            }
            else
            {
                throw new Exception("Unexpected end of file"); // Peek should not be called when Token is already EOF
            }
        }

        public bool Is(TokenType t)
        {
            return this.CurrentToken.Type == t;
        }

        public bool PeekIs(TokenType t)
        {
            return this.Peek().Type == t;
        }

        private IExpression ParsePrimary()
        {
            if (this.Is(TokenType.Number))
            {
                return new Number(double.Parse(this.CurrentToken.Literal));
            }
            else if (this.Is(TokenType.Parenthesis))
            {
                this.NextToken();
                this._parenthesisLevel++;
                return this.ParseToken(this.ParsePrimary(), level: this._parenthesisLevel);
            }
            else if (this.Is(TokenType.Variable))
            {
                return new Variable(this.CurrentToken.Literal[0]);
            }
            else
            {
                throw new SyntaxError([TokenType.Number, TokenType.Parenthesis, TokenType.Variable], this.CurrentToken);
            }
        }

        private static T CastOrThrow<T>(IToken token, TokenType expectation) where T : IToken
        {
            if (token is T)
            {
                return (T)token;
            }
            else
            {
                throw new SyntaxError(expectation, token);
            }
        }

        private IExpression
            ParseToken(IExpression lhs, int minPrecedence = 0,
                int level = 0) // https://en.wikipedia.org/wiki/Operator-precedence_parser
        {
            while (this.PeekIs(TokenType.Parenthesis) || this.PeekIs(TokenType.Operator) &&
                   ((OperatorToken)this.Peek()).Precedence >= minPrecedence)
            {
                if (level > 0 && this.PeekIs(TokenType.Parenthesis) &&
                    ((ParenthesisToken)this.Peek()).Associative == Direction.Right)
                {
                    this.NextToken();
                    this._parenthesisLevel--;
                    break;
                }
                else if (level > this._parenthesisLevel)
                {
                    break;
                }

                var op = CastOrThrow<OperatorToken>(this.Peek(), TokenType.Operator);
                this.NextToken();
                this.NextToken();
                // Debug.Assert(this.CurrentToken.Type == TokenType.Number);
                //INotation rhs = new Number(int.Parse(this.CurrentToken.Literal));
                var rhs = this.ParsePrimary();
                while (!(level > this._parenthesisLevel) && this.PeekIs(TokenType.Operator) &&
                       ((((OperatorToken)this.Peek()).Precedence > op.Precedence) ||
                        (((OperatorToken)this.Peek()).Precedence == op.Precedence &&
                         ((OperatorToken)this.Peek()).Associative == Direction.Right)))
                {
                    var peekOp = CastOrThrow<OperatorToken>(this.Peek(), TokenType.Operator);
                    rhs = this.ParseToken(rhs,
                        (int)op.Precedence +
                        (peekOp.Precedence > op.Precedence ? 1 : 0), level: level);
                }

                lhs = new Infix(lhs, op.Operator, rhs);
            }

            return lhs;
        }

        private IExpression ParseAll()
        {
            this.CurrentIndex = -1; //reset
            this.NextToken();
            //if (this.CurrentToken.Type != TokenType.Number)
            //{
            //    throw new Exception("Notation have to start with a number");
            //}
            return this.ParseToken(this.ParsePrimary());
        }

        public IExpression Parse()
        {
            var result = this.ParseAll();
            if (this._parenthesisLevel > 0)
            {
                throw new SyntaxError(TokenType.Parenthesis, this.CurrentToken.End + 1, this.CurrentToken.End + 1,
                    TokenType.Eof);
            }
            else if (this.CurrentIndex < this.Tokens.Count - 1)
            {
                throw new SyntaxError(TokenType.Eof, this.CurrentToken.End + 1, this.CurrentToken.End + 1,
                    this.Tokens[this.CurrentIndex + 1].Type);
            }

            return result;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var inputPrompt = "Equation to be evaluated: ";
            Console.Write(inputPrompt);
            var input = Console.ReadLine()!;

            //var lexer = new Lexer("1+(2+((x+4)+5))");
            var lexer = new Lexer(input);
            Parser parser;
            IExpression result;
            try
            {
                parser = new Parser(lexer);
                result = parser.Parse();
            }
            catch (Exception ex) when (ex is IErrorMessage)
            {
                var e = (IErrorMessage)ex;
                int seperatorLength;
                if ((inputPrompt.Length + input.Length) > e.Message.Length)
                {
                    seperatorLength = inputPrompt.Length + input.Length;
                }
                else
                {
                    seperatorLength = e.Message.Length;
                }

                Console.WriteLine(new string('-', seperatorLength));
                Console.WriteLine(e.Message);
                Console.WriteLine(input);
                Console.WriteLine(new string(' ', e.Start) + new string('^', e.End - e.Start + 1));
                Environment.Exit(1);
                throw;
            }

            Console.WriteLine(result);
            var variables = new Dictionary<char, int>();
            foreach (var i in result.ListVariables(new List<Variable>()))
            {
                Console.Write($"{i} = ");
                var v = Console.ReadLine()!;
                if (!int.TryParse(v, out var vint)!)
                {
                    throw new Exception("Invalid input");
                }
                else
                {
                    variables.Add(i.Name, vint);
                }
            }


            Console.WriteLine(result.Eval(
                variables
            ));
        }
    }
}