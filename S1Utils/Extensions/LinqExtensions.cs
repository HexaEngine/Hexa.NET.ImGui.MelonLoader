namespace S1Utils.Extensions
{
    using Il2CppSystem.Collections.Generic;
    using System;

    public static class LinqExtensions
    {
        public static System.Collections.Generic.IEnumerable<TOut> Select<T, TOut>(this List<T> values, Func<T, TOut> selector)
        {
            foreach (var item in values)
            {
                yield return selector(item);
            }
        }
    }
}