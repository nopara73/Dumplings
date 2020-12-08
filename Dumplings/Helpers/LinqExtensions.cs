using System;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    public static class LinqExtensions
    {
        public static double? Median<TColl, TValue>(this IEnumerable<TColl> source, Func<TColl, TValue> selector)
        {
            return source.Select(selector).Median();
        }

        public static double? Median<T>(
            this IEnumerable<T> source)
        {
            if (Nullable.GetUnderlyingType(typeof(T)) != null)
                source = source.Where(x => x != null);

            int count = source.Count();
            if (count == 0)
                return null;

            source = source.OrderBy(n => n);

            int midpoint = count / 2;
            if (count % 2 == 0)
                return (Convert.ToDouble(source.ElementAt(midpoint - 1)) + Convert.ToDouble(source.ElementAt(midpoint))) / 2.0;
            else
                return Convert.ToDouble(source.ElementAt(midpoint));
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(
           this IEnumerable<T> source, int size)
        {
            T[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new T[size];

                bucket[count++] = item;

                if (count != size)
                    continue;

                yield return bucket.Select(x => x);

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
            {
                Array.Resize(ref bucket, count);
                yield return bucket.Select(x => x);
            }
        }

        public static IEnumerable<(TColl value, int count)> GetIndistinguishable<TColl>(this IEnumerable<TColl> source, bool includeSingle)
        {
            return source.GroupBy(x => x)
                .ToDictionary(x => x.Key, y => y.Count())
                .Select(x => (x.Key, x.Value))
                .Where(x => includeSingle || x.Value > 1);
        }

        /// <summary>
        /// Count the set bits in a ulong using 24 arithmetic operations (shift, add, and)..
        /// </summary>
        /// <param name="x">The ulong of which we like to count the set bits.</param>
        /// <returns>Amount of set bits.</returns>
        public static int HammingWeight(this ulong x)
        {
            x = (x & 0x5555555555555555) + ((x >> 1) & 0x5555555555555555); //put count of each  2 bits into those  2 bits
            x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333); //put count of each  4 bits into those  4 bits
            x = (x & 0x0f0f0f0f0f0f0f0f) + ((x >> 4) & 0x0f0f0f0f0f0f0f0f); //put count of each  8 bits into those  8 bits
            x = (x & 0x00ff00ff00ff00ff) + ((x >> 8) & 0x00ff00ff00ff00ff); //put count of each 16 bits into those 16 bits
            x = (x & 0x0000ffff0000ffff) + ((x >> 16) & 0x0000ffff0000ffff); //put count of each 32 bits into those 32 bits
            x = (x & 0x00000000ffffffff) + ((x >> 32) & 0x00000000ffffffff); //put count of each 64 bits into those 64 bits
            return (int)x;
        }
    }
}
