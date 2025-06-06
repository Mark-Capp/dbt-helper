parser grammar JinjaParser;
options { tokenVocab=JinjaLexer; }

file: program;
program: statement*? EOF;

statement
    : non_macro_statements
    | macro_statement
    ;
    
non_macro_statements
    : evaluation_statement
    | body
    | if_statement
    | assignment_statement
    | while_statement
    | for_statement
    ;

assignment_statement
    : SET_BLOCK ID EQUALS expression BLOCK_END NEWLINE?
    ;
expression
    : LPAREN expression RPAREN                                         #eqPar
    | left = expression operator = (MUL|DIV) right = expression        #eqMUL
    | left = expression operator = (ADD|SUB) right = expression        #eqAdd
    | func                                                             #eqFunc
    | DOUBLE                                                           #eqDbl
    | INT                                                              #eqInt
    | STRING                                                           #eqStr
    | BOOL                                                             #eqExBool
    | ID                                                               #eqVar
    ;

boolean_expression
    : boolean_expression ((AND|OR) boolean_expression)+                         #eqMoreThanOneBoolean
    | (NOT)+ boolean_expression                                                 #notEq
    | LPAREN boolean_expression RPAREN                                          #eqBoolPar
    | left = expression operator=(GT|GTEQ|LT|LTEQ) right = expression           #relationExpr
    | left = expression operator=(EQ|NEQ) right = expression                    #boolEq
    | BOOL                                                                      #eqBool
    | expression                                                                #eqExpression
    ;

evaluation_statement
    : EXPRESSION_START (SUB)? expression (SUB)? EXPRESSION_END NEWLINE?
    | EXPRESSION_START (SUB)? boolean_expression (SUB)? EXPRESSION_END NEWLINE?
    | EXPRESSION_START (SUB)? func (SUB)? EXPRESSION_END NEWLINE?
    ;
    
macro_statement
    : macro_fragement (code_block)*? end_macro
    ;
    
macro_fragement
    : BLOCK_START (SUB)? MACRO ID func_fragment BLOCK_END NEWLINE?
    ;
    
func_fragment
    : LPAREN (variable_statement (COMMA variable_statement)*)* RPAREN;
    
variable_statement
    : expression 
    | expression EQUALS expression
    ;
    
end_macro: BLOCK_START (SUB)? END_MACRO (SUB)? BLOCK_END;
    
func
    : ID LPAREN (expression (COMMA expression)*)* RPAREN
    ;
    
if_statement
    : if_fragment (code_block)*? (elif_statement | else_statement)? endif_fragment
    ;
    
elif_statement
    : elif_fragment (code_block)*? (elif_statement | else_statement)? ;

else_statement
    : else_fragment code_block*?;

if_fragment
    : BLOCK_START (SUB)? IF_WORD LPAREN boolean_expression RPAREN (SUB)? BLOCK_END NEWLINE? 
    | BLOCK_START (SUB)? IF_WORD boolean_expression (SUB)? BLOCK_END NEWLINE? 
    ;
    
elif_fragment: BLOCK_START (SUB)? ELIF LPAREN boolean_expression RPAREN (SUB)? BLOCK_END NEWLINE?;
else_fragment: BLOCK_START (SUB)? ELSE (SUB)? BLOCK_END NEWLINE?;
endif_fragment: BLOCK_START (SUB)? ENDIF (SUB)? BLOCK_END NEWLINE?;

code_block
    : NEWLINE? body NEWLINE?
    | NEWLINE? non_macro_statements NEWLINE?
    ;

while_statement: while_fragment code_block*? endwhile_fragment ;

while_fragment
    : BLOCK_START (SUB)? WHILE LPAREN boolean_expression RPAREN (SUB)? BLOCK_END NEWLINE?
    | BLOCK_START (SUB)? WHILE boolean_expression (SUB)? BLOCK_END NEWLINE?
    ;
endwhile_fragment: END_WHILE NEWLINE?;


for_statement: for_fragment code_block*? endfor_fragment;

for_fragment
    : BLOCK_START SUB? FOR ID IN ID SUB? BLOCK_END NEWLINE?
    ;

endfor_fragment: BLOCK_START SUB? END_FOR SUB? BLOCK_END NEWLINE?;

body: contents;

contents
    : TEXT
    | ID
    ;
