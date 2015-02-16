ZScript
=======
A game scripting programming language.


Some considerations, beforehand
===============================

I did this mostly for fun and to learn about implementations of real programming languages with useful features ('useful' being used very loosely here, though).
I'm really not proud nor do I know exactly why I re-implemented a type system when .NET offers a tested and working one, pretty much for free. Other than that, most of the stuff I did was solely so I could learn about the ins and outs of writing an interpreted language that is run via a VM.

About the VM
------------

The VM (FunctionVM) is a stack-based virtual machine that utilizes special token stream constructs as instructions to execute. The function VM itself has no memory access, requiring an external object (VmContext) that provides access for memory and the runtime to call functions on.