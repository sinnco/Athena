using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ViData.Dict
{
    public class OrderExpression<T> where T : class
    {
        public OrderExpression()
        {
            OrderByList = new List<OrderInfo>();
        }

        internal IList<OrderInfo> OrderByList { get; set; }

        public OrderExpression<T> OrderBy(Expression<Func<T, object>> expression)
        {
            SetOrderList(expression);
            return this;
        }

        public OrderExpression<T> OrderByDesc(Expression<Func<T, object>> expression)
        {
            SetOrderList(expression, OrderType.Desc);
            return this;
        }

        void SetOrderList(Expression<Func<T, object>> expression, OrderType type = OrderType.Asc)
        {
            MemberExpression me;
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                me = (expression.Body as UnaryExpression).Operand as MemberExpression;
            }
            else
            {
                me = expression.Body as MemberExpression;
            }
            OrderByList.Add(new OrderInfo() { PropertyName = me.Member.Name, SortType = type });
        }
    }

    internal class OrderInfo
    {
        public string PropertyName { get; set; }
        public OrderType SortType { get; set; }
    }

    internal enum OrderType
    {
        Asc = 0,
        Desc = 1
    }
}
