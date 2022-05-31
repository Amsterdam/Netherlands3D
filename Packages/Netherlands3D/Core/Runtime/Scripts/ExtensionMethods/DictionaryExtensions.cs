using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extention class for dictionaries
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Add a dictionary to another dictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <param name="callbackDuplicate">If the dictionary tries to add a value that is already present</param>
    public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source, Action<T> onDuplicate = null)
    {
        if(target == null)
            throw new ArgumentNullException(nameof(target));
        if(source == null)
            throw new ArgumentNullException(nameof(source));
        foreach(var item in source)
        {
            if(target.Contains(item))
            {
                onDuplicate(item);
                continue;
            }
            target.Add(item);
        }
    }
}
