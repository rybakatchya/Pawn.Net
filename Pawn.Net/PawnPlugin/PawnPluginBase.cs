using PawnBindings;
using System;

namespace PawnPlugin
{
    public abstract class PawnPluginBase
    {
        protected readonly AMX _amx;
        protected readonly AMXLoader _loader;

        public AMX Amx => _amx;
        public AMXLoader Loader => _loader;


        public Action PreInit;
        public Action PostInit;
        public PawnPluginBase(AMX amx, AMXLoader loader)
        {
            _amx = amx;
            _loader = loader;

        }


    }
}
