using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtention
{
    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
    {
        return source.MinBy(selector, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <credit>
    /// https://stackoverflow.com/questions/36326737/how-to-find-nearest-datetime-keys-of-datetime-in-sorteddictionary
    /// https://stackoverflow.com/questions/914109/how-to-use-linq-to-select-object-with-minimum-or-maximum-property-value/914198#914198
    /// </credit>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
    {
        if(source == null) throw new ArgumentNullException("source");
        if(selector == null) throw new ArgumentNullException("selector");
        comparer ??= Comparer<TKey>.Default;

        using(var sourceIterator = source.GetEnumerator())
        {
            if(!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var min = sourceIterator.Current;
            var minKey = selector(min);
            while(sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if(comparer.Compare(candidateProjected, minKey) < 0)
                {
                    min = candidate;
                    minKey = candidateProjected;
                }
            }
            return min;
        }
    }

    /// <summary>
    /// Rotate an array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="count"></param>
    public static void Rotate<T>(this T[] array, int count)
    {
        if(array == null || array.Length < 2) return;
        count %= array.Length;
        if(count == 0) return;
        int left = count < 0 ? -count : array.Length + count;
        int right = count > 0 ? count : array.Length - count;
        if(left <= right)
        {
            for(int i = 0; i < left; i++)
            {
                var temp = array[0];
                Array.Copy(array, 1, array, 0, array.Length - 1);
                array[array.Length - 1] = temp;
            }
        }
        else
        {
            for(int i = 0; i < right; i++)
            {
                var temp = array[array.Length - 1];
                Array.Copy(array, 0, array, 1, array.Length - 1);
                array[0] = temp;
            }
        }
    }

    /// <summary>
    /// Convert a Vector2[] to a List<Vector3> with the y component as the height
    /// </summary>
    /// <param name="input">Vector2 array to make 3D, heigt component</param>
    /// <returns>Vector3 List</returns>
    public static List<Vector3> ToVector3List(this Vector2[] input, float height = 0)
    {
        var list = new List<Vector3>();
        for (int i = 0; i < input.Length; i++)
        {
            var p = input[i];
            list.Add(new(p.x, height, p.y));
        }
        return list;
    }

    /// <summary>
    /// Flattens a Vector3[] to a List<Vector2> by removing the y component
    /// </summary>
    /// <param name="input">Vector3 array to flatten</param>
    /// <returns>Flattened Vector2 List</returns>
    public static List<Vector2> ToVector2List(this Vector3[] input)
    {
        var list = new List<Vector2>();
        for (int i = 0; i < input.Length; i++)
        {
            var p = input[i];
            list.Add(new(p.x, p.y));
        }
        return list;
    }
}

