# ZScript  
An (once) game scripting programming language.


## Some considerations, beforehand

I did this mostly for fun and to learn about implementations of real programming languages, and also to use in my games. It contains a mish-mash of features of other languages that I find cool and useful, like static typing, type inferring and closures.

I'm also really not proud nor do I know exactly why I re-implemented a (very dysfunctional and incomplete) type system when .NET offers a tested and working one, pretty much for free. Other than that, most of the stuff I did was solely so I could learn about the ins and outs of writing an interpreted language that is run via a VM. Apart from the syntax tree parser library I used for this project (ANTLR v4.5), most of the features I ended up implementing where not really researched prior to execution, so some things are a bit janky when it comes to stability. The language is not completely implemented yet, and a full list of features with implementation and stability information are found in the next sessions of this file.


## About the language

ZScript is a static (albeit weakly) typed, imperative, object-oriented programming language inspired by other imperative "C-style" languages like .NET C#, Apple's Swift and JavaScript. In ZScript you can define functions, variables, objects and sequences (more about what sequences are later), and have the code executed by calling one of the defined functions through the runtime's `CallFunction(string functionName, object[] arguments)` method in C# code.

Supposedly, a few snippets of code talk louder than paragraphs of text to programmers, so here are examples of recursive and iterative implementations of fibonacci in ZScript:

Recursive:

```csharp
func fib(n:int) : int
{
  if(n <= 0)
    return 0;
  if(n == 1)
    return 1;
  
  return fib(n - 1) + fib(n - 2);
}
```

And iterative, showing local variables and ```for``` loops:

```csharp
func fib(n:int) : int
{
  if(n == 0) return 0;
  if(n == 1) return 1;
  
  var prevPrev = 0;
  var prev = 1;
  var result = 0;
  
  for (var i = 2; i <= n; i++)
  {
    result = prev + prevPrev;
    prevPrev = prev;
    prev = result;
  }
  
  return result;
}
```

These short examples showcase a few of the features of the language:

* Functions  
Statements to be executed in ZScript must be contained within functions that can be called either by other functions in the code, or directly by calling the ZRuntime.CallFunction() at C# code level.

* Type inferring  
Variables and constants in ZScript are typed, and a type must be provided at time of creation of the value holders, but if the type is omitted, it is inferred from the value it was assigned at time of creation: `prevPrev`, `prev`, `result` and `i` have their value inferred as integer because that's the value that was provided when they where created.


###### Language implementation status

Some of the features proposed by the language are not fully implemented, and some are still somewhat unstable, as specified by the table bellow.

Feature  | Status | Author's thoughs
------------- | ------------- | ------------
Variables & constants  | Implemented | Still missing things like analysis of initialization before usages.
Type System | Partially Implemented | A statical type system is in place, but some functionalities are not yet implemented, like binding to native types, for example. I'm also not fully sure about the ability of the type system to infer a common type between two types, it currently allows things like inferring that a list that contains integers and floats as of '[float]' type, when it really contains integers, too.
Expression Evaluation | Partially Implemented | Still missing emision of implicit and explicit type cast instructions, making the type system not fully safe. All other aspects are fully implemented, though.
Statements | Implemented | Modifications for the `switch` statement are planned, though.
Functions  | Partially Implemented | Working completely, but parameter definition checking is no yet implemented, allowing for unintended things like creating optional parameters before required ones, and specifying multiple variadic parameters in the same signature.
Export Functions  | Partially Implemented | Same implications to function parameters, see above.
Closures  | Partially Implemented | Fully working, can capture the variables of the function they are being executed in, including capturing of other closures. Same implications to function parameters, see above.
Objects (i.e. classes)  | Not Implemented | 
Sequences | Not Implemented | 
Type Alias | Not Implemented | Still evaluating the necessity of type aliases, but syntax is already drafted.


## Table of contents:

* Language Concepts
  * Types
    * Basic types
  * Value holders (Variables and Constants)
  * Statements
  * Functions
    * Export Functions
  * Closures
  * Objects
  * Sequences


### Language concepts

In ZScript, operations are executed by statements inside functions, with data being stored in variables and constants. The next sessions of this file explain the language's features and their usages.


### Types

ZScript is a typed language, meaning every value has a type associated with it, and that reflects in constructs that manipulate values: variables, constants, function parameters and function return values all have a type signature that describes their expected types. Type signatures are immutable (e.g. after creating a variable that contains integers, it cannot be changed to accept any type of value other than integers).

Types are checked when manipulating values, like assigning a variable or returning a value from a function, and any improper usage of types (say, assigning an integer when a string is expected) results in a compile-time error. This increases the confidence that the code will execute as expected, since you can be sure of the type something has from the type signature it exposes.

ZScript provides a few sets of basic types that can be used from the get-go:

#### Basic types

In ZScript, a basic type is any of the following types: ```int```, ```float```, ```bool```, ```string```, ```list```, ```object```, ```any```, ```void```, and a special ```callable``` type, and they can be used as a type for any required type signature (although with restrictions for ```void```, see in 'void' section bellow).

The next sections explain the types in depth.

##### Numeric types

ZScript understands two basic types of numbers: ```int```s and ```float```s.

```int``` is a 64-bit integer number (Equivalent to 'long' in C# or 'Int64' in Swift, for example).  
```float``` is a double precision floating point number (Equivalent to 'double' in C# or 'Double' in Swift, for example).

Numbers can be expressed as literals in the code, with integers defined with no decimal point, and floats with a decimal point:

```
10    // Literal integer value
10.0  // Literal floating-point value
```

Additionally, two syntaxes are available for creating hexadecimal and binary literal integer values:

```
0b110 // Literal binary integer values are specified by '0b' followed by a sequence of 0-1 digits
0xFFF // Literal hexadecimal integer values are specified by '0x' followed by a sequence of hex digits
```

Integers can be implicitly used in places that floats are expected, but not the other way around: Floats require casts to be converted to integers.

```csharp
// More about the syntax of variables is available in the next sections
var i:int   = 10; // Valid: Variable with type signature of 'int' receiving value of type 'int'
var f:float = i;  // Valid: Integer values can be implicitly assigned to variables of type float
i = f;            // Error: Cannot implicitly convert float type to int type
```

In order to perform the assignment `i = f`, a special construct called 'cast' is necessary:

```csharp
i = (int)f; // Valid: Floating point number is cast to an integer type
```

When casting a floating-point value to integer, the decimal places are lost, and the number is rounded down to the nearest integer.


##### Boolean type

ZScript provides a ```bool``` type for holding either of the two logical values `true` or `false`.

```csharp
var isSkyBlue:bool = true;
var canPigsFly:bool  = false;
var isThisValueFalse:bool = false; // Check out this sweet boolean paradox
```

Unlike in C/C++ (and like in C#, Swift or Java, for example), integers cannot be implicitly used in places a boolean is expected:

```csharp
var num:int = 10;
if(num) // Invalid: Condition contains an integer, but expects a boolean value.
...
```

Such cases require an operation that returns a boolean value to be made with the value:

```csharp
var num:int = 10;
if(num != 0) // Valid: Operator '!=' compares two operands for inequality and returns a true/false value
...
```

##### String type

Strings in ZScript are immutable sequences of characters that represent text. String literals can be created by wrapping text in either single or double quotes:

```csharp
"I'm a stringy-string type thingy!";
'Single quoted string';
```

Double quoted strings can use escape characters \n (newline feed), \r (carriage return) and \":

```csharp
"This is a haiku\nIt's snowing on mount fuji\nIsn't ZScript cool?";
```

The previous string internally represents the text:

```
This is a haiku
It's snowing on mount fuji
Isn't ZScript cool?
```
*(I lied about the haiku part, by the way)*

Single quoted strings ignore the \n, \r and \" escape characters, allowing only \' to be used:

```csharp
'I\'m another stringy-string type thingy!';
```

Strings can be concatenated by utilizing the '+' operator on two strings:

```csharp
"Hello, " + "World!"
```
Would result in:
```csharp
"Hello, World!"
```

Additionally, any value can be concatenated with a string:

```csharp
"Current year: " + 2015
```
Would result in:
```csharp
"Current year: 2015"
```

Single and double quoted string literals are simply two ways of representing a string, and can be used interchangeably:

```csharp
'Concatenate ' + "me";
```

Would result in the string:

```csharp
"Concatenate me"
```

Strings can be subscripted to fetch individual characters in the string:

```csharp
var bChar = "abc"[1]; // bChar now equals to char value 'b'
```
*Author note: The C#'s 'char' type has no equivalent type specified in ZScript yet, so there is no type checking of values returned by subscripting strings, meaning they will default to 'any'*

##### Lists

Lists represent typed, mutable collections of objects. List types are defined by enclosing a type of objects the list will contain within square brackets ('[' and ']'):

```csharp
[int]   // Type signature for list of integers
[float] // Type signature for list of floats
// List types can also contain list types in them:
[[int]] // Type signature for list of lists of integers
```
*Author's note: Lists currently map to the C#'s System.Collections.ArrayList object. There are plans to map it to the generic System.Collections.Generic.List<T> type for increased type safety later on.*

List literals can be created by wapping values in square brackets, separated by commas:

```csharp
var list:[int] = [0, 1, 2]; // List of integers containing values 0, 1 and 2
```

Values in a list can be accessed by subscription, that is, using square brackets around the list object, specifying the 0-based index of the value to fetch between the brackets:

```csharp
var list:[int] = [0, 1, 2];
var value:int = list[0];   // value now equals to 0
```

Lists are mutable, so values in a list can be changed with assignment operations:

```csharp
var list:[int] = [0, 1, 2];
list[0] = 1; // List now contains [1, 1, 2]
```

The count of items in a list can be accessed with the .Length member:

```csharp
var list:[int] = [0, 1, 2];
print(list.Length);
```

Prints
```csharp
3
```

##### Object type

Objects are another type of collections of items, but differently from lists, the values are accesed by key, with the type of the key always being a string. The signature for an object is the 'object' type name.
*Author's note: Currently, objects map into an encapsulated C#'s System.Collections.Generic.Dictionary<string, object> object*

Objects are not typed, and any key can contain any type of value within it.

Object literals are defined by enclosing pairs of 'key : value' entries in curly brackets ('{' and '}'):

```csharp
// Creates an object with values 10 and 20, associated with keys entry1 and entry2, respectively
{ entry1: 10, entry2: 20 };
```

Values in objects can be accessed either by subscription or by field-like access:

```csharp
var person:object = { name: "John Doe", age: 30 };

// Field access
var name = person.name;     // name now equals to "John Doe"

// Subscription; notice that the value between the brackets must be a string
var age = person["age"];    // age now equals to 30
```

Additionally, entry names in an object literal can be specified enclosed in a string

```csharp
var obj1:object = { "entry with spaces": 10 };


// Subscription; notice that the value between the brackets must be a string
var value = obj1["entry with spaces"];
```

Trying to access entries not present in an object result in a null value being returned:

```csharp
var obj1:object = { existing: 10 };

var value1 = obj1.existing; // value1 now equals to 10
var value2 = obj1.unexisting; // 'unexisting' entry does not exists in the object, so null is returned; value2 now equals to 'null'
```

##### `any` type

ZScript is a typed language, but sometimes it is handy to make operations dynamically on values without having to worry about what the actual types of these values are. Since operations on values are checked on compile time, we need to use a special type signature to specify that the type is dynamic: that is the purpose of the `any` type.

`any` types are not checked in compilation time, and any operation can be performed on `any`-typed values:

```csharp
// Creates a function that takes two parameters of type 'any', and returns an 'any' type
func sum(num1: any, num2: any) : any
{
  return num1 + num2;
}

var value1:int   = sum(2, 2);     // value1 is now the integer number 2
var value2:float = sum(2.0, 2.0); // value2 is now the float number 2.0
```

Any place you can use a type name, you can use `any`:

```csharp
var list:[any] = [0, "string", 1.0]; // Defines a variable containing list of assorted values
var num:int = list[0]; // The value at index 0 is converted at runtime to an integer
```

Careful has to be employed, though, because deliberate usage of `any` can lower the type safety of a program by enabling invalid operations to be performed at runtime:

```csharp
// Creates a function that takes two parameters of type 'any', and returns an 'any' type
func sum(num1: any, num2: any) : any
{
  return num1 + num2;
}

sum(true, true); // This causes a runtime error because boolean values cannot be summed, but the error is not catched at compile-time!
```


##### Void type

`void` is a special type that is used to mark function return values specifically. The only allowed place to use a void type is in function return type (more about functions is explained in the next sections). Trying to create value holders of type void, or assigning a void-typed value to any value holder is considered a compile-time error.

Void signals that a function does not return a value, and is used to avoid trying to perform invalid operations with functions, like assigning variables to the return type of functions that do not return a value at all.


##### Callable type

Functions in ZScript are first-class, meaning they can be passed around as values that can be later called (i.e. they are a `callable` value). Still, ZScript is statically typed, so in order to statically analyse function calls to analyse parameter types and propagate the return values of these functions properly, a special type exists to describe the signature of a callable value: The Callable type

A callable type has a special syntax that defines traits about the basic type of the callable it describes: the argument list, and the return type of the callable.

An example of a callable signature that takes two integers and returns a float is as follows:

```csharp
// Everything before the '->' arrow is the argument list separated by comma, and the type after the arrow is the return type
(int,int -> float)
```

Notice that the callable type takes only types to describe the signature of the arguments and return type.

Other samples, with different callable types:

```csharp
// The arguments can be omitted to create a parameter-less callable like this:
(-> float)

// If the return value is omitted, void is assumed when used
(->)

// Variadic arguments (more about that in the Function section) are specified with '...' after the argument type
(bool, int...->int)
```

A callable type can be used in any place a normal type can:

```csharp
func larger(i1:int, i2:int) : bool
{
  return i1 > i2;
}

// Assignments of callable types are also type-checked: if the type signature of the variable
// and the type signature of the 'comp' function didn't match, an error would be raised here
var isFirstLarger:(int,int -> bool) = comp;

// Here, the function call is allowed because isFirstLarger has a callable type
var areEqual:bool = isFirstLarger(2, 3); // returns false, by the way
```

The same rules that apply to usage of function calls apply to callable values

```csharp
var someVoidFunc:(int->void) = ...;

var assign = someVoidFunc(); // Invalid: Callable 'someVoidFunc' returns void, which is not a valid type for a variable
```


#### Value holders (Variables and Constants)

Values in ZScript can be stored in constructs called 'value holders'. There are three types of value holders: Variables, constants and function parameters. Function parameters will be discussed in the 'Function' section, the semantics of variables and constants are as follow:

Variables are created by using the 'var' keyword, followed by the variable name, and, optionally, a type and a starting value:

```csharp
var integer:int = 0; // Creates a variable named 'integer' that is of type int, containing a value 0
```

After creating a variable, you can use it in any place a value of its type is expected:

```csharp
var num1:int = 5;
var num2:int = 6;

var sum:int = num1 + num2; // sum is now equals to 11, which is the sum of the values contained within num1 and num2
```

Variables are value holders that can have the value that they contain changed. This means that at any point, a variable may be assigned a different value

```csharp
var integer:int = 0;
integer = 5;   // Set the contents of integer to 5
integer += 10; // Sum integer with 10, and assign the result to itself
// Now integer contains '15'
```

Variables may also optionally start value-less:

```csharp
var integer:int;
integer = 5;   // Set the contents of integer to 5
integer += 10; // Sum integer with 10, and assign the result to itself
// Now integer contains '15'
```
*(Author note: the check for variable usage before initialization is not implemented yet, so trying to access the value of an uninitialised variable is not detected as a compile-time error and will result in runtime-errors)*


Much like variables, constants are created by using the 'let' keyword, followed by the constant name, a starting value and, optionally, a type:

```csharp
let count:int = 10; // Creates a constant named 'integer' that is of type int, containing a value 10
```

Constants are value holders that cannot have different values assigned to them after creation:

```csharp
let count:int = 0;
count = 5;   // Error: Cannot re-assign constant after its creation
count += 10; // Error here too
count++;     // That's an error too, don't try to be sneaky and try to modify constants through increments!
```

Because constants cannot have their value changed, they, unlike variables, require an initial value to be provided at time of creation, otherwise an error is raised.

*Note: Constants only stop you from modifying the reference of the object they point to, that means that if you e.g. create a constant list `let l:[int]`, you can assign to the list through subscription `l[0] = 1`, though you cannot assign the constant value holder any other list*

##### Type inferring

When creating variables and constants, the type of the value holder may be omitted. When that is done, the variable is still typed, but the type is now derived from the type of the initialization expression:

```csharp
var integer = 10; // 'integer' was inferred the type of 'int'
let name = "Joe McDoe"; // 'name' was inferred type 'string'
integer = "sneakyString"; // Error: While 'integer' had no type provided, it was automatically inferred the type 'int', so no other value types can be assigned to it
```

Creating a type-less, valueless variable defaults the type of the value to 'any':

```csharp
var anything; // Equivalent to 'var anything:any;'
anything = 10;
anything = "abc"; // Allowed, the variable 'anything' can hold any type due to the conditions it was initialized in
```



#### Statements

ZScript features a very handy and familiar set of control flow statements:

* `if/else if/else`
* `while`
* `for`
* `switch`

and miscellaneous statements:

* `return`
* `break`
* `continue`
* variables and constant declarations (discussed earlier)

Blocks of statements can be grouped with curly brackets ('{' and '}'), and can be used in place statements are accepted.

The syntax for the control flow statements available in ZScript are as follow:

#### if/else if/else

Syntax: `if( <expression> ) <statement>`, with else: `if( <expression> ) <statement1> else <statement2>`, with chaining of if's: `if( <expression> ) <statement1> else if( <expression> ) <statement2> else if ... else < statement >`

`if` executes a statement if a condition evaluates to true:

```csharp
var a:int = 10;
var b:int = 20;

if(a < b) // a (10) is less than b (20), so this evaluates to true
{
  // Statements contained here get executed
}
```

An if statement can be immediately followed by an else statement, which executes in case the if fails:

```csharp
var a:int = 10;
var b:int = 20;

if(a > b)
{
  print("a is greater than b");
}
else
{
  print("a is not greater than b");
}
```

If/else statements can be chained, ensuring no more than one of the blocks of the chain is executed:

```csharp
var a:int = 10;
var b:int = 20;

if(a > b)
{
  print("a is greater than b");
}
else if(a == b)
{
  print("a is equals to b");
}
else
{
  print("a is less than b");
}
```

Note that the else is optional after 'else if' blocks, too:

```csharp
var a:int = 10;
var b:int = 20;

if(a > b)
{
  print("a is greater than b");
}
else if(a == b)
{
  print("a is equals to b");
}
// None of the above blocks execute
```

The curly brackets around the statements being executed is completely optional:

```csharp
var a:int = 10;
var b:int = 20;

if(a > b)
  print("a is greater than b");
else if(a == b)
  print("a is equals to b");
```

Though multiple sequential statements not in a block will fail, because only the first statement is considered part of the `if` statement:

```csharp
var a:int = 10;
var b:int = 20;

if(a > b)
  print("a is greater than b");
  print("always executes");     // Here, this print always executes because it is interpreted as being outside the `if` statement
```

#### while

Syntax: `while( <condition> ) < statement >`

While is a type of loop control flow that executes a statement until a certain condition evaluates to false.

Sample:
```csharp
var count:int = 0;

while(count < 10)
{
    print(count);
    
    // Increase count by one
    count++;
}
```

Prints:
```
0
1
2
3
4
5
6
7
8
9
```



#### for 

Syntax: `for( <init> ; <condition> ; <increment> ) <statement>`

Performs a 'for' style looping by executing the initial `<init>` statement, and performing the sequence `<condition>` -> `<statement>` -> `<increment>` over and over while `<condition>` does not evaluate to false.

The `<init>` may either be an expression, an assignment expression or a variable declare statement, and the `<condition>` needs to return a bool type.

Sample 1:
```csharp
for(var i = 0; i < 10; i++)
{
    print(i);
}
```

Prints:
```
0
1
2
3
4
5
6
7
8
9
```

The `<init>`, `<condition>` and `<increment>` expressions can be omitted in any combination:

Sample 2:
```csharp
var i = 0;
for(; i < 10;)
{
    print(i);
    i++;
}
```

Is an equivalent of the first sample, and prints:
```
0
1
2
3
4
5
6
7
8
9
```

If the `<condition>` is omitted, the loop runs indefinitely.


#### switch

Syntax: `switch( <expression> ) { case <value1>: <statements>, case <value2>: <statements>, ..., case <valueN>: <statements>, [default: <statements>] }`  
*Author's note: I'm currently evaluating an alternative syntax for the case labels to allow different types of comparisions to be performed with the values and to allow multiple value matches to be made in the same 'case' label (as an alternative to sequential 'case' labels)*

Evaluates the value of an expression, jumping to a specific internal `case` label of matching value, and if it fails to match with any `case` label, jumps to a `default` label, if present.

A switch statement works as a more terse alternative to if/else blocks, in case of chained comparisons of the same value:

Sample 1:
```csharp
var peopleCount = 3;

switch(peopleCount)
{
  case 1:
    print("One person in the house");
    break;
  
  case 2:
    print("Two people in the house");
    break;
  
  case 3:
    print("Three people in the house - the house is full!");
    break;
}
```

Prints:
```csharp
Three people in the house - the house is full!
```

Cases in a switch statement can be grouped by putting one label next to the other. In such cases, the statements after the grouped labels will execute if any of the cases matches the expression:

Sample 2:
```csharp
var peopleCount = 1;

switch(peopleCount)
{
  case 0:
  case 1:
    print("Less than two people in the house");
    break;
  
  case 2:
    print("Two people in the house");
    break;
  
  case 3:
    print("Three people in the house - the house is full!");
    break;
}
```

Prints:
```csharp
Less than two people in the house
```

The `break` statements inside the `case` labels is used to stop the code from flowing through the case labels:

Sample 3:
```csharp
var saladOrders = 2;

switch(saladOrders)
{
  case 1:
    print("One salad order for the table");
    break;
  
  case 2:
    print("Two salad orders for the table");
  
  case 3:
    print("Three salad orders for the table - damn, these 'sum healthy people!");
    break;
}
```

Prints:
```csharp
Two salad orders for the table
Three salad orders for the table - damn, these 'sum healthy people!
```

because the `case 2` label didn't contain a `break` statement: the code 'flowed' through the next case label instead.  
*Author's note: I'm currently evaluating a non-gotcha alternative to implicit case fall-through in the form of a special syntax that makes the fall-through explicit*


##### Things to consider with switch statement

###### Types in case/switch expressions must match
The types on each of the `case` label expressions must match the type of the compared expression provided to the `switch` statement, raising errors if the types do not match:

Sample 4:
```csharp
var dogId = 1;

switch(saladOrders)
{
  case 1:
    print("Spike");
    break;
  
  // Invalid: string type is not compatible with int type
  case "Ruffus":
    print("Ruffus");
  
  case 3:
    print("Fluffy");
    break;
}
```



#### break

Break statements function as context-sensitive flow-breaks. Break statements are only allowed within loop-type statements (`for` and `while`) and `switch` statements.

Switch statements require breaks in `case` labels, in cases where it is not desired to execute more than one label sequentially. If missing, the code 'falls through' the next case label in the sequence:

Sample 1:
```csharp
var saladOrders = 2;

switch(saladOrders)
{
  case 1:
    print("One salad order for the table");
    break;
  
  case 2:
    print("Two salad orders for the table");
  
  case 3:
    print("Three salad orders for the table - damn, these 'sum healthy people!");
    break;
}
```

Prints:
```csharp
Two salad orders for the table
Three salad orders for the table - damn, these 'sum healthy people!
```
because the `case 2` label didn't contain a `break` statement: the code 'flowed' through the next case label instead.

Loop-type statements can be stopped at any time by calling a `break` statement within them:

Sample 1:
```csharp
var count:int = 0;

while(count < 10)
{
    print(count);
    
    // Increase count by one
    count++;
    
    if(count > 4)
    {
        print("break!");
        break;
    }
}
```

Prints:
```
0
1
2
3
4
break!
```

Sample 2:
```csharp
var count:int = 0;

for(;;count++)
{
    print(count);
    
    // Increase count by one
    count++;
    
    if(count > 4)
    {
        print("break!");
        break;
    }
}
```

Is an equivalent of the first sample, and prints:
```
0
1
2
3
4
break!
```



#### continue

Continue statements function as context-sensitive flow-breaks inside loop-type statements (`for` and `while`).

When called inside a loop, the flow is immediately jumped to the next iteration of the loop, just before its condition, effectively skipping the rest of the statements:

```csharp
var count:int = 0;

while(count < 10)
{
    // Increase count by one
    count++;
    
    // When this continue hits, it skips the 'print' statement bellow and jumps right back to the `while`'s condition
    if(count > 4)
        continue;
    
    print(count);
}
```

Prints:
```
1
2
3
4
```

On `for` loops, `continue` statements jump to the next iteration of the loop:

```csharp
for(var i = 0; i < 10; i++)
{
    // When this continue hits, it skips the 'print' statement bellow and jumps right back to the loop's top
    if(i > 4)
        continue;
    
    print(i);
}
```

Prints:
```
0
1
2
3
4
5
```



#### return

Return works as a function-wise stop that breaks out of the currently executed function immediately and returns control to the caller, optionally specifying a value to be returned alongside:

```csharp
func printOne()
{
  print("1");
  
  // Return statement stops the function immediately, preventing the 'print("2")' statement from executing
  return;
  
  print("2");
}
```

On functions that require a return value (more on that on the next section), return statements are required to specify a value to return to the caller:

```csharp
func sum(n1:int, n2:int) : int
{
  // Values are specified by adding a value between the 'return' the keyword and the semicolon
  return n1 + n2;
}
```

More about return statements is explained in the next section about Functions



#### Functions

Statements cannot be executed in the top-level of the file (in previous examples containing what appear to be statements in the top-level are actually implied to be inside a function, and so are the next examples); in order to execute statements they must be contained within functions.

Functions in ZScript are defined with `func` keyword, followed by a parameter list (which can be empty; see bellow for function parameters):

```csharp
func printHello()
{
  print("Hello!");
}
```


###### Specifying a return type

Functions in ZScript can be made to return values to the caller by adding a `: <type>` bit after the parenthesis:

```csharp
// Function that is guaranteed to return a non-randomly chosen value 10 to the caller. Very useful when no randomness is expected
func nonrandom() : int // Specifies that the function returns an int
{
  return 10;
}
```

Return types are also the only valid place `void` is an accepted type, used to infer that a function returns no value:

```csharp
// Handy shorcut function for when routinely printing 10: It saves you exactly 0 characters over writing 'print(10)', and was a standard coding practice back in the golden 'C' ages.
func print10() : void
{
  print(10);
  
  // Notice the lack of a 'return' keyword: returns are optional in void functions
}
```

Return types can be omitted, and when omitted, a return type of `void` is implied:

```csharp
func example1() : void
{
  ...
}
func example2()
{
  
}
```

Both functions `example1` and `example2` above have a return value of `void`, with `example2` being automatically set as void because it lacked an explicit return type.

The specification of a return type changes the semantics of `return` statements contained within the function: Non-void functions must specify a value in their return statements, while void functions do not allow for values to be specified:

```csharp
func nonVoidFunc() : int
{
  return 0; // Removing the 0 value causes a compile-time error: Non-void functions require a return value!
}
func voidFunc() : void
{
  //return 0; // Uncommenting this line causes a compile-time error: Void functions cannot specify a return value!
}
```

As with the rest of the language, return values are typed, and are type-checked against the return type specified by the function signature.


###### Parameters

Functions can contain parameters, which are values that are specified by whomever is calling the function, and can be accessed like any variable:

```csharp
func printNumber(n:int)
{
  print(n);
}

...
printNumber(10);
...
```

Prints
```csharp
10
```

Functions can specify any number of parameters to be provided by separating them with commas:

```csharp
func printSum(n1:int, n2:int)
{
  print(n1 + n2);
}

...
printSum(2, 2);
...
```

Prints
```csharp
4
```

Parameters are typed values, and the type is used to check that the values being provided are valid:

```csharp
func printNumber(n:int)
{
  print(n);
}

...
print(true); // Error: Function expects an int value, but receives a boolean value
...
```

When defining function parameters, it is to sometimes useful to omit a value and let the function decide what value to use instead. That is done with optional parameters, which are specified by adding `= < value>` in front of the parameter:

```csharp
func increment(number:int, amount:int = 1) : int
{
  return number + amount;
}

...
print(increment(4));
print(increment(4, 2));
...
```

Prints
```csharp
5
6
```
*Note: In case a default value is provided, the type of the parameter can be inferred*  
*Note: The default value of a function argument must be a constant (or 'compile-time') value*

Functions can also specify a special parameter called `variadic` parameter which can receive any number of arguments passed by the caller. To create a variadic parameter, `...` is added after the parameter's type:

```csharp
func sumMany(values:int...) : int
{
  var sum = 0;
  for(var i = 0; i < values.Length; i++)
  {
    sum += values[i];
  }
  return sum;
}

...
print(sumMany(1, 2, 3, 4));
...
```

Prints
```
10
```

Variadic parameters are equivalent to lists: they can be subscripted and the count of values can be accessed with the .Length member.

*Note: Variadic parameters cannot specify a default value*

###### Things to consider with `return` statements:

Functions that have a return value require that all return statements specify a value:

```csharp
func integerDivide(n1:int, n2:int) : int
{
  if(n2 != 0)
     return n1 / n2;
  
  return; // Invalid: all return statements require a value
}
```

In case the function specifies a return value, all possible code paths must return a value, that means that e.g. in an `if/else` condition, both sides must return values:

```csharp
// Invalid: not all code paths return a value
func integerDivide(n1:int, n2:int) : int
{
  if(n1 != 0)
  {
    return n1 / n2;
  }
  else
  {
    print("Cannot divide by 0");
  }
}
```

#### Export functions

TODO

#### Closures

TODO

#### Objects

TODO

#### Sequences

TODO

##### About the VM

The VM (FunctionVM) is a stack-based virtual machine that utilizes special token stream constructs as instructions to execute. The function VM itself has no memory access, requiring an external object (VmContext) that provides access for memory and the runtime to call functions on.
