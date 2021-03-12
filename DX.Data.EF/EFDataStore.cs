using DX.Utils;
using DX.Utils.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if (NETSTANDARD2_1)
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
#else
using System.Data.Entity;
#endif

namespace DX.Data.EF
{
	public abstract class EFDataMapper<TKey, TModel, TEFClasss> :
			DataMapper<TKey, TModel, TEFClasss>,
			IEFDataMapper<TKey, TModel, TEFClasss>
		where TKey : IEquatable<TKey>
		where TModel : class, new()
		where TEFClasss : class, new()
	{

	}

	public class EFDataValidator<TKey, TModel, TEFClass> :
			DataValidator<TKey, TModel, TEFClass>,
			IEFDataStoreValidator<TKey, TModel, TEFClass>
		where TKey : IEquatable<TKey>
		where TModel : class, new()
		where TEFClass : class, new()
	{
		public override IDataValidationResults<TKey> Deleted(TKey id, TEFClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, id, string.Empty, string.Empty, 0, DataValidationEventType.Deleted));

			validationResults.AddRange(result.Results);
			return result;
		}

		public override IDataValidationResults<TKey> Deleting(TKey id, IDataValidationResults<TKey> validationResults, params object[] args)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, id, string.Empty, string.Empty, 0, DataValidationEventType.Inserting));

			validationResults.AddRange(result.Results);
			return result;
		}

		public override IDataValidationResults<TKey> Inserted(TKey id, TModel model, TEFClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, id, string.Empty, string.Empty, 0, DataValidationEventType.Inserted));

			validationResults.AddRange(result.Results);
			return result;
		}

		public override IDataValidationResults<TKey> Inserting(TModel model, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, default, string.Empty, string.Empty, 0, DataValidationEventType.Inserting));

			validationResults.AddRange(result.Results);
			return result;
		}

		public override IDataValidationResults<TKey> Updated(TKey id, TModel model, TEFClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, id, string.Empty, string.Empty, 0, DataValidationEventType.Updated));

			validationResults.AddRange(result.Results);
			return result;
		}

		public override IDataValidationResults<TKey> Updating(TKey id, TModel model, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, id, string.Empty, string.Empty, 0, DataValidationEventType.Updating));

			validationResults.AddRange(result.Results);
			return result;
		}
	}



	public abstract class EFDataStore<TDBContext, TKey, TModel, TEFClass> : DataStore<TKey, TModel>
			where TDBContext : DbContext, new()
			where TKey : IEquatable<TKey>
			where TModel : class, new()
			where TEFClass : class, new()
	{
		public EFDataStore(EFDatabase<TDBContext> db,
			IEFDataMapper<TKey, TModel, TEFClass> mapper,
			IEFDataStoreValidator<TKey, TModel, TEFClass> validator = null)
		{
			if (db == null)
				throw new ArgumentNullException(nameof(db));
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			DB = db;
			Mapper = mapper;
			Validator = validator;
		}
		public EFDatabase<TDBContext> DB { get; protected set; }

		public IEFDataMapper<TKey, TModel, TEFClass> Mapper { get; protected set; }
		public IEFDataStoreValidator<TKey, TModel, TEFClass> Validator { get; protected set; }

		public Type EFType => typeof(TEFClass);
		public Type EFDBContext => typeof(TDBContext);

		public abstract TKey GetEFModelKey(TEFClass model);
		protected virtual IQueryable<TEFClass> EFQuery(TDBContext ctx)
		{
			var result = ctx.Set<TEFClass>();
			return result;
		}

		//protected async virtual Task<IEnumerable<TModel>> QueryAsync()
		//{
		//	var result = await DB.ExecuteAsync((db, ctx, t) =>
		//	{
		//		var r = EFQuery(ctx).Select(CreateModelInstance);
		//		return r.ToList();
		//	});
		//	return result;
		//}

		protected override IEnumerable<TModel> Query()
		{
			var result = DB.Execute((db, ctx, t) =>
			{
				var r = EFQuery(ctx).Select(CreateModelInstance);
				return r.ToList();
			});
			return result;
		}
		protected virtual Func<TEFClass, TModel> CreateModelInstance => Mapper.CreateModel;

		protected TEFClass Assign(TModel source, TEFClass destination)
		{
			return Mapper.Assign(source, destination);
		}

		public override TModel GetByKey(TKey key)
		{
			var result = DB.Execute((db, ctx, t) =>
			{
				TEFClass item = ctx.Set<TEFClass>().Find(key);
				if (item != null)
					return CreateModelInstance(item);
				return null;
			});
			return result;
		}

		class InsertHelper
		{
			public InsertHelper(TModel model, EntityEntry<TEFClass> entry, IDataValidationResult<TKey> insertingResult, IDataValidationResult<TKey> insertedResult)
			{
				Model = model;
				EFEntry = entry;
				InsertingResult = insertingResult;
				InsertedResult = insertedResult;
			}
			public TModel Model { get; private set; }
			public EntityEntry<TEFClass> EFEntry { get; private set; }
			public IDataValidationResult<TKey> InsertingResult { get; private set; }
			public IDataValidationResult<TKey> InsertedResult { get; private set; }
		}

#if (!NETSTANDARD2_1)
		class EntityEntry<T> {
		  public T Entity { get; set;}
		}
#endif

		protected enum StoreMode
		{
			Create,
			Update,
			Store
		}

		protected virtual IDataValidationResults<TKey> InternalStore(IEnumerable<TModel> items, StoreMode mode, bool continueOnError)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			IDataValidationResults<TKey> result = new DataValidationResults<TKey>();

			result = DB.Execute((db, ctx, t) =>
			{
				// need to keep the xpo entities together with the model items so we can update 
				// the id's of the models afterwards.
				Dictionary<TEFClass, InsertHelper> batchPairs = new Dictionary<TEFClass, InsertHelper>();
				var r = new DataValidationResults<TKey>();
				foreach (var item in items)
				{
					var modelKey = GetModelKey(item);
					if (modelKey == null || modelKey.Equals(EmptyKeyValue) || mode == StoreMode.Create)
					{
						var canInsert = Validator?.Inserting(item, r);
						if (!canInsert.Success)
						{
							if (!continueOnError)
							{
								t.Rollback();
								break;
							}
						}
						else
						{
							TEFClass newItem = new TEFClass();
							newItem = Assign(item, newItem);

#if (NETSTANDARD2_1)
							EntityEntry<TEFClass> ee = ctx.Set<TEFClass>().Add(newItem);
#else
							EntityEntry<TEFClass> ee = new EntityEntry<TEFClass>() 
							{
									 Entity = ctx.Set<TEFClass>().Add(newItem)
							};
#endif

							var hasInserted = Validator?.Inserted(GetEFModelKey(newItem), item, newItem, r);
							if (!hasInserted.Success)
							{
								if (!continueOnError)
								{
									t.Rollback();
									break;
								}
							}
							batchPairs.Add(newItem, new InsertHelper(item, ee, canInsert.Results.FirstOrDefault(), hasInserted.Results.FirstOrDefault()));
						}
					}
					else if (!modelKey.Equals(EmptyKeyValue) && (mode != StoreMode.Create))
					{
						var canUpdate = Validator?.Updating(modelKey, item, r);
						if (!canUpdate.Success)
						{
							if (!continueOnError)
							{
								t.Rollback();
								break;
							}
						}
						else
						{
							var updatedItem = ctx.Set<TEFClass>().Find(modelKey);
							if (updatedItem == null)
							{
								r.Add(
									DataValidationResultType.Error,
									modelKey,
									"KeyField",
									$"Unable to locate {typeof(TEFClass).Name}({modelKey}) in datastore",
									0,
									DataValidationEventType.Updating);
								break;
							}

							Assign(item, updatedItem);
							ctx.Entry(updatedItem).State = EntityState.Modified;
							//ctx.Set<TEFClass>().Update(updatedItem);

							var hasUpdated = Validator?.Updated(modelKey, item, updatedItem, r);
							if (!hasUpdated.Success)
							{
								if (!continueOnError)
								{
									t.Rollback();
									break;
								}
							}
						}
					}

				}

				try
				{
					ctx.SaveChanges();
					t.Commit();
					// sync the model ids with the newly generated EF id's
					foreach (var p in batchPairs)
					{
						var key = GetEFModelKey(p.Value.EFEntry.Entity);
						SetModelKey(p.Value.Model, key);
						p.Value.InsertedResult.ID = key;
						p.Value.InsertingResult.ID = key;

					}
				}
				catch (Exception e)
				{
					t.Rollback();
					r.Add(new DataValidationResult<TKey>
					{
						ResultType = DataValidationResultType.Error,
						Message = e.GetInnerException().Message
					});
				}
				return r;
			});

			return result;
		}


		//=====
		public override IDataValidationResults<TKey> Create(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			var result = InternalStore(items, StoreMode.Create, true);
			return result;
		}

		public override IDataValidationResults<TKey> Update(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			var result = InternalStore(items, StoreMode.Update, true);
			return result;
		}
		public override IDataValidationResults<TKey> Store(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			var result = InternalStore(items, StoreMode.Store, true);
			return result;
		}

		public override IDataValidationResults<TKey> Delete(IEnumerable<TKey> ids)
		{
			var result = DB.Execute((db, ctx, tran) =>
			{
				var r = new DataValidationResults<TKey>();
				foreach (var id in ids)
				{

					var item = ctx.Set<TEFClass>().Find(id);
					if (item == null)
					{
						r.Add(DataValidationResultType.Error, id, "KeyField", $"Unable to locate {typeof(TEFClass).Name}({id}) in datastore", 0, DataValidationEventType.Deleting);
						break;
					}
					var canDelete = Validator?.Deleting(id, r, item);
					if (!canDelete.Success)
					{
						tran.Rollback();
						break;
					}

					ctx.Set<TEFClass>().Remove(item);

					//val.Deleted(id, item, r);
					var hasDeleted = Validator?.Deleted(id, item, r);
					if (!hasDeleted.Success)
					{
						tran.Rollback();
						break; // throw new Exception(hasInserted.Message);
					}
				}
				try
				{
					ctx.SaveChanges();
					tran.Commit();
				}
				catch (Exception e)
				{
					r.Add(new DataValidationResult<TKey>
					{
						ResultType = DataValidationResultType.Error,
						Message = e.InnerException != null ? e.InnerException.Message : e.Message
					});
				}
				return r;
			}, true, false);
			return result;
		}
	}
}
