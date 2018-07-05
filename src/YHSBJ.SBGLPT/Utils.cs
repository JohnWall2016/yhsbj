using System;
using System.Collections.Generic;

namespace YHSBJ.SBGLPT
{
    public static class DictionaryExtention
    {
        public static string ToDictString<K, V>(this Dictionary<K, V> dict, Func<K, string> translate = null)
        {
            var kvs = new List<string>();
            foreach (var kv in dict)
            {
                if (translate != null)
                {
                    var key = translate(kv.Key);
                    kvs.Add($"{key}: {kv.Value}");
                }
                else
                    kvs.Add($"{kv.Key}: {kv.Value}");
            }
            return "[" + string.Join(", ", kvs) + "]";
        }
    }
}
