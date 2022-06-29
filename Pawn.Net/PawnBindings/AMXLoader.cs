using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace PawnBindings
{
    
    public class AMXLoader : IDisposable
    {
        private IntPtr amxLoaderPtr;
        private AMX amx;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int NativeDelegate(IntPtr loader,
            IntPtr amx, IntPtr userdata, IntPtr returnValue,
            long argc,
            IntPtr argv
            );

        private List<object> loadedPlugins = new List<object>();

        //Have to keep a list of these natives to make sure they stay in scope.
        private List<NativeDelegate> natives = new List<NativeDelegate>();

        public static AMXLoader Init()
        {
            return new AMXLoader() { amxLoaderPtr = amx_loader_alloc() };
        }

        public unsafe long LoadFile(string filename)
        {
            var bytes = File.ReadAllBytes(filename);

            fixed (byte* ptr = bytes)
            {
                long val = 0;
                var status = amx_loader_load(amxLoaderPtr, (IntPtr)ptr, bytes.Length, &val);
                amx = new AMX(amx_loader_get_amx(amxLoaderPtr));
                if(status != AmxStatus.AMX_SUCCESS)
                    throw new Exception(status.ToString());
                return val;
            }
        }

        bool IsAssembly(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))

            // Try to read CLI metadata from the PE file.
            using (var peReader = new PEReader(fs))
            {
                if (!peReader.HasMetadata)
                {
                    return false; // File does not have CLI metadata.
                }

                // Check that file has an assembly manifest.
                MetadataReader reader = peReader.GetMetadataReader();
                return reader.IsAssembly;
            }

        }

        public void LoadLibrary(string name)
        {
            var path = Path.Combine("plugins", name);

            if(File.Exists(path) == false)
            {
                Console.WriteLine("File {0} does not exist", name);
                return;
            }

            if (IsAssembly(path))
            {
                Assembly plugin = Assembly.LoadFrom(path);

                foreach (var type in plugin.GetTypes())
                {
                    if (type.BaseType.Name != "PawnPluginBase")
                        continue;
                    loadedPlugins.Add(Activator.CreateInstance(type, amx, this));

                }
            }
            else
            {
                if(NativeLibrary.TryLoad(path, out IntPtr handle) == false)
                {
                    Console.WriteLine("Failed to load {0} invalid format.", Path.GetFileName(path));
                    return;
                }

                if(NativeLibrary.TryGetExport(handle, "plugin_init", out IntPtr address) == false)
                {
                    Console.WriteLine("Failed to find entry point for {0}", Path.GetFileName(path));
                    return;
                }

                NativeLibrary.Invoke(handle, address, typeof(InitDelegate));
            }
        }

        private delegate void InitDelegate();
        public void RegisterNative(string name, NativeDelegate native)
        {
            //NativeDelegate del = Marshal.GetFunctionPointerForDelegate(native);
            natives.Add(native);
            amx_loader_register_native(amxLoaderPtr, name, Marshal.GetFunctionPointerForDelegate(native));
        }

        public AMX GetAMX()
        {
            return amx;
        }


        [DllImport("amx64.dll")]
        public static extern IntPtr amx_loader_alloc();


        [DllImport("amx64.dll")]
        public static extern void amx_loader_register_native(IntPtr loader, string name, IntPtr callback);
        
        [DllImport("amx64.dll")]
        public static unsafe extern AmxStatus amx_loader_load(IntPtr loader, IntPtr bytes, long size, long* main);

        [DllImport("amx64.dll")]
        public static unsafe extern IntPtr amx_loader_get_amx(IntPtr loader);

        [DllImport("amx64.dll")]
        public static extern void amx_loader_free(IntPtr loader);
        
        [DllImport("amx64.dll")]
        public static extern long amx_loader_find_public(IntPtr loader, string name);

        [DllImport("axm64.dll")]
        public static extern long amx_loader_find_pubvar(IntPtr loader, string name);

        [DllImport("amx64.dll")]
        public static extern long amx_loader_find_tag(IntPtr loader, string name);

        public void Dispose()
        {
            amx_loader_free(amxLoaderPtr);
        }

        public long FindPublic(string name)
        {
            var ptr = amx_loader_find_public(amxLoaderPtr, name);
            return ptr;
        }

        public long FindPubVar(string name)
        {
            return amx_loader_find_pubvar(amxLoaderPtr, name);
        }

        public long FindTag(string name)
        {
            return amx_loader_find_tag(amxLoaderPtr, name);
        }
    }
}
