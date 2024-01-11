using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;


namespace DX.Data
{
	public enum DataMode
	{
		Create,
		Update,
		Delete
	}

	public interface IAssignable
	{
		void Assign(object source);
	}

	public interface IDataResult<TKey>
		where TKey : IEquatable<TKey>
		
	{
		bool Success { get; set; }
		DataMode Mode { get; set; }
		TKey Key { get; set; }
        ValidationException Exception { get; set; }
	}

	[Obsolete("For legacy reasons only. Use DX.Data.AutoMapper or DX.Data.Mapster descendants")]
	public interface IDataMapper<TKey, TModel, TDBModel>
		where TKey : IEquatable<TKey>
		where TModel : class, new()
		where TDBModel : class
	{
		Func<TDBModel, TModel> CreateModel { get; }
		TDBModel Assign(TModel source, TDBModel destination);
		string Map(string sourceField);
	}

	public interface IDataStore<TKey, TModel>
		where TKey : IEquatable<TKey>
		where TModel : class
	{
        Task<IDataResult<TKey>> DeleteAsync(params TModel[] items);

        string KeyField { get; }
		TModel GetByKey(TKey key);
		TKey ModelKey(TModel model);
		void SetModelKey(TModel model, TKey key);
		Task<IDataResult<TKey>> StoreAsync(params TModel[] items);
		Task<IDataResult<TKey>> CreateAsync(params TModel[] items);
		Task<IDataResult<TKey>> UpdateAsync(params TModel[] items);
		Task<IDataResult<TKey>> DeleteAsync(params TKey[] ids);
	}

	public interface IQueryableDataStore<TKey, TModel> : IDataStore<TKey, TModel>
		where TKey : IEquatable<TKey>
		where TModel : class
	{
		bool PaginateViaPrimaryKey { get; }
		
		IQueryable<T> Query<T>() where T : class, new();
		IQueryable<TModel> Query();
	}
}
