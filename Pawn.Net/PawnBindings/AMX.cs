﻿using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PawnBindings
{
    public class AMX
    {
        private readonly IntPtr amxPtr;
        public IntPtr Pointer => amxPtr;

        public AmxAlloc Allocator => allocator;

        private readonly ArrayPool<byte> bytes;
        private readonly ArrayPool<AMXCell> cells;

        private readonly AmxAlloc allocator;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal AMX(IntPtr amx)
        {
            allocator = new AmxAlloc(this);
            amxPtr = amx;
            cells = ArrayPool<AMXCell>.Create(1024, 1024);
            bytes = ArrayPool<byte>.Create(1024, 1024);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public IntPtr Translate(long va, long count)
        {
            return amx_mem_translate(amxPtr, va, count);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public AmxStatus Call(AMXCell cip)
        {
            return amx_call(amxPtr, cip.Pointer);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus ReadInt32(int index, out int value)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);

            long val;

            var status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &val);

            value = (int)val;

            return status;
            
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus WriteInt32(int index, int value)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long valuePtr = 0;
            amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &valuePtr);
            var status = amx_cell_write(amxPtr, valuePtr, value);
            
            return status;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus ReadInt64(int index, out long value)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long val;

            var status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &val);
            value = val;
            return status;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus WriteInt64(int index, long value)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long valuePtr = 0;
            amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &valuePtr);
            var status = amx_cell_write(amxPtr, valuePtr, value);
            
            return status;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus WriteFloat(int index, float value)
        {
            var val = ReinterpretCast<float, long>(value);

            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long valuePtr = 0;
            amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &valuePtr);
            var status = amx_cell_write(amxPtr, valuePtr, val);



            return status;

        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus ReadFloat(int index, out float value)


        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);

            long val;
            var status = amx_cell_read(amxPtr, stk + Marshal .SizeOf<long>() * index, &val);
            value = (float)ReinterpretCast<long, double>(val);
            return status;

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
        public unsafe AmxStatus WriteString(int index, string value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);

            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);

            long str_ptr = 0;
            var status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &str_ptr);
            if (status != AmxStatus.AMX_SUCCESS)
                return status;
            var str_base = amx_mem_translate(amxPtr, str_ptr, value.Length);

           

            Span<long> cells = new Span<long>(str_base.ToPointer(), value.Length);
            for(int i = 0; i < value.Length; i ++)
            {
                cells[i] = (long)bytes[i];
            }
            status = amx_cell_write(amxPtr, stk + Marshal.SizeOf<long>() * index, (long)str_base);
            

            return status;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe string ReadString(int index)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);

            long str_len = 0;
            var status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>(), &str_len);

            if (status != AmxStatus.AMX_SUCCESS)
                return null;

            long str_ptr = 0;
            status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index, &str_ptr);
            if (status != AmxStatus.AMX_SUCCESS)
                return null;

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
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus Call(AMXCell cip, AMXCell[] cells)
        {
            long* cell = (long*)Marshal.AllocHGlobal(Marshal.SizeOf<AMXCell>() * cells.Length);
            var span = new Span<long>(cell, cells.Length);
            
            for(int i = 0; i < cells.Length; i++)
            {
                span[i] = cells[i].Pointer;
            }
            var status = amx_call_n(amxPtr, cip.Pointer, cells.Length, cell);
            Marshal.FreeHGlobal((IntPtr)cell);
            return status;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public AMXCell RegisterRead(AmxRegister register)
        {
            return new AMXCell(amx_register_read(amxPtr, register));
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public void RegisterWrite(AmxRegister register, AMXCell value)
        {
            amx_register_write(amxPtr, register, value.Pointer);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus CellReadAligned(int index, out AMXCell value)
        {
            if (index == 0)
            {
                value = new AMXCell(0);
                return AmxStatus.AMX_ACCESS_VIOLATION;
            }
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            long val = 0;
            var status = amx_cell_read(amxPtr, stk + Marshal.SizeOf<long>() * index , &val);
            value = new AMXCell(val);
            return status;
        }

        public unsafe AmxStatus CellWriteAligned(int index, AMXCell value)
        {
            var stk = amx_register_read(amxPtr, AmxRegister.AMX_STK);
            var status = amx_cell_write(amxPtr, stk + Marshal.SizeOf<long>() * index, value.Pointer);
            amx_register_write(amxPtr, AmxRegister.AMX_STK, value.Pointer);
            return status;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public AmxStatus Push(AMXCell cell)
        {
            return amx_push(amxPtr, cell.Pointer);
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public unsafe AmxStatus Call(AMXCell index, long argc, long* longs)
        {
            return amx_call_v(amxPtr, index.Pointer, argc, longs);
        }



        public unsafe AmxStatus Call(AMX amx, AMXCell cip, long argc, long* args)
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
            var status = amx_run(amxPtr, cip.Pointer);

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