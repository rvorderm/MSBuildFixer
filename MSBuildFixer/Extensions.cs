using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureToggle.Core;

namespace MSBuildFixer
{
	public static class Extensions
	{
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
		{
			var hashSet = new HashSet<T>();
			foreach (var element in enumerable)
			{
				hashSet.Add(element);
			}
			return hashSet;
		}

		public static Dictionary<TValue, HashSet<TKey>> Invert<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> oldDictionary)
		{
			var newDictionary = new Dictionary<TValue, HashSet<TKey>>();
			var newKeys = oldDictionary.SelectMany(x => x.Value).Distinct();
			foreach (var newKey in newKeys)
			{
				var vals = oldDictionary.Keys.Where(k => oldDictionary[k].Contains(newKey)).ToHashSet();
				newDictionary[newKey] = vals;
			}

			return newDictionary;
		}

		public static Dictionary<TNewKey, HashSet<TValue>> ConvertAndMerge<TKey, TNewKey, TValue>(this IDictionary<TKey, HashSet<TValue>> oldDictionary, Func<TKey, TNewKey> convert)
		{
			var newDictionary = new Dictionary<TNewKey, HashSet<TValue>>();
			foreach (var keyValuePair in oldDictionary)
			{
				var newKey = convert(keyValuePair.Key);
				HashSet<TValue> newValue;
				var keyPresent = newDictionary.TryGetValue(newKey, out newValue);
				if (keyPresent)
				{
					AddRange(newValue, keyValuePair.Value);
				}
				else
				{
					newDictionary[newKey] = keyValuePair.Value;
				}
			}
			return newDictionary;
		}

		private static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> toInsert)
		{
			foreach (var value in toInsert)
			{
				hashSet.Add(value);
			}
		}

		public static IEnumerable<TResult> Merge<TKey, TValue, TResult>(
			this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, TResult> merge)
		{
			var newValues = new List<TResult>();
			foreach (var kvp in dictionary)
			{
				newValues.Add(merge(kvp));
			}
			return newValues;
		}
	}
}
