using System;
using System.Collections.Generic;
using System.Text;

namespace DX.Utils.Data
{
	public abstract class DataMapper<TKey, TModel, TDBModel> : IDataMapper<TKey, TModel, TDBModel>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TDBModel : class, IDataStoreModel<TKey>

	{
		public abstract Func<TDBModel, TModel> CreateModel { get; }

		public abstract TDBModel Assign(TModel source, TDBModel destination);

		public abstract string Map(string sourceField);
	}
}
