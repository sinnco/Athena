using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using System.Web;
using System.Linq;
using ViData.Dict;

namespace ViData
{
    public class FluDataAdapter : DbDataAdapter
    {
        #region 属性

        internal FluDataAdapter(DbConntionType type = DbConntionType.WriteRead, string dbString = null)
            :base()
        {
            DMSession session;
            if (HttpContext.Current != null)
            {
                session = DMHelper.Instance.GetSession(type, dbString);
            }
            else
            {
                session = new DMWinHelper().GetSession(type, dbString);
            }
            this.SelectCommand = session.Connection.CreateCommand();
            this.ShowSql = session.ShowSql;
            //this.SelectCommand.CommandTimeout = session.Timeout;
            if (string.IsNullOrEmpty(dbString))
            {
                DbPro = DMHelper.Instance.ProviderEx;
            }
            else
            {
                var SList = DMHelper.Instance._sessionfactory.SessionArray.Where(a => a.SessionKey == dbString).First();
                DbPro = SList.ProviderEx;
            }
        }

        internal bool ShowSql { get; set; }
        internal DbProviderEx DbPro { get; set; }
        
        /// <summary>
        /// 数据库连接.
        /// </summary>
        internal DbConnection Connection
        {
            get
            {
                return this.SelectCommand.Connection;
            }
            set
            {
                this.SelectCommand.Connection = value;
            }
        }

        /// <summary>
        /// SQL 命令文本.
        /// </summary>
        internal string CommandText
        {
            get
            {
                return this.SelectCommand.CommandText;
            }
            set
            {
                this.SelectCommand.CommandText = value;
            }
        }

        /// <summary>
        /// 命令超时.
        /// </summary>
        internal int CommandTimeout
        {
            get
            {
                return this.SelectCommand.CommandTimeout;
            }
            set
            {
                this.SelectCommand.CommandTimeout = value;
            }
        }

        /// <summary>
        /// 命令类型.
        /// </summary>
        internal CommandType CommandType
        {
            get
            {
                return this.SelectCommand.CommandType;
            }
            set
            {
                this.SelectCommand.CommandType = value;
            }
        }
        #endregion

        /// <summary>
        /// 清楚参数
        /// </summary>
        internal void ClearParameter()
        {
            this.SelectCommand.Parameters.Clear();
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        internal IDbDataParameter AddParameter(IDbDataParameter para)
        {
            this.SelectCommand.Parameters.Add(para);
            return para;
        }

        /// <summary>
        /// 添加常规参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal IDbDataParameter AddParameter(string name, object value)
        {
            IDbDataParameter para = this.SelectCommand.CreateParameter();
            para.ParameterName = name;
            if (value == null)
            {
                para.Value = DBNull.Value;
            }
            else
            {
                para.Value = value;
            }
            if (this.Connection is OracleConnection)
            {
                if (value != null && value.GetType() == typeof(string))
                {
                    if (value.ToString().Length > 1333)
                    {
                        (para as OracleParameter).OracleType = OracleType.Clob;
                    }
                }
            }
            this.SelectCommand.Parameters.Add(para);
            return para;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        internal IDbDataParameter AddParameter(string name, object value, DbType type, ParameterDirection direction = ParameterDirection.Input, int size = 0)
        {
            IDbDataParameter para = this.SelectCommand.CreateParameter();
            para.ParameterName = name;
            if (value != null)
            {
                para.Value = value;
            }
            else
            {
                para.Value = DBNull.Value;
            }
            para.DbType = type;
            para.Direction = direction;
            if (size > 0)
            {
                para.Size = size;
            }
            if (this.Connection is OracleConnection)
            {
                if (value != null && value.GetType() == typeof(string))
                {
                    if (value.ToString().Length > 1333)
                    {
                        (para as OracleParameter).OracleType = OracleType.Clob;
                    }
                }
            }
            this.SelectCommand.Parameters.Add(para);
            return para;
        }

        ///// <summary>
        ///// 取得分页数据
        ///// </summary>
        ///// <param name="page"></param>
        ///// <returns></returns>
        //internal DataTable GetPagingData(PagingInfo page)
        //{
        //    this.CommandText = "Sys_Get_PagingData_Count";
        //    this.CommandType = CommandType.StoredProcedure;
        //    this.AddParameter("TableNames", page.TableName);
        //    this.AddParameter("KeyFieldNames", page.KeyField);
        //    this.AddParameter("Conditions", page.Conditions);
        //    this.AddParameter("PageSize", page.PageSize);
        //    this.AddParameter("PageIndex", page.PageIndex);
        //    this.AddParameter("FieldNames", page.Fileds);
        //    this.AddParameter("SortFieldNames", page.SortFields);
        //    this.AddParameter("GroupFieldNames", page.GroupFields);
        //    IDbDataParameter recordPara = this.AddParameter("RecordCount", null, DbType.Int32, ParameterDirection.Output);
        //    IDbDataParameter pagecountPara = this.AddParameter("PageCount", null, DbType.Int32, ParameterDirection.Output);
        //    ResetCommandText();
        //    DataTable dt = new DataTable();
        //    this.Fill(dt);
        //    page.RecordCount = Convert.ToInt32(recordPara.Value);
        //    page.PageCount = Convert.ToInt32(pagecountPara.Value);
        //    return dt;
        //}

        /// <summary>
        /// 取得分页数据，同时统计总数据量
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        internal DataTable GetPagingData2(PagingInfo page)
        {
            string condition = string.Empty, groupFields = string.Empty, sortFields = string.Empty;

            //条件
            if (!string.IsNullOrEmpty(page.Conditions))
            {
                condition = " WHERE " + page.Conditions;
            }
            //分组
            if (!string.IsNullOrEmpty(page.GroupFields))
            {
                groupFields = " GROUP BY " + page.GroupFields;
            }
            //排序
            if (!string.IsNullOrEmpty(page.SortFields))
            {
                sortFields = " ORDER BY " + page.SortFields;
            }
            foreach (var item in page.Parameters)
            {
                this.AddParameter(item.Key, item.Value);
            }
            foreach (var item in page.DbParameters)
            {
                this.AddParameter(item);
            }
            if (page.PageSize <= 0)
            {
                page.PageSize = 10;
            }
            if (page.PageIndex <= 0)
            {
                page.PageIndex = 1;
            }

            if (!page.NoCount)
            {
                //总记录数/总页数/当前页码
                if (string.IsNullOrEmpty(groupFields))
                {
                    this.CommandText = string.Format("SELECT COUNT(0) FROM {0} {1}", page.TableName, condition);
                }
                else
                {
                    this.CommandText = string.Format("SELECT COUNT(0) FROM (SELECT COUNT(0) AS C0 FROM {0} {1} {2}) T1", page.TableName, condition, groupFields);
                }
                page.RecordCount = Convert.ToInt32(this.SelectCommand.ExecuteScalar());
                page.PageCount = page.RecordCount / page.PageSize + 1;

                if (page.RecordCount > 0 && page.RecordCount % page.PageSize == 0)
                {
                    page.PageCount -= 1;
                }
                if (page.PageIndex > page.PageCount)
                {
                    page.PageIndex = page.PageCount;
                }
                if (this.ShowSql)
                {
                    DataHelper.SetSqlShow(this.CommandText, parameters: page.Parameters);
                }
            }

            int startIndex = page.PageSize * (page.PageIndex - 1) + 1;
            int endIndex = page.PageSize * page.PageIndex;
            //若不采用自然分页，则使用起始页、结束页方式
            if (page.BeginIndex > 0 && page.EndIndex > 0)
            {
                startIndex = page.BeginIndex;
                endIndex = page.EndIndex;
                page.PageSize = endIndex - startIndex + 1;
            }

            //分页数据            
            switch (DbPro.ProviderType)
            {
                case DbProviderType.SqlServer:
                    {
                        string commandText = "SELECT TOP {0} * FROM ( SELECT TOP {1} ROW_NUMBER() OVER({2}) AS ROWNUM, {3} FROM {4} {5} {6}) AS QUERY_T1 WHERE ROWNUM >= {7}";
                        this.CommandText = string.Format(commandText, page.PageSize, endIndex, sortFields, page.Fileds, page.TableName, condition, groupFields, startIndex);
                        break;
                    }
                case DbProviderType.MySql:
                    {
                        string commandText = "SELECT {0} FROM {1} {2} {3} {4} LIMIT {5},{6}";
                        this.CommandText = string.Format(commandText, page.Fileds, page.TableName, condition, groupFields, sortFields, startIndex - 1, page.PageSize);
                        break;
                    }
                default:
                    {
                        //string commandText = "SELECT * FROM (SELECT QUERY_T1.*,ROWNUM rn FROM (SELECT {1} FROM {0} {2} {3} {4}) QUERY_T1) WHERE rn BETWEEN {5} AND {6}";
                        //20130529 DBA优化分页SQL
                        string commandText = @"SELECT * FROM (SELECT * FROM (SELECT A.*,ROWNUM RN FROM (SELECT {1} FROM {0} {2} {3} {4}) A) WHERE ROWNUM <= {6}) WHERE RN> = {5}";
                        this.CommandText = string.Format(commandText, page.TableName, page.Fileds, condition, groupFields, sortFields, startIndex, endIndex);
                        break;
                    }
            }
            DataTable dt = new DataTable();
            this.Fill(dt);
            if (this.ShowSql)
            {
                DataHelper.SetSqlShow(this.CommandText, parameters: page.Parameters);
            }
            return dt;
        }

        /// <summary>
        /// 根据条件查询数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="conditions"></param>
        /// <param name="orderby"></param>
        /// <param name="groupby"></param>
        /// <returns></returns>
        internal DataTable GetData2(string tableName, string columns, string conditions, string orderby, string groupby)
        {
            //条件
            if (!string.IsNullOrEmpty(conditions))
            {
                conditions = " WHERE " + conditions;
            }
            //分组
            if (!string.IsNullOrEmpty(groupby))
            {
                groupby = " GROUP BY " + groupby;
            }
            //排序
            if (!string.IsNullOrEmpty(orderby))
            {
                orderby = " ORDER BY " + orderby;
            }
            string commandText = "SELECT {0} FROM {1} {2} {3} {4}";
            CommandText = string.Format(commandText, columns, tableName, conditions , groupby, orderby);
            DataTable dt = new DataTable();
            this.Fill(dt);
            if (this.ShowSql)
            {
                DataHelper.SetSqlShow(CommandText);
            }
            return dt;
        }

        ///// <summary>
        ///// 取得查询数据
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <param name="columns"></param>
        ///// <param name="conditions"></param>
        ///// <param name="orderby"></param>
        ///// <param name="groupby"></param>
        ///// <returns></returns>
        //internal DataTable GetData(string tableName, string columns, string conditions, string orderby, string groupby)
        //{
        //    this.CommandText = "Sys_Get_Data";
        //    this.AddParameter("TableNames", tableName);
        //    this.AddParameter("Conditions", conditions);
        //    this.AddParameter("FieldNames", columns);
        //    this.AddParameter("SortFieldNames", orderby);
        //    this.AddParameter("GroupFieldNames", groupby);
        //    this.CommandType = CommandType.StoredProcedure;
        //    ResetCommandText();
        //    DataTable dt = new DataTable();
        //    this.Fill(dt);
        //    return dt;
        //}

        internal void CloseConnection()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        internal void OpenConnection()
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        ///// <summary>
        ///// 设置Oralce命令参数
        ///// </summary>
        //private void ResetCommandText()
        //{
        //    if (this.Connection is OracleConnection)
        //    {
        //        this.CommandText = string.Format("DMedia_{0}", this.CommandText);
        //        this.CommandText = this.CommandText.Replace("Sys_", "Sys.");

        //        foreach (IDataParameter item in this.SelectCommand.Parameters)
        //        {
        //            item.ParameterName = string.Format("P_{0}", item.ParameterName);
        //        }

        //        if (this.CommandText.Contains("Get_PagingInfo"))
        //        {
        //            this.AddParameter("P_Count", null, DbType.Int32, ParameterDirection.Output);
        //            this.AddParameter("P_PageCount", null, DbType.Int32, ParameterDirection.Output);
        //        }
        //        else if (this.CommandText.Contains("Get_Count"))
        //        {
        //            this.AddParameter("P_Count", null, DbType.Int32, ParameterDirection.Output);
        //        }
        //        else
        //        {
        //            OracleCommand oc = this.SelectCommand as OracleCommand;
        //            oc.Parameters.Add("P_Cursor", OracleType.Cursor).Direction = ParameterDirection.Output;
        //        }
        //    }
        //}

        /// <summary>
        /// 关闭连接，并释放由 System.Data.Common.DbDataAdapter 占用的非托管资源，还可以另外再释放托管资源。
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Connection.Close();
            base.Dispose(disposing);            
        }
    }
}