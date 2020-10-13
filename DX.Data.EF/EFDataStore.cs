using DX.Utils;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if (NETSTANDARD2_1)
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace DX.Data.EF
{
	public abstract class EFDataMapper<TKey, TModel, TEFClasss> :
			DataMapper<TKey, TModel, TEFClasss>,
			IEFDataMapper<TKey, TModel, TEFClasss>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TEFClasss : class, IDataStoreModel<TKey>
	{

	}

	public class EFDataValidator<TKey, TModel, TEFClass> :
			DataValidator<TKey, TModel, TEFClass>,
			IEFDataStoreValidator<TKey, TModel, TEFClass>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TEFClass : class, IDataStoreModel<TKey>
	{
		public override IDataValidationResult<TKey> Deleted(TKey id, TEFClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = id
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Deleting(TKey id, IDataValidationResults<TKey> validationResults, params object[] args)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = id
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Inserted(TModel model, TEFClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = dbModel.ID
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Inserting(TModel model, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = model.ID
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Updated(TModel model, TEFClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = dbModel.ID
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Updating(TModel model, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = model.ID
			};
			validationResults.Add(result);
			return result;
		}
	}



	public abstract class EFDataStore<TDBContext, TKey, TModel, TEFClass> : DataStore<TKey, TModel>
			where TDBContext : DbContext, new()
			where TKey : IEquatable<TKey>
			where TModel : class, IDataStoreModel<TKey>, new()
			where TEFClass : class, IDataStoreModel<TKey>
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

		protected virtual IQueryable<TEFClass> EFQuery(TDBContext ctx)
		{
			var result = from n in ctx.Set<TEFClass>()
						 select n;
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
				TEFClass item = EFQuery(ctx).Where(model => model.ID.Equals(key)).FirstOrDefault();
				if (item != null)
					return CreateModelInstance(item);
				return null;
			});
			return result;
		}

		public override IDataValidationResults<TKey> Create(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			IDataValidationResults<TKey> result = new DataValidationResults<TKey>();

			result = DB.Execute((db, ctx, tran) =>
			{
				// need to keep the xpo entities together with the model items so we can update 
				// the id's of the models afterwards.
				Dictionary<TEFClass, TModel> batchPairs = new Dictionary<TEFClass, TModel>();

				var r = new DataValidationResults<TKey>();
				foreach (var item in items)
				{
					var canInsert = Validator?.Inserting(item, r);
					if (canInsert.ResultType == DataValidationResultType.Error)
					{
						tran.Rollback();
						r.Add(canInsert);
						break;
					}

					TEFClass newItem = Assign(item, Activator.CreateInstance(typeof(TEFClass)) as TEFClass);
					batchPairs.Add(newItem, item);
					ctx.Entry(newItem).State = EntityState.Added;

					var hasInserted = Validator?.Inserted(item, newItem, r);
					if (hasInserted.ResultType == DataValidationResultType.Error)
					{
						tran.Rollback();
						r.Add(hasInserted);
						break;
					}
				}

				try
				{
					//ctx.ObjectSaved += (s, e) => {
					//	// sync the model ids with the newly generated xpo id's
					//	var xpoItem = e.Object as TEFClass;
					//	if (xpoItem != null && batchPairs.ContainsKey(xpoItem))
					//	{
					//		//var model = batchPairs[xpoItem];
					//		batchPairs[xpoItem].ID = xpoItem.ID;
					//	}
					//};
					ctx.SaveChanges();
					tran.Commit();

				}
				catch (Exception e)
				{
					tran.Rollback();
					r.Add(new DataValidationResult<TKey>
					{
						ResultType = DataValidationResultType.Error,
						Message = e.GetInnerException().Message
					});
				}
				return r;
			}, true, false);

			return result;
		}

		public override IDataValidationResults<TKey> Update(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			IDataValidationResults<TKey> result = new DataValidationResults<TKey>();
			result = DB.Execute((db, ctx, tran) =>
			{
				var r = new DataValidationResults<TKey>();
				foreach (var item in items)
				{
					var canUpdate = Validator?.Updating(item, r);
					if (canUpdate.ResultType == DataValidationResultType.Error)
					{
						tran.Rollback();
						r.Add(canUpdate);
						break;
					}

					var updatedItem = EFQuery(ctx).Where(m => m.ID.Equals(item.ID)).FirstOrDefault();
					if (updatedItem == null)
					{
						r.Add(DataValidationResultType.Error, item.ID, "KeyField", $"Unable to locate {typeof(TEFClass).Name}({item.ID}) in datastore", 0, DataValidationEventType.Updating);
						break;
					}

					Assign(item, updatedItem);
					ctx.Entry(updatedItem).State = EntityState.Modified;

					var hasUpdated = Validator?.Updated(item, updatedItem, r);
					if (hasUpdated.ResultType == DataValidationResultType.Error)
					{
						tran.Rollback();
						r.Add(hasUpdated);
						break;
					}
				}
				try
				{
					ctx.SaveChanges();
					tran.Commit();
				}
				catch (Exception e)
				{
					tran.Rollback();
					r.Add(new DataValidationResult<TKey>
					{
						ResultType = DataValidationResultType.Error,
						Message = e.GetInnerException().Message
					});
				}
				return r;
			}, true, false);

			return result;
		}

		public override IDataValidationResults<TKey> Delete(IEnumerable<TKey> ids)
		{
			var result = DB.Execute((db, ctx, tran) =>
			{
				var r = new DataValidationResults<TKey>();
				foreach (var id in ids)
				{

					var item = EFQuery(ctx).Where(m => m.ID.Equals(id)).FirstOrDefault();
					if (item == null)
					{
						r.Add(DataValidationResultType.Error, item.ID, "KeyField", $"Unable to locate {typeof(TEFClass).Name}({item.ID}) in datastore", 0, DataValidationEventType.Deleting);
						break;
					}
					var canDelete = Validator?.Deleting(id, r, item);
					if (canDelete.ResultType == DataValidationResultType.Error)
					{
						tran.Rollback();
						r.Add(canDelete);
						break;
					}

					ctx.Entry(item).State = EntityState.Deleted;

					//val.Deleted(id, item, r);
					var hasDeleted = Validator?.Deleted(id, item, r);
					if (hasDeleted.ResultType == DataValidationResultType.Error)
					{
						tran.Rollback();
						r.Add(hasDeleted);
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
