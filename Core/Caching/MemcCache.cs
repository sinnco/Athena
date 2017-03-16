using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using Memcached.ClientLibrary;

namespace ViCore.Caching
{
    public class MemcCache : ObjectCache
    {
        public MemcCache()
            : base()
        {
            _mclient = new MemcachedClient();
        }

        MemcachedClient _mclient;

        public override bool Add(CacheItem value, CacheItemPolicy policy)
        {
            return Add(value.Key, value.Value, policy);
        }

        public override bool Add(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (policy == null)
            {
                policy = new CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(2)
                };
            }
            TimeSpan ts = policy.AbsoluteExpiration - DateTimeOffset.Now;
            return _mclient.Add(key, value, DateTime.Now + ts);
        }

        public override bool Add(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            CacheItem item = new CacheItem(key, value, regionName);
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = absoluteExpiration;
            return Add(item, policy);
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            CacheItem item = GetCacheItem(key, regionName);
            if (item == null)
            {
                Set(new CacheItem(key, value, regionName), policy);
                return value;
            }
            return item.Value;
        }

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            CacheItem item = GetCacheItem(value.Key, value.RegionName);
            if (item == null)
            {
                Set(value, policy);
                return value;
            }
            return item;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            CacheItem item = new CacheItem(key, value, regionName);
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = absoluteExpiration;
            return AddOrGetExisting(item, policy);
        }

        public override bool Contains(string key, string regionName = null)
        {
            return _mclient.KeyExists(key);
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            return null;
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get
            {
                return DefaultCacheCapabilities.OutOfProcessProvider | DefaultCacheCapabilities.AbsoluteExpirations | DefaultCacheCapabilities.SlidingExpirations | DefaultCacheCapabilities.CacheRegions;
            }
        }

        public override object Get(string key, string regionName = null)
        {
            return _mclient.Get(key);
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            object value = Get(key, regionName);
            if (value != null)
            {
                return new CacheItem(key, value, regionName);
            }
            return null;
        }

        public override long GetCount(string regionName = null)
        {
            return -1;
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return null;
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (string item in keys)
            {
                object obj = _mclient.Get(item);
                if (obj != null)
                {
                    dict.Add(item, obj);
                }
            }
            return dict;
        }

        public override string Name
        {
            get { return "Memcached Provider"; }
        }

        public override object Remove(string key, string regionName = null)
        {
            _mclient.Delete(key);
            return null;
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (policy == null)
            {
                policy = new CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(2)
                };
            }
            TimeSpan ts = policy.AbsoluteExpiration - DateTimeOffset.Now;
            _mclient.Set(key, value, DateTime.Now + ts);
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            if (item == null) { return; }
            Set(item.Key, item.Value, policy);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            CacheItem item = new CacheItem(key, value, regionName);
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = absoluteExpiration;
            Set(item, policy);
        }

        public override object this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value, DateTimeOffset.Now.AddMinutes(2));
            }
        }

        public bool FlushAll()
        {
            return _mclient.FlushAll();
        }
    }
}
