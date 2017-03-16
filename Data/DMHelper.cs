using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Data.Common;
using System.Data.OracleClient;
using System.Web;
using ViData.Dict;
using System.IO;

namespace ViData
{
    public class DMHelper
    {
        const string CurrentSessionKey = "dm.current_session";
        const string ReadCurrentSessionKey = "dm.current_session.read";
        const string CfgFileName = "ViCore.config";
        const string DefaultProvider = "System.Data.OracleClient";
        static readonly DMHelper _instance = new DMHelper();
        internal readonly DMSessionFactory _sessionfactory;
        internal DbProviderEx ProviderEx { get; private set; }
        public static DMHelper Instance
        {
            get { return _instance; }
        }
        static DMHelper() { }
        DMHelper()
        {
            this._sessionfactory = InitSession();
            if (string.IsNullOrEmpty(_sessionfactory.DbProviderName))
            {
                _sessionfactory.DbProviderName = DefaultProvider;
            }
            this.ProviderEx = SetParameterType(_sessionfactory.DbProviderName);
            SetDMSessionArrayProvider();
        }

        /// <summary>
        /// 解析映射关系
        /// </summary>
        public void ExportMapping()
        {
            foreach (var ass in _sessionfactory.Mapping)
            {
                foreach (var type in ass.GetExportedTypes())
                {
                    if (type.GetInterfaces().Contains(typeof(IClassMap)))
                    {
                        Activator.CreateInstance(type);
                    }
                }
            }
        }

        internal DMSession GetSession(DbConntionType type = DbConntionType.WriteRead, string dbString = null)
        {
            if (type == DbConntionType.WriteRead)
            {
                DMSession session = HttpContext.Current.Items[CurrentSessionKey + dbString] as DMSession;
                if (session == null)
                {
                    session = _sessionfactory.BuildSession(type, dbString);
                    HttpContext.Current.Items[CurrentSessionKey + dbString] = session;
                }
                return session;
            }
            DMSession readSession = HttpContext.Current.Items[ReadCurrentSessionKey + dbString] as DMSession;
            if (readSession == null)
            {
                readSession = _sessionfactory.BuildSession(type, dbString);
                HttpContext.Current.Items[ReadCurrentSessionKey + dbString] = readSession;
            }
            return readSession;
        }

        internal DMSessionFactory InitSession()
        {
            DMSessionFactory dm = new DMSessionFactory();
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            if (!basePath.EndsWith(@"\"))
            {
                basePath += @"\";
            }
            string fileName = basePath + @"Config\" + CfgFileName;
            //if (type == DbConntionType.OnlyRead)
            //{
            //    fileName = basePath + @"Config\" + ReadCfgFileName; 
            //}
            XmlReaderSettings sett = new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true };
            using (XmlReader assReader = XmlReader.Create(fileName, sett))
            {
                while (assReader.Read())
                {
                    if (assReader.NodeType == XmlNodeType.Element)
                    {
                        switch (assReader.LocalName)
                        {
                            case "mapping":
                                string assembly = assReader.ReadString();
                                Assembly ass = Assembly.Load(assembly);
                                if (!dm.Mapping.Contains(ass))
                                {
                                    dm.Mapping.Add(ass);
                                }
                                break;
                            case "property":
                                string propertyName = assReader.GetAttribute("name").ToLower();
                                switch (propertyName)
                                {
                                    case "connection.string": dm.ConnectionString = assReader.ReadString(); break;
                                    case "connection.string.read": dm.ConnectionStringRead = assReader.ReadString(); break;
                                    case "connection.provider": dm.DbProviderName = assReader.ReadString().Trim(); break;
                                    case "connection.timeout": dm.Timeout = Convert.ToInt32(assReader.ReadString()); break;
                                    case "connection.showsql": dm.ShowSql = Convert.ToBoolean(assReader.ReadString()); break;
                                }
                                if (propertyName != "connection.string" && propertyName != "connection.string.read" && propertyName != "connection.provider")
                                {
                                    if (propertyName.StartsWith("connection.string.read."))
                                    {
                                        string dmsKey = propertyName.Replace("connection.string.read.","");
                                        DMSessionArray dms = GetSessionArray(dm, dmsKey);
                                        dms.ConnectionStringRead = assReader.ReadString();
                                    }
                                    else if (propertyName.StartsWith("connection.string."))
                                    {
                                        string dmsKey = propertyName.Replace("connection.string.", "");
                                        DMSessionArray dms = GetSessionArray(dm, dmsKey);
                                        dms.ConnectionString = assReader.ReadString();
                                    }
                                    else if (propertyName.StartsWith("connection.provider."))
                                    {
                                        string dmsKey = propertyName.Replace("connection.provider.", "");
                                        DMSessionArray dms = GetSessionArray(dm, dmsKey);
                                        dms.DbProviderName = assReader.ReadString();
                                        dms.ProviderEx = SetParameterType(dms.DbProviderName);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            return dm;
        }

        DMSessionArray GetSessionArray(DMSessionFactory dm, string dmsKey)
        {
            DMSessionArray dms = dm.SessionArray.FirstOrDefault(s => s.SessionKey == dmsKey);
            if (dms == null)
            {
                dms = new DMSessionArray();
                dms.SessionKey = dmsKey;
                dm.SessionArray.Add(dms);
            }
            return dms;
        }

        void SetDMSessionArrayProvider()
        {
            foreach (DMSessionArray dma in _sessionfactory.SessionArray)
            {
                if (string.IsNullOrEmpty(dma.DbProviderName))
                {
                    dma.DbProviderName = DefaultProvider;
                }
                if (dma.ProviderEx == null)
                {
                    dma.ProviderEx = new DbProviderEx()
                    {
                        ParameterType = ":",
                        ProviderName = DefaultProvider,
                        ProviderType = DbProviderType.Oracle
                    };
                }
            }
        }

        DbProviderEx SetParameterType(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = DefaultProvider;
            }
            DbProviderEx pex = new DbProviderEx();
            pex.ProviderName = providerName.Trim();
            switch (pex.ProviderName)
            {
                case "System.Data.SqlClient":
                    pex.ParameterType = "@";
                    pex.ProviderType = DbProviderType.SqlServer;
                    break;
                case "MySql.Data.MySqlClient":
                    pex.ParameterType = "@";
                    pex.ProviderType = DbProviderType.MySql;
                    break;
                default:
                    pex.ParameterType = ":";
                    pex.ProviderType = DbProviderType.Oracle;
                    break;
            }
            return pex;
        }

        public void CloseSession()
        {
            DMSession readSession = HttpContext.Current.Items[ReadCurrentSessionKey] as DMSession;
            if (readSession != null)
            {
                readSession.Close();
            }
            DMSession session = HttpContext.Current.Items[CurrentSessionKey] as DMSession;
            if (session != null)
            {
                session.Close();
            }
        }
    }
}
