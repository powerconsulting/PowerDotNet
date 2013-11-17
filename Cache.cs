using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PowerDotNet
{
    /// <summary>
    /// Summary description for Cache
    /// </summary>
    public static class Cache
    {
        public static Enumies.CacheKeys GetEnum(int id)
        {
            Enumies.CacheKeys enumValue = Enumies.CacheKeys.All;
            try
            {
                enumValue = (Enumies.CacheKeys)Enum.Parse(enumValue.GetType(), id.GetString());
            }
            catch { }
            return enumValue ;
        }

        public static bool Exists(this Enumies.CacheKeys key, string suffix = "")
        {
            if (System.Web.HttpContext.Current.Cache[getCacheString(key, suffix)] != null)
                return true;

            return false;
        }

        public static string getCacheString(this Enumies.CacheKeys key, string suffix = "")
        {
            return "CacheKey_" + key.GetInt() + suffix; 
        }

        public static void SetCache(Enumies.CacheKeys key, object value, int duration = 30, string keySuffix = "", bool isSliding = false)
        {
            var cachedObject = System.Web.HttpContext.Current.Cache[key.getCacheString(keySuffix)];

            int days = 0, hours = 0, minutes = 0;
            TimeSpan expirationSpan = System.Web.Caching.Cache.NoSlidingExpiration;
            DateTime expirationDateTime = System.Web.Caching.Cache.NoAbsoluteExpiration;
            if (isSliding)
            {

                if (duration < 60)
                {
                    minutes = duration;
                }
                else if (duration >= 1440)
                {
                    days = (duration / 1440);
                    hours = (duration - (days * 1440)) / 60;
                    minutes = (duration - (days * 1440)) - (hours * 60);
                }
                else
                {
                    hours = duration / 60;
                    minutes = duration - (hours * 60);
                }

                expirationSpan = new TimeSpan(days, hours, minutes, 0);
            }
            else
            {
                expirationDateTime = DateTime.Now.AddMinutes(duration);
            }
            System.Web.HttpContext.Current.Cache.Insert(key.getCacheString(keySuffix), value, null, expirationDateTime, expirationSpan);
        }

        public static object GetCache(Enumies.CacheKeys key, string keySuffix = "")
        {
            return System.Web.HttpContext.Current.Cache[key.getCacheString(keySuffix)];
        }

        public static T GetCache<T>(Enumies.CacheKeys key, string keySuffix = "")
        {
            object value = System.Web.HttpContext.Current.Cache[key.getCacheString(keySuffix)];
            if (value == null)
                return default(T);

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Int32:
                    return (T)(object)Convert.ToInt32(value);
                case TypeCode.String:
                    return (T)(object)value;
                case TypeCode.Object:
                    return (T)value;
                    if (value.GetType().Name == "Dictionary`2")
                    {
                        //TODO: deal with more complex types, Dictionaries have to be handled manually, most other object, can just be stored as a generic object, even custom complex objects

                    }
                    break;
            }

            return (T)System.Convert.ChangeType(value, Type.GetTypeCode(typeof(T)));
        }

        //Custom Cache Object to store data in, it will allow for easy checking of expiration date.
        //I do this, since I found it lighter than using reflection to detect expiration time / date.
        //I wanted to be able to check if it expires within X minutes and then updated it before it expired, to prevent suddent death hits.
        //Set cache value new CacheObject(Enumies.CacheKeys.Header, DATA_Object, 40, "Custom_Suffix", true);
        //Get cache value CacheObject headerCache = new CacheObject(Enumies.CacheKeys.Header, "Custom_Suffix");
        public class CacheObject
        {
            public DateTime ExpiresAt = new DateTime();
            public int Duration = 30;
            public string KeySuffix = "";
            public bool IsSliding = false;
            public object Data = null;
            public Enumies.CacheKeys Key = Enumies.CacheKeys.NotSet;

            public bool ExpiresWithinMinutes(int minutes)
            {
                return ((ExpiresAt - DateTime.Now).TotalMinutes) < minutes;
            }

            public CacheObject(Enumies.CacheKeys key, string keySuffix = "")
            {
                CacheObject cacheObject = (CacheObject)Cache.GetCache(key, keySuffix);

                if (cacheObject != null)
                {
                    this.Duration = cacheObject.Duration;
                    this.ExpiresAt = cacheObject.ExpiresAt;
                    this.Key = cacheObject.Key;
                    this.Data = cacheObject.Data;
                    this.KeySuffix = cacheObject.KeySuffix;
                    this.IsSliding = cacheObject.IsSliding;
                }
            }

            public CacheObject(Enumies.CacheKeys key, object data, int duration = 30, string keySuffix = "", bool isSliding = false)
            {
                this.Key = key;
                this.Data = data;
                this.Duration = duration;
                this.KeySuffix = keySuffix;
                this.IsSliding = isSliding;
                this.ExpiresAt = DateTime.Now.AddSeconds(duration * 60);

                Cache.SetCache(key, this, duration, keySuffix, isSliding);
            }
        }

        public static object ClearCache(Enumies.CacheKeys key, string keySuffix = "")
        {
            if (key == Enumies.CacheKeys.All)
                return ClearAllCache();
            return System.Web.HttpContext.Current.Cache.Remove(key.getCacheString(keySuffix));
        }

        public static object ClearCache(string key = "")
        {
            return System.Web.HttpContext.Current.Cache.Remove(key);
        }

        public static string ClearAllCache()
        {
            List<string> keys = new List<string>();

            // retrieve application Cache enumerator
            System.Collections.IDictionaryEnumerator enumerator = System.Web.HttpContext.Current.Cache.GetEnumerator();

            // copy all keys that currently exist in Cache
            while (enumerator.MoveNext())
            {
                keys.Add(enumerator.Key.ToString());
            }

            // delete every key from cache
            for (int i = 0; i < keys.Count; i++)
            {
                System.Web.HttpContext.Current.Cache.Remove(keys[i]);
            }
            return "All Cache Cleared";
        }
    }
}