namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Cache
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	internal sealed class PersonCache
	{
		private readonly ConcurrentDictionary<Type, IReadOnlyList<ApiObject>> cache = new();

		public void SetCache<T>(IEnumerable<T> objects)
			where T : ApiObject
		{
			var type = typeof(T);
			if (type == typeof(ApiObject))
			{
				throw new InvalidOperationException("Cannot use ApiObject directly. Use a derived type.");
			}

			if (objects == null)
			{
				throw new ArgumentNullException(nameof(objects));
			}

			var objectList = objects.ToList();

			if (objectList.Count == 0)
			{
				return;
			}

			if (objectList.Any(o => o == null))
			{
				throw new ArgumentException("objects collection contains null values", nameof(objects));
			}

			cache[type] = objectList.Cast<ApiObject>().ToList().AsReadOnly();
		}

		public void AddToCache<T>(IEnumerable<T> objects)
			where T : ApiObject
		{
			var type = typeof(T);
			if (type == typeof(ApiObject))
			{
				throw new InvalidOperationException("Cannot use ApiObject directly. Use a derived type.");
			}

			if (objects == null)
			{
				throw new ArgumentNullException(nameof(objects));
			}

			var objectList = objects.Cast<ApiObject>().ToList();

			if (objectList.Count == 0)
			{
				return;
			}

			if (objectList.Any(o => o == null))
			{
				throw new ArgumentException("objects collection contains null values", nameof(objects));
			}

			cache.AddOrUpdate(
				type,
				addValueFactory: _ => objectList.AsReadOnly(),
				updateValueFactory: (_, existing) =>
				{
					var result = new List<ApiObject>(existing);
					foreach (var apiObject in objectList)
					{
						var idx = result.FindIndex(o => o.Id == apiObject.Id);
						if (idx >= 0)
						{
							result[idx] = apiObject;
						}
						else
						{
							result.Add(apiObject);
						}
					}

					return result.AsReadOnly();
				});
		}

		public IEnumerable<T> GetFromCache<T>()
			where T : ApiObject
		{
			var type = typeof(T);
			if (type == typeof(ApiObject))
			{
				throw new InvalidOperationException("Cannot use ApiObject directly. Use a derived type.");
			}

			if (!cache.TryGetValue(type, out var cachedObjects))
			{
				return Enumerable.Empty<T>();
			}

			return cachedObjects.OfType<T>();
		}
	}
}
