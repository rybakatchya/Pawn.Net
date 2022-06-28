namespace PawnBindings
{
    public enum AmxStatus
    {
        AMX_SUCCESS = 0,
        AMX_OUT_OF_RESOURCES,
        AMX_ACCESS_VIOLATION,
        AMX_DIVIDE_BY_ZERO,
        AMX_NATIVE_ERROR,
        AMX_RUNTIME_ERROR,
        AMX_NOT_AN_ENTRY_POINT,
        AMX_MALFORMED_CODE,
        AMX_UNSUPPORTED,
        AMX_COMPILE_ERROR,

        // loader errors

        AMX_LOADER_MALFORMED_FILE = 0x1000,
        AMX_LOADER_UNKNOWN_NATIVE
    };
}
