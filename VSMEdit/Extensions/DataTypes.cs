using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VWLmergeR.Extensions
{
    public static class DataTypes
    {

        public static int ToInteger(this string str, int defaultValue = 0)
        {
            return int.TryParse(str, out int result) ? result : defaultValue;
        }

        public static bool Like(this string str, string value)
        {
            return str.Trim().ToLower().Equals(value.Trim().ToLower());
        }

        public static bool IsEmpty(this string str, bool trim = false)
        {
            if (str == null) return true;
            return (trim ? str.Trim() : str).Length <= 0;
        }

        public static string Repeat(this string str, int n)
        {
            if (n <= 0) return "";
            if (str.IsEmpty()) str = " ";
            return new StringBuilder(str.Length * n).AppendJoin(str, new string[n + 1]).ToString();
        }

        public static int Count(this string str, string value, StringComparison comparisonType)
        {

            int count = 0;
            int index = 0;
            do
            {
                index = str.IndexOf(value, index, comparisonType);
                if (index < 0) break;
                index += value.Length;
                count++;
            }
            while (index != -1);
            return count;
        }

        public static string Template(this string str, char @char = ' ')
            => @char.ToString().Repeat(str.Length);

        public static string ToASCII(this string str)
            => Regex.Replace(str, @"[^\u0020-\u007E]", string.Empty);

        public static bool Is<T>(this List<T> list, T value)
        {
            foreach (var item in list)
                if (item.Equals(value)) return true;
            return false;
        }

        public static string Part(this string text, int index=0, int max=0)
        {
            var pos = text.Contains(' ') ? text.IndexOf(' ', index) : 0;              
            var result = text;
            if (pos >= 0) result = text.Substring(0, pos);
            if (max > 0 && max < result.Length) result = result.Substring(0, max);
            return result.Trim();
        }

        public static DateTime? Timestamp(this string input)
        {
            // matchs a timestamp. Groups: (yyyy)(MM)(dd)[*][(HH)(mm)[(ss)]] | [...] = oprtional
            const string pattern = @"(\d{4})(0[1-9]|1[0-2])(0[1-9]|[1-2][0-9]|3[0-1])[^0-9]?(?:([0-1][0-9]|2[0-3])(0[0-9]|[1-5][0-9])(0[0-9]|[1-5][0-9])?)?";
            var m = new Regex(pattern).Match(input);

            if (!m.Success) return null;

            var date = new DateTime(Group(1), Group(2), Group(3));
            date = date + new TimeSpan(Group(4), Group(5), Group(6));
            return date;

            int Group(int index)
            {
                if (m.Groups.Count <= index) return 0;
                return int.TryParse(m.Groups[index].Value, out var number) ? number : 0;
            }
        }

        public static string Timestamp(this DateTime date)
            => date.ToString("yyyyMMddHHmmss");

    }
}

