using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViData.Dict
{
    internal class DMSessionArray
    {
        internal string SessionKey { get; set; }
        internal string ConnectionString { get; set; }
        internal string ConnectionStringRead { get; set; }
        internal string DbProviderName { get; set; }
        internal DbProviderEx ProviderEx { get; set; }
    }
}
