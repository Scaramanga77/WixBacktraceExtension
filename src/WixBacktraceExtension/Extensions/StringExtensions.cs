﻿namespace WixBacktraceExtension.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public static partial class StringExtensions
    {
        /// <summary>
        /// Take a delimited string of file extensions and return a collection that
        /// can be compared to the output of `Path.GetExtension`
        /// </summary>
        /// <param name="patterns">a string of file extensions delimited by '|' or ','</param>
        public static ICollection<string> SplitFileExtensions(this string patterns)
        {
            return patterns
                .Split(new[] { "|", "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim('*'))
                .Select(p => (p.Contains(".")) ? p : "." + p)
                .ToArray();
        }

        /// <summary>
        /// Remove non file name characters  from a string
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string FilterJunk(this string src)
        {
            var dst = new StringBuilder();
            var filter = Path.GetInvalidFileNameChars().Concat(new[] { ' ', '-', '.'}).ToArray();
            foreach (var ch in src)
            {
                dst.Append(filter.Contains(ch) ? '_' : ch);
            }
            return dst.ToString();
        }

        /// <summary>
        /// Last element from a file or directory path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string LastPathElement(this string path)
        {
            var try1 = Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(try1)) return try1;

            return Path.GetFileNameWithoutExtension(Path.GetDirectoryName(path));
        }

        /// <summary>
        /// Return at most `numChars` from a string, retaining right-most characters first
        /// </summary>
        public static string LimitRight(int numChars, string src)
        {
            return src.Length <= numChars ? src : src.Substring(src.Length - numChars);
        }
        /// <summary>
        /// Return at most `numChars` from a string, retaining right-most characters first
        /// </summary>
        public static string LimitRight(this string src, int numChars)
        {
            return LimitRight(numChars, src);
        }
    }
}