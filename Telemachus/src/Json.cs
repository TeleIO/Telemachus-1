using System;
using System.Collections;
using System.Collections.Generic;
using EdyCommonTools;

namespace Telemachus
{
    /// <summary>
    /// Thin wrapper around KSP's built-in MiniJSON that converts generic
    /// collections to Hashtable/ArrayList (which MiniJSON expects) and
    /// sanitizes Infinity/NaN values to string representations.
    /// </summary>
    static class Json
    {
        public static string Encode(object obj) =>
            MiniJSON.jsonEncode(ToMiniJson(obj));

        public static IDictionary<string, object> DecodeObject(string json)
        {
            var decoded = MiniJSON.jsonDecode(json);
            if (decoded is Hashtable ht)
            {
                var dict = new Dictionary<string, object>(ht.Count);
                foreach (DictionaryEntry entry in ht)
                    dict[entry.Key.ToString()] = entry.Value;
                return dict;
            }
            return null;
        }

        /// Converts generic collections to Hashtable/ArrayList for MiniJSON
        /// and sanitizes Infinity/NaN to strings.
        static object ToMiniJson(object value)
        {
            switch (value)
            {
                case double d when double.IsInfinity(d) || double.IsNaN(d):
                    return d.ToString();
                case float f when float.IsInfinity(f) || float.IsNaN(f):
                    return f.ToString();
                case IDictionary dict:
                    var ht = new Hashtable(dict.Count);
                    foreach (DictionaryEntry entry in dict)
                        ht[entry.Key] = ToMiniJson(entry.Value);
                    return ht;
                case IList list:
                    var al = new ArrayList(list.Count);
                    foreach (var item in list)
                        al.Add(ToMiniJson(item));
                    return al;
                default:
                    return value;
            }
        }
    }
}
