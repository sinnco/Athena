using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace ViCore.Caching
{
    public class InprocCache : MemoryCache
    {
        public InprocCache()
            : base("Inproc Cache", null)
        {
            
        }
    }
}
