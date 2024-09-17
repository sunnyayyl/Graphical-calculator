using System.Diagnostics;
using Calculator;

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
        EOF,
        Number,
        Operator,
        Parenthesis,
        Variable,
    }

    public interface Token
    {
        string Literal { get; }
        TokenType Type { get; }
    }

    public class EOFToken : Token
    {
        public string Literal { get; private set; } = "\0";
        public TokenType Type { get; private set; } = TokenType.EOF;

        public override string ToString()
        {
            return "EOFToken{}";
        }
    }

    public class ParenthesisToken : Token
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

    public class VariableToken : Token
    {
        public string Literal { get; private set; }
        public TokenType Type { get; private set; } = TokenType.Variable;

        public VariableToken(string tokenLiteral)
        {
            this.Literal = tokenLiteral;
        }
    }

    public class InvalidToken : Token
    {
        public string Literal { get; private set; }
        public TokenType Type { get; private set; } = TokenType.Invalid;

        public InvalidToken(string literal)
        {
            this.Literal = literal;
        }

        public override string ToString()
        {
            return "InvalidToken{Literal=" + this.Literal + "}";
        }
    }

    public class OperatorToken : Token
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

    public class NumberToken : Token
    {
        public string Literal { get; private set; }
        public int Value { get; private set; }
        public TokenType Type { get; private set; } = TokenType.Number;

        public NumberToken(string tokenLiteral)
        {
            this.Literal = tokenLiteral;
            this.Value = int.Parse(tokenLiteral);
        }

        public NumberToken(int tokenValue)
        {
            this.Literal = tokenValue.ToString();
            this.Value = tokenValue;
        }

        public override string ToString()
        {
            return "NumberToken{Literal = " + Literal + "}";
        }
    }

    public class Lexer // FIXME: Cannot be generalized as there is neither Union or a interface Indexable type? 
    {
        public string Input { get; private set; }
        public char CurrentCharater { get; private set; }
        public int CurrentIndex { get; private set; }

        public Lexer(string input)
        {
            this.Input = input;
            this.CurrentIndex = -1;
            this.NextToken();
        }

        public bool NextToken()
        {
            if (this.CurrentIndex + 1 < this.Input.Length)
            {
                this.CurrentIndex++;
                this.CurrentCharater = this.Input[this.CurrentIndex];
                return true;
            }
            else
            {
                this.CurrentIndex++;
                this.CurrentCharater = '\0';
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
            return _is(this.CurrentCharater.ToString(), tt);
        }

        public bool PeekIs(TokenType tt)
        {
            return _is(this.Peek().ToString(), tt);
        }

        public Token ParseToken()
        {
            var result = "";
            if (this.Is(TokenType.Number))
            {
                result = this.CurrentCharater.ToString();
                this.NextToken();
                while (this.Is(TokenType.Number))
                {
                    result += this.CurrentCharater;
                    this.NextToken();
                }

                return new NumberToken(result);
            }
            else if (this.Is(TokenType.Operator))
            {
                var op = this.CurrentCharater;
                this.NextToken();
                return new OperatorToken(op.ToString());
            }
            else if (this.CurrentIndex >= this.Input.Length)
            {
                return new EOFToken();
            }
            else if (this.CurrentCharater == '(' || this.CurrentCharater == ')')
            {
                var token = this.CurrentCharater.ToString();
                this.NextToken();
                return new ParenthesisToken(token);
            }
            else if (char.IsAsciiLetter(this.CurrentCharater) &&
                     !char.IsAsciiLetter(this.Peek())) // only single letter variable allowed
            {
                var name = this.CurrentCharater.ToString();
                this.NextToken();
                return new VariableToken(name);
            }
            else
            {
                var literal = this.CurrentCharater;
                this.NextToken();
                return new InvalidToken(literal.ToString());
            }
        }

        public List<Token> ParseAll()
        {
            // Reset
            this.CurrentIndex = -1;
            this.NextToken();
            var tokens = new List<Token>();
            tokens.Add(this.ParseToken());
            while (tokens[^1].Type != TokenType.EOF)
            {
                tokens.Add(this.ParseToken());
            }

            return tokens;
        }
    }

    public interface INotation
    {
        double Eval(Dictionary<char, int> variables);
    }

    public class Number : INotation
    {
        public int Value { get; private set; }

        public Number(int value)
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
    }

    public class Variable : INotation
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
    }

    public class Inflix : INotation
    {
        public INotation Lhs { get; private set; }
        public Operators Op { get; private set; }
        public INotation Rhs { get; private set; }

        public Inflix(INotation lhs, Operators op, INotation rhs)
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
                    return Math.Pow((double)this.Lhs.Eval(variables), (double)this.Rhs.Eval(variables));
                default:
                    throw new NotSupportedException($"Operator {this.Op} is not supported");
            }
        }
    }

    public class Parser // FIXME: A lot of duplicate code
    {
        public List<Token> Tokens { get; private set; }
        public int CurrentIndex { get; private set; }
        public Token CurrentToken { get; private set; }
        private int _parenthesis_level = 0;

        public Parser(List<Token> tokens)
        {
            this.Tokens = tokens;
            CurrentIndex = -1;
            this.NextToken();
        }

        public Parser(Lexer lexer)
        {
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

        public Token Peek()
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

        private INotation ParsePrimary()
        {
            if (this.Is(TokenType.Number))
            {
                return new Number(int.Parse(this.CurrentToken.Literal));
            }
            else if (this.Is(TokenType.Parenthesis))
            {
                this.NextToken();
                this._parenthesis_level++;
                return this.ParseToken(this.ParsePrimary(), inParenthesis: true, level: this._parenthesis_level);
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

        private INotation
            ParseToken(INotation lhs, int minPrecedence = 0,
                bool inParenthesis = false, int level = 0) // https://en.wikipedia.org/wiki/Operator-precedence_parser
        {
            while (this.PeekIs(TokenType.Parenthesis) || this.PeekIs(TokenType.Operator) &&
                   ((OperatorToken)this.Peek()).Precedence >= minPrecedence)
            {
                if (inParenthesis && this.PeekIs(TokenType.Parenthesis) &&
                    ((ParenthesisToken)this.Peek()).Associative == Direction.Right)
                {
                    if (!inParenthesis)
                    {
                        throw new Exception("Mismatched parenthesis");
                    }

                    this.NextToken();
                    this._parenthesis_level--;
                    break;
                }
                else if (this._parenthesis_level > level && inParenthesis)
                {
                    this._parenthesis_level--;
                    break;
                }

                var op = (OperatorToken)this.Peek();
                this.NextToken();
                this.NextToken();
                // Debug.Assert(this.CurrentToken.Type == TokenType.Number);
                //INotation rhs = new Number(int.Parse(this.CurrentToken.Literal));
                var rhs = this.ParsePrimary();
                while (this.PeekIs(TokenType.Operator) &&
                       ((((OperatorToken)this.Peek()).Precedence > op.Precedence) ||
                        (((OperatorToken)this.Peek()).Precedence == op.Precedence &&
                         ((OperatorToken)this.Peek()).Associative == Direction.Right)))
                {
                    var peekOp = (OperatorToken)this.Peek();
                    rhs = this.ParseToken(rhs,
                        (int)op.Precedence +
                        (peekOp.Precedence > op.Precedence ? 1 : 0), inParenthesis: inParenthesis, level: level);
                }

                lhs = new Inflix(lhs, op.Operator, rhs);
            }

            return lhs;
        }

        public INotation Parse()
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
            var lexer = new Lexer("1+(2+((x+4)+5))");
            //var lexer = new Lexer("x+2");
            // var tokens = lexer.ParseAll();
            var parser = new Parser(lexer);
            var result = parser.Parse();
            Console.WriteLine(result);
            Console.WriteLine(
                result.Eval(
                    new Dictionary<char, int>
                    {
                        { 'x', 3 }
                    }
                )
            );
        }
    }
}