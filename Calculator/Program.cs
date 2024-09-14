
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
    public class Token(TokenType tokenType, string tokenLiteral)
    {
        public TokenType Type { get; private set; }=tokenType;
        public string Literal { get; private set; }=tokenLiteral;

        public override string ToString()
        {
            return "Token{Type = "+Type+", Literal = "+Literal+"}";
        }

        public Operators? GetOperator()
        {
            if (Global.OperatorsMap.TryGetValue(Literal, out var op))
            {
                return op;
            }
            else
            {
                return null;
            }
        }

        public uint? GetPrecedence()
        {
            var op = this.GetOperator();
            if (op != null)
            {
                return Global.PrecedenceMap[op.Value];
            }
            else
            {
                return null;
            }
        }
    }
    public class Lexer // FIXME: Cannot be generalized as there is neither Union or a interface Indexable type? 
    {
        public string Input { get; private set; }
        public char CurrentCharater { get; private set; }
        public int CurrentIndex {get; private set;}

        public Lexer(string input)
        {
            this.Input = input;
            this.CurrentIndex = -1;
            this.NextToken();
        }

        public bool NextToken()
        {
            if (this.CurrentIndex+1 < this.Input.Length)
            {
                this.CurrentIndex++;
                this.CurrentCharater=this.Input[this.CurrentIndex];
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
            }else
            {
                return '\0';
            }
        }

        public bool Is(TokenType tt, char obj='\0')
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
                return tt==TokenType.Operator;
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
                return new Token(TokenType.Number, result);
            }else if (this.Is(TokenType.Operator))
            {
                var op=this.CurrentCharater;
                this.NextToken();
                return new Token(TokenType.Operator, op.ToString());
            } else if (this.CurrentIndex >= this.Input.Length)
            {
                return new Token(TokenType.EOF, "\0");
            }
            else
            {
                var literal=this.CurrentCharater;
                this.NextToken();
                return new Token(TokenType.Invalid, literal.ToString());
            }
            
        }

        public List<Token> ParseAll()
        {
            // Reset
            this.CurrentIndex = -1;
            this.NextToken();
            var tokens=new List<Token>();
            tokens.Add(this.ParseToken());
            while (tokens[^1].Type!=TokenType.EOF)
            {
                tokens.Add(this.ParseToken());
            }

            return tokens;
        }
    }

    public class Notation
    {
    }

    public class Number : Notation
    {
        public int Value { get; set; }

        public Number(int value)
        {
            this.Value = value;
        }
    }
    public class Inflix:Notation
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
            return "("+Lhs+" "+Op+" "+Rhs+")";
        }
    }
    public class Parser // FIXME: A lot of duplicate code
    {
        public List<Token> Tokens{get; private set;}
        public int CurrentIndex {get; private set;}
        public Token CurrentToken {get; private set;}
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
            if (this.CurrentIndex+1 < this.Tokens.Count)
            {
                this.CurrentIndex++;
                this.CurrentToken=this.Tokens[this.CurrentIndex];
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
            }else
            {
                throw new Exception("Unexpected end of file"); // Peek should not be called when Token is already EOF
            }
        }
        public bool Is(TokenType t)
        {
            return CurrentToken.Type == t;
        }

        public Notation ParseToken(Notation lhs, int minPrecedence=0) // https://en.wikipedia.org/wiki/Operator-precedence_parser
        {
           while (this.Peek().Type==TokenType.Operator&&this.Peek().GetPrecedence() >= minPrecedence)
           {
               this.NextToken();
               Console.WriteLine(this.CurrentToken);
               var op=this.CurrentToken;
               this.NextToken();
               Debug.Assert(this.CurrentToken.Type==TokenType.Number);
               Notation rhs=new Number(int.Parse(this.CurrentToken.Literal));
               while (this.Peek().Type==TokenType.Operator&&this.Peek().GetPrecedence()>op.GetPrecedence())
               {
                   rhs=this.ParseToken(rhs,(int)op.GetPrecedence().Value+(this.Peek().GetPrecedence().Value>op.GetPrecedence().Value?1:0));
                   //this.NextToken();
               }

               return new Inflix(lhs, op.GetOperator().Value, rhs);
           }
           throw new Exception("Unexpected token");
        }

        public Notation Parse()
        {
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
            var lexer = new Lexer("100+2*3");
            var tokens = lexer.ParseAll();
            for (var i = 0; i < tokens.Count; i++)
            {
                Console.WriteLine(tokens[i]);
            }

            var parser = new Parser(lexer);
            Console.WriteLine(parser.Parse());
        }
    }
}

