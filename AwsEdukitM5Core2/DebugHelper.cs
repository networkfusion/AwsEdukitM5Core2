using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace AwsEdukitM5Core2
{
    public static class DebugHelper
    {
        public static void DumpHashTable(Hashtable hash, int level)
        {
            foreach (string key in hash.Keys)
            {
                if (typeof(Hashtable) == hash[key].GetType())
                {
                    Debug.Write(GetSpaces(level));
                    Debug.WriteLine($"key={key}");
                    DumpHashTable((Hashtable)hash[key], level + 1);
                }
                else
                {
                    Debug.Write(GetSpaces(level));
                    Debug.WriteLine($"Key={key},value={hash[key]}");
                    if (key.ToString() == "timestamp")
                    {
                        Debug.Write(GetSpaces(level));
                        Debug.WriteLine($"as ISO date: {DateTime.FromUnixTimeSeconds((int)hash[key]).ToString("o")}");
                    }
                }
            }
        }

        public static string GetSpaces(int level)
        {
            string spaces = string.Empty;
            for (int i = 0; i < level; i++)
            {
                spaces += "  ";
            }

            return spaces;
        }
    }
}
