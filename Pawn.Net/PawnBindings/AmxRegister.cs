namespace PawnBindings
{
    public enum AmxRegister
    {
        AMX_PRI,
        AMX_ALT,
        AMX_FRM,// 32 bit
        AMX_STK,// 32 bit
        AMX_HEA,// 32 bit
                // AMX_DAT, // use amx_cell_{read|write} instead
                // AMX_CIP, // not implemented
                // AMX_COD, // not implemented
                // AMX_STP, // not implemented
    };
}
