using PawnBindings;
using System;
using System.Runtime.InteropServices;

namespace ManagedTests
{
    internal class Program
    {
       
        static int Print(IntPtr loader, IntPtr _amx, IntPtr userdata)
        {
            //Start at 1 not 0.
            var cell = amx.GetCell(1);

            Console.WriteLine();
            return 1;
        }


        static AMXLoader loader;
        static AMX amx;
        static unsafe void Main(string[] args)
        {
            loader = AMXLoader.Init();

            
            loader.RegisterNative("managedPrint", Print);
          

            //loader.LoadLibrary("TestPlugin.dll");
            //loader.LoadLibrary("NativePluginTest.dll");
            
            long main = loader.LoadFile("ProjectTemplate/main.amx");
            //AMXCell tagTest = loader.FindPublic("TagTest");

            amx = loader.GetAMX();

            amx.Call(main);

            var test = new TagTestData()
            {
                //name = "test",
                id = 343,
                rot = 3.2f
            };

            var other = new TagTestData()
            {
                id = 344,
                rot = 3.2f
            };
            long val = (long)amx.Allocator.Allocate((ulong)Marshal.SizeOf<TagTestData>());
            
            var ptr = amx.Translate((long)val, Marshal.SizeOf<TagTestData>());
            //Marshal.StructureToPtr(test, ptr, true);
            var span = new Span<TagTestData>(ptr.ToPointer(), Marshal.SizeOf<TagTestData>());
            span[0] = test;
            var idx = loader.FindPublic("runTest");
            var stat = amx.Call(amx, idx,1, &val); 
            Console.WriteLine(stat.ToString());
            Console.ReadLine();
        }
    }

    
    [StructLayout(LayoutKind.Sequential)]
    public struct TagTestData
    {
        public int id;
        public float rot;
    }
}
