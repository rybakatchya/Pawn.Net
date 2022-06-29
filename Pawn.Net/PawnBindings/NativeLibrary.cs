
using System;
using System.Runtime.InteropServices;

public class NativeLibrary
{
#if WINDOWS
	private const string Kernel32 = "Kernel32";
	[DllImport(Kernel32, EntryPoint = "LoadLibraryW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Winapi)]
	private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string name);

	[DllImport(Kernel32, EntryPoint = "GetProcAddress", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Winapi)]
	private static extern IntPtr GetProcAddress(IntPtr module, [MarshalAs(UnmanagedType.LPStr)] string name);

	[DllImport(Kernel32, EntryPoint = "FreeLibrary", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
	private static extern bool FreeLibrary(IntPtr module);
	
#elif LINUX

	private const string LibDl = "libdl";
	private const UnmanagedType StringType =
			UnmanagedType.LPStr;

	[Flags]
	private enum LibDlFlags
	{
		// ReSharper disable UnusedMember.Local
		Lazy = 0x00001,
		Now = 0x00002,
		Global = 0x00100,
		Local = 0,
		NoDelete = 0x01000,
		NoLoad = 0x00004,
		DeepBind = 0x00008
		// ReSharper restore UnusedMember.Local
	}


	[DllImport(LibDl, EntryPoint = "dlopen", CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr DlOpen([MarshalAs(StringType)] string name, LibDlFlags flags);

	[DllImport(LibDl, EntryPoint = "dlsym", CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr DlSym(IntPtr module, [MarshalAs(StringType)] string name);

	[DllImport(LibDl, EntryPoint = "dlclose", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
	private static extern int DlClose(IntPtr handle);

	[DllImport(LibDl, EntryPoint = "dlerror", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr DlError();
#endif

	public static bool TryLoad(string name, out IntPtr handle)
	{
		try
		{ 
#if WINDOWS
		handle = LoadLibrary(name);
#elif LINUX

			handle = DlOpen(null, LibDlFlags.Local | LibDlFlags.Now);
#endif
		}
		catch
		{
			handle = IntPtr.Zero;
			return false;
		}
			return handle != IntPtr.Zero;
	}

	public static IntPtr Load(string name)
	{
		IntPtr handle = IntPtr.Zero;

#if WINDOWS
		handle = LoadLibrary(name);
#elif LINUX
		handle = DlOpen(null, LibDlFlags.Local | LibDlFlags.Now);
#endif
		return handle;

	}

	public static IntPtr GetExport(IntPtr handle, string name)
	{
		IntPtr symbol = IntPtr.Zero;
#if WINDOWS
		symbol = GetProcAddress(handle, name);
#elif LINUX

		if (name == null)
			throw new ArgumentNullException(nameof(name));
		if (string.IsNullOrWhiteSpace(name) || name.Trim() == "\0")
			throw new ArgumentException("Empty or whitespace symbol names are not allowed", nameof(name));
		symbol = DlSym(handle, name);
#endif
		return symbol;
	}

	public static bool TryGetExport(IntPtr handle, string name, out IntPtr address)
	{
		IntPtr symbol = IntPtr.Zero;
        try
        {
#if WINDOWS
			symbol = GetProcAddress(handle, name);
#elif LINUX
            symbol = DlSym(handle, name);
#endif
		}
        catch
        {
			address = IntPtr.Zero;
			return false;
        }

		address = symbol;
		return symbol != IntPtr.Zero;
	}

	 
	public static void Invoke(IntPtr handle, IntPtr exportAddress, Type type, params object[] args)
    {
		
		var i = Marshal.GetDelegateForFunctionPointer(exportAddress, type);
		i.DynamicInvoke(args);
    }
}