using DevExpress.Xpo;

using System;
using System.Linq;

namespace DX.Data.Xpo
{
	[Obsolete("For legacy reasons only. Use DX.Data.AutoMapper or DX.Data.Mapster descendants")]
	public interface IXPDataMapper<TKey, TModel, TXPOClass> : IDataMapper<TKey, TModel, TXPOClass>
		where TKey : IEquatable<TKey>
		where TModel : class, new()
		where TXPOClass : class, IXPSimpleObject
	{
	}

	//public interface IXPDataStoreValidator<TKey, TModel, TXPOClass> : IDataStoreValidator<TKey, TModel, TXPOClass>
	//	where TKey : IEquatable<TKey>
	//	where TModel : class
	//	where TXPOClass : class, IXPSimpleObject
	//{
	//}

	//public interface IXPDataStore<TKey, TModel, TXPOClass> : IDataStore<TKey, TModel>
	//	where TKey : IEquatable<TKey>
	//	where TModel : class, new()
	//	where TXPOClass : class, IXPSimpleObject
	//{
	//	XpoDatabase DB { get; }
	//	IDataMapper<TKey, TModel, TXPOClass> Mapper { get; }
	//	IDataStoreValidator<TKey, TModel, TXPOClass> Validator { get; }
	//}
}
