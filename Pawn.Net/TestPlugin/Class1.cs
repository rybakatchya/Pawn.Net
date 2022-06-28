using PawnBindings;
using PawnPlugin;
using System;
using System.Runtime.InteropServices;
namespace TestPlugin
{
    public class TestReflection
    {
        public static void NormalMethod()
        {

        }

        public static void ObjectMethod(IntPtr ptr)
        {

        }


        public static int DoFib(int n)
        {
            int last = 0;
            int cur = 1;
            n = n - 1;
            while (n != 0)
            {
                --n;
                int tmp = cur;
                cur = last + cur;
                last = tmp;
            }
            return cur;
        }
    }
    public class Class1 : PawnPluginBase
    {

        public int TestPlugin(IntPtr loader, IntPtr _amx, IntPtr userData)
        {
            Console.WriteLine("Called from plugin");
            return 1;
        }
        public Class1(AMX amx, AMXLoader loader) : base(amx, loader)
        {
            PreInit += test;
            PostInit += postInit;
            loader.RegisterNative("testPlugin", TestPlugin);
            Console.WriteLine("Loaded");
        }

        private void postInit()
        {
            throw new NotImplementedException();
        }

        private void test()
        {
            throw new NotImplementedException();
        }
    }
}
