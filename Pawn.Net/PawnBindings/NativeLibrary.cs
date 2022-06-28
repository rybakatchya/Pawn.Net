
using System;
using System.Runtime.InteropServices;

public class NativeLibrary
{
	private const string Kernel32 = "Kernel32";
	[DllImport(Kernel32, EntryPoint = "LoadLibraryW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Winapi)]
	private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string name);

	[DllImport(Kernel32, EntryPoint = "GetProcAddress", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Winapi)]
	private static extern IntPtr GetProcAddress(IntPtr module, [MarshalAs(UnmanagedType.LPStr)] string name);

	[DllImport(Kernel32, EntryPoint = "FreeLibrary", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
	private static extern bool FreeLibrary(IntPtr module);
	public static bool TryLoad(string name, out IntPtr handle)
	{
		handle = LoadLibrary(name);
		return handle != IntPtr.Zero;
	}

	public static IntPtr Load(string name)
	{
		return LoadLibrary(name);
	}

	public static IntPtr GetExport(IntPtr handle, string name)
	{
		return GetProcAddress(handle, name);
	}

	public static bool TryGetExport(IntPtr handle, string name, out IntPtr address)
	{
		var val = GetProcAddress(handle, name);
		address = val;
		return val != IntPtr.Zero;
	}

	 
	public static void Invoke(IntPtr handle, IntPtr exportAddress, Type type, params object[] args)
    {
		
		var i = Marshal.GetDelegateForFunctionPointer(exportAddress, type);
		i.DynamicInvoke(args);
    }
}