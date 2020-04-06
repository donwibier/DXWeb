using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace DX.Data.EF
{
	public interface IEFDataMapper<TKey, TModel, TEFClass> : IDataMapper<TKey, TModel, TEFClass>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TEFClass : class, IDataStoreModel<TKey>
	{

		//Func<TDBModel, TModel> CreateModel { get; }
		//TDBModel Assign(TModel source, TDBModel destination);
		//string Map(string sourceField);
	}

	public interface IEFDataStoreValidator<TKey, TModel, TEFClass> : IDataStoreValidator<TKey, TModel, TEFClass>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TEFClass : class, IDataStoreModel<TKey>
	{
	}
}
