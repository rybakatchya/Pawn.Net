# Pawnx64.Net

A simple, efficient wrapper around [PawnX64](https://github.com/rybakatchya/Pawn.Net/blob/main/PawnX64.md).


When benchmarked against other .net scripting solutions PawnX64.Net performs much better.

## Pawnx64.net
![Alt text](res/pawn_benchmark.png?raw=true "Pawnx64.net")

## Lua via MoonSharp
![Alt text](res/lua_benchmark.png?raw=true "Lua")

## js via Jint
![Alt text](res/js_benchmark.png?raw=true "Js")


## C# via reflection
![Alt text](res/reflection_benchmark.png?raw=true "Pawn")

## Why use Pawnx64.Net and not reflection then?
Use Pawnx64.Net when you want to run untrusted code in a sanbox, or when you need memory efficciency. Pawn has no garbage collector and thus no runtime memory allocations.

## Extending Pawnx64.net
You can register natives in your host application. You can also write plugins in C# or C see NativePluginTest.c and PluginTest.cs

## Documentation
Coming soon see ManagedTests for now and get the pawn scripts from Releases.