// See https://aka.ms/new-console-template for more information
using Benchmark;
using BenchmarkDotNet.Running;
using System.Diagnostics;

PawnBenchmark pb = new PawnBenchmark();
LUABenhmark lua = new LUABenhmark();
JSBenchmark js = new JSBenchmark();
ReflectionBenchmark reflection = new ReflectionBenchmark();
//var summary = BenchmarkRunner.Run<PawnBenchmark>();

Stopwatch sw = new Stopwatch();
sw.Start();
long ticks = sw.ElapsedTicks;
double ns = 1000000000.0 * (double)ticks / Stopwatch.Frequency;

pb.CallFuntion1000Times();
pb.CallFunctionWithObject1000Times();
pb.DoFib();
lua.CallFuntion1000Times();
lua.CallFunctionWithObject1000Times();
lua.DoFib();
js.CallFunction1000Times();
js.CallFunctionWithObject1000Times();
js.DoFib();
reflection.CallFunction1000Times();
reflection.CallFunctionWithObject1000Times();
reflection.DoFib();
sw.Restart();
pb.CallFuntion1000Times();
ticks = sw.ElapsedTicks;
ns = 1000000000.0 * (double)ticks / Stopwatch.Frequency;
Console.WriteLine("Execute parameterless callback 1000 times in pawn {0}ms", ns / 1000000);


sw.Restart();
lua.CallFuntion1000Times();
ticks = sw.ElapsedTicks;
ns = 1000000000.0 * (double)ticks / Stopwatch.Frequency;
Console.WriteLine("Execute parameterless callback 1000 times in lua {0}ms", ns / 1000000);

sw.Restart();
pb.CallFunctionWithObject1000Times();
ticks = sw.ElapsedTicks;
ns = 1000000000.0 * (double)ticks / Stopwatch.Frequency;
Console.WriteLine("Execute parameterized callback 1000 times in pawn {0}ms", ns / 1000000);

sw.Restart();
lua.CallFunctionWithObject1000Times();
ticks = sw.ElapsedTicks;
ns = 1000000000.0 * (double)ticks / Stopwatch.Frequency;
Console.WriteLine("Execute parameterized callback 1000 times in lua {0}ms", ns / 1000000);


sw.Restart();
pb.DoFib();
ticks = sw.ElapsedTicks;
ns = 1000000000.0 * (double)ticks / Stopwatch.Frequency;
Console.WriteLine("Execute fib in pawn {0}ms", ns / 1000000);

sw.Restart();
lua.DoFib();
ticks = sw.ElapsedTicks;
ns = 1000000000.0 * (double)ticks / Stopwatch.Frequency;
Console.WriteLine("Execute fib in lua {0}ms", ns / 1000000);



BenchmarkRunner.Run<PawnBenchmark>();
//Console.WriteLine(summary.text
//Console.WriteLine("Hello, World!");
Console.ReadLine();
