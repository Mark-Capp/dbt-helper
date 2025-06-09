// DELETE THIS CONTENT IF YOU PUT COMBINED GRAMMAR IN Parser TAB
lexer grammar JinjaLexer;

COMMA : ',';
INT: '-'? DIGIT+ ;
DOUBLE: '-'? DIGIT+ '.' DIGIT+
    | '-'? '.' DIGIT+
    ;

STRING : '\'' (ESC|.)*? '\'' ;
BOOL : TRUE | FALSE;

ADD : '+';
SUB : '-';
MUL : '*';
DIV : '/';
NOT : '!';
TRUE : 'True';
FALSE : 'False';
EQ: '==';
NEQ: '!=';
LPAREN: '(';
RPAREN: ')';
EQUALS: '=';
RSQBRACKET : ']';
LSQBRACKET : '[';

EXPRESSION_START: '{{';
EXPRESSION_END: '}}';

AND: '&&';
OR: '||';
GT : '>';
LT : '<';
GTEQ : '>=';
LTEQ : '<=';

IF_WORD: 'if';
ELIF: 'elif';
ENDIF: 'endif';
BLOCK_START: '{%';
BLOCK_END: '%}';
ELSE : 'else';
WHILE : 'while';
END_WHILE: '{% endwhile %}';
SET_BLOCK: '{% set';
MACRO: 'macro';
END_MACRO: 'endmacro';
FOR: 'for';
END_FOR: 'endfor';
IN: 'in';



ID: ([a-zA-Z]) ([a-zA-Z] | [0-9] | '_')* ;
WS: [ \t]->skip;
NEWLINE: [\r\n]+;
COMMENT: '{#' .*? '#}' NEWLINE ->skip;
SYMBOLS: ('_'  | '/' | ';' | '="' | '"');
TEXT : ([a-zA-Z0-9()\\/"';.*,] | SYMBOLS |NEWLINE | [ \t])*? NEWLINE;

fragment
ESC: '\\"'|'\\\\';
fragment
DIGIT: [0-9];
