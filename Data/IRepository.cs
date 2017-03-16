using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ViData.Dict;

namespace ViData
{
    /// <summary>
    /// 基础接口
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public interface IRepository<V> : IDisposable where V : class 
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        string DBString { get; set; }
        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="model"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        object Insert(V model, FluDataAdapter adapter = null);
        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        object Insert(Expression<Func<V>> expression, FluDataAdapter adapter = null);
        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="expression">需要插入的字段</param>
        /// <param name="values">对应值</param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        [Obsolete("该方法已过期，请使用其他重载")]
        object Insert(Expression<Func<V, object[]>> expression, object[] values, FluDataAdapter adapter = null);   
        /// <summary>
        /// 根据ID更新对象所有字段
        /// </summary>
        /// <param name="model"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        int Update(V model, FluDataAdapter adapter = null);
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="expression">需要更新的字段</param>
        /// <param name="where">更新条件</param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        int Update(Expression<Func<V>> expression, Expression<Func<V, bool>> where, FluDataAdapter adapter = null);
        /// <summary>
        /// 更新需要更新的字段FOR SQL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="expression">需要更新的字段</param>
        /// <param name="newValues">更新字段的值</param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        int Update(object id, Expression<Func<V, object[]>> expression, object[] newValues, FluDataAdapter adapter = null);
        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adapter">事务adapter</param>
        /// <returns></returns>
        int Delete(object id, FluDataAdapter adapter = null);
        /// <summary>
        /// 删除一系列对象
        /// </summary>
        /// <param name="expression">where表达式</param>
        /// <param name="adapter"></param>
        /// <returns></returns>
        int Delete(Expression<Func<V, bool>> expression, FluDataAdapter adapter = null);
        ///// <summary>
        ///// 查询列表，无条件
        ///// </summary>
        ///// <param name="order">排序表达式</param>
        ///// <param name="flash"></param>
        ///// <returns></returns>
        //IList<V> Get(Expression<Func<OrderExpression<V>, object>> order = null, bool flash = false);
        /// <summary>
        /// 查询列表所有字段
        /// </summary>
        /// <param name="where"></param>
        /// <param name="order">排序表达式</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        IList<V> Get(Expression<Func<V, bool>> where = null, Expression<Func<OrderExpression<V>, object>> order = null, bool flash = false);
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="expression">需要查询的字段</param>
        /// <param name="where"></param>
        /// <param name="order">排序表达式</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        IList<V> Get(Expression<Func<V, object[]>> expression, Expression<Func<V, bool>> where = null, Expression<Func<OrderExpression<V>, object>> order = null, bool flash = false);
        /// <summary>
        /// 取得分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="flash"></param>
        /// <returns></returns>
        IList<V> GetPaging(PagingInfo page, bool flash = false);
        /// <summary>
        /// 取得单个对象，FOR SQL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="express">需要查询的属性字段</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        V GetById(object id, Expression<Func<V, object[]>> express, bool flash = false);
        /// <summary>
        /// 取得单个对象所有属性，FOR SQL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flash"></param>
        /// <returns></returns>
        V GetById(object id, bool flash = false);
        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <param name="values">查询条件值</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        int Exists(Expression<Func<V, object[]>> expression, object[] values, bool flash = false);
        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        [Obsolete("该方法已过期，请使用Count()方法")]
        int Exists(Expression<Func<V>> expression, bool flash = false);
        /// <summary>
        /// 统计对象总数
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <param name="flash"></param>
        /// <returns></returns>
        int Count(Expression<Func<V, bool>> expression = null, bool flash = false);
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
        IList<V> GetData(string tableName, string columns, string conditions = null, string orderby = null, string groupby = null, bool flash = false);
        /// <summary>
        /// 发送到消息队列
        /// </summary>
        /// <param name="model"></param>
        void SendMesssage(V model);        
    }
}
