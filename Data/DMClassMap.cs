using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ViData.Dict;

namespace ViData
{
    public class DMClassMap
    {
        public static IDictionary<string, PCDictionary> PFCDictionary = new Dictionary<string, PCDictionary>();
        public static PCDictionary GetPcDictionary(string key)
        {
            if (PFCDictionary != null && PFCDictionary.ContainsKey(key))
            {
                return PFCDictionary[key];
            }
            return null;
        }
    }

    public enum DbConntionType
    {
        WriteRead = 0,
        OnlyRead = 1
    }

    public interface IClassMap
    {

    }

    public abstract class DMClassMap<T> : IClassMap
        where T : class
    {
        string _typeName;
        string TypeName
        {
            get
            {
                if (string.IsNullOrEmpty(_typeName))
                {
                    _typeName = typeof(T).FullName;
                }
                return _typeName;
            }
        }

        /// <summary>
        /// 映射表名
        /// </summary>
        /// <param name="tableName"></param>
        protected void Table(string tableName)
        {
            this.AddToPfc(TypeName, tableName);
        }

        /// <summary>
        /// 映射ID
        /// </summary>
        /// <param name="memberExpression"></param>
        protected DMPropertyPart Id(Expression<Func<T, object>> memberExpression)
        {
            DMPropertyPart pp = new DMPropertyPart() { TypeName = TypeName };
            string pName = this.GetPropertyName(memberExpression);
            this.AddToPfc(TypeName, pName, pName, idName: pName);
            pp.ColumnName = pName;
            return pp;
        }

        /// <summary>
        /// 映射ID
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <param name="column"></param>
        protected DMPropertyPart Id(Expression<Func<T, object>> memberExpression, string column)
        {
            DMPropertyPart pp = new DMPropertyPart() { TypeName = TypeName };
            string pName = this.GetPropertyName(memberExpression);
            this.AddToPfc(TypeName, pName, column, idName: column);
            pp.ColumnName = pName;
            return pp;
        }

        /// <summary>
        /// 映射字段
        /// </summary>
        /// <param name="memberExpression"></param>
        public DMPropertyPart Map(Expression<Func<T, object>> memberExpression)
        {
            DMPropertyPart pp = new DMPropertyPart() { TypeName = TypeName };
            string pName = this.GetPropertyName(memberExpression);
            this.AddToPfc(TypeName, pName, pName);
            pp.ColumnName = pName;
            return pp;
        }

        /// <summary>
        /// 映射字段
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <param name="columnName"></param>
        protected DMPropertyPart Map(Expression<Func<T, object>> memberExpression, string columnName)
        {
            DMPropertyPart pp = new DMPropertyPart() { TypeName = TypeName };
            string pName = this.GetPropertyName(memberExpression);
            this.AddToPfc(TypeName, pName, columnName);
            pp.ColumnName = columnName;
            return pp;
        }

        private string GetPropertyName(Expression<Func<T, object>> memberExpression)
        {
            MemberExpression me = null;
            if (memberExpression.Body.NodeType == ExpressionType.Convert)
            {
                me = (memberExpression.Body as UnaryExpression).Operand as MemberExpression;   
            }
            else if (memberExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                me = memberExpression.Body as MemberExpression;
            }
            if (me == null)
            {
                throw new ArgumentException("Not a member access", "expression");
            }
            string pName = me.Member.Name.ToLower();
            return pName;
        }

        private void AddToPfc(string typeName, string tableName)
        {
            this.AddToPfc(TypeName, null, null, tableName);
        }

        private void AddToPfc(string typeName, string proName, string columnName, string tableName = null, string idName = null)
        {
            PCDictionary pc;
            if (DMClassMap.PFCDictionary.ContainsKey(typeName))
            {
                pc = DMClassMap.PFCDictionary[typeName];                
            }
            else
            {
                pc = new PCDictionary();
                DMClassMap.PFCDictionary.Add(typeName, pc);
            }
            if (!string.IsNullOrEmpty(tableName))
            {
                pc.TableName = tableName;
            }
            else if (!string.IsNullOrEmpty(idName))
            {
                pc.IdName = idName;
                if (!pc.ObjectDictionary.ContainsKey(proName))
                {
                    pc.ObjectDictionary.Add(proName, idName);
                }
            }
            else
            {
                if (!pc.ObjectDictionary.ContainsKey(proName))
                {
                    pc.ObjectDictionary.Add(proName, columnName);
                }
            }
        }
    }
}
