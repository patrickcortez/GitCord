using System;
using System.Collections.Generic;
using System.Text;

namespace Gitbot2.Source.Commands
{
    internal static class RepoCache
    {
        private static List<string> Cache;

        public static void SetCache(List<string> Primary)
        {
            Cache = new(Primary);
        }

        public static string GetValue(string Key)
        {
            if (Cache.Contains(Key))
            {
                return Cache[Cache.IndexOf(Key)];
            }

            return string.Empty;
        }

        public static List<string> GetCache()
        {
            return Cache;
        }

        public static bool PopKey(string Key)
        {
            return Cache.Remove(Key);
        }

        public static void AddElement(string Element)
        {
            Cache.Add(Element);
        }
    }
}
