using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DX.Utils.Data
{
	public interface IAssignable
	{
		void Assign(object source);
	}
	//public interface IAssignable<TFrom>
	//{
	//	void Assign(TFrom source);
	//}
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

		IDataValidationResults<TKey> Create(IEnumerable<TModel> items);
		IDataValidationResults<TKey> Create(TModel item);
		Task<IDataValidationResults<TKey>> CreateAsync(IEnumerable<TModel> items);
		Task<IDataValidationResults<TKey>> CreateAsync(TModel item);

		IDataValidationResults<TKey> Update(IEnumerable<TModel> items);
		IDataValidationResults<TKey> Update(TKey id, TModel item);
		Task<IDataValidationResults<TKey>> UpdateAsync(IEnumerable<TModel> items);
		Task<IDataValidationResults<TKey>> UpdateAsync(TKey id, TModel item);

		IDataValidationResults<TKey> Delete(IEnumerable<TKey> ids);
		IDataValidationResults<TKey> Delete(IEnumerable<TModel> items);
		IDataValidationResults<TKey> Delete(TKey id);
		Task<IDataValidationResults<TKey>> DeleteAsync(TKey id);
		Task<IDataValidationResults<TKey>> DeleteAsync(IEnumerable<TKey> ids);
		Task<IDataValidationResults<TKey>> DeleteAsync(IEnumerable<TModel> items);

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

	public interface IDataValidationResult<TKey> 
	where TKey : IEquatable<TKey>
	{
		DataValidationResultType ResultType { get; set; }
		string FieldName { get; set; }
		string Message { get; set; }
		int Code { get; set; }
		TKey ID { get; set; }

	}

	public interface IDataValidationResults<TKey> 
	where TKey : IEquatable<TKey>
	{
		IEnumerable<IDataValidationResult<TKey>> Results { get ; }
		void Add(IDataValidationResult<TKey> error);
		void Add(DataValidationResultType resultType, TKey id, string fieldName, string message, int code);

		bool Success { get;}

	}

	public interface IDataStoreValidator<TKey, TModel>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
	{
		IDataValidationResult<TKey> Inserting(TModel model, IDataValidationResults<TKey> validationResults);
		IDataValidationResult<TKey> Updating(TModel model, IDataValidationResults<TKey> validationResults);
		IDataValidationResult<TKey> Deleting(TKey id, object arg, IDataValidationResults<TKey> validationResults);

	}
	public interface IDataStoreValidator<TKey, TModel, TDBModel>: IDataStoreValidator<TKey, TModel>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TDBModel : class, IDataStoreModel<TKey>
	{
		IDataValidationResult<TKey> Inserted(TModel model, TDBModel dbModel, IDataValidationResults<TKey> validationResults);
		IDataValidationResult<TKey> Updated(TModel model, TDBModel dbModel, IDataValidationResults<TKey> validationResults);
		IDataValidationResult<TKey> Deleted(TKey id, TDBModel dbModel, IDataValidationResults<TKey> validationResults);
	}

	public interface IDataMapper<TKey, TModel, TDBModel>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TDBModel : class, IDataStoreModel<TKey>
	{
		Func<TDBModel, TModel> CreateModel { get; }
		TDBModel Assign(TModel source, TDBModel destination);
		string Map(string sourceField);
	}
}
