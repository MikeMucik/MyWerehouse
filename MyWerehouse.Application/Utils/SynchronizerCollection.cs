using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Utils
{
	public static class CollectionSynchronizer
	{
		public static void SynchronizeCollection<TSource, TDestination, TKey>(
			ICollection<TDestination> existingCollection,
			IEnumerable<TSource> incomingCollection,
			Func<TDestination, TKey> destinationKeySelector,
			Func<TSource, TKey> sourceKeySelector,
			Func<TSource, TDestination> addMapper,
			Action<TSource, TDestination> updateMapper,
			Action<TDestination> removeMapper = null)
			where TDestination : class
		{
			var existingMap = existingCollection.ToDictionary(destinationKeySelector);
			var incomingMap = incomingCollection.ToDictionary(sourceKeySelector);

			foreach (var incomingItem in incomingMap.Values)
			{
				if(existingMap.TryGetValue(sourceKeySelector(incomingItem),out var existingItem))
				{
					updateMapper(incomingItem, existingItem);
				}
				else
				{
					existingCollection.Add(addMapper(incomingItem));
				}
			}
			if(removeMapper != null)
			{
				var itemesToRemove = existingMap.Values
					.Where(ei => !incomingMap.ContainsKey(destinationKeySelector(ei)))
					.ToList();
				foreach (var item in itemesToRemove)
				{
					removeMapper(item);
				}
			}
		}
	}
}

			//foreach (var existingItem in existingMap.Values.Where(ei =>
			//!incomingMap.ContainsKey(destinationKeySelector(ei))).ToList())
			//{
			//	existingCollection.Remove(existingItem);
			//}