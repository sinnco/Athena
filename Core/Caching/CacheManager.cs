using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using Memcached.ClientLibrary;

namespace ViCore.Caching
{
    public sealed class CacheManager
    {
        private CacheManager() { }
        private ObjectCache cache = CacheFactory.Create();
        private static readonly CacheManager _cacheManager = new CacheManager();
        public static CacheManager Instance
        {
            get { return _cacheManager; }
        }
        
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="minutes">默认2分钟过期</param>
        /// <param name="seconds">秒，结果为'分钟+秒=过期时间'</param>
        public bool Add(string key, object value, int minutes = 2, int seconds = 0)
        {
            var policy = new CacheItemPolicy();
            seconds += minutes * 60;
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(seconds);
            return cache.Add(key, value, policy);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="minutes">默认2分钟过期</param>
        /// <param name="seconds">秒，结果为'分钟+秒=过期时间'</param>
        public void Set(string key, object value, int minutes = 2, int seconds = 0)
        {
            var policy = new CacheItemPolicy();
            seconds += minutes * 60;
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(seconds);
            cache.Set(key, value, policy);
        }

        /// <summary>
        /// 移除并获取该缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Remove(string key)
        {
            var obj = cache.Remove(key);
            return obj;
        }

        /// <summary>
        /// 是否存在缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            return cache.Contains(key);
        }

        /// <summary>
        /// 获取缓存数
        /// </summary>
        /// <returns></returns>
        public long Count
        {
            get { return cache.GetCount(); }
        }

        /// <summary>
        /// 取得缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            return cache.Get(key);
        }

        /// <summary>
        /// 取得缓存对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return (T)Get(key);
        }

        /// <summary>
        /// 取得缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get { return cache.Get(key); }
        }

        /// <summary>
        /// 清除Memcached所有缓存
        /// </summary>
        /// <returns></returns>
        public bool RemoveMemcachedAll()
        {
            return new MemcCache().FlushAll();
        }
    }
}
