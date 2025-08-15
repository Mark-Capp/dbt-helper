parser grammar JinjaParser;
options { tokenVocab=JinjaLexer; }

template 
    : ( text | expression | statement | macro | comment)* EOF
    ;
    
macro_template 
    : ( text | expression | statement | comment)*
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
    | functionCall #eqFunctionCall
    | concat+ #eqConcat
    | ID DOT functionCall #eqObjectFunction
    | ID DOT ID #eqObjectProperty
    | (concat+) DOT functionCall #eqConcatFuntion
    ;
    
concat
    : LPARAN concat RPARAN #eqInnerConcat
    | left=concat_expression_body (CONCAT right=concat_expression_body)+ #eqOuterConcat
    ;
    
concat_expression_body
    : ID
    | STRING
    | INT
    ;
    
statement
    : OPEN_STMT statement_body CLOSE_STMT
    ;
    
statement_body
    : IF boolean_expression #eqIF
    ;
    
macro
    : OPEN_STMT MACRO ID LPARAN (params)? RPARAN CLOSE_STMT
        macro_template
     OPEN_STMT END_MACRO CLOSE_STMT  #eqMacro
    ;
    
params
    : param (COMMA param)*
    ;

param: id=ID (EQUALS value=expression_body)?;
   
boolean_expression
    : ID
    ;
    
functionCall
    : ID LPARAN argList? RPARAN
    ;

argList
    : expression_body (COMMA expression_body)*
    ;
    
comment
    : OPEN_COMMENT COMMENT_TEXT CLOSE_COMMENT;
    
text : TEXT;
