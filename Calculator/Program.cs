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
    }

    public class BetterEnum<TValue>(string name, TValue value)
    {
        public string Name { get; private set; } = name;
        public TValue Value { get; private set; } = value;
        public static bool operator ==(BetterEnum<TValue> a, BetterEnum<TValue> b) => a.Name == b.Name;
        public static bool operator !=(BetterEnum<TValue> a, BetterEnum<TValue> b) => a.Name != b.Name;
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

        public bool Is(TokenType tt, char obj = '\0')
        {
            var compared = (obj == '\0' ? this.CurrentCharater : obj).ToString();
            if (compared == "\0")
            {
                return false;
            }
            else if (int.TryParse(compared, out _))
            {
                return tt == TokenType.Number;
            }
            else if (Global.OperatorsMap.ContainsKey(compared))
            {
                return tt == TokenType.Operator;
            }
            else
            {
                return false;
            }
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

    public interface Notation
    {
        double Eval();
    }

    public class Number : Notation
    {
        public int Value { get; set; }

        public Number(int value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public double Eval()
        {
            return this.Value;
        }
    }

    public class Inflix : Notation
    {
        public Notation Lhs { get; set; }
        public Operators Op { get; set; }
        public Notation Rhs { get; set; }

        public Inflix(Notation lhs, Operators op, Notation rhs)
        {
            this.Lhs = lhs;
            this.Op = op;
            this.Rhs = rhs;
        }

        public override string ToString()
        {
            return "(" + this.Lhs + " " + this.Op + " " + this.Rhs + ")";
        }

        public double Eval()
        {
            switch (this.Op)
            {
                case (Operators.Plus):
                    return this.Lhs.Eval() + this.Rhs.Eval();
                case (Operators.Minus):
                    return this.Lhs.Eval() - this.Rhs.Eval();
                case (Operators.Multiply):
                    return this.Lhs.Eval() * this.Rhs.Eval();
                case (Operators.Divide):
                    return this.Lhs.Eval() / this.Rhs.Eval();
                case (Operators.Power):
                    return Math.Pow((double)this.Lhs.Eval(), (double)this.Rhs.Eval());
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
            return CurrentToken.Type == t;
        }

        public Notation
            ParseToken(Notation lhs, int minPrecedence = 0) // https://en.wikipedia.org/wiki/Operator-precedence_parser
        {
            while (this.Peek().Type == TokenType.Operator && ((OperatorToken)this.Peek()).Precedence >= minPrecedence)
            {
                var op = (OperatorToken)this.Peek();
                this.NextToken();
                this.NextToken();
                Debug.Assert(this.CurrentToken.Type == TokenType.Number);
                Notation rhs = new Number(int.Parse(this.CurrentToken.Literal));
                while (this.Peek().Type == TokenType.Operator &&
                       ((((OperatorToken)this.Peek()).Precedence > op.Precedence) ||
                        (((OperatorToken)this.Peek()).Precedence == op.Precedence &&
                         ((OperatorToken)this.Peek()).Associative == Direction.Right)))
                {
                    var peekOp = (OperatorToken)this.Peek();
                    rhs = this.ParseToken(rhs,
                        (int)op.Precedence +
                        (peekOp.Precedence > op.Precedence ? 1 : 0));
                }

                lhs = new Inflix(lhs, op.Operator, rhs);
            }

            Console.WriteLine(lhs);
            return lhs;
        }

        public Notation Parse()
        {
            this.CurrentIndex = -1; //reset
            this.NextToken();
            if (this.CurrentToken.Type != TokenType.Number)
            {
                throw new Exception("Notation have to start with a number");
            }

            return this.ParseToken(new Number(int.Parse(this.CurrentToken.Literal)));
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var lexer = new Lexer("2*5+2*4^3");
            var tokens = lexer.ParseAll();
            for (var i = 0; i < tokens.Count; i++)
            {
                Console.WriteLine(tokens[i]);
            }

            var parser = new Parser(lexer);
            var result = parser.Parse();
            Console.WriteLine(result);
            Console.WriteLine(result.Eval());
        }
    }
}