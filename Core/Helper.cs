using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Diagnostics;

namespace ViCore
{
    public class Helper
    {
        /// <summary>
        /// 复制单个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static T Copy<T>(object model)
        {
            if (model == null)
            {
                return default(T);
            }
            T target = Activator.CreateInstance<T>();
            Type targetType = target.GetType();
            PropertyInfo[] perties = model.GetType().GetProperties();
            foreach (var item in perties)
            {
                var per = targetType.GetProperty(item.Name, BindingFlags.IgnoreCase | BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (per != null && per.CanWrite)
                {
                    per.SetValue(target, item.GetValue(model, null), null);
                }
            }
            
            return target;
        }

        /// <summary>
        /// 复制列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IList<T> CopyList<T,D>(IList<D> list)
        {
            IList<T> newList = new List<T>();
            foreach (object item in list)
            {
                T obj = Copy<T>(item);
                newList.Add(obj);
            }
            return newList;
        }

        /// <summary>
        /// 根据DataTable取得对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IList<T> GetObjects<T>(DataTable dataTable)
        {
            IList<T> objects = new List<T>();
            foreach (DataRow dr in dataTable.Rows)
            {
                T t = Activator.CreateInstance<T>();
                foreach (DataColumn dc in dataTable.Columns)
                {
                    string columnName = dc.ColumnName.Replace("_", "").ToLower();
                    PropertyInfo[] pro = t.GetType().GetProperties();
                    foreach (PropertyInfo p in pro)
                    {
                        if (p.Name.ToLower() == columnName && p.CanWrite && !(dr[dc.ColumnName] is DBNull))
                        {
                            SetPropertyValue<T>(t, p, dr[dc.ColumnName]);
                        }
                    }
                }
                objects.Add(t);
            }
            return objects;
        }

        private static void SetPropertyValue<T>(T obj, PropertyInfo pro, object columnValue)
        {
            if (pro.PropertyType == columnValue.GetType())
            {
                pro.SetValue(obj, columnValue, null);
            }
            else
            {
                if (pro.PropertyType == typeof(short))
                {
                    columnValue = Convert.ToInt16(columnValue);
                }
                else if (pro.PropertyType == typeof(int))
                {
                    columnValue = Convert.ToInt32(columnValue);
                }
                else if (pro.PropertyType == typeof(long))
                {
                    columnValue = Convert.ToInt64(columnValue);
                }
                else if (pro.PropertyType == typeof(bool))
                {
                    columnValue = Convert.ToBoolean(columnValue);
                }
                pro.SetValue(obj, columnValue, null);
            }
        }
    }
}
