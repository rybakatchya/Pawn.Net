using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PawnBindings
{
    public class AMXExeption : Exception
    {
        public AMXExeption()
        {
        }

        public AMXExeption(string message)
            : base(message)
        {
        }

        public AMXExeption(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


    public class AMX
    {
        private readonly IntPtr amxPtr;
        public IntPtr Pointer => amxPtr;

        public AmxAlloc Allocator => allocator;

        private static ArrayPool<byte> bytes;
        //private readonly ArrayPool<long> cells;
        public static ArrayPool<byte> ByteBuffer => bytes;
        private readonly AmxAlloc allocator;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal AMX(IntPtr amx)
        {
            allocator = new AmxAlloc(this);
            amxPtr = amx;
            //cells = ArrayPool<long>.Create(1024, 1024);
            bytes = ArrayPool<byte>.Create(1024, 1024);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public IntPtr Translate(long va, long count)
        {
            return amx_mem_translate(amxPtr, va, count);
        }


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public AmxStatus Call(long cip)
        {
            return amx_call(amxPtr, cip);
        }

        public unsafe long GetCell(int index)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long valuePtr = 0;
            var status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &valuePtr);
            if (status != AmxStatus.AMX_SUCCESS)
                throw new AMXExeption(status.ToString());
            return valuePtr;
        }

        public unsafe void SetCell(int index, long va, long value)
        {
            var status = amx_cell_write(amxPtr, va, value);
            if (status != AmxStatus.AMX_SUCCESS)
                throw new AMXExeption(status.ToString());

        }
        /*[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteInt32RefArg(int index, int value)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long valuePtr = 0;
            amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &valuePtr);
            var status = amx_cell_write(amxPtr, valuePtr, value);
            if(status == AmxStatus.AMX_SUCCESS)
            {
                throw new AMXExeption("[AMXEXCEPTION]: " + status.ToString());
            }
            
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe long ReadInt64RefArg(int index)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long val;

            var status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &val);

            if (status == AmxStatus.AMX_SUCCESS)
            {
                throw new AMXExeption("[AMXEXCEPTION]: " + status.ToString());
            }
            return val;
            
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteInt64RefArg(int index, long value)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long valuePtr = 0;
            amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &valuePtr);
            var status = amx_cell_write(amxPtr, valuePtr, value);

            if (status == AmxStatus.AMX_SUCCESS)
            {
                throw new AMXExeption("[AMXEXCEPTION]: " + status.ToString());
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloatRefArg(int index, float value)
        {
            //var val = ReinterpretCast<float, long>(value);
            var val = ReinterpretCast<double, long>(value);
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long valuePtr = 0;
            amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &valuePtr);
            var status = amx_cell_write(amxPtr, valuePtr, val);
            if (status == AmxStatus.AMX_SUCCESS)
            {
                throw new AMXExeption("[AMXEXCEPTION]: " + status.ToString());
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe float ReadFloatRefArg(int index)


        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);

            long val;
            var status = amx_cell_read(amxPtr, stk + Marshal .SizeOf<long>() * index, &val);
            
            if (status == AmxStatus.AMX_SUCCESS)
            {
                throw new AMXExeption("[AMXEXCEPTION]: " + status.ToString());
            }
            return (float)ReinterpretCast<long, double>(val); ;

        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        static unsafe TDest ReinterpretCast<TSource, TDest>(TSource source)
        {
            var sourceRef = __makeref(source);
            var dest = default(TDest);
            var destRef = __makeref(dest);
            *(IntPtr*)&destRef = *(IntPtr*)&sourceRef;
            return __refvalue(destRef, TDest);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteStringRefArg(int index, string value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);

            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);

            long str_ptr = 0;
            var status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &str_ptr);
            if (status == AmxStatus.AMX_SUCCESS)
            {
                throw new AMXExeption("[AMXEXCEPTION]: " + status.ToString());
            }
            var str_base = amx_mem_translate(amxPtr, str_ptr, value.Length);

           

            Span<long> cells = new Span<long>(str_base.ToPointer(), value.Length);
            for(int i = 0; i < value.Length; i ++)
            {
                cells[i] = (long)bytes[i];
            }
            status = amx_cell_write(amxPtr, stk + Marshal.SizeOf<long>() * index, (long)str_base);
            if (status == AmxStatus.AMX_SUCCESS)
            {
                throw new AMXExeption("[AMXEXCEPTION]: " + status.ToString());
            }
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe string ReadStringRefArg(int index)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);

            long str_len = 0;
            var status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>(), &str_len);

            if (status == AmxStatus.AMX_SUCCESS)
            {
                throw new AMXExeption("[AMXEXCEPTION]: " + status.ToString());
            }

            long str_ptr = 0;
            status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &str_ptr);
            if (status == AmxStatus.AMX_SUCCESS)
            {
                throw new AMXExeption("[AMXEXCEPTION]: " + status.ToString());
            }

            var str_base = amx_mem_translate(amxPtr, str_ptr, str_len);

            if (str_base == null)
                return null;

            ReadOnlySpan<long> cells = new ReadOnlySpan<long>(str_base.ToPointer(), (int)str_len);

            byte[] text = bytes.Rent((int)str_len);
        
            for (int i = 0; i < str_len; i++)
            {
                text[i] = (byte)cells[i];
            }
            var str = System.Text.Encoding.UTF8.GetString(text);
            

            return str;
        }*/

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus Call(long cip, AmxCell[] cells)
        {
            long* cell = (long*)Marshal.AllocHGlobal(Marshal.SizeOf<AmxCell>() * cells.Length);
            var span = new Span<long>(cell, cells.Length);
            
            for(int i = 0; i < cells.Length; i++)
            {
                span[i] = cells[i].Pointer;
            }
            var status = amx_call_n(amxPtr, cip, cells.Length, cell);
            Marshal.FreeHGlobal((IntPtr)cell);
            return status;
        }

        
       



        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public AmxStatus Push(long address)
        {
            return amx_push(amxPtr, address);
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus Call(long va, long argc, long* longs)
        {
            return amx_call_v(amxPtr, va, argc, longs);
        }



        public unsafe AmxStatus Call(AMX amx, long va, long argc, long* args)
        {

            //1.read STK and FRM, save both           
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            var frm = amx_register_read(amxPtr, AmxRegister.AMX_FRM);

            //2.subtract(2 + argc) * sizeof(amx_cell) from STK
            var modifiedStack = stk - (2 + argc) * Marshal.SizeOf<long>();

            //3.write it back
            amx_register_write(amxPtr, AmxRegister.AMX_STK, modifiedStack);

            //4.translate of count(2 + argc) from va STK
            var ptr = (long*)amx_mem_translate(amxPtr, modifiedStack, (2 + argc));

            // 5.write 0 in translated[0], write argc*sizeof(amx_cell) in translated[1] and write the arguments in translated[2 + n], left to right
            ptr[0] = 0;
            ptr[1] = argc * Marshal.SizeOf<long>();
            for (int i = 0; i < argc; i++)
            {
                ptr[2 + i] = args[i];
            }

            // 6.use amx_run on the cip
            var status = amx_run(amxPtr, va);

            // 7.restore the old STK and FRM
            amx_register_write(amxPtr, AmxRegister.AMX_STK, stk);
            amx_register_write(amxPtr, AmxRegister.AMX_FRM, frm);

            return status;


        }

        public const string dll = "amx64.dll";
        [DllImport(dll)]
        internal static unsafe extern AmxStatus amx_push_n(IntPtr amx, long* v, long count);
                [DllImport(dll)]
        internal static extern AmxStatus amx_run(IntPtr amx, long cip);
        
        [DllImport(dll)]
        internal static extern AmxStatus amx_call(IntPtr amx, long cip);

        
        [DllImport(dll)]
        internal static unsafe extern AmxStatus amx_call_n(IntPtr amx, long cip, long argc, long* argv);

     
        [DllImport(dll)]
        internal static unsafe extern AmxStatus amx_call_v(IntPtr amx, long cip, long argc, long* longs);

        [DllImport(dll)]
        internal static extern IntPtr amx_mem_translate(IntPtr amx, long va, long count);

        [DllImport(dll)]
        internal static extern long amx_register_read(IntPtr amx, AmxRegister reg);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        [DllImport(dll)]
        internal static extern void amx_register_write(IntPtr amx, AmxRegister reg, long val);

        [DllImport(dll)]
        internal static unsafe extern AmxStatus amx_cell_read(IntPtr amx, long stk, long* value);

        [DllImport(dll)]
        internal static extern AmxStatus amx_cell_write(IntPtr amx, long va, long val);
       
        [DllImport(dll)]
        internal static extern AmxStatus amx_push(IntPtr amx, long v);
    }
}
