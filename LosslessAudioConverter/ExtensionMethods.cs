using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LosslessAudioConverter
{
    public static class ExtensionMethods
    {
        public static T SetIfNull<T>(this T value, ref T valueToSet, T newValue)
        {
            return (value == null)
                ? value = valueToSet //Set ref value and return
                : value;
        }

        public static T SetIfNull<T>(this T value, ref T valueToSet, Func<T> newValueExpression)
        {
            return (value == null)
                ? valueToSet = newValueExpression() //Set ref value and return
                : value;
        }

        public static IEnumerable<T> Subset<T>(this IEnumerable<T> collection, int count)
        {
            if (count > 0)
            {
                int index = 0;
                foreach (var item in collection)
                {
                    yield return item;

                    if (++index >= count) break;
                }
            }
        }

        public static IEnumerable<T> Pipe<T>(this IEnumerable<T> collection, Action<T> jackson)
        {
            if (collection == null || jackson == null)
                throw new ArgumentNullException();
            else
                return PipeInternal(collection, jackson);
        }

        private static IEnumerable<T> PipeInternal<T>(this IEnumerable<T> collection, Action<T> jackson)
        {
            foreach (T item in collection)
            {
                jackson(item);
                yield return item;
            }
        }
    }
}
