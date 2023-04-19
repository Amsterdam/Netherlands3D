using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for extension functions for the System.Collections.Generic.IList{T} and System.Collections.IList
/// interfaces that inserts elements lists that are presumed to be already sorted such that sort ordering is preserved
/// </summary>
/// <author>Jackson Dunstan, http://JacksonDunstan.com/articles/3189</author>
/// <license>MIT</license>
public static class IListInsertIntoSortedListExtensions
{
	/// <summary>
	/// Insert a value into an IList{T} that is presumed to be already sorted such that sort
	/// ordering is preserved
	/// </summary>
	/// <param name="list">List to insert into</param>
	/// <param name="value">Value to insert</param>
	/// <typeparam name="T">Type of element to insert and type of elements in the list</typeparam>
	public static void InsertIntoSortedList<T>(this IList<T> list, T value)
		where T : IComparable<T>
	{
		InsertIntoSortedList(list, value, (a, b) => a.CompareTo(b));
	}

	/// <summary>
	/// Insert a value into an IList{T} that is presumed to be already sorted such that sort
	/// ordering is preserved
	/// </summary>
	/// <param name="list">List to insert into</param>
	/// <param name="value">Value to insert</param>
	/// <param name="comparison">Comparison to determine sort order with</param>
	/// <typeparam name="T">Type of element to insert and type of elements in the list</typeparam>
	public static void InsertIntoSortedList<T>(
		this IList<T> list,
		T value,
		Comparison<T> comparison
	)
	{
		var startIndex = 0;
		var endIndex = list.Count;
		while (endIndex > startIndex)
		{
			var windowSize = endIndex - startIndex;
			var middleIndex = startIndex + (windowSize / 2);
			var middleValue = list[middleIndex];
			var compareToResult = comparison(middleValue, value);
			if (compareToResult == 0)
			{
				list.Insert(middleIndex, value);
				return;
			}
			else if (compareToResult < 0)
			{
				startIndex = middleIndex + 1;
			}
			else
			{
				endIndex = middleIndex;
			}
		}
		list.Insert(startIndex, value);
	}

	/// <summary>
	/// Insert a value into an IList that is presumed to be already sorted such that sort ordering is preserved
	/// </summary>
	/// <param name="list">List to insert into</param>
	/// <param name="value">Value to insert</param>
	public static void InsertIntoSortedList(this IList list, IComparable value)
	{
		InsertIntoSortedList(list, value, (a, b) => a.CompareTo(b));
	}

	public static void InsertIntoSortedList(
		this IList list,
		object value,
		IComparable keyToCompare,
		Comparison<IComparable> comparison
	)
	{
		var startIndex = 0;
		var endIndex = list.Count;
		while (endIndex > startIndex)
		{
			var windowSize = endIndex - startIndex;
			var middleIndex = startIndex + (windowSize / 2);
			var middleValue = (IComparable)middleIndex;
			var compareToResult = comparison(middleValue, keyToCompare);
			if (compareToResult == 0)
			{
				list.Insert(middleIndex, value);
				return;
			}
			else if (compareToResult < 0)
			{
				startIndex = middleIndex + 1;
			}
			else
			{
				endIndex = middleIndex;
			}
		}
		list.Insert(startIndex, value);
	}
	
	/// <summary>
	/// Insert a value into an IList that is presumed to be already sorted such that sort ordering is preserved
	/// </summary>
	/// <param name="list">List to insert into</param>
	/// <param name="value">Value to insert</param>
	/// <param name="comparison">Comparison to determine sort order with</param>
	public static void InsertIntoSortedList(
		this IList list,
		IComparable value,
		Comparison<IComparable> comparison
	)
	{
		var startIndex = 0;
		var endIndex = list.Count;
		while (endIndex > startIndex)
		{
			var windowSize = endIndex - startIndex;
			var middleIndex = startIndex + (windowSize / 2);
			var middleValue = (IComparable)list[middleIndex];
			var compareToResult = comparison(middleValue, value);
			if (compareToResult == 0)
			{
				list.Insert(middleIndex, value);
				return;
			}
			else if (compareToResult < 0)
			{
				startIndex = middleIndex + 1;
			}
			else
			{
				endIndex = middleIndex;
			}
		}
		list.Insert(startIndex, value);
	}

    /// <summary>
    /// Convert a List<Vector2> to a List<Vector3> with the y component as the height
    /// </summary>
    /// <param name="input">Vector2 list to make 3D, heigt component</param>
    /// <returns>Vector3 List</returns>
    public static List<Vector3> ToVector3List(this List<Vector2> input, float height = 0)
    {
        var list = new List<Vector3>();
        for (int i = 0; i < input.Count; i++)
        {
            var p = input[i];
            list.Add(new(p.x, height, p.y));
        }
        return list;
    }

    /// <summary>
    /// Flattens a List<Vector3> to a List<Vector2> by removing the y component
    /// </summary>
    /// <param name="input">Vector3 list to flatten</param>
    /// <returns>vector2 List</returns>
    public static List<Vector2> ToVector2List(this List<Vector3> input)
    {
        var list = new List<Vector2>();
        for (int i = 0; i < input.Count; i++)
        {
            var p = input[i];
            list.Add(new(p.x, p.y));
        }
        return list;
    }
}
