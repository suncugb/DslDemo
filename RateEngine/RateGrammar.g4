grammar RateGrammar;

/*
 * Parser Rules
 */

compileUnit
	:	EOF
	;

case_head
	: CASEHEAD
	;

left_parktime
	: NUMBER
	;

right_parktime
	: NUMBER
	;

left_time
	: TIME
	;

middle_time
	: TIME
	;

right_time
	: TIME
	;

workday
	: LEFT_BRACKET WORKDAY RIGHT_BRACKET
	;

case_condition
	: LEFT_BRACKET left_parktime HYPHEN right_parktime RIGHT_BRACKET
	| LEFT_BRACKET left_parktime HYPHEN right_parktime RIGHT_BRACKET workday
	| LEFT_BRACKET left_time HYPHEN right_time RIGHT_BRACKET
	| LEFT_BRACKET left_time HYPHEN right_time RIGHT_BRACKET workday
	| LEFT_BRACKET left_time HYPHEN middle_time HYPHEN right_time RIGHT_BRACKET
	| LEFT_BRACKET left_time HYPHEN middle_time HYPHEN right_time RIGHT_BRACKET workday
	| LEFT_BRACKET OUTDAY RIGHT_BRACKET
	| LEFT_BRACKET OUTDAY RIGHT_BRACKET workday
	;

value
	: NUMBER
	;

parameter
	: NUMBER
	;

api
	: APINAME LEFT_PAREN RIGHT_PAREN
	| APINAME LEFT_PAREN parameter RIGHT_PAREN
	;

case_body
	: value
	| api
	| api MUL NUMBER
	;

case_expression
	: case_head COLON case_condition case_body
	;

case_expressions
	: case_expression+
	;

limit_head
	: LIMITHEAD
	;

limit_condition
	: LEFT_BRACKET case_head RIGHT_BRACKET
	| LEFT_BRACKET DAYLIMIT RIGHT_BRACKET
	| LEFT_BRACKET MONTHLIMIT RIGHT_BRACKET
	;

limit_body
	: value
	;

limit_expression
	: limit_head COLON limit_condition limit_body
	;

limit_expressions
	: limit_expression+
	;

cartype_head
	: CARTYPE
	;

cartype_body
	: case_expressions 
	| case_expressions limit_expressions
	;

cartype_expression
	: cartype_head LEFT_CURLY cartype_body RIGHT_CURLY
	;

cartype_expressions
	: cartype_expression+
	;

configfile
	: cartype_expressions
	;

/*
 * Lexer Rules
 */
CARTYPE
	: [\u4e00-\u9fa5]+
	;

NUMBER
	: [0]|[1-9][0-9]*|([0-9]+[.][0-9]+) 
	;  

TIME
	: [0-9][0-9]?[:][0-9][0-9]
	;

CASEHEAD
	: 'case'[1-9]+
	;

LIMITHEAD
	: 'limit'[1-9]+
	;

DAYLIMIT
	: 'day'
	;

MONTHLIMIT
	: 'month'
	;

OUTDAY
	: '24h+'
	;

WORKDAY
	: 'workday'
	| 'weekend'
	| 'holiday'
	;

APINAME
	: 'SplitMonth'
	| 'SplitDay'
	| 'SplitTimeRegion'
	| 'SplitTime'
	;

LEFT_CURLY 
	: '{' 
	;

RIGHT_CURLY 
	: '}' 
	;

COLON
	: ':'
	;

LEFT_BRACKET
	: '['
	;

RIGHT_BRACKET
	: ']'
	;

HYPHEN
	: '-'
	;

LEFT_PAREN 
	: '(' 
	;

RIGHT_PAREN 
	: ')' 
	;
	
MUL
	: '*'
	;

LINE_COMMENT 
	: '#' .*? '\n' -> skip 
	;

WS
	: [\t\r\n]+ -> skip
	;
