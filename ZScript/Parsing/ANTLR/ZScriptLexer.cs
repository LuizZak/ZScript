//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:/Users/Luiz Fernando/Documents/Visual Studio 2013/Engines/ZScript/ZScript\ZScript.g4 by ANTLR 4.5

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591

using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.5")]
[System.CLSCompliant(false)]
public partial class ZScriptLexer : Lexer {
	public const int
		StringLiteral=1, StringEscape=2, T_VAR=3, T_LET=4, T_CONST=5, T_NEW=6, 
		T_EXPORT=7, T_FUNCTION=8, T_OVERRIDE=9, T_OBJECT=10, T_SEQUENCE=11, T_IF=12, 
		T_ELSE=13, T_WHILE=14, T_FOR=15, T_BREAK=16, T_CONTINUE=17, T_SWITCH=18, 
		T_CASE=19, T_DEFAULT=20, T_RETURN=21, T_LEFT_PAREN=22, T_RIGHT_PAREN=23, 
		T_LEFT_BRACKET=24, T_RIGHT_BRACKET=25, T_LEFT_CURLY=26, T_RIGHT_CURLY=27, 
		T_CLOSURE_RETURN=28, T_CLOSURE_CALL=29, INT=30, HEX=31, BINARY=32, FLOAT=33, 
		T_FALSE=34, T_TRUE=35, T_NULL=36, T_QUOTES=37, T_DOUBLE_QUOTES=38, T_TRIPPLE_DOT=39, 
		T_DOUBLE_COLON=40, T_SEMICOLON=41, T_PERIOD=42, T_COMMA=43, T_MULT=44, 
		T_DIV=45, T_MOD=46, T_NOT=47, T_PLUS=48, T_MINUS=49, T_INCREMENT=50, T_DECREMENT=51, 
		T_BITWISE_AND=52, T_BITWISE_XOR=53, T_BITWISE_OR=54, T_EQUALITY=55, T_UNEQUALITY=56, 
		T_MORE_THAN_OR_EQUALS=57, T_LESS_THAN_OR_EQUALS=58, T_MORE_THAN=59, T_LESS_THAN=60, 
		T_LOGICAL_AND=61, T_LOGICAL_OR=62, T_EQUALS=63, T_PLUS_EQUALS=64, T_MINUS_EQUALS=65, 
		T_TIMES_EQUALS=66, T_DIV_EQUALS=67, T_MOD_EQUALS=68, T_XOR_EQUALS=69, 
		T_AND_EQUALS=70, T_TILDE_EQUALS=71, T_OR_EQUALS=72, IDENT=73, Whitespace=74, 
		Newline=75, BlockComment=76, LineComment=77, ImportDirective=78;
	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"StringLiteral", "StringEscape", "T_VAR", "T_LET", "T_CONST", "T_NEW", 
		"T_EXPORT", "T_FUNCTION", "T_OVERRIDE", "T_OBJECT", "T_SEQUENCE", "T_IF", 
		"T_ELSE", "T_WHILE", "T_FOR", "T_BREAK", "T_CONTINUE", "T_SWITCH", "T_CASE", 
		"T_DEFAULT", "T_RETURN", "T_LEFT_PAREN", "T_RIGHT_PAREN", "T_LEFT_BRACKET", 
		"T_RIGHT_BRACKET", "T_LEFT_CURLY", "T_RIGHT_CURLY", "T_CLOSURE_RETURN", 
		"T_CLOSURE_CALL", "INT", "HEX", "BINARY", "FLOAT", "T_FALSE", "T_TRUE", 
		"T_NULL", "T_QUOTES", "T_DOUBLE_QUOTES", "T_TRIPPLE_DOT", "T_DOUBLE_COLON", 
		"T_SEMICOLON", "T_PERIOD", "T_COMMA", "T_MULT", "T_DIV", "T_MOD", "T_NOT", 
		"T_PLUS", "T_MINUS", "T_INCREMENT", "T_DECREMENT", "T_BITWISE_AND", "T_BITWISE_XOR", 
		"T_BITWISE_OR", "T_EQUALITY", "T_UNEQUALITY", "T_MORE_THAN_OR_EQUALS", 
		"T_LESS_THAN_OR_EQUALS", "T_MORE_THAN", "T_LESS_THAN", "T_LOGICAL_AND", 
		"T_LOGICAL_OR", "T_EQUALS", "T_PLUS_EQUALS", "T_MINUS_EQUALS", "T_TIMES_EQUALS", 
		"T_DIV_EQUALS", "T_MOD_EQUALS", "T_XOR_EQUALS", "T_AND_EQUALS", "T_TILDE_EQUALS", 
		"T_OR_EQUALS", "IDENT", "CHAR_azAZ_", "CHAR_09azAZ_", "DoubleQuoteStringChar", 
		"SingleQuoteStringChar", "Binary", "Hexadecimal", "Decimal", "DecimalFraction", 
		"ExponentPart", "Sign", "Whitespace", "Newline", "BlockComment", "LineComment", 
		"ImportDirective"
	};


	public ZScriptLexer(ICharStream input)
		: base(input)
	{
		Interpreter = new LexerATNSimulator(this,_ATN);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, "'var'", "'let'", "'const'", "'new'", "'@'", "'func'", 
		"'override'", "'object'", "'sequence'", "'if'", "'else'", "'while'", "'for'", 
		"'break'", "'continue'", "'switch'", "'case'", "'default'", "'return'", 
		"'('", "')'", "'['", "']'", "'{'", "'}'", "'->'", "'=>'", null, null, 
		null, null, "'false'", "'true'", "'null'", "'''", "'\"'", "'...'", "':'", 
		"';'", "'.'", "','", "'*'", "'/'", "'%'", "'!'", "'+'", "'-'", "'++'", 
		"'--'", "'&'", "'^'", "'|'", "'=='", "'!='", "'>='", "'<='", "'>'", "'<'", 
		"'&&'", "'||'", "'='", "'+='", "'-='", "'*='", "'/='", "'%='", "'^='", 
		"'&='", "'~='", "'|='"
	};
	private static readonly string[] _SymbolicNames = {
		null, "StringLiteral", "StringEscape", "T_VAR", "T_LET", "T_CONST", "T_NEW", 
		"T_EXPORT", "T_FUNCTION", "T_OVERRIDE", "T_OBJECT", "T_SEQUENCE", "T_IF", 
		"T_ELSE", "T_WHILE", "T_FOR", "T_BREAK", "T_CONTINUE", "T_SWITCH", "T_CASE", 
		"T_DEFAULT", "T_RETURN", "T_LEFT_PAREN", "T_RIGHT_PAREN", "T_LEFT_BRACKET", 
		"T_RIGHT_BRACKET", "T_LEFT_CURLY", "T_RIGHT_CURLY", "T_CLOSURE_RETURN", 
		"T_CLOSURE_CALL", "INT", "HEX", "BINARY", "FLOAT", "T_FALSE", "T_TRUE", 
		"T_NULL", "T_QUOTES", "T_DOUBLE_QUOTES", "T_TRIPPLE_DOT", "T_DOUBLE_COLON", 
		"T_SEMICOLON", "T_PERIOD", "T_COMMA", "T_MULT", "T_DIV", "T_MOD", "T_NOT", 
		"T_PLUS", "T_MINUS", "T_INCREMENT", "T_DECREMENT", "T_BITWISE_AND", "T_BITWISE_XOR", 
		"T_BITWISE_OR", "T_EQUALITY", "T_UNEQUALITY", "T_MORE_THAN_OR_EQUALS", 
		"T_LESS_THAN_OR_EQUALS", "T_MORE_THAN", "T_LESS_THAN", "T_LOGICAL_AND", 
		"T_LOGICAL_OR", "T_EQUALS", "T_PLUS_EQUALS", "T_MINUS_EQUALS", "T_TIMES_EQUALS", 
		"T_DIV_EQUALS", "T_MOD_EQUALS", "T_XOR_EQUALS", "T_AND_EQUALS", "T_TILDE_EQUALS", 
		"T_OR_EQUALS", "IDENT", "Whitespace", "Newline", "BlockComment", "LineComment", 
		"ImportDirective"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "ZScript.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public static readonly string _serializedATN =
		"\x3\x430\xD6D1\x8206\xAD2D\x4417\xAEF1\x8D80\xAADD\x2P\x237\b\x1\x4\x2"+
		"\t\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x4\b\t\b\x4"+
		"\t\t\t\x4\n\t\n\x4\v\t\v\x4\f\t\f\x4\r\t\r\x4\xE\t\xE\x4\xF\t\xF\x4\x10"+
		"\t\x10\x4\x11\t\x11\x4\x12\t\x12\x4\x13\t\x13\x4\x14\t\x14\x4\x15\t\x15"+
		"\x4\x16\t\x16\x4\x17\t\x17\x4\x18\t\x18\x4\x19\t\x19\x4\x1A\t\x1A\x4\x1B"+
		"\t\x1B\x4\x1C\t\x1C\x4\x1D\t\x1D\x4\x1E\t\x1E\x4\x1F\t\x1F\x4 \t \x4!"+
		"\t!\x4\"\t\"\x4#\t#\x4$\t$\x4%\t%\x4&\t&\x4\'\t\'\x4(\t(\x4)\t)\x4*\t"+
		"*\x4+\t+\x4,\t,\x4-\t-\x4.\t.\x4/\t/\x4\x30\t\x30\x4\x31\t\x31\x4\x32"+
		"\t\x32\x4\x33\t\x33\x4\x34\t\x34\x4\x35\t\x35\x4\x36\t\x36\x4\x37\t\x37"+
		"\x4\x38\t\x38\x4\x39\t\x39\x4:\t:\x4;\t;\x4<\t<\x4=\t=\x4>\t>\x4?\t?\x4"+
		"@\t@\x4\x41\t\x41\x4\x42\t\x42\x4\x43\t\x43\x4\x44\t\x44\x4\x45\t\x45"+
		"\x4\x46\t\x46\x4G\tG\x4H\tH\x4I\tI\x4J\tJ\x4K\tK\x4L\tL\x4M\tM\x4N\tN"+
		"\x4O\tO\x4P\tP\x4Q\tQ\x4R\tR\x4S\tS\x4T\tT\x4U\tU\x4V\tV\x4W\tW\x4X\t"+
		"X\x4Y\tY\x3\x2\x3\x2\x3\x2\a\x2\xB7\n\x2\f\x2\xE\x2\xBA\v\x2\x3\x2\x3"+
		"\x2\x3\x2\a\x2\xBF\n\x2\f\x2\xE\x2\xC2\v\x2\x3\x2\x5\x2\xC5\n\x2\x3\x3"+
		"\x3\x3\x3\x3\x3\x4\x3\x4\x3\x4\x3\x4\x3\x5\x3\x5\x3\x5\x3\x5\x3\x6\x3"+
		"\x6\x3\x6\x3\x6\x3\x6\x3\x6\x3\a\x3\a\x3\a\x3\a\x3\b\x3\b\x3\t\x3\t\x3"+
		"\t\x3\t\x3\t\x3\n\x3\n\x3\n\x3\n\x3\n\x3\n\x3\n\x3\n\x3\n\x3\v\x3\v\x3"+
		"\v\x3\v\x3\v\x3\v\x3\v\x3\f\x3\f\x3\f\x3\f\x3\f\x3\f\x3\f\x3\f\x3\f\x3"+
		"\r\x3\r\x3\r\x3\xE\x3\xE\x3\xE\x3\xE\x3\xE\x3\xF\x3\xF\x3\xF\x3\xF\x3"+
		"\xF\x3\xF\x3\x10\x3\x10\x3\x10\x3\x10\x3\x11\x3\x11\x3\x11\x3\x11\x3\x11"+
		"\x3\x11\x3\x12\x3\x12\x3\x12\x3\x12\x3\x12\x3\x12\x3\x12\x3\x12\x3\x12"+
		"\x3\x13\x3\x13\x3\x13\x3\x13\x3\x13\x3\x13\x3\x13\x3\x14\x3\x14\x3\x14"+
		"\x3\x14\x3\x14\x3\x15\x3\x15\x3\x15\x3\x15\x3\x15\x3\x15\x3\x15\x3\x15"+
		"\x3\x16\x3\x16\x3\x16\x3\x16\x3\x16\x3\x16\x3\x16\x3\x17\x3\x17\x3\x18"+
		"\x3\x18\x3\x19\x3\x19\x3\x1A\x3\x1A\x3\x1B\x3\x1B\x3\x1C\x3\x1C\x3\x1D"+
		"\x3\x1D\x3\x1D\x3\x1E\x3\x1E\x3\x1E\x3\x1F\x3\x1F\x3 \x3 \x3 \x3 \x3 "+
		"\x3!\x3!\x3!\x3!\x3!\x3\"\x3\"\x5\"\x158\n\"\x3\"\x5\"\x15B\n\"\x3#\x3"+
		"#\x3#\x3#\x3#\x3#\x3$\x3$\x3$\x3$\x3$\x3%\x3%\x3%\x3%\x3%\x3&\x3&\x3\'"+
		"\x3\'\x3(\x3(\x3(\x3(\x3)\x3)\x3*\x3*\x3+\x3+\x3,\x3,\x3-\x3-\x3.\x3."+
		"\x3/\x3/\x3\x30\x3\x30\x3\x31\x3\x31\x3\x32\x3\x32\x3\x33\x3\x33\x3\x33"+
		"\x3\x34\x3\x34\x3\x34\x3\x35\x3\x35\x3\x36\x3\x36\x3\x37\x3\x37\x3\x38"+
		"\x3\x38\x3\x38\x3\x39\x3\x39\x3\x39\x3:\x3:\x3:\x3;\x3;\x3;\x3<\x3<\x3"+
		"=\x3=\x3>\x3>\x3>\x3?\x3?\x3?\x3@\x3@\x3\x41\x3\x41\x3\x41\x3\x42\x3\x42"+
		"\x3\x42\x3\x43\x3\x43\x3\x43\x3\x44\x3\x44\x3\x44\x3\x45\x3\x45\x3\x45"+
		"\x3\x46\x3\x46\x3\x46\x3G\x3G\x3G\x3H\x3H\x3H\x3I\x3I\x3I\x3J\x6J\x1C9"+
		"\nJ\rJ\xEJ\x1CA\x3J\aJ\x1CE\nJ\fJ\xEJ\x1D1\vJ\x3K\x5K\x1D4\nK\x3L\x5L"+
		"\x1D7\nL\x3M\x3M\x3N\x3N\x3O\x6O\x1DE\nO\rO\xEO\x1DF\x3P\x6P\x1E3\nP\r"+
		"P\xEP\x1E4\x3Q\x6Q\x1E8\nQ\rQ\xEQ\x1E9\x3R\x3R\x3R\x3S\x3S\x3S\x6S\x1F2"+
		"\nS\rS\xES\x1F3\x3T\x3T\x3U\x6U\x1F9\nU\rU\xEU\x1FA\x3U\x3U\x3V\x3V\x5"+
		"V\x201\nV\x3V\x5V\x204\nV\x3V\x3V\x3W\x3W\x3W\x3W\aW\x20C\nW\fW\xEW\x20F"+
		"\vW\x3W\x3W\x3W\x3W\x3W\x3X\x3X\x3X\x3X\aX\x21A\nX\fX\xEX\x21D\vX\x3X"+
		"\x3X\x3Y\x3Y\x5Y\x223\nY\x3Y\x3Y\x3Y\x3Y\x3Y\x3Y\x3Y\x3Y\x3Y\x5Y\x22E"+
		"\nY\x3Y\aY\x231\nY\fY\xEY\x234\vY\x3Y\x3Y\x3\x20D\x2Z\x3\x3\x5\x4\a\x5"+
		"\t\x6\v\a\r\b\xF\t\x11\n\x13\v\x15\f\x17\r\x19\xE\x1B\xF\x1D\x10\x1F\x11"+
		"!\x12#\x13%\x14\'\x15)\x16+\x17-\x18/\x19\x31\x1A\x33\x1B\x35\x1C\x37"+
		"\x1D\x39\x1E;\x1F= ?!\x41\"\x43#\x45$G%I&K\'M(O)Q*S+U,W-Y.[/]\x30_\x31"+
		"\x61\x32\x63\x33\x65\x34g\x35i\x36k\x37m\x38o\x39q:s;u<w=y>{?}@\x7F\x41"+
		"\x81\x42\x83\x43\x85\x44\x87\x45\x89\x46\x8BG\x8DH\x8FI\x91J\x93K\x95"+
		"\x2\x97\x2\x99\x2\x9B\x2\x9D\x2\x9F\x2\xA1\x2\xA3\x2\xA5\x2\xA7\x2\xA9"+
		"L\xABM\xADN\xAFO\xB1P\x3\x2\x10\x3\x2$$\x3\x2))\x4\x2pptt\x5\x2\x43\\"+
		"\x61\x61\x63|\x6\x2\x32;\x43\\\x61\x61\x63|\x5\x2\f\f\xF\xF$$\x5\x2\f"+
		"\f\xF\xF))\x3\x2\x32\x33\x5\x2\x32;\x43H\x63h\x3\x2\x32;\x4\x2GGgg\x4"+
		"\x2--//\x4\x2\v\v\"\"\x4\x2\f\f\xF\xF\x240\x2\x3\x3\x2\x2\x2\x2\x5\x3"+
		"\x2\x2\x2\x2\a\x3\x2\x2\x2\x2\t\x3\x2\x2\x2\x2\v\x3\x2\x2\x2\x2\r\x3\x2"+
		"\x2\x2\x2\xF\x3\x2\x2\x2\x2\x11\x3\x2\x2\x2\x2\x13\x3\x2\x2\x2\x2\x15"+
		"\x3\x2\x2\x2\x2\x17\x3\x2\x2\x2\x2\x19\x3\x2\x2\x2\x2\x1B\x3\x2\x2\x2"+
		"\x2\x1D\x3\x2\x2\x2\x2\x1F\x3\x2\x2\x2\x2!\x3\x2\x2\x2\x2#\x3\x2\x2\x2"+
		"\x2%\x3\x2\x2\x2\x2\'\x3\x2\x2\x2\x2)\x3\x2\x2\x2\x2+\x3\x2\x2\x2\x2-"+
		"\x3\x2\x2\x2\x2/\x3\x2\x2\x2\x2\x31\x3\x2\x2\x2\x2\x33\x3\x2\x2\x2\x2"+
		"\x35\x3\x2\x2\x2\x2\x37\x3\x2\x2\x2\x2\x39\x3\x2\x2\x2\x2;\x3\x2\x2\x2"+
		"\x2=\x3\x2\x2\x2\x2?\x3\x2\x2\x2\x2\x41\x3\x2\x2\x2\x2\x43\x3\x2\x2\x2"+
		"\x2\x45\x3\x2\x2\x2\x2G\x3\x2\x2\x2\x2I\x3\x2\x2\x2\x2K\x3\x2\x2\x2\x2"+
		"M\x3\x2\x2\x2\x2O\x3\x2\x2\x2\x2Q\x3\x2\x2\x2\x2S\x3\x2\x2\x2\x2U\x3\x2"+
		"\x2\x2\x2W\x3\x2\x2\x2\x2Y\x3\x2\x2\x2\x2[\x3\x2\x2\x2\x2]\x3\x2\x2\x2"+
		"\x2_\x3\x2\x2\x2\x2\x61\x3\x2\x2\x2\x2\x63\x3\x2\x2\x2\x2\x65\x3\x2\x2"+
		"\x2\x2g\x3\x2\x2\x2\x2i\x3\x2\x2\x2\x2k\x3\x2\x2\x2\x2m\x3\x2\x2\x2\x2"+
		"o\x3\x2\x2\x2\x2q\x3\x2\x2\x2\x2s\x3\x2\x2\x2\x2u\x3\x2\x2\x2\x2w\x3\x2"+
		"\x2\x2\x2y\x3\x2\x2\x2\x2{\x3\x2\x2\x2\x2}\x3\x2\x2\x2\x2\x7F\x3\x2\x2"+
		"\x2\x2\x81\x3\x2\x2\x2\x2\x83\x3\x2\x2\x2\x2\x85\x3\x2\x2\x2\x2\x87\x3"+
		"\x2\x2\x2\x2\x89\x3\x2\x2\x2\x2\x8B\x3\x2\x2\x2\x2\x8D\x3\x2\x2\x2\x2"+
		"\x8F\x3\x2\x2\x2\x2\x91\x3\x2\x2\x2\x2\x93\x3\x2\x2\x2\x2\xA9\x3\x2\x2"+
		"\x2\x2\xAB\x3\x2\x2\x2\x2\xAD\x3\x2\x2\x2\x2\xAF\x3\x2\x2\x2\x2\xB1\x3"+
		"\x2\x2\x2\x3\xC4\x3\x2\x2\x2\x5\xC6\x3\x2\x2\x2\a\xC9\x3\x2\x2\x2\t\xCD"+
		"\x3\x2\x2\x2\v\xD1\x3\x2\x2\x2\r\xD7\x3\x2\x2\x2\xF\xDB\x3\x2\x2\x2\x11"+
		"\xDD\x3\x2\x2\x2\x13\xE2\x3\x2\x2\x2\x15\xEB\x3\x2\x2\x2\x17\xF2\x3\x2"+
		"\x2\x2\x19\xFB\x3\x2\x2\x2\x1B\xFE\x3\x2\x2\x2\x1D\x103\x3\x2\x2\x2\x1F"+
		"\x109\x3\x2\x2\x2!\x10D\x3\x2\x2\x2#\x113\x3\x2\x2\x2%\x11C\x3\x2\x2\x2"+
		"\'\x123\x3\x2\x2\x2)\x128\x3\x2\x2\x2+\x130\x3\x2\x2\x2-\x137\x3\x2\x2"+
		"\x2/\x139\x3\x2\x2\x2\x31\x13B\x3\x2\x2\x2\x33\x13D\x3\x2\x2\x2\x35\x13F"+
		"\x3\x2\x2\x2\x37\x141\x3\x2\x2\x2\x39\x143\x3\x2\x2\x2;\x146\x3\x2\x2"+
		"\x2=\x149\x3\x2\x2\x2?\x14B\x3\x2\x2\x2\x41\x150\x3\x2\x2\x2\x43\x155"+
		"\x3\x2\x2\x2\x45\x15C\x3\x2\x2\x2G\x162\x3\x2\x2\x2I\x167\x3\x2\x2\x2"+
		"K\x16C\x3\x2\x2\x2M\x16E\x3\x2\x2\x2O\x170\x3\x2\x2\x2Q\x174\x3\x2\x2"+
		"\x2S\x176\x3\x2\x2\x2U\x178\x3\x2\x2\x2W\x17A\x3\x2\x2\x2Y\x17C\x3\x2"+
		"\x2\x2[\x17E\x3\x2\x2\x2]\x180\x3\x2\x2\x2_\x182\x3\x2\x2\x2\x61\x184"+
		"\x3\x2\x2\x2\x63\x186\x3\x2\x2\x2\x65\x188\x3\x2\x2\x2g\x18B\x3\x2\x2"+
		"\x2i\x18E\x3\x2\x2\x2k\x190\x3\x2\x2\x2m\x192\x3\x2\x2\x2o\x194\x3\x2"+
		"\x2\x2q\x197\x3\x2\x2\x2s\x19A\x3\x2\x2\x2u\x19D\x3\x2\x2\x2w\x1A0\x3"+
		"\x2\x2\x2y\x1A2\x3\x2\x2\x2{\x1A4\x3\x2\x2\x2}\x1A7\x3\x2\x2\x2\x7F\x1AA"+
		"\x3\x2\x2\x2\x81\x1AC\x3\x2\x2\x2\x83\x1AF\x3\x2\x2\x2\x85\x1B2\x3\x2"+
		"\x2\x2\x87\x1B5\x3\x2\x2\x2\x89\x1B8\x3\x2\x2\x2\x8B\x1BB\x3\x2\x2\x2"+
		"\x8D\x1BE\x3\x2\x2\x2\x8F\x1C1\x3\x2\x2\x2\x91\x1C4\x3\x2\x2\x2\x93\x1C8"+
		"\x3\x2\x2\x2\x95\x1D3\x3\x2\x2\x2\x97\x1D6\x3\x2\x2\x2\x99\x1D8\x3\x2"+
		"\x2\x2\x9B\x1DA\x3\x2\x2\x2\x9D\x1DD\x3\x2\x2\x2\x9F\x1E2\x3\x2\x2\x2"+
		"\xA1\x1E7\x3\x2\x2\x2\xA3\x1EB\x3\x2\x2\x2\xA5\x1EE\x3\x2\x2\x2\xA7\x1F5"+
		"\x3\x2\x2\x2\xA9\x1F8\x3\x2\x2\x2\xAB\x203\x3\x2\x2\x2\xAD\x207\x3\x2"+
		"\x2\x2\xAF\x215\x3\x2\x2\x2\xB1\x220\x3\x2\x2\x2\xB3\xB8\a$\x2\x2\xB4"+
		"\xB7\x5\x5\x3\x2\xB5\xB7\n\x2\x2\x2\xB6\xB4\x3\x2\x2\x2\xB6\xB5\x3\x2"+
		"\x2\x2\xB7\xBA\x3\x2\x2\x2\xB8\xB6\x3\x2\x2\x2\xB8\xB9\x3\x2\x2\x2\xB9"+
		"\xBB\x3\x2\x2\x2\xBA\xB8\x3\x2\x2\x2\xBB\xC5\a$\x2\x2\xBC\xC0\a)\x2\x2"+
		"\xBD\xBF\n\x3\x2\x2\xBE\xBD\x3\x2\x2\x2\xBF\xC2\x3\x2\x2\x2\xC0\xBE\x3"+
		"\x2\x2\x2\xC0\xC1\x3\x2\x2\x2\xC1\xC3\x3\x2\x2\x2\xC2\xC0\x3\x2\x2\x2"+
		"\xC3\xC5\a)\x2\x2\xC4\xB3\x3\x2\x2\x2\xC4\xBC\x3\x2\x2\x2\xC5\x4\x3\x2"+
		"\x2\x2\xC6\xC7\a^\x2\x2\xC7\xC8\t\x4\x2\x2\xC8\x6\x3\x2\x2\x2\xC9\xCA"+
		"\ax\x2\x2\xCA\xCB\a\x63\x2\x2\xCB\xCC\at\x2\x2\xCC\b\x3\x2\x2\x2\xCD\xCE"+
		"\an\x2\x2\xCE\xCF\ag\x2\x2\xCF\xD0\av\x2\x2\xD0\n\x3\x2\x2\x2\xD1\xD2"+
		"\a\x65\x2\x2\xD2\xD3\aq\x2\x2\xD3\xD4\ap\x2\x2\xD4\xD5\au\x2\x2\xD5\xD6"+
		"\av\x2\x2\xD6\f\x3\x2\x2\x2\xD7\xD8\ap\x2\x2\xD8\xD9\ag\x2\x2\xD9\xDA"+
		"\ay\x2\x2\xDA\xE\x3\x2\x2\x2\xDB\xDC\a\x42\x2\x2\xDC\x10\x3\x2\x2\x2\xDD"+
		"\xDE\ah\x2\x2\xDE\xDF\aw\x2\x2\xDF\xE0\ap\x2\x2\xE0\xE1\a\x65\x2\x2\xE1"+
		"\x12\x3\x2\x2\x2\xE2\xE3\aq\x2\x2\xE3\xE4\ax\x2\x2\xE4\xE5\ag\x2\x2\xE5"+
		"\xE6\at\x2\x2\xE6\xE7\at\x2\x2\xE7\xE8\ak\x2\x2\xE8\xE9\a\x66\x2\x2\xE9"+
		"\xEA\ag\x2\x2\xEA\x14\x3\x2\x2\x2\xEB\xEC\aq\x2\x2\xEC\xED\a\x64\x2\x2"+
		"\xED\xEE\al\x2\x2\xEE\xEF\ag\x2\x2\xEF\xF0\a\x65\x2\x2\xF0\xF1\av\x2\x2"+
		"\xF1\x16\x3\x2\x2\x2\xF2\xF3\au\x2\x2\xF3\xF4\ag\x2\x2\xF4\xF5\as\x2\x2"+
		"\xF5\xF6\aw\x2\x2\xF6\xF7\ag\x2\x2\xF7\xF8\ap\x2\x2\xF8\xF9\a\x65\x2\x2"+
		"\xF9\xFA\ag\x2\x2\xFA\x18\x3\x2\x2\x2\xFB\xFC\ak\x2\x2\xFC\xFD\ah\x2\x2"+
		"\xFD\x1A\x3\x2\x2\x2\xFE\xFF\ag\x2\x2\xFF\x100\an\x2\x2\x100\x101\au\x2"+
		"\x2\x101\x102\ag\x2\x2\x102\x1C\x3\x2\x2\x2\x103\x104\ay\x2\x2\x104\x105"+
		"\aj\x2\x2\x105\x106\ak\x2\x2\x106\x107\an\x2\x2\x107\x108\ag\x2\x2\x108"+
		"\x1E\x3\x2\x2\x2\x109\x10A\ah\x2\x2\x10A\x10B\aq\x2\x2\x10B\x10C\at\x2"+
		"\x2\x10C \x3\x2\x2\x2\x10D\x10E\a\x64\x2\x2\x10E\x10F\at\x2\x2\x10F\x110"+
		"\ag\x2\x2\x110\x111\a\x63\x2\x2\x111\x112\am\x2\x2\x112\"\x3\x2\x2\x2"+
		"\x113\x114\a\x65\x2\x2\x114\x115\aq\x2\x2\x115\x116\ap\x2\x2\x116\x117"+
		"\av\x2\x2\x117\x118\ak\x2\x2\x118\x119\ap\x2\x2\x119\x11A\aw\x2\x2\x11A"+
		"\x11B\ag\x2\x2\x11B$\x3\x2\x2\x2\x11C\x11D\au\x2\x2\x11D\x11E\ay\x2\x2"+
		"\x11E\x11F\ak\x2\x2\x11F\x120\av\x2\x2\x120\x121\a\x65\x2\x2\x121\x122"+
		"\aj\x2\x2\x122&\x3\x2\x2\x2\x123\x124\a\x65\x2\x2\x124\x125\a\x63\x2\x2"+
		"\x125\x126\au\x2\x2\x126\x127\ag\x2\x2\x127(\x3\x2\x2\x2\x128\x129\a\x66"+
		"\x2\x2\x129\x12A\ag\x2\x2\x12A\x12B\ah\x2\x2\x12B\x12C\a\x63\x2\x2\x12C"+
		"\x12D\aw\x2\x2\x12D\x12E\an\x2\x2\x12E\x12F\av\x2\x2\x12F*\x3\x2\x2\x2"+
		"\x130\x131\at\x2\x2\x131\x132\ag\x2\x2\x132\x133\av\x2\x2\x133\x134\a"+
		"w\x2\x2\x134\x135\at\x2\x2\x135\x136\ap\x2\x2\x136,\x3\x2\x2\x2\x137\x138"+
		"\a*\x2\x2\x138.\x3\x2\x2\x2\x139\x13A\a+\x2\x2\x13A\x30\x3\x2\x2\x2\x13B"+
		"\x13C\a]\x2\x2\x13C\x32\x3\x2\x2\x2\x13D\x13E\a_\x2\x2\x13E\x34\x3\x2"+
		"\x2\x2\x13F\x140\a}\x2\x2\x140\x36\x3\x2\x2\x2\x141\x142\a\x7F\x2\x2\x142"+
		"\x38\x3\x2\x2\x2\x143\x144\a/\x2\x2\x144\x145\a@\x2\x2\x145:\x3\x2\x2"+
		"\x2\x146\x147\a?\x2\x2\x147\x148\a@\x2\x2\x148<\x3\x2\x2\x2\x149\x14A"+
		"\x5\xA1Q\x2\x14A>\x3\x2\x2\x2\x14B\x14C\a\x32\x2\x2\x14C\x14D\az\x2\x2"+
		"\x14D\x14E\x3\x2\x2\x2\x14E\x14F\x5\x9FP\x2\x14F@\x3\x2\x2\x2\x150\x151"+
		"\a\x32\x2\x2\x151\x152\a\x64\x2\x2\x152\x153\x3\x2\x2\x2\x153\x154\x5"+
		"\x9DO\x2\x154\x42\x3\x2\x2\x2\x155\x157\x5\xA1Q\x2\x156\x158\x5\xA3R\x2"+
		"\x157\x156\x3\x2\x2\x2\x157\x158\x3\x2\x2\x2\x158\x15A\x3\x2\x2\x2\x159"+
		"\x15B\x5\xA5S\x2\x15A\x159\x3\x2\x2\x2\x15A\x15B\x3\x2\x2\x2\x15B\x44"+
		"\x3\x2\x2\x2\x15C\x15D\ah\x2\x2\x15D\x15E\a\x63\x2\x2\x15E\x15F\an\x2"+
		"\x2\x15F\x160\au\x2\x2\x160\x161\ag\x2\x2\x161\x46\x3\x2\x2\x2\x162\x163"+
		"\av\x2\x2\x163\x164\at\x2\x2\x164\x165\aw\x2\x2\x165\x166\ag\x2\x2\x166"+
		"H\x3\x2\x2\x2\x167\x168\ap\x2\x2\x168\x169\aw\x2\x2\x169\x16A\an\x2\x2"+
		"\x16A\x16B\an\x2\x2\x16BJ\x3\x2\x2\x2\x16C\x16D\a)\x2\x2\x16DL\x3\x2\x2"+
		"\x2\x16E\x16F\a$\x2\x2\x16FN\x3\x2\x2\x2\x170\x171\a\x30\x2\x2\x171\x172"+
		"\a\x30\x2\x2\x172\x173\a\x30\x2\x2\x173P\x3\x2\x2\x2\x174\x175\a<\x2\x2"+
		"\x175R\x3\x2\x2\x2\x176\x177\a=\x2\x2\x177T\x3\x2\x2\x2\x178\x179\a\x30"+
		"\x2\x2\x179V\x3\x2\x2\x2\x17A\x17B\a.\x2\x2\x17BX\x3\x2\x2\x2\x17C\x17D"+
		"\a,\x2\x2\x17DZ\x3\x2\x2\x2\x17E\x17F\a\x31\x2\x2\x17F\\\x3\x2\x2\x2\x180"+
		"\x181\a\'\x2\x2\x181^\x3\x2\x2\x2\x182\x183\a#\x2\x2\x183`\x3\x2\x2\x2"+
		"\x184\x185\a-\x2\x2\x185\x62\x3\x2\x2\x2\x186\x187\a/\x2\x2\x187\x64\x3"+
		"\x2\x2\x2\x188\x189\a-\x2\x2\x189\x18A\a-\x2\x2\x18A\x66\x3\x2\x2\x2\x18B"+
		"\x18C\a/\x2\x2\x18C\x18D\a/\x2\x2\x18Dh\x3\x2\x2\x2\x18E\x18F\a(\x2\x2"+
		"\x18Fj\x3\x2\x2\x2\x190\x191\a`\x2\x2\x191l\x3\x2\x2\x2\x192\x193\a~\x2"+
		"\x2\x193n\x3\x2\x2\x2\x194\x195\a?\x2\x2\x195\x196\a?\x2\x2\x196p\x3\x2"+
		"\x2\x2\x197\x198\a#\x2\x2\x198\x199\a?\x2\x2\x199r\x3\x2\x2\x2\x19A\x19B"+
		"\a@\x2\x2\x19B\x19C\a?\x2\x2\x19Ct\x3\x2\x2\x2\x19D\x19E\a>\x2\x2\x19E"+
		"\x19F\a?\x2\x2\x19Fv\x3\x2\x2\x2\x1A0\x1A1\a@\x2\x2\x1A1x\x3\x2\x2\x2"+
		"\x1A2\x1A3\a>\x2\x2\x1A3z\x3\x2\x2\x2\x1A4\x1A5\a(\x2\x2\x1A5\x1A6\a("+
		"\x2\x2\x1A6|\x3\x2\x2\x2\x1A7\x1A8\a~\x2\x2\x1A8\x1A9\a~\x2\x2\x1A9~\x3"+
		"\x2\x2\x2\x1AA\x1AB\a?\x2\x2\x1AB\x80\x3\x2\x2\x2\x1AC\x1AD\a-\x2\x2\x1AD"+
		"\x1AE\a?\x2\x2\x1AE\x82\x3\x2\x2\x2\x1AF\x1B0\a/\x2\x2\x1B0\x1B1\a?\x2"+
		"\x2\x1B1\x84\x3\x2\x2\x2\x1B2\x1B3\a,\x2\x2\x1B3\x1B4\a?\x2\x2\x1B4\x86"+
		"\x3\x2\x2\x2\x1B5\x1B6\a\x31\x2\x2\x1B6\x1B7\a?\x2\x2\x1B7\x88\x3\x2\x2"+
		"\x2\x1B8\x1B9\a\'\x2\x2\x1B9\x1BA\a?\x2\x2\x1BA\x8A\x3\x2\x2\x2\x1BB\x1BC"+
		"\a`\x2\x2\x1BC\x1BD\a?\x2\x2\x1BD\x8C\x3\x2\x2\x2\x1BE\x1BF\a(\x2\x2\x1BF"+
		"\x1C0\a?\x2\x2\x1C0\x8E\x3\x2\x2\x2\x1C1\x1C2\a\x80\x2\x2\x1C2\x1C3\a"+
		"?\x2\x2\x1C3\x90\x3\x2\x2\x2\x1C4\x1C5\a~\x2\x2\x1C5\x1C6\a?\x2\x2\x1C6"+
		"\x92\x3\x2\x2\x2\x1C7\x1C9\x5\x95K\x2\x1C8\x1C7\x3\x2\x2\x2\x1C9\x1CA"+
		"\x3\x2\x2\x2\x1CA\x1C8\x3\x2\x2\x2\x1CA\x1CB\x3\x2\x2\x2\x1CB\x1CF\x3"+
		"\x2\x2\x2\x1CC\x1CE\x5\x97L\x2\x1CD\x1CC\x3\x2\x2\x2\x1CE\x1D1\x3\x2\x2"+
		"\x2\x1CF\x1CD\x3\x2\x2\x2\x1CF\x1D0\x3\x2\x2\x2\x1D0\x94\x3\x2\x2\x2\x1D1"+
		"\x1CF\x3\x2\x2\x2\x1D2\x1D4\t\x5\x2\x2\x1D3\x1D2\x3\x2\x2\x2\x1D4\x96"+
		"\x3\x2\x2\x2\x1D5\x1D7\t\x6\x2\x2\x1D6\x1D5\x3\x2\x2\x2\x1D7\x98\x3\x2"+
		"\x2\x2\x1D8\x1D9\n\a\x2\x2\x1D9\x9A\x3\x2\x2\x2\x1DA\x1DB\n\b\x2\x2\x1DB"+
		"\x9C\x3\x2\x2\x2\x1DC\x1DE\t\t\x2\x2\x1DD\x1DC\x3\x2\x2\x2\x1DE\x1DF\x3"+
		"\x2\x2\x2\x1DF\x1DD\x3\x2\x2\x2\x1DF\x1E0\x3\x2\x2\x2\x1E0\x9E\x3\x2\x2"+
		"\x2\x1E1\x1E3\t\n\x2\x2\x1E2\x1E1\x3\x2\x2\x2\x1E3\x1E4\x3\x2\x2\x2\x1E4"+
		"\x1E2\x3\x2\x2\x2\x1E4\x1E5\x3\x2\x2\x2\x1E5\xA0\x3\x2\x2\x2\x1E6\x1E8"+
		"\t\v\x2\x2\x1E7\x1E6\x3\x2\x2\x2\x1E8\x1E9\x3\x2\x2\x2\x1E9\x1E7\x3\x2"+
		"\x2\x2\x1E9\x1EA\x3\x2\x2\x2\x1EA\xA2\x3\x2\x2\x2\x1EB\x1EC\a\x30\x2\x2"+
		"\x1EC\x1ED\x5\xA1Q\x2\x1ED\xA4\x3\x2\x2\x2\x1EE\x1EF\t\f\x2\x2\x1EF\x1F1"+
		"\x5\xA7T\x2\x1F0\x1F2\x5\xA1Q\x2\x1F1\x1F0\x3\x2\x2\x2\x1F2\x1F3\x3\x2"+
		"\x2\x2\x1F3\x1F1\x3\x2\x2\x2\x1F3\x1F4\x3\x2\x2\x2\x1F4\xA6\x3\x2\x2\x2"+
		"\x1F5\x1F6\t\r\x2\x2\x1F6\xA8\x3\x2\x2\x2\x1F7\x1F9\t\xE\x2\x2\x1F8\x1F7"+
		"\x3\x2\x2\x2\x1F9\x1FA\x3\x2\x2\x2\x1FA\x1F8\x3\x2\x2\x2\x1FA\x1FB\x3"+
		"\x2\x2\x2\x1FB\x1FC\x3\x2\x2\x2\x1FC\x1FD\bU\x2\x2\x1FD\xAA\x3\x2\x2\x2"+
		"\x1FE\x200\a\xF\x2\x2\x1FF\x201\a\f\x2\x2\x200\x1FF\x3\x2\x2\x2\x200\x201"+
		"\x3\x2\x2\x2\x201\x204\x3\x2\x2\x2\x202\x204\a\f\x2\x2\x203\x1FE\x3\x2"+
		"\x2\x2\x203\x202\x3\x2\x2\x2\x204\x205\x3\x2\x2\x2\x205\x206\bV\x2\x2"+
		"\x206\xAC\x3\x2\x2\x2\x207\x208\a\x31\x2\x2\x208\x209\a,\x2\x2\x209\x20D"+
		"\x3\x2\x2\x2\x20A\x20C\v\x2\x2\x2\x20B\x20A\x3\x2\x2\x2\x20C\x20F\x3\x2"+
		"\x2\x2\x20D\x20E\x3\x2\x2\x2\x20D\x20B\x3\x2\x2\x2\x20E\x210\x3\x2\x2"+
		"\x2\x20F\x20D\x3\x2\x2\x2\x210\x211\a,\x2\x2\x211\x212\a\x31\x2\x2\x212"+
		"\x213\x3\x2\x2\x2\x213\x214\bW\x2\x2\x214\xAE\x3\x2\x2\x2\x215\x216\a"+
		"\x31\x2\x2\x216\x217\a\x31\x2\x2\x217\x21B\x3\x2\x2\x2\x218\x21A\n\xF"+
		"\x2\x2\x219\x218\x3\x2\x2\x2\x21A\x21D\x3\x2\x2\x2\x21B\x219\x3\x2\x2"+
		"\x2\x21B\x21C\x3\x2\x2\x2\x21C\x21E\x3\x2\x2\x2\x21D\x21B\x3\x2\x2\x2"+
		"\x21E\x21F\bX\x2\x2\x21F\xB0\x3\x2\x2\x2\x220\x222\a%\x2\x2\x221\x223"+
		"\x5\xA9U\x2\x222\x221\x3\x2\x2\x2\x222\x223\x3\x2\x2\x2\x223\x224\x3\x2"+
		"\x2\x2\x224\x225\ak\x2\x2\x225\x226\ap\x2\x2\x226\x227\a\x65\x2\x2\x227"+
		"\x228\an\x2\x2\x228\x229\aw\x2\x2\x229\x22A\a\x66\x2\x2\x22A\x22B\ag\x2"+
		"\x2\x22B\x22D\x3\x2\x2\x2\x22C\x22E\x5\xA9U\x2\x22D\x22C\x3\x2\x2\x2\x22D"+
		"\x22E\x3\x2\x2\x2\x22E\x232\x3\x2\x2\x2\x22F\x231\n\xF\x2\x2\x230\x22F"+
		"\x3\x2\x2\x2\x231\x234\x3\x2\x2\x2\x232\x230\x3\x2\x2\x2\x232\x233\x3"+
		"\x2\x2\x2\x233\x235\x3\x2\x2\x2\x234\x232\x3\x2\x2\x2\x235\x236\bY\x2"+
		"\x2\x236\xB2\x3\x2\x2\x2\x19\x2\xB6\xB8\xC0\xC4\x157\x15A\x1CA\x1CF\x1D3"+
		"\x1D6\x1DF\x1E4\x1E9\x1F3\x1FA\x200\x203\x20D\x21B\x222\x22D\x232\x3\b"+
		"\x2\x2";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}