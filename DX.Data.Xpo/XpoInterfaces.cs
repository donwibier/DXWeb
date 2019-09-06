using DevExpress.Xpo;
using DX.Utils.Data;
using System;
using System.Linq;

namespace DX.Data.Xpo
{
	public interface IXPDataMapper<TKey, TModel, TXPOClass> : IDataMapper<TKey, TModel, TXPOClass>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TXPOClass : class, IXPSimpleObject, IDataStoreModel<TKey>
	{
		
		//Func<TDBModel, TModel> CreateModel { get; }
		//TDBModel Assign(TModel source, TDBModel destination);
		//string Map(string sourceField);
	}

	public interface IXPDataStoreValidator<TKey, TModel, TXPOClass> : IDataStoreValidator<TKey, TModel, TXPOClass>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TXPOClass : class, IXPSimpleObject, IDataStoreModel<TKey>
	{
	}
}
