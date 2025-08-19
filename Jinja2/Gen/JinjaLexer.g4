// DELETE THIS CONTENT IF YOU PUT COMBINED GRAMMAR IN Parser TAB
lexer grammar JinjaLexer;

// Default mode: everything except template markers is captured as TEXT
TEXT
  : ~[{]+ -> channel(DEFAULT_TOKEN_CHANNEL)
  ;


// When we see a tag intro, switch into the corresponding mode
OPEN_EXPR    : '{{'  -> pushMode(EXPR);
OPEN_STMT    : '{%'  -> pushMode(STMT);
OPEN_COMMENT : '{#'  -> pushMode(COMMENT);


mode EXPR;
  // Close expression tags
  CLOSE_EXPR : '}}' -> popMode, type(CLOSE_EXPR);
  // You can embed a small expression sub-lexer here (identifiers, literals…)
  WS_EXPR    : [ \t\r\n]+ -> skip;
  
  ID         : [a-zA-Z_][a-zA-Z0-9_]*;
  INT        : [0-9]+;
  ESC: ('\\"'|'\\\\');
  STRING : '\'' (ESC|.)*? '\'' ;
  // Operators, punctuation…
  PLUS       : '+';
  MINUS      : '-';
  MUL : '*';
  DIV : '/';
  EQUALS : '=';
  // Comparison operators (also available in STMT via remap)
  GE : '>=';
  LE : '<=';
  GT : '>';
  LT : '<';
  NEQ: '!=';
  // …more tokens as needed
  LPARAN: '(';
  RPARAN: ')';
  COMMA: ',';
  CONCAT: '~';
  DOT : '.';

mode STMT;
  CLOSE_STMT : '%}' -> popMode, type(CLOSE_STMT);
  WS_STMT    : [ \t\r\n]+ -> skip ;
  
  SET       : 'set';
  IF         : 'if' ;
  ELIF       : 'elif' ;
  ELSE       : 'else' ;
  ENDIF      : 'endif' ;
  FOR        : 'for' ;
  END        : 'end' ;
  EQ         : '==' ;
  MACRO      : 'macro';
  END_MACRO  : 'endmacro';
  
  STMT_ID : ID -> type(ID);
  STMT_INT: INT -> type(INT);
  STMT_STRING: STRING -> type(STRING);
  
  STMT_PLUS: PLUS -> type(PLUS);
  STMT_MINUS: MINUS -> type(MINUS);
  STMT_MUL: MUL -> type(MUL);
  STMT_DIV: DIV -> type(DIV);
  STMT_EQUALS: EQUALS -> type(EQUALS);
  
  // remap comparison operators into STMT mode
  STMT_GE: GE -> type(GE);
  STMT_LE: LE -> type(LE);
  STMT_GT: GT -> type(GT);
  STMT_LT: LT -> type(LT);
  STMT_NEQ: NEQ -> type(NEQ);
  
  STMT_LPARAN: LPARAN -> type(LPARAN);
  STMT_RPARAN: RPARAN -> type(RPARAN);
  STMT_COMMA: COMMA -> type(COMMA);
  STMT_CONCAT: CONCAT -> type(CONCAT);
  STMT_DOT: DOT -> type(DOT);

mode COMMENT;
  CLOSE_COMMENT : '#}' -> popMode, type(CLOSE_COMMENT);
  // Everything until the closing tag is ignored
  COMMENT_TEXT  : .+? ;





