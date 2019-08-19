using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Paden.Aspects.Caching.Redis
{
    public static class CacheExtensions
    {
        static readonly HashSet<char> redisSpecialCharacters = new HashSet<char>() { '[', ']' };

        public static void InvalidateCache<T, TResult>(this T target, Expression<Func<T, TResult>> expression)
        {
            CacheAttribute.InvalidateCache(expression);
        }

        public static string EscapeRedisString(string input)
        {
            var resultBuilder = new StringBuilder();

            foreach (var c in input)
            {
                if (redisSpecialCharacters.Contains(c))
                {
                    resultBuilder.Append('\\');
                }
                resultBuilder.Append(c);
            }

            return resultBuilder.ToString();
        }

        public static T Any<T>()
        {
            return default;
        }
    }
}
