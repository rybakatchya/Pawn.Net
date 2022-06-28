using BenchmarkDotNet.Attributes;
using Jint;
using MoonSharp.Interpreter;
using PawnBindings;
using System.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Benchmark
{
    internal abstract class BenchmarkBase
    {
        
        public abstract void CallFuntion1000Times();

        public abstract void DoFib();

        public abstract void CallFunctionWithObject1000Times();


    }


    [MemoryDiagnoser]
    public class ReflectionBenchmark
    {
        private MethodInfo? normalMethod;
        private MethodInfo? objectMethod;
        private MethodInfo? fibMethod;
        private Assembly ass;
        public ReflectionBenchmark()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "scripts", "TestPlugin.dll");
            
            ass = Assembly.LoadFrom(path);

          

            var type = ass.GetType("TestPlugin.TestReflection");
            if(type == null)
            {
                Console.WriteLine("type not found");
                return;
            }    
            normalMethod = type.GetMethod("NormalMethod", BindingFlags.Static);
            objectMethod = type.GetMethod("ObjectMtheod", BindingFlags.Static);
            fibMethod = type.GetMethod("DoFib", BindingFlags.Static);
        }

        [Benchmark]
        public void CallFunction1000Times()
        {
            for (int i = 0; i < 1000; i++)
                normalMethod?.Invoke(null, null);
        }

        IntPtr ptr = IntPtr.Zero;

        [Benchmark]
        public unsafe void CallFunctionWithObject1000Times()
        {
            if (ptr == IntPtr.Zero)
            {
                var obj = new TestObject();
                obj.valueOne = 35;
                obj.valueTwo = 40;

                var spa = stackalloc TestObject[1];

                spa[0] = obj;
                ptr = (IntPtr)spa;


            }
            for (int i = 0; i < 1000; i ++)
            {
                objectMethod?.Invoke(null, new object[] { ptr });
            }
        }

        [Benchmark]
        public void DoFib()
        {
            fibMethod?.Invoke(null, null);
        }
    }

    [MemoryDiagnoser]
    public class JSBenchmark
    {
        string scriptCode = @"   
        function doTest()
        {
    
        }

        function doObject(obj)
        {      
            return 1;
        }

        function doFib(n)
        {
            var last = 0;
            var cur = 1;
            n = n - 1;
            while(n)
            {
                --n;
                var tmp = cur;
                cur = last + cur;
                last = tmp;
            }
            return cur;
        }
        ";

        Engine engine;
        public JSBenchmark()
        {
            engine = new Engine();
            engine.Execute(scriptCode);
        }

        [Benchmark]
        public void CallFunction1000Times()
        {
            var val = engine.GetValue("doTest");
            for(int i = 0; i < 1000; i ++)
                engine.Invoke(val);
        }

        [Benchmark]
        public void DoFib()
        {
            engine.Invoke(engine.GetValue("doFib"), 33);
        }

        IntPtr ptr = IntPtr.Zero;
        [Benchmark]
        public unsafe void CallFunctionWithObject1000Times()
        {
            if (ptr == IntPtr.Zero)
            {
                var obj = new TestObject();
                obj.valueOne = 35;
                obj.valueTwo = 40;

                var spa = stackalloc TestObject[1];

                spa[0] = obj;
                ptr = (IntPtr)spa;


            }
            var val = engine.GetValue("doObject");
            for (int i = 0; i < 1000; i++)
            {
                engine.Invoke(val, ptr);
            }

        }


    }

    [MemoryDiagnoser]
    public class LUABenhmark
    {
        string scriptCode = @"

        function doTest()
        
        
        end

        function doObject(ptr)
        
        end

        

		-- defines a factorial function
        function doFib(n)

            local last = 0
            local cur = 1
            n = n - 1
            while (n > 0)
            do
                n = n - 1
                local tmp = cur
                cur = last + cur
                last = tmp
            end
            return cur
        end
        ";

        private Script script;
        public LUABenhmark()
        {
            script = new Script();
            script.DoString(scriptCode);
            
        }

        [Benchmark]
        public void CallFuntion1000Times()
        {
            for (int i = 0; i < 1000; i++)
            {
                DynValue res = script.Call(script.Globals["doTest"]);
                
            }

        }
        IntPtr ptr = IntPtr.Zero;

        [Benchmark]
        public unsafe void CallFunctionWithObject1000Times()
        {
            if (ptr == IntPtr.Zero)
            {
                var obj = new TestObject();
                obj.valueOne = 35;
                obj.valueTwo = 40;

                var spa = stackalloc TestObject[1];

                spa[0] = obj;
                ptr = (IntPtr)spa;


            }


            for (int i = 0; i < 1000; i++)
            {
                // create a userdata, again, explicitely.
                DynValue obj = UserData.Create(ptr);
                DynValue res = script.Call(script.Globals["doObject"], obj);
            }
        }

        [Benchmark]
        public void DoFib()
        {
            DynValue res = script.Call(script.Globals["doFib"], 33);
        }


    }

    [MemoryDiagnoser]
    public class PawnBenchmark
    {
        AMXLoader loader;
        AMX amx;


        int Print(IntPtr loader, IntPtr _amx, IntPtr userdata)
        {
            Console.WriteLine(amx.ReadString(2));
            return 1;
        }

      

        public int Add(IntPtr loader, IntPtr _amx, IntPtr userdata)
        {
            amx.ReadInt32(1, out int a);
            amx.ReadInt32(2, out int b);
            return a + b;
        }

        private AMXCell testCallback;
        private AMXCell objectCallback;
        private AMXCell fibCallback;
        private ArrayPool<long> ptrPool;
        
        public PawnBenchmark()
        {
            ptrPool = ArrayPool<long>.Create(1024, 1024);
            loader = AMXLoader.Init();

            loader.RegisterNative("managedPrint", Print);

            AMXCell main;
            var status = loader.LoadFile("scripts/main.amx", out main);
           
            if (status != AmxStatus.AMX_SUCCESS)
            {
                Console.WriteLine("File cannot be loaded {0}", status.ToString());
            }
            amx = loader.GetAMX();
            
            testCallback = loader.FindPublic("test");
            objectCallback = loader.FindPublic("test_int");
            fibCallback = loader.FindPublic("do_fib");
            amx.Call(main);


           

        }
        [Benchmark]
        public void CallFuntion1000Times()
        {
            for (int i = 0; i < 1000; i++)
            {
                var status = amx.Call(testCallback);
                if (status != AmxStatus.AMX_SUCCESS)
                    Console.WriteLine((status.ToString()));
            }
        }
        IntPtr ptr = IntPtr.Zero;

        [Benchmark]
        public unsafe void CallFunctionWithObject1000Times()
        {
            ulong val = 0;
            if (ptr == IntPtr.Zero)
            {
                var obj = new TestObject();
                obj.valueOne = 35;
                obj.valueTwo = 40;


                ulong size = (ulong)Marshal.SizeOf<TestObject>();
               
                val = amx.Allocator.Allocate(size);

                ptr = amx.Translate((long)val, (long)size);
                Marshal.StructureToPtr(obj, ptr, true);

            }
                
            
            for(int i = 0; i < 1000; i++)
            {
                //amx.Push(AMXCell.FromPointer(ptr));
                // var status = amx.Call(objectCallback, new AMXCell[] { AMXCell.FromPointer(ptr) });
                //var ptrs = ptrPool.Rent(1);
                //ptrs[0] = (long)ptr;
                //var status = amx.Call(objectCallback, 1, (long*)ptr);
                var status = amx.Call(amx, objectCallback, 1, (long*)ptr);
                //ptrPool.Return(ptrs, true);
                if(status != AmxStatus.AMX_SUCCESS)
                {
                    Console.WriteLine(status.ToString());
                }

            }
            amx.Allocator.Free(val);
        }
        [Benchmark]
        public void DoFib()
        {
            //amx.Push(AMXCell.FromInt(33));
            //var status = amx.Call(fibCallback);
            //var status = amx.Call(fibCallback, new AMXCell[] { AMXCell.FromInt(33) });
            var data = ptrPool.Rent(1);
            //data[0] = (long)33;
           // var status = amx.Call(fibCallback, 1, data);
            ptrPool.Return(data, true);
            var status = AmxStatus.AMX_SUCCESS;
            if(status != AmxStatus.AMX_SUCCESS)
            {
                Console.WriteLine(status.ToString());
            }
        }
    }

    public struct TestObject
    {
        public int valueOne;
        public int valueTwo;
    }
    
}
