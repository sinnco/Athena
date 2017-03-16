using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ViData
{
    /// <summary>
    /// 分页对象
    /// </summary>
    public class PagingInfo
    {
        public PagingInfo()
        {
            Parameters = new Dictionary<string, object>();
            DbParameters = new List<IDbDataParameter>();
        }

        /// <summary>
        /// 传入值，表名，可实现外联等
        /// </summary>
        public string TableName { get; set;}
        /// <summary>
        /// 传入值，关键字段，可不填
        /// </summary>
        public string KeyField { get; set; }
        /// <summary>
        /// 传入值，查询条件，可不填
        /// </summary>
        public string Conditions { get; set; }
        /// <summary>
        /// 传入值，每页数据量
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 传入值，当前页
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 传入值，数据的起始索引值，若该值大于0，则PageSize和PageIndex不可用
        /// </summary>
        public int BeginIndex { get; set; }
        /// <summary>
        /// 传入值，数据的结束索引值，若该值大与0，则PageSize和PageIndex不可用
        /// </summary>
        public int EndIndex { get; set; }
        /// <summary>
        /// 传入值，查询字段，最好不使用*
        /// </summary>
        public string Fileds { get; set; }
        /// <summary>
        /// 传入值，排序字段，可不填
        /// </summary>
        public string SortFields { get; set; }
        /// <summary>
        /// 传入值，分组字段，可不填
        /// </summary>
        public string GroupFields { get; set; }
        /// <summary>
        /// SQL参数值
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }
        /// <summary>
        /// SQL参数值
        /// </summary>
        public IList<IDbDataParameter> DbParameters { get; set; }
        /// <summary>
        /// 返回值，返回数据总量
        /// </summary>
        public int RecordCount { get; set; }
        /// <summary>
        /// 返回值，返回总页数
        /// </summary>
        public int PageCount { get; set; }
        /// <summary>
        /// 输入值，是否执行统计查询Count，默认false执行统计，true不执行统计，为false则返回RecordCount, PageCount属性
        /// </summary>
        public bool NoCount { get; set; }
    }
}
