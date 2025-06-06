using System.Diagnostics;


namespace DbtHelper.Jinja;

internal class Lexer
{
    internal Lexer(string s)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(s));

        _s = s;
        _l = s.Length;
    }

    internal Token GetToken(int i)
    {
        var start = SkipWhitespaces(i);

        switch (CharAt(start))
        {
            case '{':
                return CharAt(start + 1) switch
                {
                    '%' => new Token(start, start + 2, TokenType.StatementBegin),
                    '{' => new Token(start, start + 2, TokenType.ExpressionBegin),
                    _ => Token.Invalid(start)
                };

            case '%':
                return CharAt(start + 1) switch
                {
                    '}' => new Token(start, start + 2, TokenType.StatementEnd),
                    _ => new Token(start, start + 1, TokenType.Percent)
                };

            case '}':
                return CharAt(start + 1) switch
                {
                    '}' => new Token(start, start + 2, TokenType.ExpressionEnd),
                    _ => Token.Invalid(start)
                };

            case '+':
                return new Token(start, start + 1, TokenType.Plus);

            case '-':
                return new Token(start, start + 1, TokenType.Minus);

            case '|':
                return new Token(start, start + 1, TokenType.Pipe);

            case '(':
                return new Token(start, start + 1, TokenType.LeftParenthesis);

            case ')':
                return new Token(start, start + 1, TokenType.RightParenthesis);

            case '[':
                return new Token(start, start + 1, TokenType.LeftSquareBracket);

            case ']':
                return new Token(start, start + 1, TokenType.RightSquareBracket);

            case '.':
                return CharAt(start + 1) switch
                {
                    var c when char.IsDigit(c) => GetDotStartedFloat(start),
                    _ => new Token(start, start + 1, TokenType.Dot)
                };

            case '*':
                return new Token(start, start + 1, TokenType.Star);

            case '/':
                return CharAt(start + 1) switch
                {
                    '/' => new Token(start, start + 2, TokenType.DoubleSlash),
                    _ => new Token(start, start + 1, TokenType.Slash)
                };

            case '<':
                return CharAt(start + 1) switch
                {
                    '=' => new Token(start, start + 2, TokenType.LessEqual),
                    _ => new Token(start, start + 1, TokenType.Less)
                };

            case '=':
                return CharAt(start + 1) switch
                {
                    '=' => new Token(start, start + 2, TokenType.DoubleEqual),
                    _ => Token.Invalid(start)
                };

            case '>':
                return CharAt(start + 1) switch
                {
                    '=' => new Token(start, start + 2, TokenType.GreaterEqual),
                    _ => new Token(start, start + 1, TokenType.Greater)
                };

            case '!':
                return CharAt(start + 1) switch
                {
                    '=' => new Token(start, start + 2, TokenType.ExclamationEqual),
                    _ => Token.Invalid(start)
                };

            case '\'':
                return GetString(start);

            case ',':
                return new Token(start, start + 1, TokenType.Comma);

            case '_':
            case var c when char.IsLetter(c):
                return GetSymbolOrKeyword(start);

            case var c when char.IsDigit(c):
                return GetIntegerOrFloat(start);
        }

        return Token.Invalid(start);
    }

    private Token GetString(int start)
    {
        Debug.Assert(CharAt(start) == '\'');

        var i = start + 1;
        while (CharAt(i) != '\0' && CharAt(i) != '\'')
            i++;
        return new Token(start, i + 1, TokenType.String);
    }

    private Token GetDotStartedFloat(int start)
    {
        Debug.Assert(CharAt(start) == '.');

        var i = start + 1;
        while (char.IsDigit(CharAt(i)))
            i++;
        return new Token(start, i, TokenType.Float);
    }

    private Token GetIntegerOrFloat(int start)
    {
        Debug.Assert(char.IsDigit(CharAt(start)));

        var i = start + 1;
        while (char.IsDigit(CharAt(i)))
            i++;

        if (CharAt(i) != '.' || !char.IsDigit(CharAt(i + 1)))
            return new Token(start, i, TokenType.Integer);
        
        i++;
        while (char.IsDigit(CharAt(i)))
            i++;
        return new Token(start, i, TokenType.Float);

    }

    private Token GetSymbolOrKeyword(int start)
    {
        Debug.Assert(CharAt(start) == '_' || char.IsLetter(CharAt(start)));

        var i = start + 1;
        while (CharAt(i) == '_' || char.IsDigit(CharAt(i)) || char.IsLetter(CharAt(i)))
            i++;

        var str = _s.Substring(start, i - start);
        return str switch
        {
            "if" => new Token(start, i, TokenType.If),
            "elif" => new Token(start, i, TokenType.ElseIf),
            "else" => new Token(start, i, TokenType.Else),
            "endif" => new Token(start, i, TokenType.EndIf),
            "for" => new Token(start, i, TokenType.For),
            "in" => new Token(start, i, TokenType.In),
            "endfor" => new Token(start, i, TokenType.EndFor),
            "not" => new Token(start, i, TokenType.Not),
            "or" => new Token(start, i, TokenType.Or),
            "and" => new Token(start, i, TokenType.And),
            "true" => new Token(start, i, TokenType.True),
            "false" => new Token(start, i, TokenType.False),
            _ => new Token(start, i, TokenType.Symbol)
        };
    }

    private int SkipWhitespaces(int i)
    {
        var e = i;
        while (char.IsWhiteSpace(CharAt(e)))
            e++;
        return e;
    }

    private char CharAt(int i)
    {
        return i >= 0 && i < _l ? _s[i] : '\0';
    }

    private readonly string _s;
    private readonly int _l;
}