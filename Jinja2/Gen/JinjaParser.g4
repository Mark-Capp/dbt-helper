parser grammar JinjaParser;
options { tokenVocab=JinjaLexer; }

template 
    : ( text | expression | statement | comment)* EOF
    ;
    
expression
    : OPEN_EXPR expression_body CLOSE_EXPR
    ;
    
expression_body
    : LPARAN expression_body RPARAN #eqParan
    | left = expression_body  operator = (PLUS|MINUS)  right = expression_body        #eqAdd
    | left = expression_body operator = (MUL|DIV) right = expression_body #eqMul
    | SET ID EQUALS expression_body #eqAssign
    | ID #eqID
    | INT #eqINT
    | STRING #eqString
    ;
    
statement
    : OPEN_STMT statement_body CLOSE_STMT;
    
statement_body
    : IF ID
    ;
    
comment
    : OPEN_COMMENT COMMENT_TEXT CLOSE_COMMENT;
    
text : TEXT;
