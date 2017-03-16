using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViCore.Messaging
{
    /// <summary>
    /// 消息队列管理对象
    /// </summary>
    public class MQManager
    {
        static readonly MQManager _instance = new MQManager();
        private MQManager() { }
        public static MQManager Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// 队列是否可用
        /// </summary>
        public bool Enable
        {
            get
            {
                return MsgQueue.Enable;
            }
        }

        /// <summary>
        /// 发送消息到队列，若队列不可用，将不会发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">要发送的对象</param>
        /// <param name="queueName">队列节点名称，可为空，默认"T"名</param>
        /// <param name="label">消息概述，可为空，默认"T"名</param>
        public void SendMessage<T>(T model, string queueName = null, string label = null)
        {
            string typeName = typeof(T).Name;
            MsgQueue mq = new MsgQueue();
            if (Enable)
            {
                if (string.IsNullOrEmpty(queueName))
                {
                    mq.HostName = typeName;
                }
                else
                {
                    mq.HostName = queueName;
                }
                if (string.IsNullOrEmpty(label))
                {
                    mq.Input(model, typeName);
                }
                else
                {
                    mq.Input(model, label);
                }
            }
        }
    }
}
