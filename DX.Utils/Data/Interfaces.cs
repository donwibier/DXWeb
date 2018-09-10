using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DX.Utils.Data
{
	public interface IDataStoreModel<TKey>
		where TKey : IEquatable<TKey>
	{
		TKey ID { get; set; }
	}

	public interface IDataStoreLookupModel<TKey> : IDataStoreModel<TKey>
		where TKey : IEquatable<TKey>
	{
		int Order { get; set; }
		string Name { get; set; }
		string Code { get; set; }
	}
	public interface IDataStoreLookupModel : IDataStoreLookupModel<Guid>
	{

	}

	public interface IDataStore<TKey, TModel>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
	{
		Type KeyType { get; }
		Type ModelType { get; }

		//IEnumerable<TModel> Query();
		//Task<IEnumerable<TModel>> QueryAsync();
		TModel GetByKey(TKey key);

		void Create(IEnumerable<TModel> items);
		void Create(TModel item);
		Task CreateAsync(IEnumerable<TModel> items);
		Task CreateAsync(TModel item);

		void Update(IEnumerable<TModel> items);
		void Update(TKey id, TModel item);
		Task UpdateAsync(IEnumerable<TModel> items);
		Task UpdateAsync(TKey id, TModel item);

		void Delete(IEnumerable<TKey> ids);
		void Delete(IEnumerable<TModel> items);
		void Delete(TKey id);
		Task DeleteAsync(TKey id);
		Task DeleteAsync(IEnumerable<TKey> ids);
		Task DeleteAsync(IEnumerable<TModel> items);

	}

	public interface ILookupDataStore<TKey, TModel> : IDataStore<TKey, TModel>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
	{

		IEnumerable<IDataStoreLookupModel<TKey>> QueryByName(string name);
		IEnumerable<IDataStoreLookupModel<TKey>> QueryByCode(string code);

		TKey FindKeyByCode(string code);
		TKey FindKeyByName(string name);

	}

	public interface IDataStoreValidator<TKey, TModel, TDBModel>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TDBModel : class
	{
		bool Inserting(TModel model);
		bool Inserted(TModel model, TDBModel dbModel);
		bool Updating(TModel model);
		bool Updated(TModel model, TDBModel dbModel);
		bool Deleting(TDBModel model);
		bool Deleted(TDBModel model);
	}

}
