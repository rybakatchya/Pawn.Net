using PawnBindings;
using System;
using System.Runtime.InteropServices;

namespace ManagedTests
{
    internal class Program
    {
       
        static unsafe int Print(IntPtr loader, IntPtr _amx, IntPtr userdata, IntPtr returnValue, long argc, IntPtr argv)
        {
             
            var span = new ReadOnlySpan<long>(argv.ToPointer(), (int)argc);
            var str = AmxCell.FromVirtualAddress(span[1]);
            Console.WriteLine(str.ReadString(_amx, (int)span[0]));
            return 1;
        }


        static AMXLoader loader;
        static AMX amx;
        static unsafe void Main(string[] args)
        {
            loader = AMXLoader.Init();

            
            loader.RegisterNative("managedPrint", Print);
          

            
            long main = loader.LoadFile("ProjectTemplate/main.amx");
            

            amx = loader.GetAMX();
            
            AmxStatus status;
            if((status = amx.Call(main)) != AmxStatus.AMX_SUCCESS)
                Console.WriteLine(status.ToString());

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

            /*
            long val = (long)amx.Allocator.Allocate((ulong)Marshal.SizeOf<TagTestData>());
            
            var ptr = amx.Translate((long)val, Marshal.SizeOf<TagTestData>());
            //Marshal.StructureToPtr(test, ptr, true);
            var span = new Span<TagTestData>(ptr.ToPointer(), Marshal.SizeOf<TagTestData>());
            span[0] = test;
            var idx = loader.FindPublic("runTest");
            var stat = amx.Call(amx, idx,1, &val); */
            //Console.WriteLine(stat.ToString());
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
