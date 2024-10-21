namespace Calculator;

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
            else if (this._peek != null && this._peek.Type == TokenType.Invalid)
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
        else if (this.Is(TokenType.Operator) && ((OperatorToken)this.CurrentToken).Operator == Operators.Minus &&
                 !this.PeekIs(TokenType.Operator))
        {
            this.NextToken();
            return new Infix(new Number(-1), Operators.Multiply, this.ParsePrimary());
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
        else if (this.CurrentIndex < this.Tokens.Count - 2)
        {
            throw new SyntaxError(TokenType.Eof, this.CurrentToken.End + 1, this.CurrentToken.End + 1,
                this.Tokens[this.CurrentIndex + 1].Type);
        }


        return result;
    }
}