using System;

namespace PawnBindings
{

    public struct AMXCell
    {
        private readonly long cellPtr;
        public long Pointer => cellPtr;
        internal AMXCell(long ptr)
        {
            cellPtr = ptr;
        }

        public static AMXCell FromVirtualAddress(long val)
        {
            return new AMXCell(val);
        }

        public static AMXCell FromPointer(IntPtr ptr)
        {
            return new AMXCell((long)ptr);
        }

     
    }
}
