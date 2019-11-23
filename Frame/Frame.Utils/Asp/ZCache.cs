using System;
using System.Web;
using System.Web.Caching;
using System.Collections;
using System.Text.RegularExpressions;

namespace Frame.Utils
{
    /// <summary>
    /// <para>　</para>
    /// 　常用工具类——缓存类
    /// </summary>
    public class ZCache
    {
        static Cache GetCacheObject()
        {
            HttpContext context = HttpContext.Current;
            return context != null ? context.Cache : HttpRuntime.Cache;
        }

        /// <summary>
        /// 设置当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="cacheKey">键</param>
        /// <param name="objObject"></param>
        public static void SetCache(string cacheKey, object objObject)
        {
            Cache objCache = GetCacheObject();
            objCache.Insert(cacheKey, objObject);
        }

        /// <summary>
        /// 获取当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <returns></returns>
        public static object GetCache(string CacheKey)
        {
            Cache objCache = GetCacheObject();
            return objCache[CacheKey];
        }

        /// <summary>
        /// 设置当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="objObject"></param>
        /// <param name="absoluteExpiration">可设置为Cache.NoAbsoluteExpiration</param>
        /// <param name="slidingExpiration">可设置为Cache.NoSlidingExpiration</param>
        public static void SetCache(string cacheKey, object objObject, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            System.Web.Caching.Cache objCache = GetCacheObject();
            objCache.Insert(cacheKey, objObject, null, absoluteExpiration, slidingExpiration);
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="CacheKey"></param>
        public static void RemoveCache(string CacheKey)
        {
            Cache objCache = GetCacheObject();
            objCache.Remove(CacheKey);
        }

        /// <summary>
        /// 清空Cash对象
        /// </summary>
        public static void Clear()
        {
            Cache cache = GetCacheObject();
            IDictionaryEnumerator cacheEnum = cache.GetEnumerator();
            ArrayList al = new ArrayList();
            while (cacheEnum.MoveNext())
            {
                al.Add(cacheEnum.Key);
            }
            foreach (string key in al)
            {
                cache.Remove(key);
            }
        }

        /// <summary>
        /// 根据正则表达式的模式移除Cache
        /// </summary>
        /// <param name="pattern">模式</param>
        public static void RemoveByPattern(string pattern)
        {
            Cache cache = GetCacheObject();
            IDictionaryEnumerator cacheEnum = cache.GetEnumerator();
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            while (cacheEnum.MoveNext())
            {
                if (regex.IsMatch(cacheEnum.Key.ToString()))
                    cache.Remove(cacheEnum.Key.ToString());
            }
        }
    }
}

