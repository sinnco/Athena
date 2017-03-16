using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViData.Dict
{
    internal class DbProviderEx
    {
        internal DbProviderType ProviderType { get; set; }
        internal string ProviderName { get; set; }
        internal string ParameterType { get; set; }
    }

    internal enum DbProviderType
    {
        Oracle = 0,
        SqlServer = 1,
        MySql = 2
    }
}
