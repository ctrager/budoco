using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace budoco
{
    public static class bd_session
    {
        /*
            I created this because we can't store objects in HttpContext.Session
            the way we could in the old Session. We can only store strings and ints
            in the new Session.
        */

        static Dictionary<string, dynamic> cache = new Dictionary<string, dynamic>();
        static object my_lock = new object();

        public static void Set(HttpContext context, string key, dynamic obj)
        {
            // because multiple threads could be using this.
            lock (my_lock)
            {
                // we make the thing unique per session by prepending session.
                cache[context.Session.Id + key] = obj;
            }
        }

        public static dynamic Get(HttpContext context, string key)
        {
            // because multple threads could be using this.
            lock (my_lock)
            {
                string session_id_and_key = context.Session.Id + key;
                if (cache.ContainsKey(session_id_and_key))
                    return cache[session_id_and_key];
                else
                    return null;
            }
        }
    }
}
