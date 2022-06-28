

using PawnBindings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

using System.Runtime.InteropServices;



public class AmxAlloc
{
    // must be power of 2
    private const ulong PageSize = 0x1000;

    // must be power of 2
    private const ulong MinAllocationSize = 8;

    // must be power of PageSize
    private const ulong AllocationRegionBase = 0x80000000;

    // must be power of PageSize
    private const ulong AllocationRegionSize = 0x80000000;

    private const int AllocationRegionPageCount = (int)(AllocationRegionSize / PageSize);

    static int SizeToBucketIdx(ulong size) => Log2(size) - Log2(MinAllocationSize);

    private static readonly int BucketCount = SizeToBucketIdx(PageSize);

    private readonly AMX _amx;

    private readonly LinkedList<ulong>[] _buckets =
        Enumerable.Repeat(0, BucketCount).Select(_ => new LinkedList<ulong>()).ToArray();

    private readonly Dictionary<ulong, LinkedList<ulong>> _freeArray = new Dictionary<ulong, LinkedList<ulong>>();
    private readonly Dictionary<ulong, ulong> _bigFreeArray = new Dictionary<ulong, ulong>();

    public AmxAlloc(AMX amx)
    {
        if (AllocationRegionPageCount != (AllocationRegionSize / PageSize))
            throw new InvalidConstraintException();

        this._amx = amx;
    }
    public static int Log2(UInt64 value)
    {
        int i;
        for (i = -1; value != 0; i++)
            value >>= 1;

        return (i == -1) ? 0 : i;
    }

    // 128 kB of page usage bitmap
    private readonly BitArray _pageMap = new BitArray(AllocationRegionPageCount);

    private ulong _pageIter;

    private ulong AllocPages(ulong count)
    {
        var pageIterOld = _pageIter;
        do
        {
            var found = true;
            for (ulong i = 0; i < count; ++i)
            {
                if (_pageMap[(int)(_pageIter + i) % AllocationRegionPageCount])
                {
                    found = false;
                    break;
                }
            }

            if (found)
            {
                for (ulong i = 0; i < count; ++i)
                    _pageMap[(int)(_pageIter + i) % AllocationRegionPageCount] = true;
                var va = AllocationRegionBase + _pageIter * PageSize;
                amx_mem_alloc(_amx.Pointer, va, count * PageSize);
                _pageIter = (_pageIter + count) % AllocationRegionPageCount;
                return va;
            }

            _pageIter = (_pageIter + 1) % AllocationRegionPageCount;
        } while (_pageIter != pageIterOld);

        throw new OutOfMemoryException();
    }

    private void FreePages(ulong va, ulong count)
    {
        amx_mem_free(_amx.Pointer, va, count * PageSize);
        for (ulong i = 0; i < count; ++i)
            _pageMap[(int)((va - AllocationRegionBase) / PageSize)] = false;
    }

    [DllImport("amx64.dll")]
    public static extern AmxStatus amx_mem_free(IntPtr amx, ulong va, ulong size);

    [DllImport("amx64.dll")]
    public static extern AmxStatus amx_mem_alloc(IntPtr amx, ulong va, ulong size);
    public ulong Allocate(ulong size)
    {
        if (size < MinAllocationSize)
            size = MinAllocationSize;

        var roundUpPages = (size + (PageSize - 1)) / PageSize * PageSize;
        var roundUpPowerOf2 = powerOf2(size);
        var rounded = Math.Min(roundUpPages, roundUpPowerOf2);

        if (rounded >= PageSize)
        {
            var count = rounded / PageSize;
            var va = AllocPages(count);
            _bigFreeArray[va] = count;
            return va;
        }

        var bucket = _buckets[SizeToBucketIdx(rounded)];
        if (bucket.First == null)
        {
            var page = AllocPages(1);
            for (ulong i = 0; i < PageSize; i += rounded)
                bucket.AddFirst(page + i);
        }

        var val = bucket.First.Value;
        _freeArray[val] = bucket;
        bucket.RemoveFirst();
        return val;
    }

    public ulong powerOf2(ulong v)
    {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v |= v >> 32;
        v++;
        return v;
    }
    public void Free(ulong va)
    {
        if (_freeArray.TryGetValue(va, out var bucket))
        {
            bucket.AddFirst(va);
            _freeArray.Remove(va);
        }
        else if (_bigFreeArray.TryGetValue(va, out var count))
        {
            FreePages(va, count);
            _bigFreeArray.Remove(va);
        }
        else
        {
            throw new InvalidDataException();
        }
    }
}

