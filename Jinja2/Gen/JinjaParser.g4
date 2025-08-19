parser grammar JinjaParser;
options { tokenVocab=JinjaLexer; }

template 
    : ( text | expression | statement | macro | if_stmt | comment)* EOF
    ;
    
macro_template 
    : ( text | expression | statement | if_stmt | comment)*
    ;
    
if_template 
    : ( text | expression | statement  | comment)*
    ;
    
expression
    : OPEN_EXPR (MINUS)? expression_body (MINUS)? CLOSE_EXPR
    ;
    
expression_body
    : LPARAN expression_body RPARAN #eqParan
    | left = expression_body  operator = (PLUS|MINUS)  right = expression_body        #eqAdd
    | left = expression_body operator = (MUL|DIV) right = expression_body #eqMul
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
    : OPEN_STMT (MINUS)? statement_body (MINUS)? CLOSE_STMT
    ;
    
statement_body
    : SET ID EQUALS expression_body #eqAssign
    ;
    
if_stmt
    : OPEN_STMT (MINUS)? IF boolean_expression (MINUS)? CLOSE_STMT
        if_template
      (OPEN_STMT (MINUS)? ELIF boolean_expression (MINUS)? CLOSE_STMT if_template)*
      (OPEN_STMT (MINUS)? ELSE (MINUS)? CLOSE_STMT if_template)?
      OPEN_STMT (MINUS)? ENDIF (MINUS)? CLOSE_STMT #eqIfBlock
    ;
    
macro
    : OPEN_STMT (MINUS)? MACRO ID LPARAN (params)? RPARAN  (MINUS)? CLOSE_STMT
        macro_template
     OPEN_STMT (MINUS)? END_MACRO (MINUS)? CLOSE_STMT  #eqMacro
    ;
    
params
    : param (COMMA param)*
    ;

param: id=ID (EQUALS value=expression_body)?;
   
boolean_expression
    : LPARAN boolean_expression RPARAN #eqBoolParens
    | left=expression_body op=(GT|LT|GE|LE|EQ|NEQ) right=expression_body #eqBoolCompare
    | expression_body #eqBoolExpr
    ;
    
functionCall
    : ID LPARAN argList? RPARAN
    ;

argList
    : expression_body (COMMA expression_body)*
    ;
    
comment
    : OPEN_COMMENT (COMMENT_TEXT)* CLOSE_COMMENT;
    
text : TEXT;
