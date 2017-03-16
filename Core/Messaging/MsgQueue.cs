using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Text;
using ViCore.Config;

namespace ViCore.Messaging
{
    public class MsgQueue
    {
        #region 构造
        public MsgQueue(string hostName = null)
        {
            if (!string.IsNullOrEmpty(hostName))
            {
                this.HostName = hostName;
            }
        }
        #endregion

        #region 属性设置
        /// <summary>
        /// 队列是否可用
        /// </summary>
        public static bool Enable
        {
            get
            {
                if (QueueSection == null) { return false; }
                return QueueSection.Enable;
            }
        }

        /// <summary>
        /// 配置项
        /// </summary>
        private static MQSection QueueSection = ConfigurationManager.GetSection("MQSection") as MQSection;

        private string _hostName;
        /// <summary>
        /// 队列名
        /// </summary>
        public string HostName
        {
            get
            {
                if (string.IsNullOrEmpty(_hostName))
                {
                    _hostName = QueueSection.Hosts.Default.Name;
                }
                return _hostName;
            }
            set { _hostName = value; }
        }

        private MQHostElement _host;
        /// <summary>
        /// 配置节点
        /// </summary>
        private MQHostElement Host
        {
            get
            {
                if (_host == null)
                {
                    _host = QueueSection.Hosts.Get(HostName);
                }
                return _host;
            }
        }

        private string _path;
        /// <summary>
        /// 队列路径
        /// </summary>
        private string path
        {
            get
            {
                if (string.IsNullOrEmpty(_path) && !string.IsNullOrEmpty(HostName))
                {
                    if (Host == null)
                    {
                        throw new ArgumentException("配置文件中未找到该队列节点：" + HostName, "Host");
                    }
                    _path = GetLinkPath(Host.Ip, Host.Path);
                }
                return _path;
            }
            set { _path = value; }
        }

        private string _confirmPath;
        /// <summary>
        /// 消息确认路径，含默认值
        /// </summary>
        private string ConfirmPath
        {
            get
            {
                var confirm = Host.AcknowledgeHost;
                if (!string.IsNullOrEmpty(confirm))
                {
                    var item = QueueSection.Hosts.Get(confirm);
                    if (item == null)
                    {
                        throw new Exception("配置文件中未找到该队列节点：" + confirm);
                    }
                    _confirmPath = GetLinkPath(item.Ip, item.Path);
                }
                return _confirmPath;
            }
            set { _confirmPath = value; }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 写入消息队列，单个对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="label"></param>
        public void Input(object obj, string label)
        {
            using (MessageQueue queue = new MessageQueue(path, QueueAccessMode.Send))
            {
                using (Message msg = new Message(obj, GetXmlFormatter()))
                {
                    if (!string.IsNullOrEmpty(ConfirmPath))
                    {
                        msg.AdministrationQueue = new MessageQueue(ConfirmPath);
                        msg.AcknowledgeType = (AcknowledgeTypes)Host.AcknowledgeType;
                    }
                    msg.UseJournalQueue = Host.UseJournal;
                    msg.UseDeadLetterQueue = Host.UseDeadLetter;
                    //if (queue.Transactional)
                    //{
                    //    using (MessageQueueTransaction ts = new MessageQueueTransaction())
                    //    {
                    //        ts.Begin();
                    //        queue.Send(msg, label, ts);
                    //        ts.Commit();
                    //    }
                    //}
                    //else
                    //{
                        queue.Send(msg, label);
                    //}
                }
            }
        }

        /// <summary>
        /// 写入消息队列，列表对象
        /// </summary>
        /// <param name="list"></param>
        /// <param name="label"></param>
        public void Input(IList<object> list, string label)
        {
            using (MessageQueue queue = new MessageQueue(path, QueueAccessMode.Send))
            {
                //using (MessageQueueTransaction ts = new MessageQueueTransaction())
                //{
                    //if (queue.Transactional)
                    //{
                    //    ts.Begin();
                    //}
                    foreach (var obj in list)
                    {
                        using (Message msg = new Message(obj, GetXmlFormatter()))
                        {
                            if (!string.IsNullOrEmpty(ConfirmPath))
                            {
                                msg.AdministrationQueue = new MessageQueue(ConfirmPath);
                                msg.AcknowledgeType = (AcknowledgeTypes)Host.AcknowledgeType;
                            }
                            msg.UseJournalQueue = Host.UseJournal;
                            msg.UseDeadLetterQueue = Host.UseDeadLetter;
                            //if (queue.Transactional)
                            //{
                            //    queue.Send(msg, label, ts);
                            //}
                            //else
                            //{
                                queue.Send(msg, label);
                            //}
                        }
                    }
                    //if (queue.Transactional)
                    //{
                    //    ts.Commit();
                    //}
                //}
            }
        }

        /// <summary>
        /// 设置消息队列最大长度，千字节
        /// </summary>
        /// <param name="size"></param>
        public void SetMaxQueue(long size)
        {
            using (MessageQueue queue = new MessageQueue(path))
            {
                queue.MaximumQueueSize = size;
            }
        }

        /// <summary>
        /// 设置日志队列最大长度，千字节
        /// </summary>
        /// <param name="size"></param>
        public void SexMaxJournal(long size)
        {
            using (MessageQueue queue = new MessageQueue(path))
            {
                queue.MaximumJournalSize = size;
            }
        }

        /// <summary>
        /// 清除队列下所有消息
        /// </summary>
        public void ClearQueue()
        {
            if (string.IsNullOrEmpty(path)) { return; }
            using (MessageQueue queue = new MessageQueue(path))
            {
                queue.Purge();
            }
        }

        /// <summary>
        /// 设置队列存取权限
        /// </summary>
        /// <param name="user"></param>
        /// <param name="rights"></param>
        public void SetPermission(string user, MessageQueueAccessRights rights)
        {
            using (MessageQueue queue = new MessageQueue(path))
            {
                queue.SetPermissions(user, rights);
            }
        }

        /// <summary>
        /// 取得并移除当前队列消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seconds">超时时间，秒</param>
        /// <returns></returns>
        public T Output<T>(int seconds = 5)
        {
            using (MessageQueue queue = new MessageQueue(path, QueueAccessMode.ReceiveAndAdmin))
            {
                //queue.DenySharedReceive = true;
                queue.Formatter = GetXmlFormatter<T>();
                try
                {
                    using (var message = queue.Receive(TimeSpan.FromSeconds(seconds)))
                    {
                        return (T)message.Body;
                    }
                }
                catch
                {
                    return default(T);
                }
            }
        }

        /// <summary>
        /// 取得多条消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count">取消息的数量</param>
        /// <param name="seconds">超时时间，秒</param>
        /// <returns></returns>
        public IList<T> Output<T>(int count, int seconds = 0)
        {
            using (MessageQueue queue = new MessageQueue(path, QueueAccessMode.ReceiveAndAdmin))
            {
                //queue.DenySharedReceive = true;
                queue.Formatter = GetXmlFormatter<T>();
                List<T> list = new List<T>();
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        using (var message = queue.Receive(TimeSpan.FromSeconds(seconds)))
                        {
                            list.Add((T)message.Body);
                        }
                    }
                }
                catch{  }
                return list;
            }
        }

        /// <summary>
        /// 查看当前消息，不清除消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seconds">超时时间，秒</param>
        /// <returns></returns>
        public T View<T>(int seconds = 0)
        {
            using (MessageQueue queue = new MessageQueue(path, QueueAccessMode.Peek))
            {
                queue.Formatter = GetXmlFormatter<T>();

                if (seconds > 0)
                {
                    try
                    {
                        using (var message = queue.Peek(TimeSpan.FromSeconds(seconds)))
                        {
                            return (T)message.Body;
                        }
                    }
                    catch
                    {
                        return default(T);
                    }
                }
                using (var message2 = queue.Peek())
                {
                    return (T)message2.Body;
                }
            }
        }

        /// <summary>
        /// 取得XML格式化类型
        /// </summary>
        /// <returns></returns>
        private XmlMessageFormatter GetXmlFormatter()
        {
            return new XmlMessageFormatter();
        }

        /// <summary>
        /// 取得XML格式化类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private XmlMessageFormatter GetXmlFormatter<T>()
        {
            var formatter = new XmlMessageFormatter();
            formatter.TargetTypes = new Type[] { typeof(T) };
            return formatter;
        }

        /// <summary>
        /// 返回IP与路径
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetLinkPath(string ip, string path)
        {
            if (!ip.ToLower().StartsWith("os:") && !ip.StartsWith(".") && !ip.StartsWith("FormatName"))
            {
                ip = "FormatName:Direct=tcp:" + ip;
            }
            return ip + path;
        }
        #endregion
    }
}
