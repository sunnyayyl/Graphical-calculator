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
    }

    public class EofToken : IToken
    {
        public string Literal { get; private set; } = "\0";
        public TokenType Type { get; private set; } = TokenType.Eof;

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

        public ParenthesisToken(string tokenLiteral)
        {
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

    public class VariableToken(string tokenLiteral) : IToken
    {
        public string Literal { get; private set; } = tokenLiteral;
        public TokenType Type { get; private set; } = TokenType.Variable;
    }

    public class InvalidToken(string literal) : IToken
    {
        public string Literal { get; private set; } = literal;
        public TokenType Type { get; private set; } = TokenType.Invalid;


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

        public OperatorToken(string tokenLiteral)
        {
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

    public class NumberToken(string tokenLiteral) : IToken
    {
        public string Literal { get; private set; } = tokenLiteral;
        public TokenType Type { get; private set; } = TokenType.Number;

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
                return this.Input[this.CurrentIndex + 1];
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

                return new NumberToken(result);
            }
            else if (this.Is(TokenType.Operator))
            {
                var op = this.CurrentCharacter;
                this.NextToken();
                return new OperatorToken(op.ToString());
            }
            else if (this.CurrentIndex >= this.Input.Length)
            {
                return new EofToken();
            }
            else if (this.CurrentCharacter == '(' || this.CurrentCharacter == ')')
            {
                var token = this.CurrentCharacter.ToString();
                this.NextToken();
                return new ParenthesisToken(token);
            }
            else if (char.IsAsciiLetter(this.CurrentCharacter) &&
                     !char.IsAsciiLetter(this.Peek())) // only single letter variable allowed
            {
                var name = this.CurrentCharacter.ToString();
                this.NextToken();
                return new VariableToken(name);
            }
            else
            {
                var literal = this.CurrentCharacter;
                this.NextToken();
                return new InvalidToken(literal.ToString());
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

    public class Parser // FIXME: A lot of duplicate code
    {
        public List<IToken> Tokens { get; private set; }
        public int CurrentIndex { get; private set; }
        public IToken CurrentToken { get; private set; }
        private int _parenthesisLevel;

        public Parser(List<IToken> tokens)
        {
            this.CurrentToken = new InvalidToken("\0"); // To make my IDE stop shouting at me about null
            this.Tokens = tokens;
            CurrentIndex = -1;
            this.NextToken();
        }

        public Parser(Lexer lexer)
        {
            this.CurrentToken = new InvalidToken("\0"); // To make my IDE stop shouting at me about null
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
                return this.Tokens[this.CurrentIndex + 1];
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
                throw new Exception($"Unexpected token type: {this.CurrentToken.Type}");
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

                var op = (OperatorToken)this.Peek();
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
                    var peekOp = (OperatorToken)this.Peek();
                    rhs = this.ParseToken(rhs,
                        (int)op.Precedence +
                        (peekOp.Precedence > op.Precedence ? 1 : 0), level: level);
                }

                lhs = new Infix(lhs, op.Operator, rhs);
            }

            return lhs;
        }

        public IExpression Parse()
        {
            this.CurrentIndex = -1; //reset
            this.NextToken();
            //if (this.CurrentToken.Type != TokenType.Number)
            //{
            //    throw new Exception("Notation have to start with a number");
            //}
            return this.ParseToken(this.ParsePrimary());
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("Equation to be evaluated: ");
            var input = Console.ReadLine();
            if (input == null)
            {
                throw new Exception("Null input");
            }

            //var lexer = new Lexer("1+(2+((x+4)+5))");
            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var result = parser.Parse();
            Console.WriteLine(result);
            var variables = new Dictionary<char, int>();
            foreach (var i in result.ListVariables(new List<Variable>()))
            {
                Console.Write($"{i} = ");
                var v = Console.ReadLine();
                int vint;
                if (v == null || !int.TryParse(v, out vint))
                {
                    throw new Exception("Invalid input");
                }
                else
                {
                    variables.Add(i.Name, vint);
                }
            }

            Console.WriteLine(
                result.Eval(
                    variables
                )
            );
        }
    }
}