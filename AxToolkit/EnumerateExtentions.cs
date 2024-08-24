using System.Collections;
using System.IO;

namespace AxToolkit
{
    public static class EnumerateExtentions
    {
        /// <summary>Transform an enumeration of enumeration into a simple collection</summary>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            foreach (var list in source)
            {
                foreach (var item in list)
                {
                    yield return item;
                }
            }
        }

        /// <summary>Transform an enumeration of enumeration into a simple collection</summary>
        public static IEnumerable<TResult> Flatten<TInput, TResult>(this IEnumerable<TInput> source, Func<TInput, IEnumerable<TResult>> selector)
        {
            foreach (var list in source)
            {
                foreach (var item in selector(list))
                {
                    yield return item;
                }
            }
        }

        /// <summary>Keep only a echantillon of the provided element, keeping one of {every} items.</summary>
        public static IEnumerable<T> OneEvery<T>(this IEnumerable<T> source, int every, bool includeLast)
        {
            int count = 0;
            T last = default;
            bool lastSend = true;
            foreach (var item in source)
            {
                lastSend = false;
                last = item;
                if (count % every == 0)
                {
                    lastSend = true;
                    yield return item;
                }
                count++;
            }
            if (includeLast && !lastSend)
                yield return last;
        }

        /// <summary>Execute an action for each element. The action is executed immediatly</summary>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            var results = new List<T>();
            foreach (var item in source)
            {
                action(item);
                results.Add(item);
            }
            return results;
        }

        /// <summary>Execute an action for each element. The action is executed immediatly</summary>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int i = 0;
            var results = new List<T>();
            foreach (var item in source)
            {
                action(item, i++);
                results.Add(item);
            }
            return results;
        }

        /// <summary>Execute an action for each element. The action is delayed on iteration of the collection</summary>
        public static IEnumerable<T> ForEachDelayed<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>Both convert and filter element based on the provided type</summary>
        public static IEnumerable<TResult> ForType<TResult>(this IEnumerable source)
        {
            foreach (var item in source)
            {
                if (item is TResult value)
                    yield return value;
            }
        }
        
    }
}