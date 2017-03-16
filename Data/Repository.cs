using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ViCore;
using ViData.Dict;
using ViData.ExpressionEval;
using ViCore.Messaging;
using System.Web;
using System.Data.OracleClient;

namespace ViData
{
    /// <summary>
    /// 基类
    /// </summary>
    /// <typeparam name="V">值对象Contract</typeparam>
    /// <typeparam name="D">域对象Domain</typeparam>
    public abstract class Repository<V, D> : IRepository<V>
        where V : class
        where D : class
    {
        #region 属性
        string _typeName;
        string TypeName
        {
            get
            {
                if (string.IsNullOrEmpty(_typeName))
                {
                    _typeName = typeof(D).FullName;
                }
                return _typeName;
            }
        }

        DbProviderEx providerEx
        {
            get 
            {
                if (!string.IsNullOrEmpty(DBString))
                {
                    var dm = DMHelper.Instance._sessionfactory.SessionArray.Where(s => s.SessionKey == DBString).First();
                    return dm.ProviderEx;
                }
                return DMHelper.Instance.ProviderEx; 
            }
        }

        public string DBString { get; set; }
        #endregion

        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="model"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        public object Insert(V model, FluDataAdapter adapter = null)
        {
            var pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            D obj = Helper.Copy<D>(model);
            object newId;
            var command = GetPropertiesInsert(obj, out newId, adapter);
            var pList = GetOraParameters(command);
            long result = 0;
            if (pList == null)  //含oracletype配置项
            {
                if (providerEx.ProviderType == DbProviderType.Oracle || !pc.IdIdentity)
                {
                    result = DataHelper.ExcuteNonQuery(command.Key, CommandType.Text, command.Value.Key, command.Value.Value, adapter, DBString);
                }
                else
                {
                    string idsql = GetInsertId(providerEx.ProviderType, adapter, pc.TableName).ToString();
                    newId = result = Convert.ToInt64(DataHelper.ExcuteScalar(command.Key + idsql, CommandType.Text, command.Value.Key, command.Value.Value, DbConntionType.WriteRead, adapter, DBString));
                    
                }
            }
            else
            {
                if (providerEx.ProviderType == DbProviderType.Oracle || !pc.IdIdentity)
                {
                    result = DataHelper.ExcuteNonQuery3(command.Key, pList, adapter: adapter, dbString: DBString);
                }
                else
                {
                    string idsql = GetInsertId(providerEx.ProviderType, adapter, pc.TableName).ToString();
                    newId = result = Convert.ToInt64(DataHelper.ExcuteScalar3(command.Key + idsql, pList, CommandType.Text, DbConntionType.WriteRead, adapter, DBString));
                }
            }
            if (result > 0)
            {
                return newId;
            }
            return null;
        }

        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="expression">需要插入的字段</param>
        /// <param name="values">对应值</param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        [Obsolete("该方法已过期，请使用其他重载")]
        public object Insert(Expression<Func<V, object[]>> expression, object[] values, FluDataAdapter adapter = null)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            int index = -1;
            var cols = GetPropertiesInsert(expression, ref index);
            IList<string> param = cols.Value.ToList();
            IList<object> vals = values.ToList();
            object id = null;
            if (pc.IdIdentity)
            {
                if (providerEx.ProviderType == DbProviderType.Oracle)
                {
                    id = GetInsertId(providerEx.ProviderType, adapter, pc.TableName, pc.SeqName);
                    if (index >= 0)
                    {
                        vals.RemoveAt(index);
                    }
                    vals.Add(id);
                }
            }
            else if (index >= 0)    //非自增主键ID
            {
                id = values[index];
            }
            string sql = string.Format("INSERT INTO {0}{1}", pc.TableName, cols.Key);
            var pList = GetOraParameters(param.ToArray(), vals.ToArray());
            int result = 0;
            if (pList == null)
            {
                result = DataHelper.ExcuteNonQuery(sql, CommandType.Text, param.ToArray(), vals.ToArray(), adapter, DBString);
            }
            else
            {
                result = DataHelper.ExcuteNonQuery3(sql, pList, adapter: adapter, dbString: DBString);
            }
            if (result > 0)
            {
                if (providerEx.ProviderType != DbProviderType.Oracle && id == null)
                {
                    id = GetInsertId(providerEx.ProviderType, adapter, pc.TableName);
                }
                return id;
            }
            return null;
        }

        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        public object Insert(Expression<Func<V>> expression, FluDataAdapter adapter = null)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            object id;
            var command = GetPropertiesInsert(expression, out id, adapter);
            var pList = GetOraParameters(command);
            long result = 0;
            if (pList == null)
            {
                if (providerEx.ProviderType == DbProviderType.Oracle || !pc.IdIdentity)
                {
                    result = DataHelper.ExcuteNonQuery(command.Key, CommandType.Text, command.Value.Key, command.Value.Value, adapter, DBString);
                }
                else
                {
                    string idsql = GetInsertId(providerEx.ProviderType, adapter, pc.TableName).ToString();
                    id = result = Convert.ToInt64(DataHelper.ExcuteScalar(command.Key + idsql, CommandType.Text, command.Value.Key, command.Value.Value, DbConntionType.WriteRead, adapter, DBString));
                }
            }
            else
            {
                if (providerEx.ProviderType == DbProviderType.Oracle || !pc.IdIdentity)
                {
                    result = DataHelper.ExcuteNonQuery3(command.Key, pList, adapter: adapter, dbString: DBString);
                }
                else
                {
                    string idsql = GetInsertId(providerEx.ProviderType, adapter, pc.TableName).ToString();
                    id = result = Convert.ToInt64(DataHelper.ExcuteScalar3(command.Key, pList, CommandType.Text, DbConntionType.WriteRead, adapter, DBString));
                }
            }
            if (result > 0)
            {
                return id;
            }
            return null;
        }
         
        /// <summary>
        /// 批量插入对象
        /// </summary>
        /// <param name="list"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        public object InsertBatch(IList<V> list, FluDataAdapter adapter = null)
        {
            //string sql = "INSERT INTO TABLES(NAME,EMAIL) SELECT 'NAME','EMAIL' UNION SELECT 'NAME2','EMAIL2'";
            return null;
        }

        /// <summary>
        /// 根据ID更新对象所有字段
        /// </summary>
        /// <param name="model"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        public int Update(V model, FluDataAdapter adapter = null)
        {
            var pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            D obj = Helper.Copy<D>(model);
            var command = GetPropertiesUpdate(obj);
            var pList = GetOraParameters(command);
            if (pList == null)
            {
                return DataHelper.ExcuteNonQuery(command.Key, CommandType.Text, command.Value.Key, command.Value.Value, adapter, DBString);
            }
            return DataHelper.ExcuteNonQuery3(command.Key, pList, adapter: adapter, dbString: DBString);
        }

        /// <summary>
        /// 更新需要更新的字段
        /// </summary>
        /// <param name="id"></param>
        /// <param name="expression"></param>
        /// <param name="newValues"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        public int Update(object id, Expression<Func<V, object[]>> expression, object[] newValues, FluDataAdapter adapter = null)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            var updates = GetPropertiesUpdate(expression);
            string sql = string.Format("UPDATE {0} SET {2} WHERE {1} = {3}p__{1}", pc.TableName, pc.IdName, updates.Key, providerEx.ParameterType);
            updates.Value.Add(providerEx.ParameterType+ "p__"+ pc.IdName);
            var newV = newValues.ToList();
            newV.Add(id);
            var pList = GetOraParameters(updates.Value.ToArray(), newV.ToArray());
            if (pList == null)
            {
                return DataHelper.ExcuteNonQuery(sql, CommandType.Text, updates.Value.ToArray(), newV.ToArray(), adapter, DBString);
            }
            return DataHelper.ExcuteNonQuery3(sql, pList, adapter: adapter, dbString: DBString);
        }

        /// <summary>
        /// 更新需要更新的字段
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="where"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        public int Update(Expression<Func<V>> expression, Expression<Func<V, bool>> where, FluDataAdapter adapter = null)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            var command = GetPropertiesUpdate(expression, where);
            var pList = GetOraParameters(command);
            if (pList == null)
            {
                return DataHelper.ExcuteNonQuery(command.Key, CommandType.Text, command.Value.Key, command.Value.Value, adapter, DBString);
            }
            return DataHelper.ExcuteNonQuery3(command.Key, pList, adapter: adapter, dbString: DBString);
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        public int Delete(object id, FluDataAdapter adapter = null)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            string sql = string.Format("DELETE FROM {0} WHERE {1} = {2}p__{1}", pc.TableName, pc.IdName, providerEx.ParameterType);
            string[] param = new string[] { providerEx.ParameterType + "p__"+ pc.IdName };
            object[] pv = new object[] { id };
            return DataHelper.ExcuteNonQuery(sql, CommandType.Text, param, pv, adapter, DBString);
        }

        /// <summary>
        /// 删除一系列对象
        /// </summary>
        /// <param name="expression">where表达式</param>
        /// <param name="adapter"></param>
        /// <returns></returns>
        public int Delete(Expression<Func<V, bool>> expression, FluDataAdapter adapter = null)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            var command = GetPropertiesDelete(expression);
            return DataHelper.ExcuteNonQuery(command.Key, CommandType.Text, command.Value.Key, command.Value.Value, adapter, DBString);
        }

        ///// <summary>
        ///// 查询列表，无条件
        ///// </summary>
        ///// <param name="order">排序表达式</param>
        ///// <param name="flash"></param>
        ///// <returns></returns>
        //public IList<V> Get(Expression<Func<OrderExpression<V>, object>> order = null, bool flash = false)
        //{
        //    var pc = DMClassMap.GetPcDictionary(TypeName);
        //    StringBuilder cols = new StringBuilder();
        //    foreach (var item in typeof(D).GetProperties())
        //    {
        //        string column = pc.GetColumn(item.Name);
        //        if (!string.IsNullOrEmpty(column))
        //        {
        //            cols.AppendFormat("{0},", column);
        //        }
        //    }
        //    string orderSql = GetOrderSql(order);
        //    string sql = string.Format("SELECT {0} FROM {1} {2}", cols.ToString().TrimEnd(','), pc.TableName, orderSql);
        //    return DataHelper.Fill<V>(sql, type: flash ? DbConntionType.WriteRead : DbConntionType.OnlyRead);
        //}

        /// <summary>
        /// 查询列表所有字段
        /// </summary>
        /// <param name="where"></param>
        /// <param name="order">排序表达式</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        public IList<V> Get(Expression<Func<V, bool>> where = null, Expression<Func<OrderExpression<V>, object>> order = null, bool flash = false)
        {
            return Get(a => new object[] { }, where, order, flash);
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="expression">需要查询的字段</param>
        /// <param name="where"></param>
        /// <param name="order">排序表达式</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        public IList<V> Get(Expression<Func<V, object[]>> expression, Expression<Func<V, bool>> where = null, Expression<Func<OrderExpression<V>, object>> order = null, bool flash = false)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            var command = GetPropertiesSelect(expression, where, order);
            IList<IDbDataParameter> pList = GetOraParameters(command);
            DataTable dt;
            if (pList != null)
            {
                dt = DataHelper.Fill3(command.Key, pList, dbString: DBString);
            }
            else
            {
                dt = DataHelper.Fill(command.Key, CommandType.Text, command.Value.Key, command.Value.Value, dbString: DBString);
            }
            return Helper.GetObjects<V>(dt);
        }

        /// <summary>
        /// 取得分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="flash"></param>
        /// <returns></returns>
        public IList<V> GetPaging(PagingInfo page, bool flash = false)
        {
            DataTable dt = DataHelper.GetPagingData(page, flash ? DbConntionType.WriteRead : DbConntionType.OnlyRead, DBString);
            return Helper.GetObjects<V>(dt);
        }

        /// <summary>
        /// 取得单个对象所有属性，FOR SQL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flash"></param>
        /// <returns></returns>
        public V GetById(object id, bool flash = false)
        {
            return this.GetById(id, a => new object[] { });
        }

        /// <summary>
        /// 取得单个对象，FOR SQL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="expression">需要查询的属性字段</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        public V GetById(object id, Expression<Func<V, object[]>> expression, bool flash = false)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            string columns = GetProperties(expression);
            string sql = string.Format("SELECT {2} FROM {0} WHERE {1} = {3}p__{1}", pc.TableName, pc.IdName, columns, providerEx.ParameterType);
            string[] param = new string[] { providerEx.ParameterType +"p__"+ pc.IdName };
            object[] pv = new object[] { id };
            DataTable dt = DataHelper.Fill(sql, CommandType.Text, param, pv, flash ? DbConntionType.WriteRead : DbConntionType.OnlyRead, dbString: DBString);
            return Helper.GetObjects<V>(dt).FirstOrDefault();
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="values"></param>
        /// <param name="flash"></param>
        /// <returns></returns>
        public int Exists(Expression<Func<V, object[]>> expression, object[] values, bool flash = false)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            var kvp = GetPropertiesWhere(expression);
            string sql = string.Format("SELECT 1 FROM {0} WHERE ROWNUM = 1 {1} ", pc.TableName, kvp.Key);
            DataTable dt = DataHelper.Fill(sql, CommandType.Text, kvp.Value, values, flash ? DbConntionType.WriteRead : DbConntionType.OnlyRead, dbString: DBString);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows.Count;
            }
            return 0;
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        public int Exists(Expression<Func<V>> expression, bool flash = false)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            var command = GetPropertiesWhere(expression);
            var pList = GetOraParameters(command);
            DataTable dt;
            if (pList == null)
            {
                dt = DataHelper.Fill(command.Key, CommandType.Text, command.Value.Key, command.Value.Value, flash ? DbConntionType.WriteRead : DbConntionType.OnlyRead, DBString);
            }
            else
            {
                dt = DataHelper.Fill3(command.Key,pList, type: flash? DbConntionType.WriteRead : DbConntionType.OnlyRead, dbString: DBString);
            }
            return dt.Rows.Count;
        }

        /// <summary>
        /// 统计对象总数
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        public int Count(Expression<Func<V, bool>> expression = null, bool flash = false)
        {
            PCDictionary pc = DMClassMap.GetPcDictionary(TypeName);
            CheckSqlTable(pc);
            var command = GetPropertiesCount(expression);
            var pList = GetOraParameters(command);
            int counts = 0;
            if (pList == null)
            {
                counts = Convert.ToInt32(DataHelper.ExcuteScalar(command.Key, CommandType.Text, command.Value.Key, command.Value.Value, flash ? DbConntionType.WriteRead : DbConntionType.OnlyRead, dbString: DBString));
            }
            else
            {
                counts = Convert.ToInt32(DataHelper.ExcuteScalar3(command.Key, pList, CommandType.Text, flash ? DbConntionType.WriteRead : DbConntionType.OnlyRead, dbString: DBString));
            }
            return counts;
        }

        /// <summary>
        /// 取得查询数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="conditions"></param>
        /// <param name="orderby"></param>
        /// <param name="groupby"></param>
        /// <param name="flash"></param>
        /// <returns></returns>
        public IList<V> GetData(string tableName, string columns, string conditions = null, string orderby = null, string groupby = null, bool flash = false)
        {
            DataTable dt = DataHelper.GetData(tableName, columns, conditions, orderby, groupby, flash ? DbConntionType.WriteRead : DbConntionType.OnlyRead, DBString);
            return Helper.GetObjects<V>(dt);
        }

        /// <summary>
        /// 发送到消息队列
        /// </summary>
        /// <param name="model"></param>
        public void SendMesssage(V model)
        {
            if (MQManager.Instance.Enable)
            {
                MQManager.Instance.SendMessage(model);
            }
            else
            {
                Insert(model);
            }
        }

        #region 私有方法
        private KeyValuePair<string, KeyValuePair<string[], object[]>> GetPropertiesInsert(D obj, out object id, FluDataAdapter adapter = null)
        {
            string sql = "INSERT INTO {0}({1}) VALUES({2})";
            var pc = DMClassMap.GetPcDictionary(TypeName);
            StringBuilder cols = new StringBuilder();
            StringBuilder vals = new StringBuilder();
            List<string> param = new List<string>();
            List<object> pvalue = new List<object>();
            int i = 0;
            id = null;
            foreach (var item in obj.GetType().GetProperties())
            {
                string column = pc.GetColumn(item.Name);
                if (!string.IsNullOrEmpty(column))
                {
                    if (column == pc.IdName && pc.IdIdentity)
                    {
                        if (providerEx.ProviderType == DbProviderType.Oracle)
                        {
                            id = GetInsertId(providerEx.ProviderType, adapter, pc.TableName, pc.SeqName);
                            cols.AppendFormat("{0},", pc.IdName);
                            vals.AppendFormat("{1}p__{0},", pc.IdName, providerEx.ParameterType);
                            param.Add(providerEx.ParameterType + "p__" + pc.IdName);
                            pvalue.Add(id);
                        }
                    }
                    else
                    {
                        string paraName = string.Format("{2}p{0}__{1}", i.ToString(), column, providerEx.ParameterType);
                        cols.AppendFormat("{0},", column);
                        vals.AppendFormat("{0},", paraName);
                        param.Add(paraName);
                        pvalue.Add(item.GetValue(obj, null));
                        if (pc.IdName == column)
                        {
                            id = item.GetValue(obj, null);
                        }
                    }
                    i++;
                }
            }
            sql = string.Format(sql, pc.TableName, cols.ToString().TrimEnd(','), vals.ToString().TrimEnd(','));
            return new KeyValuePair<string, KeyValuePair<string[], object[]>>(sql, new KeyValuePair<string[], object[]>(
param.ToArray(), pvalue.ToArray()));
        }

        private KeyValuePair<string, string[]> GetPropertiesInsert(Expression<Func<V, object[]>> expression, ref int index)
        {
            StringBuilder cols = new StringBuilder("(");
            StringBuilder colsParam = new StringBuilder(" VALUES(");
            List<string> param = new List<string>();
            int i = 0;
            var pcDict = DMClassMap.GetPcDictionary(TypeName);

            foreach (MemberExpression exp in GetExpressionMembers(expression))
            {
                string column = pcDict.GetColumn(exp.Member.Name);
                if (!string.IsNullOrEmpty(column))
                {
                    if (pcDict.IdIdentity && pcDict.IdName == column)
                    {
                        index = i;  //主键ID所在位置
                    }
                    else
                    {
                        string paramName = string.Format("{2}p{0}__{1}", i.ToString(), column, providerEx.ParameterType);
                        cols.AppendFormat("{0},", column);
                        colsParam.AppendFormat("{0},", paramName);
                        param.Add(paramName);
                    }
                    i++;
                }
            }
            if (pcDict.IdIdentity && !string.IsNullOrEmpty(pcDict.SeqName))
            {
                cols.AppendFormat("{0},", pcDict.IdName);
                colsParam.AppendFormat("{1}p__{0},", pcDict.IdName, providerEx.ParameterType);
                param.Add(providerEx.ParameterType +"p__"+ pcDict.IdName);
            }
            if (cols.Length > 0 && cols.ToString().EndsWith(","))
            {
                cols.Remove(cols.Length - 1, 1);
            }
            if (colsParam.Length > 0 && colsParam.ToString().EndsWith(","))
            {
                colsParam.Remove(colsParam.Length - 1, 1);
            }
            cols.Append(")");
            colsParam.Append(")");
            return new KeyValuePair<string, string[]>(cols.Append(colsParam).ToString(), param.ToArray());
        }

        private KeyValuePair<string, KeyValuePair<string[], object[]>> GetPropertiesInsert(Expression<Func<V>> expression, out object id, FluDataAdapter adapter = null)
        {
            var insertExp = expression.Body as MemberInitExpression;
            var insertList = insertExp.Bindings.Cast<MemberAssignment>().Select(a => new
            {
                Name = a.Member.Name,
                Value = GetMemeberValue(a)
            });
            StringBuilder sqlInsert = new StringBuilder();
            StringBuilder valueInsert = new StringBuilder();
            int i = 0;
            id = null;
            List<string> param = new List<string>();
            List<object> pvalue = new List<object>();
            var pc = DMClassMap.GetPcDictionary(TypeName);
            if (pc.IdIdentity)
            {
                if (providerEx.ProviderType == DbProviderType.Oracle)
                {
                    id = GetInsertId(providerEx.ProviderType, adapter, pc.TableName, pc.SeqName);
                    sqlInsert.AppendFormat("{0},", pc.IdName);
                    valueInsert.AppendFormat("{1}p__{0},", pc.IdName, providerEx.ParameterType);
                    param.Add(providerEx.ParameterType + "p__" + pc.IdName);
                    pvalue.Add(id);
                }
            }
            foreach (var item in insertList)
            {
                string column = pc.GetColumn(item.Name);
                if (!string.IsNullOrEmpty(column))
                {
                    if (pc.IdName == column && pc.IdIdentity)
                    {
                    }
                    else
                    {
                        string pname = string.Format("{2}p{0}__{1}", i.ToString(), column, providerEx.ParameterType);
                        sqlInsert.AppendFormat("{0},", column);
                        valueInsert.AppendFormat("{0},", pname);
                        param.Add(pname);
                        pvalue.Add(item.Value);
                        i++;
                        if (column == pc.IdName)
                        {
                            id = item.Value;
                        }
                    }
                }
            }
            string insert = sqlInsert.ToString().TrimEnd(',');
            string values = valueInsert.ToString().TrimEnd(',');
            string sql = string.Format("INSERT INTO {0}({1}) VALUES({2})", DMClassMap.GetPcDictionary(TypeName).TableName, insert, values);
            KeyValuePair<string, KeyValuePair<string[], object[]>> kvp = new KeyValuePair<string, KeyValuePair<string[], object[]>>(sql, new KeyValuePair<string[], object[]>(param.ToArray(), pvalue.ToArray()));
            return kvp;
        }

        private KeyValuePair<string, KeyValuePair<string[], object[]>> GetPropertiesUpdate(D model)
        {
            StringBuilder cols = new StringBuilder();
            StringBuilder where = new StringBuilder();
            int i = 0;
            var pc = DMClassMap.GetPcDictionary(TypeName);
            List<string> param = new List<string>();
            List<object> vals = new List<object>();
            foreach (var item in model.GetType().GetProperties())
            {
                string columnName = pc.GetColumn(item.Name);
                if (!string.IsNullOrEmpty(columnName))
                {
                    if (columnName == pc.IdName)
                    {
                        param.Add(providerEx.ParameterType +"p__"+ pc.IdName);
                    }
                    else
                    {
                        string paramName = string.Format("{2}p{0}__{1}", i, columnName, providerEx.ParameterType);
                        cols.AppendFormat(" {0} = {1},", columnName, paramName);
                        param.Add(paramName);
                    }
                    vals.Add(item.GetValue(model, null));
                    i++;
                }
            }
            string sql = string.Format("UPDATE {0} SET {1} WHERE {2} = {3}p__{2}", pc.TableName, cols.ToString().TrimEnd(','), pc.IdName, providerEx.ParameterType);
            return new KeyValuePair<string, KeyValuePair<string[], object[]>>(sql, new KeyValuePair<string[], object[]>(param.ToArray(), vals.ToArray()));
        }

        private KeyValuePair<string, List<string>> GetPropertiesUpdate(Expression<Func<V, object[]>> expression)
        {
            StringBuilder cols = new StringBuilder();
            List<string> param = new List<string>();
            int i = 0;
            var pc = DMClassMap.GetPcDictionary(TypeName);
            foreach (MemberExpression exp in GetExpressionMembers(expression))
            {
                string column = pc.GetColumn(exp.Member.Name);
                string paramName = string.Format("{2}p{0}__{1}", i.ToString(), column, providerEx.ParameterType);
                if (!string.IsNullOrEmpty(column))
                {
                    cols.AppendFormat("{0} = {1},", column, paramName);
                    param.Add(paramName);
                    i++;
                }
            }
            return new KeyValuePair<string, List<string>>(cols.ToString().TrimEnd(','), param);
        }

        private KeyValuePair<string, KeyValuePair<string[], object[]>> GetPropertiesUpdate(Expression<Func<V>> expression, Expression<Func<V, bool>> where)
        {
            ConditionBuilder whereBuilder = new ConditionBuilder(TypeName, this.providerEx); ;
            whereBuilder.Build(where.Body);
            var updateExp = expression.Body as MemberInitExpression;
            var updateList = updateExp.Bindings.Cast<MemberAssignment>().Select(a => new
            {
                Name = a.Member.Name,
                Value = GetMemeberValue(a)
            });
            StringBuilder sqlUpdate = new StringBuilder();
            int i = 0;
            List<string> param = new List<string>();
            List<object> pvalue = new List<object>();
            var pc = DMClassMap.GetPcDictionary(TypeName);
            foreach (var item in updateList)
            {
                string column = pc.GetColumn(item.Name);
                string pname = string.Format("{2}p{0}__{1}", i.ToString(), column, providerEx.ParameterType);                
                if (!string.IsNullOrEmpty(column))
                {
                    sqlUpdate.AppendFormat(" {0} = {1},", column, pname);
                    param.Add(pname);
                    pvalue.Add(item.Value);
                    i++;
                }
            }
            param.AddRange(whereBuilder.WhereParam);
            pvalue.AddRange(whereBuilder.Arguments);
            string sql = string.Format("UPDATE {0} SET {1} WHERE {2}", pc.TableName, sqlUpdate.ToString().TrimEnd(','), whereBuilder.Condition);
            KeyValuePair<string, KeyValuePair<string[], object[]>> kvp = new KeyValuePair<string, KeyValuePair<string[], object[]>>(sql, new KeyValuePair<string[], object[]>(param.ToArray(), pvalue.ToArray()));
            return kvp;
        }

        private KeyValuePair<string, KeyValuePair<string[], object[]>> GetPropertiesWhere(Expression<Func<V>> expression)
        {
            var updateExp = expression.Body as MemberInitExpression;
            var updateList = updateExp.Bindings.Cast<MemberAssignment>().Select(a => new
            {
                Name = a.Member.Name,
                Value = GetMemeberValue(a)
            });
            StringBuilder sqlwhere = new StringBuilder();
            int i = 0;
            List<string> param = new List<string>();
            List<object> pvalue = new List<object>();
            var pc = DMClassMap.GetPcDictionary(TypeName);
            foreach (var item in updateList)
            {
                string column = pc.GetColumn(item.Name);
                string pname = string.Format("{2}p{0}__{1}", i.ToString(), column, providerEx.ParameterType);
                if (!string.IsNullOrEmpty(column))
                {
                    sqlwhere.AppendFormat(" {0} = {1},", column, pname);
                    param.Add(pname);
                    pvalue.Add(item.Value);
                    i++;
                }
            }
            string sql = string.Format("SELECT 1 FROM {0} WHERE {1}", DMClassMap.GetPcDictionary(TypeName).TableName, sqlwhere.ToString().TrimEnd(','));
            return new KeyValuePair<string, KeyValuePair<string[], object[]>>(sql, new KeyValuePair<string[], object[]>(param.ToArray(), pvalue.ToArray()));
        }

        private KeyValuePair<string, string[]> GetPropertiesWhere(Expression<Func<V, object[]>> expression)
        {
            StringBuilder cols = new StringBuilder();
            List<string> param = new List<string>();
            int i = 0;
            var pc = DMClassMap.GetPcDictionary(TypeName);
            foreach (MemberExpression exp in GetExpressionMembers(expression))
            {
                string column = pc.GetColumn(exp.Member.Name);
                string paramName = string.Format("{2}p{0}__{1}", i.ToString(), column, providerEx.ParameterType);
                
                if (!string.IsNullOrEmpty(column))
                {
                    cols.AppendFormat(" AND {0} = {1} ", column, paramName);
                    param.Add(paramName);
                    i++;
                }
            }
            return new KeyValuePair<string, string[]>(cols.ToString(), param.ToArray());
        }

        private KeyValuePair<string, KeyValuePair<string[], object[]>> GetPropertiesCount(Expression<Func<V, bool>> expression)
        {
            ConditionBuilder whereBuilder = new ConditionBuilder(TypeName, this.providerEx);
            string whereStr = "";
            if (expression != null)
            {
                whereBuilder.Build(expression.Body);
                whereStr = "WHERE " + whereBuilder.Condition;
            }
            string sql = string.Format("SELECT COUNT(0) FROM {0} {1}", DMClassMap.GetPcDictionary(TypeName).TableName, whereStr);
            KeyValuePair<string, KeyValuePair<string[], object[]>> kvp = new KeyValuePair<string, KeyValuePair<string[], object[]>>(sql, new KeyValuePair<string[], object[]>(whereBuilder.WhereParam.ToArray(), whereBuilder.Arguments));
            return kvp;
        }

        private KeyValuePair<string, KeyValuePair<string[], object[]>> GetPropertiesDelete(Expression<Func<V, bool>> expression)
        {
            ConditionBuilder whereBuilder = new ConditionBuilder(TypeName, this.providerEx);
            whereBuilder.Build(expression.Body);
            string sql = string.Format("DELETE FROM {0} WHERE {1}", DMClassMap.GetPcDictionary(TypeName).TableName, whereBuilder.Condition);
            KeyValuePair<string, KeyValuePair<string[], object[]>> kvp = new KeyValuePair<string, KeyValuePair<string[], object[]>>(sql, new KeyValuePair<string[], object[]>(whereBuilder.WhereParam.ToArray(), whereBuilder.Arguments));
            return kvp;
        }

        private KeyValuePair<string, KeyValuePair<string[], object[]>> GetPropertiesSelect(Expression<Func<V, object[]>> expression, Expression<Func<V, bool>> where, Expression<Func<OrderExpression<V>, object>> order = null)
        {
            ConditionBuilder whereBuilder = new ConditionBuilder(TypeName, this.providerEx);
            string whereStr = "";
            if (where != null)
            {
                whereBuilder.Build(where.Body);
                whereStr += "WHERE "+ whereBuilder.Condition;
            }
            string columns = GetProperties(expression);
            string orderSql = GetOrderSql(order);
            string sql = string.Format("SELECT {0} FROM {1} {2} {3}", columns, DMClassMap.GetPcDictionary(TypeName).TableName, whereStr, orderSql);
            KeyValuePair<string, KeyValuePair<string[], object[]>> kvp = new KeyValuePair<string, KeyValuePair<string[], object[]>>(sql, new KeyValuePair<string[], object[]>(whereBuilder.WhereParam.ToArray(), whereBuilder.Arguments));
            return kvp;
        }

        private string GetProperties(Expression<Func<V, object[]>> expression)
        {
            StringBuilder cols = new StringBuilder();
            var pc = DMClassMap.GetPcDictionary(TypeName);
            foreach (MemberExpression exp in GetExpressionMembers(expression))
            {
                string column = pc.GetColumn(exp.Member.Name);
                if (!string.IsNullOrEmpty(column))
                {
                    cols.Append(column + ",");
                }
            }
            if (cols.Length == 0)
            {
                foreach (var item in pc.ObjectDictionary.Values)
                {
                    cols.Append(item + ",");
                }
            }
            return cols.ToString().TrimEnd(',');
        }

        private string GetOrderSql(Expression<Func<OrderExpression<V>, object>> order = null)
        {
            if (order != null)
            {
                var oe = order.Compile().Invoke(new OrderExpression<V>()) as OrderExpression<V>;
                if (oe.OrderByList.Count > 0)
                {
                    var pc = DMClassMap.GetPcDictionary(TypeName);
                    string orderSql = " ORDER BY ";
                    foreach (var item in oe.OrderByList)
                    {
                        string col = pc.GetColumn(item.PropertyName);
                        string by = ",";
                        if (item.SortType == OrderType.Desc)
                        {
                            by = " DESC,";
                        }
                        orderSql += col + by;
                    }
                    return orderSql.TrimEnd(',');
                }
            }
            return null;
        }

        private IList<MemberExpression> GetExpressionMembers(Expression<Func<V, object[]>> expression)
        {
            IList<MemberExpression> list = new List<MemberExpression>();
            if (expression.Body.NodeType == ExpressionType.NewArrayInit)
            {
                var arrExp = (expression.Body as NewArrayExpression).Expressions;
                foreach (Expression exp in arrExp)
                {
                    MemberExpression me;
                    if (exp.NodeType == ExpressionType.Convert)
                    {
                        me = (exp as UnaryExpression).Operand as MemberExpression;
                    }
                    else
                    {
                        me = exp as MemberExpression;
                    }
                    list.Add(me);
                }
            }
            return list;
        }

        private void CheckSqlTable(PCDictionary pc)
        {
            if (pc == null)
            {
                throw new NullReferenceException("No PCDictionary Init : " + typeof(D).FullName);
            }
            if (string.IsNullOrEmpty(pc.TableName))
            {
                throw new NullReferenceException("No Table Name Assigned : " + typeof(D).FullName);
            }
        }

        private object GetMemeberValue(MemberAssignment assgin)
        {
            if (assgin.Expression is ConstantExpression)
            {
                return (assgin.Expression as ConstantExpression).Value;
            }
            LambdaExpression lambda = Expression.Lambda(assgin.Expression);
            Delegate fn = lambda.Compile();
            return Expression.Constant(fn.DynamicInvoke(null), assgin.Expression.Type).Value;
        }

        private IList<IDbDataParameter> GetOraParameters(KeyValuePair<string, KeyValuePair<string[], object[]>> command)
        {
            return GetOraParameters(command.Value.Key, command.Value.Value);
        }

        private IList<IDbDataParameter> GetOraParameters(string[] pName, object[] pValue)
        {
            var pc = DMClassMap.GetPcDictionary(TypeName);
            if (pc.OraParaDictionary.Count > 0)
            {
                List<IDbDataParameter> pList = new List<IDbDataParameter>();
                int count = pName.Count();
                for (int i = 0; i < count; i++)
                {
                    var ops = pc.OraParaDictionary.Keys.Where(k => pName[i].EndsWith("__" + k)).FirstOrDefault();
                    OracleParameter op = new OracleParameter();
                    if (!string.IsNullOrEmpty(ops))
                    {
                        var p1 = pc.OraParaDictionary[ops];
                        op.OracleType = p1.OracleType;
                        op.SourceColumn = p1.SourceColumn;
                    }
                    else
                    {
                        op = new OracleParameter();
                    }
                    op.ParameterName = pName[i];
                    op.Value = pValue[i];
                    if (op.Value == null && (op.Direction == ParameterDirection.Input || op.Direction == ParameterDirection.InputOutput))
                    {
                        op.Value = DBNull.Value;
                    }
                    pList.Add(op);
                }
                return pList;
            }
            return null;
        }

        object GetInsertId(DbProviderType type, FluDataAdapter adapter = null, string tableName = null, string seqName  = null)
        {
            switch (type)
            {
                case DbProviderType.SqlServer:
                    {
                        string idSql = ";SELECT SCOPE_IDENTITY();";
                        return idSql;
                        //return DataHelper.ExcuteScalar(idSql, type: DbConntionType.WriteRead, adapter: adapter);
                    }
                case DbProviderType.MySql:
                    {
                        string idSql = string.Format(";SELECT LAST_INSERT_ID() AS ID FROM {0} LIMIT 1;", tableName);
                        return idSql;
                        //return DataHelper.ExcuteScalar(idSql, type: DbConntionType.WriteRead, adapter: adapter);
                    }
                default:
                    {
                        string seq = string.Format("SELECT {0}.NEXTVAL FROM DUAL", seqName);
                        return DataHelper.ExcuteScalar(seq, type: DbConntionType.WriteRead, adapter: adapter, dbString:DBString);
                    }
            }
        }

        #endregion

        /// <summary>
        /// 释放连接
        /// </summary>
        public void Dispose()
        {
            DMHelper.Instance.CloseSession();
        }
    }
}
