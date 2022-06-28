using PawnBindings;
using System;
using System.Runtime.InteropServices;

namespace ManagedTests
{
    internal class Program
    {

        static int TestWriteString(IntPtr loader, IntPtr _amx, IntPtr userdata)
        {
            AmxStatus status;
            if((status = amx.WriteString(1, "Well fiddle dee dee")) != AmxStatus.AMX_SUCCESS)
            {
                Console.WriteLine(status.ToString());
                return 0;
            }
            return 1;
        }

        
        static int PrintInt(IntPtr loader, IntPtr _amx, IntPtr userdata)
        {
            int val = 0;
            var status = amx.ReadInt32(1, out val);
            if(status != AmxStatus.AMX_SUCCESS)
            {
                Console.WriteLine(status.ToString());
            }
            Console.WriteLine(val.ToString());
            return 1;
        }
        static int Print(IntPtr loader, IntPtr _amx, IntPtr userdata)
        {
            Console.WriteLine(amx.ReadString(2));
            return 1;
        }

        static int TestInt(IntPtr loader, IntPtr _amx, IntPtr userdata)
        {
            int value = 0;
            var status = amx.ReadInt32(1, out value);
            if (status != AmxStatus.AMX_SUCCESS)
            {
                Console.WriteLine(status.ToString());
                return 0;
            }
            Console.WriteLine(value.ToString());
            return 1;
        }

        //currently broken.
        static int TestFloat(IntPtr loader, IntPtr _amx, IntPtr userdata)
        {
            float value = 0;
            var status = amx.ReadFloat(1, out value);
            if(status != AmxStatus.AMX_SUCCESS)
            {
                Console.WriteLine(status.ToString());
                return 0;
            }
            Console.WriteLine(value.ToString());
            return 1;
        }

        static int SetInt(IntPtr loader, IntPtr _amx, IntPtr userdata)
        {
            var status = amx.WriteInt32(1, 3444);
            if (status != AmxStatus.AMX_SUCCESS)
            {
                Console.WriteLine(status.ToString());
                return 0;
            }

            return 1;
        }

        static int TestTag(IntPtr loader, IntPtr _amx, IntPtr userData)
        {
            AMXCell cell = default(AMXCell);
            var status = amx.CellReadAligned(1, out cell);
            if (status != AmxStatus.AMX_SUCCESS)
            {
                Console.WriteLine(status.ToString());
                return 0;
            }
            var data = Marshal.PtrToStructure<TagTestData>((IntPtr)cell.Pointer);
            Console.WriteLine("{0} {1}", data.id, data.rot);
            return 1;
        }


        static AMXLoader loader;
        static AMX amx;
        static unsafe void Main(string[] args)
        {
            loader = AMXLoader.Init();

            
            loader.RegisterNative("managedPrint", Print);
            /*loader.RegisterNative("testInt", TestInt);
            loader.RegisterNative("testFloat", TestFloat);
            loader.RegisterNative("testTag", TestTag);
            loader.RegisterNative("setInt", SetInt);
            loader.RegisterNative("setString", TestWriteString);*/
            loader.RegisterNative("print_int", PrintInt);

            //loader.LoadLibrary("TestPlugin.dll");
            //loader.LoadLibrary("NativePluginTest.dll");
            AMXCell main;
            var status = loader.LoadFile("ProjectTemplate/main.amx", out main);
            //AMXCell tagTest = loader.FindPublic("TagTest");
            if(status != AmxStatus.AMX_SUCCESS)
            {
                Console.WriteLine("File cannot be loaded {0}", status.ToString());
            }
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
