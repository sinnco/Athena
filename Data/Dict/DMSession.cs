using System.Data.Common;
using System.Data.OracleClient;
using System.Data;

namespace ViData.Dict
{
    internal class DMSession
    {
        internal DMSession(string connectionString, string dbProvider)
        {
            ConnectionString = connectionString;
            DbProviderName = dbProvider;
            SetConnection();
        }
        internal string ConnectionString { get; set; }
        internal string DbProviderName { get; set; }
        internal DbConnection Connection { get; set; }
        internal DbProviderEx ProviderEx { get; set; }
        internal int Timeout { get; set; }
        internal bool ShowSql { get; set; }

        internal void SetConnection()
        {
            if (Connection == null)
            {
                DbProviderFactory Dbfactory = DbProviderFactories.GetFactory(DbProviderName);
                Connection = Dbfactory.CreateConnection();
                Connection.ConnectionString = ConnectionString;
            }
        }

        internal void Close()
        {
            if (Connection != null && Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }
    }
}
