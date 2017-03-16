using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Web;
using ViCore;
using ViData.Dict;

namespace ViData
{
    public class DataHelper : IDataHelper
    {
        public static string SqlText
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    var obj = HttpContext.Current.Items[SqlKey];
                    if (obj != null)
                    {
                        return obj.ToString();
                    }
                }
                return null;
            }
        }

        const string SqlKey = "Current_Excute_Sql";
        /// <summary>
        /// 获取数据操作适配器
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="cmdType"></param>
        /// <param name="paraName"></param>
        /// <param name="paraValue"></param>
        /// <param name="type"></param>
        /// <param name="dbString"></param>
        /// <returns></returns>
        public static FluDataAdapter GetAdapter(string commandText = null, CommandType cmdType = CommandType.Text, string[] paraName = null, object[] paraValue = null, DbConntionType type = DbConntionType.WriteRead, string dbString = null)
        {
            FluDataAdapter da = new FluDataAdapter(type, dbString);
            da.CommandText = commandText;
            da.CommandType = cmdType;
            da.ClearParameter();
            if (paraName != null && paraValue != null && paraName.Length == paraValue.Length)
            {
                for (int i = 0; i < paraName.Length; i++)
                {
                    da.AddParameter(paraName[i], paraValue[i]);
                }
            }
            if (da.ShowSql)
            {
                SetSqlShow(commandText, paraName, paraValue);
            }
            return da;
        }

        /// <summary>
        /// 获取数据操作适配器
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <param name="type"></param>
        /// <param name="dbString"></param>
        /// <returns></returns>
        public static FluDataAdapter GetAdapter2(string commandText = null, IDictionary<string, object> parameters = null, CommandType cmdType = CommandType.Text, DbConntionType type = DbConntionType.WriteRead, string dbString = null)
        {
            FluDataAdapter da = new FluDataAdapter(type, dbString);
            da.CommandText = commandText;
            da.CommandType = cmdType;
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    da.AddParameter(item.Key, item.Value);
                }
            }
            if (da.ShowSql)
            {
                SetSqlShow(commandText, parameters: parameters);
            }
            return da;
        }

        /// <summary>
        /// 获取数据操作适配器，须执行dispose
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <param name="type"></param>
        /// <param name="dbString"></param>
        /// <returns></returns>
        public static FluDataAdapter GetAdapter3(string commandText = null, IList<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text, DbConntionType type = DbConntionType.WriteRead, string dbString = null)
        {
            FluDataAdapter da = new FluDataAdapter(type, dbString);
            da.CommandText = commandText;
            da.CommandType = cmdType;
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    da.AddParameter(item);
                }
            }
            if (da.ShowSql)
            {
                SetSqlShow(commandText, dbParameters: parameters);
            }
            return da;
        }

        internal static void SetSqlShow(string cmdText, string[] paraName = null, object[] paraValue = null, IDictionary<string, object> parameters = null, IList<IDbDataParameter> dbParameters = null)
        {
            cmdText += ";";
            if (paraName != null && paraValue != null)
            {
                for (int i = 0; i < paraName.Length; i++)
                {
                    cmdText += string.Format(" ({0} = {1});", paraName[i], paraValue[i]);
                }
            }
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    cmdText += string.Format(" ({0} = {1});", item.Key, item.Value);
                }
            }
            if (dbParameters != null)
            {
                foreach (var item in dbParameters)
                {
                    cmdText += string.Format(" ({0} = {1});", item.ParameterName, item.Value);
                }
            }
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Items[SqlKey] != null)
                {
                    HttpContext.Current.Items[SqlKey] = string.Format(" {0}; {1};", HttpContext.Current.Items[SqlKey], cmdText);
                }
                else
                {
                    HttpContext.Current.Items[SqlKey] = cmdText;
                }
            }
            else
            {
                Console.WriteLine(cmdText);
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="cmdType"></param>
        /// <param name="paraName">参数名数组</param>
        /// <param name="paraValue">参数值数组</param>
        /// <param name="adapter">是否外部传递，如使用事务</param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static int ExcuteNonQuery(string commandText, CommandType cmdType = CommandType.Text, string[] paraName = null, object[] paraValue = null, FluDataAdapter adapter = null, string dbString = null)
        {
            if (adapter == null)
            {
                using (adapter = GetAdapter(commandText, cmdType, paraName, paraValue, DbConntionType.WriteRead, dbString))
                {
                    adapter.OpenConnection();
                    int result = adapter.SelectCommand.ExecuteNonQuery();
                    adapter.CloseConnection();
                    return result;
                }
            }
            adapter.ClearParameter();
            adapter.CommandText = commandText;
            adapter.CommandType = cmdType;
            if (paraName != null && paraValue != null && paraName.Length == paraValue.Length)
            {
                for (int i = 0; i < paraName.Length; i++)
                {
                    adapter.AddParameter(paraName[i], paraValue[i]);
                }
            }
            int result2 = adapter.SelectCommand.ExecuteNonQuery();
            return result2;
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters">参数化键值对</param>
        /// <param name="cmdType"></param>
        /// <param name="adapter">事务adapter</param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static int ExcuteNonQuery2(string commandText, IDictionary<string,object> parameters = null, CommandType cmdType = CommandType.Text, FluDataAdapter adapter = null, string dbString = null)
        {
            if (adapter == null)
            {
                using (adapter = GetAdapter2(commandText, parameters, cmdType, DbConntionType.WriteRead, dbString))
                {
                    adapter.OpenConnection();
                    int result = adapter.SelectCommand.ExecuteNonQuery();
                    adapter.CloseConnection();
                    return result;
                }
            }
            adapter.ClearParameter();
            adapter.CommandText = commandText;
            adapter.CommandType = cmdType;
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    adapter.AddParameter(p.Key, p.Value);
                }
            }
            int result2 = adapter.SelectCommand.ExecuteNonQuery();
            return result2;
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters">参数化列表</param>
        /// <param name="cmdType"></param>
        /// <param name="adapter"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static int ExcuteNonQuery3(string commandText, IList<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text, FluDataAdapter adapter = null, string dbString = null)
        {
            if (adapter == null)
            {
                using (adapter = GetAdapter3(commandText, parameters, cmdType, DbConntionType.WriteRead, dbString))
                {
                    adapter.OpenConnection();
                    int result = adapter.SelectCommand.ExecuteNonQuery();
                    adapter.CloseConnection();
                    return result;
                }
            }
            adapter.ClearParameter();
            adapter.CommandText = commandText;
            adapter.CommandType = cmdType;
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    adapter.AddParameter(p);
                }
            }
            int result2 = adapter.SelectCommand.ExecuteNonQuery();
            return result2;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="dbString"></param>
        /// <returns></returns>
        public static object ExcuteStoredProcedure(string commandText, IList<IDbDataParameter> parameters = null,  string dbString = null)
        {
            using (FluDataAdapter adapter = GetAdapter3(commandText, parameters, CommandType.StoredProcedure, DbConntionType.WriteRead, dbString))
            {
                adapter.OpenConnection();
                object result = adapter.SelectCommand.ExecuteNonQuery();
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        if (p.Direction == ParameterDirection.Output)
                        {
                            return p.Value;
                        }
                    }
                    adapter.CloseConnection();
                }
                return result;
            }
        }

        /// <summary>
        /// 含事务处理DataAdapter
        /// </summary>
        /// <param name="action">一系列的数据库更新语句</param>
        /// <param name="dbString">数据库名称</param>
        public static bool ExcuteTransaction(Action<FluDataAdapter> action, string dbString = null)
        {
            using (FluDataAdapter da = GetAdapter(dbString: dbString))
            {
                da.OpenConnection();
                using (DbTransaction ts = da.Connection.BeginTransaction())
                {
                    try
                    {
                        da.SelectCommand.Transaction = ts;
                        action.Invoke(da);
                        ts.Commit();     
                        return true;
                    }
                    catch(Exception ex)
                    {
                        ts.Rollback();
                        //Logging4net.WriteError(ex, "事务执行出错："+ da.SelectCommand.CommandText);
                        throw ex;
                    }
                    finally
                    {
                        da.CloseConnection();
                    }
                }
            }
        }

        /// <summary>
        /// 含事务处理DataAdapter
        /// </summary>
        /// <param name="action">DataApapter, Transaction需要手动commit or rollback</param>
        /// <param name="dbString">数据库名称</param>
        /// <returns></returns>
        public static bool ExcuteTransaction(Action<FluDataAdapter, DbTransaction> action, string dbString = null)
        {
            using (FluDataAdapter da = GetAdapter(dbString: dbString))
            {
                da.OpenConnection();
                using (DbTransaction ts = da.Connection.BeginTransaction())
                {
                    try
                    {
                        da.SelectCommand.Transaction = ts;
                        action.Invoke(da, ts);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ts.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        da.CloseConnection();
                    }
                }
            }
        }

        /// <summary>
        /// 返回DataTable
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="cmdType"></param>
        /// <param name="paraName"></param>
        /// <param name="paraValue"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static DataTable Fill(string commandText, CommandType cmdType = CommandType.Text, string[] paraName = null, object[] paraValue = null, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            using (FluDataAdapter da = GetAdapter(commandText, cmdType, paraName, paraValue, type, dbString))
            {
                da.OpenConnection();
                DataTable dt = new DataTable();
                da.Fill(dt);
                da.CloseConnection();
                return dt;
            }
        }

        /// <summary>
        /// 返回DataTable
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters">参数化键值对</param>
        /// <param name="cmdType"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static DataTable Fill2(string commandText, IDictionary<string,object> parameters = null, CommandType cmdType = CommandType.Text, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            using (FluDataAdapter da = GetAdapter2(commandText,parameters, cmdType, type, dbString))
            {
                da.OpenConnection();
                DataTable dt = new DataTable();
                da.Fill(dt);
                da.CloseConnection();
                return dt;
            }
        }

        /// <summary>
        /// 返回DataTable
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters">参数对象列表</param>
        /// <param name="cmdType"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static DataTable Fill3(string commandText, IList<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            using (FluDataAdapter da = GetAdapter3(commandText, parameters, cmdType, type, dbString))
            {
                da.OpenConnection();
                DataTable dt = new DataTable();
                da.Fill(dt);
                da.CloseConnection();
                return dt;
            }
        }

        /// <summary>
        /// 返回IList对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static IList<T> Fill3<T>(string commandText, IList<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            DataTable dt = Fill3(commandText, parameters, cmdType, type, dbString);
            return Helper.GetObjects<T>(dt);
        }

        /// <summary>
        /// 返回IList对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static IList<T> Fill2<T>(string commandText, IDictionary<string,object> parameters = null, CommandType cmdType = CommandType.Text, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            DataTable dt = Fill2(commandText,parameters, cmdType, type, dbString);
            return Helper.GetObjects<T>(dt);
        }

        /// <summary>
        /// 返回IList对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="cmdType"></param>
        /// <param name="paraName"></param>
        /// <param name="paraValue"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static IList<T> Fill<T>(string commandText, CommandType cmdType = CommandType.Text, string[] paraName = null, object[] paraValue = null, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            DataTable dt = Fill(commandText, cmdType, paraName, paraValue, type, dbString);
            return Helper.GetObjects<T>(dt);
        }

        /// <summary>
        /// 返回查询结果中第一行第一列
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="cmdType"></param>
        /// <param name="paraName"></param>
        /// <param name="paraValue"></param>
        /// <param name="type"></param>
        /// <param name="adapter"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static object ExcuteScalar(string commandText, CommandType cmdType = CommandType.Text, string[] paraName = null, object[] paraValue = null, DbConntionType type = DbConntionType.OnlyRead, FluDataAdapter adapter = null, string dbString = null)
        {
            if (adapter == null)
            {
                using (FluDataAdapter da = GetAdapter(commandText, cmdType, paraName, paraValue, type, dbString))
                {
                    da.OpenConnection();
                    object obj = da.SelectCommand.ExecuteScalar();
                    da.CloseConnection();
                    return obj;
                }
            }
            adapter.ClearParameter();
            adapter.CommandText = commandText;
            adapter.CommandType = cmdType;
            if (paraName != null && paraValue != null && paraName.Length == paraValue.Length)
            {
                for (int i = 0; i < paraName.Length; i++)
                {
                    adapter.AddParameter(paraName[i], paraValue[i]);
                }
            }
            object result2 = adapter.SelectCommand.ExecuteScalar();
            return result2;
        }

        /// <summary>
        /// 返回查询结果中第一行第一列
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <param name="type"></param>
        /// <param name="adapter"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static object ExcuteScalar2(string commandText, IDictionary<string,object> parameters = null, CommandType cmdType = CommandType.Text, DbConntionType type = DbConntionType.OnlyRead, FluDataAdapter adapter = null, string dbString = null)
        {
            if (adapter == null)
            {
                using (FluDataAdapter da = GetAdapter2(commandText, parameters, cmdType, type, dbString))
                {
                    da.OpenConnection();
                    object obj = da.SelectCommand.ExecuteScalar();
                    da.CloseConnection();
                    return obj;
                }
            }
            adapter.ClearParameter();
            adapter.CommandText = commandText;
            adapter.CommandType = cmdType;
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    adapter.AddParameter(p.Key, p.Value);
                }
            }
            object result2 = adapter.SelectCommand.ExecuteScalar();
            return result2;
        }

        /// <summary>
        /// 返回查询结果中第一行第一列
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <param name="type"></param>
        /// <param name="adapter"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static object ExcuteScalar3(string commandText, IList<IDbDataParameter> parameters = null, CommandType cmdType = CommandType.Text, DbConntionType type = DbConntionType.OnlyRead, FluDataAdapter adapter = null, string dbString = null)
        {
            if (adapter == null)
            {
                using (FluDataAdapter da = GetAdapter3(commandText, parameters, cmdType, type, dbString))
                {
                    da.OpenConnection();
                    object obj = da.SelectCommand.ExecuteScalar();
                    da.CloseConnection();
                    return obj;
                }
            }
            adapter.ClearParameter();
            adapter.CommandText = commandText;
            adapter.CommandType = cmdType;
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    adapter.AddParameter(p);
                }
            }
            object result2 = adapter.SelectCommand.ExecuteScalar();
            return result2;
        }

        /// <summary>
        /// 返回DataReader，must using dispose
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="cmdType"></param>
        /// <param name="paraName"></param>
        /// <param name="paraValue"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(string commandText, CommandType cmdType = CommandType.Text, string[] paraName = null, object[] paraValue = null, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            FluDataAdapter da = GetAdapter(commandText, cmdType, paraName, paraValue, type, dbString);
            da.OpenConnection();
            IDataReader dr = da.SelectCommand.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
        }

        /// <summary>
        /// 取得分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static DataTable GetPagingData(PagingInfo page, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            using (FluDataAdapter da = GetAdapter(type: type, dbString: dbString))
            {
                da.OpenConnection();
                DataTable dt = da.GetPagingData2(page);
                da.CloseConnection();
                return dt;
            }
        }

        /// <summary>
        /// 取得分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="tableName"></param>
        /// <param name="fields"></param>
        /// <param name="condition"></param>
        /// <param name="sortfields"></param>
        /// <param name="groupfields"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static DataTable GetPagingData(PagingInfo page, string tableName, string fields, string condition = null, string sortfields = null, string groupfields = null, int pageIndex = 1, int pageSize = 10, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            page.TableName = tableName;
            page.Fileds = fields;
            page.Conditions = condition;
            page.SortFields = sortfields;
            page.GroupFields = groupfields;
            page.PageIndex = pageIndex;
            page.PageSize = pageSize;
            DataTable dt = GetPagingData(page, type, dbString);
            return dt;
        }

        /// <summary>
        /// 取得分页数据对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="tableName"></param>
        /// <param name="fields"></param>
        /// <param name="condition"></param>
        /// <param name="sortfields"></param>
        /// <param name="groupfields"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static IList<T> GetPagingData<T>(PagingInfo page, string tableName, string fields, string condition = null, string sortfields = null, string groupfields = null, int pageIndex = 1, int pageSize = 10, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            DataTable dt = GetPagingData(page, tableName, fields, condition, sortfields, groupfields, pageIndex, pageSize, type, dbString);
            return Helper.GetObjects<T>(dt);
        }

        /// <summary>
        /// 取得分页数据对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static IList<T> GetPagingData<T>(PagingInfo page, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            DataTable dt = GetPagingData(page, type, dbString);
            return Helper.GetObjects<T>(dt);
        }

        /// <summary>
        /// 取得查询数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="conditions"></param>
        /// <param name="orderby"></param>
        /// <param name="groupby"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static DataTable GetData(string tableName, string columns, string conditions, string orderby, string groupby, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            using (FluDataAdapter da = GetAdapter(type: type, dbString: dbString))
            {
                da.OpenConnection();
                DataTable dt = da.GetData2(tableName, columns, conditions, orderby, groupby);
                da.CloseConnection();
                return dt;
            }
        }

        /// <summary>
        /// 取得查询数据对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="conditions"></param>
        /// <param name="orderby"></param>
        /// <param name="groupby"></param>
        /// <param name="type"></param>
        /// <param name="dbString">分库名</param>
        /// <returns></returns>
        public static IList<T> GetData<T>(string tableName, string columns, string conditions, string orderby, string groupby, DbConntionType type = DbConntionType.OnlyRead, string dbString = null)
        {
            DataTable dt = GetData(tableName, columns, conditions, orderby, groupby, type, dbString);
            return Helper.GetObjects<T>(dt);
        }

        /// <summary>
        /// 返回连接字符串
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbString"></param>
        /// <returns></returns>
        public static string GetConnString(DbConntionType type = DbConntionType.WriteRead, string dbString = null)
        {
            DMSession session = DMHelper.Instance.GetSession(type, dbString);
            return session.ConnectionString;
        }
    }
}
