using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DX.Utils
{
	/// <summary>
	/// Extension methods for use with DevExpress Blazor DataGrid
	/// (Probably for DevExtreme MVC / ASP.NET Core Datagrid as well)
	/// </summary>
	public static class PropertyExtensions
	{

		static readonly ConcurrentDictionary<Tuple<Type, string>, PropertyInfo> cache =
			new ConcurrentDictionary<Tuple<Type, string>, PropertyInfo>();

		static PropertyInfo GetInfo(Type objectType, string propertyName)
		{
			var k = new Tuple<Type, string>(objectType, propertyName);
			PropertyInfo result = cache.GetOrAdd(k, (key) => objectType.GetProperty(propertyName));

			// make sure the cache instance doesn't grow too big
			if (cache.Count > 1000)
			{
				Task.Run(() =>
				{
					while (cache.Count > 1000)
						if (!cache.TryRemove(cache.Keys.Last(), out PropertyInfo r))
							return; // break if can't be removed
				});
			}

			return result;
		}

		public static bool HasProperty(this object obj, string propertyName)
		{
			return GetInfo(obj.GetType(), propertyName) != null;
		}

		public static T GetPropertyValue<T>(this object obj, string propertyName)
		{
			var propType = GetInfo(obj.GetType(), propertyName);
			var result = (T)propType?.GetValue(obj);
			return result;
		}

		public static void SetPropertyValue<T>(this object obj, string name, T value)
		{
			var propType = obj.GetType().GetProperty(name);
			propType.SetValue(obj, value);
		}
		public static void SetPropertyValue(this object obj, string name, object value)
		{
			var propType = obj.GetType().GetProperty(name);
			propType.SetValue(obj, value);
		}

		public static T AssignToObject<T>(this IDictionary<string, object> dictionary, T dataItem)
		{
			foreach (var field in dictionary.Keys)
			{
				if (dataItem.HasProperty(field))
					dataItem.SetPropertyValue(field, dictionary[field]);
			}
			return dataItem;
		}

		//public static T Assign<T>(this object source, T destination, params string[] excludeProps)
		//{
		//	foreach(var propInfo in source.GetType().GetProperties())
		//	{
		//		var name = propInfo.Name;
		//		var excl = excludeProps != null && excludeProps.Contains(name);
		//		if (!excl && destination.HasProperty(propInfo.Name))
		//		{
		//			destination.SetPropertyValue(name, source.GetPropertyValue(name));
		//		} 
		//	}
		//	return destination;
		//}
	}
}
