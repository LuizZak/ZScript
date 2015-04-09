![](http://i.imgur.com/YR3ejlH.png)  
An (once) game scripting programming language implemented in C#.

[![Build status](https://ci.appveyor.com/api/projects/status/19mw81qroui8q8xp?svg=true)](https://ci.appveyor.com/project/LuizZak/zscript)

## Some considerations, beforehand

I did this mostly for fun and to learn about implementations of real programming languages, and also to use in my games. It contains a mish-mash of features of other languages that I find cool and useful, like static typing, type inferring and closures. I don't intend on this becoming in the future a "real" game/general scripting language in any way, and this is mostly a personal hobby project.

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

##### About the VM

The VM (FunctionVM) is a stack-based virtual machine that utilizes special token stream constructs as instructions to execute. The function VM itself has no memory access, requiring an external object (VmContext) that provides access for memory and the runtime to call functions on.

## Licensing

The project is licensed under LGPL 2.1. The license can be found in the [License.txt](https://github.com/LuizZak/ZScript/blob/master/License.txt) file in the root of the repository tree.

## More Information

For more information about the language, please visit the [project's wiki](https://github.com/LuizZak/ZScript/wiki) which contains an assortment of documentation related to the language.
