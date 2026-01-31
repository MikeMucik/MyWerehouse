using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Utils
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
			Action<TDestination>? removeMapper = null)
			where TDestination : class
		{
			var existingMap = existingCollection.ToDictionary(destinationKeySelector); // daje warning bo 
			var incomingItems = incomingCollection.ToList();			
			//update
			foreach (var incomingItem in incomingItems.Where(i => !EqualityComparer<TKey>.Default.Equals(sourceKeySelector(i), default)))
			{
				if (existingMap.TryGetValue(sourceKeySelector(incomingItem), out var existingItem))
				{
					updateMapper(incomingItem, existingItem);
				}				
			}
			// potem dodania nowych (Id = default)
			foreach (var incomingItem in incomingItems.Where(i => EqualityComparer<TKey>.Default.Equals(sourceKeySelector(i), default)))
			{
				existingCollection.Add(addMapper(incomingItem));
			}
			//usunięcie niepotrzebnych			
			if (removeMapper != null)
			{
				var incomingKeys = incomingItems
					.Where(i => !EqualityComparer<TKey>.Default.Equals(sourceKeySelector(i), default))
					.Select(sourceKeySelector)
					.ToHashSet();
				var itemsToRemove = existingMap.Values
					.Where(ei => !incomingKeys.Contains(destinationKeySelector(ei)))
					.ToList();
				foreach (var item in itemsToRemove)
				{
					removeMapper(item);
				}
			}			
		}
	}
}
