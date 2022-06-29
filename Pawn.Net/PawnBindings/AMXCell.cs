using System;
using System.Runtime.CompilerServices;

namespace PawnBindings
{

    public struct AmxCell
    {
        private readonly long cellPtr;
        public long Pointer => cellPtr;
        private AmxCell(long ptr)
        {
            cellPtr = ptr;
        }

        public static AmxCell FromVirtualAddress(long va)
        {
            return new AmxCell(va);
        }

        public int ReadInt32()
        {
            return (int)cellPtr;
        }

        public unsafe string ReadString(IntPtr amx, int length)
        {
            var str_base = AMX.amx_mem_translate(amx, cellPtr, length);

            if (str_base == null)
                return null;

            ReadOnlySpan<long> cells = new ReadOnlySpan<long>(str_base.ToPointer(), length);

            byte[] text = AMX.ByteBuffer.Rent((int)length);

            for (int i = 0; i < length; i++)
            {
                text[i] = (byte)cells[i];
            }
            var str = System.Text.Encoding.UTF8.GetString(text);

            return str;
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

        public float ReadFloat()
        {            
            return (float)ReinterpretCast<long, double>(cellPtr); ;
        }     
    }
}
