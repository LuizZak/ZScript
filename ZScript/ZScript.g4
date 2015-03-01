/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

// Grammar configuration
grammar ZScript;

options
{
    language=CSharp;
}

program: scriptBody;

scriptBody : (functionDefinition | globalVariable | exportDefinition | classDefinition | sequenceBlock | typeAlias)*;

////
//// Class Definition
////
classDefinition : 'class' className classInherit? classBody;
classInherit : ':' className;
className : IDENT;
classBody : '{' (classField | classMethod)* '}';
classField : valueDeclareStatement;
classMethod : (override='override')? functionDefinition;

////
//// Global variable
////
globalVariable : valueDeclareStatement;

////
//// Sequence Block
////
sequenceBlock       : 'sequence' sequenceName sequenceBody;
sequenceName        : IDENT;
sequenceBody        : '[' (classField | sequenceFrame | sequenceFrameChange)* ']';
sequenceFrame       : (frameName ':')? frameRange? functionBody;
sequenceFrameChange : '=' frameNumber | ('-' frameNumber);
frameName           : IDENT;
frameRange          : frameRangeElement (',' frameRangeElement)*;
frameRangeElement   : relative='+'? frameNumber ('-' frameNumber)?;
frameNumber         : INT;

////
//// Function definition
////
functionDefinition : 'func' functionName functionArguments? returnType? functionBody;
exportDefinition : '@' functionName functionArguments? returnType?;

functionName : IDENT;
functionBody : blockStatement;
functionArguments : '(' argumentList? ')';
argumentList : functionArg (',' functionArg)*;
returnType : ':' type;
functionArg : argumentName (':' type)? (variadic='...' | ('=' compileConstant))?;
argumentName : IDENT;

////
//// Type alias
////
typeAlias : 'typeAlias' typeAliasName typeAliasInherit? ':' stringLiteral (';' | typeAliasBody);
typeAliasBody : '{' (typeAliasVariable | typeAliasFunction ';')* '}';
typeAliasVariable : valueDeclareStatement;
typeAliasFunction : 'func' functionName functionArguments returnType;
typeAliasName     : complexTypeName;
typeAliasInherit  : '<-' typeAliasName;

////
//// Statements
////
statement : (((expression | assignmentExpression) ';') | blockStatement | ';' | ifStatement | whileStatement | forStatement | switchStatement | returnStatement | breakStatement | continueStatement | valueDeclareStatement);
blockStatement : '{' statement* '}';

////
//// Control flow statements
////

// If
ifStatement : 'if' '(' expression ')' statement elseStatement?;
elseStatement : 'else' statement;

// Switch
switchStatement : 'switch' '(' (valueHolderDecl | expression) ')' switchBlock;
switchBlock : '{' caseBlock* defaultBlock? '}';
caseBlock : 'case' expression ':' statement*;
defaultBlock : 'default' ':' statement+;

// While
whileStatement : 'while' '(' expression ')' statement;

// For
forStatement : 'for' '(' forInit? ';' forCondition? ';' forIncrement? ')' statement;
forInit : valueHolderDecl | expression | assignmentExpression;
forCondition : expression;
forIncrement : expression;

// Return statement
returnStatement : 'return' value=expression? ';';

breakStatement : 'break' ';';
continueStatement : 'continue' ';';

////
//// Value holder declare statements
////
valueDeclareStatement : valueHolderDecl ';';

valueHolderDecl : (var='var' | let='let') valueHolderName (':' type)? ('=' expression)?;

valueHolderName : memberName;

// Types
type : objectType | typeName | callableType | listType | dictionaryType;
objectType       : 'object';
typeName         : primitiveType | complexTypeName;
complexTypeName  : IDENT ('.' IDENT)*;
primitiveType    : T_INT | T_FLOAT | T_VOID | T_ANY | T_STRING | T_BOOL;
callableType     : '(' callableTypeList? '->' type? ')';
listType         : '[' type ']';
dictionaryType   : '[' keyType=type ':' valueType=type ']';
callableTypeList : callableArgType (',' callableArgType)*;
callableArgType  : type variadic='...'?;

////
//// Expressions
////
expression:  '(' expression ')' valueAccess?
           | '(' assignmentExpression ')'
           // Primary expressions
           |  T_THIS objectAccess?
           |  T_BASE valueAccess?
           // Prefix and postfix
           |  prefixOperator leftValue
           |  leftValue postfixOperator
           |  closureExpression valueAccess?
           // Literals
           |  memberName valueAccess?
           |  objectLiteral objectAccess?
           |  arrayLiteral valueAccess?
           |  dictionaryLiteral valueAccess?
           // 'new' expression
           |  newExpression valueAccess?
           // Type casting
           |  '(' type ')' expression
           // Unary expressions
           |  unaryOperator expression
           // Binary expressions
           |  expression multOp expression
           |  expression additionOp expression
           |  expression relationalOp expression
           |  expression T_IS type
           |  expression bitwiseShift expression
           |  expression bitwiseAndOp expression
           |  expression bitwiseXOrOp expression
           |  expression bitwiseOrOp expression
           |  expression equalityOp expression
           |  expression logicalAnd expression
           |  expression logicalOr expression
           // Ternary operator
           |  <assoc=right>expression '?' expression ':' expression
           // Constant
           |  constantAtom objectAccess?
           ;

multOp : ('*' | '/' | '%');
additionOp : ('+' | '-');
bitwiseShift : (T_SHIFTLEFT | T_SHIFTRIGHT);
bitwiseAndOp : ('&' | '^');
bitwiseXOrOp : ('^');
bitwiseOrOp : ('|');
relationalOp : '>=' | '<=' | '>' | '<';
equalityOp : '==' | '!=';
logicalAnd : '&&';
logicalOr  : '||';

assignmentExpression: leftValue assignmentOperator (expression | assignmentExpression);
newExpression : 'new' typeName funcCallArguments;
closureExpression : (functionArg | functionArguments) returnType? '=>' functionBody;

prefixOperator : '++' | '--';
postfixOperator : '++' | '--';

unaryOperator : '-' | '!';
assignmentOperator : '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '^=' | '&=' | '~=' | '|=' | '<<=' | '>>=';

funcCallArguments : '(' expressionList? ')';
expressionList : expression (',' expression)*;

leftValue : (memberName | 'this') leftValueAccess?;
leftValueAccess : (functionCall leftValueAccess) | (fieldAccess leftValueAccess?) | (arrayAccess leftValueAccess?);
functionCall : funcCallArguments;
fieldAccess  : '.' memberName;
arrayAccess : '[' expression ']';

objectAccess : (fieldAccess | arrayAccess) valueAccess?;
valueAccess : (functionCall | fieldAccess | arrayAccess) valueAccess?;

memberName : IDENT;

// Literal values
arrayLiteral : '[' expressionList? ']';
dictionaryLiteral : '[' dictionaryEntryList ']';
objectLiteral: '{' objectEntryList? '}';
stringLiteral : StringLiteral;

dictionaryEntryList: ':' | (dictionaryEntry (',' dictionaryEntry)*);
dictionaryEntry : expression ':' expression;

objectEntryList: objectEntryDefinition (',' objectEntryDefinition)*;
objectEntryDefinition: entryName ':' expression;
entryName : IDENT | stringLiteral;

// Atomics
compileConstant :  ('-')? numericAtom | T_FALSE | T_TRUE | T_NULL | stringLiteral;

constantAtom : numericAtom
             | T_FALSE | T_TRUE | T_NULL
             | stringLiteral;

numericAtom : hexadecimalNumber | binaryNumber | (FLOAT | INT);
hexadecimalNumber : HEX;
binaryNumber : BINARY;

StringLiteral : '"' (DoubleQuoteEscape | ~('"'))* '"'
              | '\'' (SingleQuoteEscape | ~('\''))* '\'';

DoubleQuoteEscape : '\\' ('n' | 'r' | '\\' | '"');
SingleQuoteEscape : '\\' ('\'' | '\\');

////
//// Token Definitions
////

T_EXPORT   : '@';
T_FUNCTION : 'func';
T_OVERRIDE : 'override';
T_OBJECT   : 'object';
T_SEQUENCE : 'sequence';
T_THIS     : 'this';
T_BASE     : 'base';

// Statements
T_VAR   : 'var';
T_LET   : 'let';
T_CONST : 'const';
T_NEW   : 'new';

T_IS    : 'is';

T_IF       : 'if';
T_ELSE     : 'else';
T_WHILE    : 'while';
T_FOR      : 'for';

T_BREAK    : 'break';
T_CONTINUE : 'continue';

T_SWITCH   : 'switch';
T_CASE     : 'case';
T_DEFAULT  : 'default';

T_RETURN   : 'return';

T_LEFT_PAREN  : '(';
T_RIGHT_PAREN : ')';

T_LEFT_BRACKET : '[';
T_RIGHT_BRACKET : ']';

T_LEFT_CURLY  : '{';
T_RIGHT_CURLY : '}';

T_CLOSURE_RETURN : '->';
T_CLOSURE_CALL : '=>';

// Primitive types
T_INT    : 'int';
T_FLOAT  : 'float';
T_VOID   : 'void';
T_ANY    : 'any';
T_STRING : 'string';
T_BOOL   : 'bool';

// Atoms
INT     : Decimal;
HEX     : '0x' Hexadecimal;
BINARY  : '0b' Binary;
FLOAT   : Decimal DecimalFraction? ExponentPart?;

T_FALSE : 'false';
T_TRUE  : 'true';
T_NULL  : 'null';

T_QUOTES : '\'';
T_DOUBLE_QUOTES : '"';

T_TRIPPLE_DOT : '...';

T_DOUBLE_COLON : ':';
T_SEMICOLON : ';';
T_PERIOD : '.';
T_COMMA  : ',';

// All these operators are sorted by precedence
T_MULT  : '*';
T_DIV   : '/';
T_MOD   : '%';

T_NOT   : '!';
T_PLUS  : '+';
T_MINUS : '-';

T_INCREMENT : '++';
T_DECREMENT : '--';

T_BITWISE_AND : '&';
T_BITWISE_XOR : '^';

T_BITWISE_OR : '|';

T_SHIFTLEFT : '<<';
T_SHIFTRIGHT : '<<';

T_EQUALITY : '==';
T_UNEQUALITY : '!=';
T_MORE_THAN_OR_EQUALS : '>=';
T_LESS_THAN_OR_EQUALS : '<=';
T_MORE_THAN : '>';
T_LESS_THAN : '<';

T_LOGICAL_AND : '&&';
T_LOGICAL_OR : '||';

// Equality operators
T_EQUALS : '=';
T_PLUS_EQUALS : '+=';
T_MINUS_EQUALS : '-=';
T_TIMES_EQUALS : '*=';
T_DIV_EQUALS : '/=';
T_MOD_EQUALS : '%=';
T_XOR_EQUALS : '^=';
T_AND_EQUALS : '&=';
T_TILDE_EQUALS : '~=';
T_OR_EQUALS : '|=';
T_SHIFTLEFT_EQUALS: '<<=';
T_SHIFTRIGHT_EQUALS: '>>=';

IDENT : CHAR_azAZ_+ CHAR_09azAZ_*;

fragment CHAR_azAZ_ : ([a-z] | [A-Z] | '_');
fragment CHAR_09azAZ_ : ([a-z] | [A-Z] | '_' | [0-9]);

fragment DoubleQuoteStringChar : ~('\r' | '\n' | '"');
fragment SingleQuoteStringChar : ~('\r' | '\n' | '\'');

fragment Binary : [01]+;
fragment Hexadecimal : [0-9a-fA-F]+;
fragment Decimal : [0-9]+;
fragment DecimalFraction : '.' Decimal;
fragment ExponentPart : [eE] Sign Decimal+;
fragment Sign : [+\-];

Whitespace : [ \t]+ -> skip;
Newline : ( '\r' '\n'? | '\n') -> skip;
BlockComment : '/*' .*? '*/' -> skip;
LineComment : '//' ~[\r\n]* -> skip;
ImportDirective : '#' Whitespace? 'include' Whitespace? ~('\r' | '\n')* -> skip;