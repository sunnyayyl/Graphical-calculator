namespace Calculator;

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