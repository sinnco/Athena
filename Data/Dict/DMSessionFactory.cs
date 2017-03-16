using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace ViData.Dict
{
    internal class DMSessionFactory
    {
        internal DMSessionFactory()
        {
            Mapping = new List<Assembly>();
            SessionArray = new List<DMSessionArray>();
        }
        int _timeout = 30;
        internal int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
        internal string DbProviderName { get; set; }
        internal string ConnectionString { get; set; }
        internal string ConnectionStringRead { get; set; }
        internal IList<Assembly> Mapping { get; set; }
        internal bool ShowSql { get; set; }
        internal IList<DMSessionArray> SessionArray { get; set; }
        internal DMSession BuildSession(DbConntionType type = DbConntionType.WriteRead, string dbString = null)
        {
            DMSession session;
            string connString = ConnectionString;
            string connStringRead = ConnectionStringRead;
            string provider = DbProviderName;
            if (!string.IsNullOrEmpty(dbString))
            {
                DMSessionArray dms = SessionArray.First(s => s.SessionKey == dbString);
                connString = dms.ConnectionString;
                connStringRead = dms.ConnectionStringRead;
                provider = dms.DbProviderName;
            }
            if (type == DbConntionType.WriteRead)
            {
                session = new DMSession(connString, provider);
            }
            else
            {
                if (string.IsNullOrEmpty(connStringRead))
                {
                    connStringRead = connString;
                }
                session = new DMSession(connStringRead, provider);
            }
            session.Timeout = Timeout;
            session.ShowSql = ShowSql;
            return session;
        }

    }
}
