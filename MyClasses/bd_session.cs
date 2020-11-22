using System;
using System.Collections.Generic;

namespace budoco
{
    public static class bd_session
    {
        //Because we can't store objects in HttpContext.Session

        static Dictionary<string, object> cache = new Dictionary<string, object>();
        static object my_lock = new object();

        public static void Set(string key, object obj)
        {
            lock (my_lock)
            {
                cache[key] = obj;
            }
        }

        public static object Get(string key)
        {
            lock (my_lock)
            {
                if (cache.ContainsKey(key))
                    return cache[key];
                else
                    return null;
            }
        }
    }
}
