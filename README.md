# Pawnx64.Net

A simple, efficient wrapper around [PawnX64](https://github.com/rybakatchya/Pawn.Net/blob/main/PawnX64.md).

## What is it?
Pawn is a simple, typeless, scripting language with a C-like syntax. The Pawn compiler outputs P-code (or bytecode) that subsequently runs on an
abstract machine. Execution speed, stability, simplicity and a small footprint were essential design criterions for both the language and the abstract
machine. Pawn prealloates all memory at startup and 

This is an updated version of the pawn scripting language. Using Pawnx64 as a jit compiler. Please see [PawnX64](https://github.com/rybakatchya/Pawn.Net/blob/main/PawnX64.md)
for a list of changes from the original language.

## What is it useful for?
Allowing end users to extend your applications in a safe sandboxed enviroment. There is no built in standard library so the language has no functions to call
unless you implement them. This is greate for securely executing untrusted code on any machine.

Pawn has no runtime memory allocations or even a garbage collector, this makes pawn an ideal choice for making a mod sdk for video games.

When benchmarked against other .net scripting solutions PawnX64.Net performs much better.

## Benchcmarks 

## Pawnx64.net
![Alt text](res/pawn_benchmark.png?raw=true "Pawnx64.net")

## Lua via [MoonSharp](https://github.com/moonsharp-devs/moonsharp)
![Alt text](res/lua_benchmark.png?raw=true "Lua")

## js via [Jint](https://github.com/sebastienros/jint)
![Alt text](res/js_benchmark.png?raw=true "Js")


## C# via [Reflection](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection)
![Alt text](res/reflection_benchmark.png?raw=true "Pawn")

## Why use Pawnx64.Net and not reflection then?
Use Pawnx64.Net when you want to run untrusted code in a sanbox. As CAS is not longer supported, you cannot sandbox reflected assemblies. When you need memory efficciency. 
Pawn has no garbage collector and thus no runtime memory allocations. All memory for pawn is allocated at startup in a linear block.

## Extending Pawnx64.net
You can register natives in your host application. You can also write plugins in C# or C see [NativePluginTest](https://github.com/rybakatchya/Pawn.Net/blob/main/Pawn.Net/NativePluginTest/plugin.h) and [PluginTest.cs](https://github.com/rybakatchya/Pawn.Net/blob/e120bba0ce4f51c573e456ba0b16bff848456dbf/Pawn.Net/TestPlugin/Class1.cs#L35)

## Documentation
Coming soon see ManagedTests for now and get the pawn scripts from Releases.