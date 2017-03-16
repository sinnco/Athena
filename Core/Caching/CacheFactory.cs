using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using ViCore.UnityEx;

namespace ViCore.Caching
{
    public sealed class CacheFactory
    {
        public static ObjectCache Create()
        {
            return UnityConfigEx.GetService<ObjectCache>();
        }
    }
}
