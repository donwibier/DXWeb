using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DX.Data
{
	[Obsolete("For legacy reasons only. Use DX.Data.AutoMapper or DX.Data.Mapster descendants")]
	public abstract class DataMapper<TKey, TModel, TDBModel> : IDataMapper<TKey, TModel, TDBModel>
			where TKey : IEquatable<TKey>
			where TModel : class, new()
			where TDBModel : class

	{
		public abstract Func<TDBModel, TModel> CreateModel { get; }

		public abstract TDBModel Assign(TModel source, TDBModel destination);

		public abstract string Map(string sourceField);
	}
}
