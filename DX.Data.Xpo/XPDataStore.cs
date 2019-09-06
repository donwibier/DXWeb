using DevExpress.Xpo;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DX.Data.Xpo
{
	public abstract class XPDataMapper<TKey, TModel, TXPOClass> : DataMapper<TKey, TModel, TXPOClass>, 
																  IXPDataMapper<TKey, TModel, TXPOClass>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TXPOClass : class, IXPSimpleObject, IDataStoreModel<TKey>
	{		

	}


	public class XPDataValidator<TKey, TModel, TXPOClass> : DataValidator<TKey, TModel, TXPOClass>,
															IXPDataStoreValidator<TKey, TModel, TXPOClass>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TXPOClass : class, IXPSimpleObject, IDataStoreModel<TKey>
	{
		public override IDataValidationResult<TKey> Deleted(TKey id, TXPOClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = id
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Deleting(TKey id, object arg, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = id
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Inserted(TModel model, TXPOClass dbModel, IDataValidationResults<TKey> validationResults)
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

		public override IDataValidationResult<TKey> Updated(TModel model, TXPOClass dbModel, IDataValidationResults<TKey> validationResults)
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

	public abstract class XPDataStore<TKey, TModel, TXPOClass> : DataStore<TKey, TModel>
		where TKey : IEquatable<TKey>
		where TModel : class, IDataStoreModel<TKey>, new()
		where TXPOClass : XPBaseObject, IDataStoreModel<TKey>		
	{
		public XPDataStore(XpoDatabase db, IXPDataMapper<TKey, TModel, TXPOClass> mapper, IXPDataStoreValidator<TKey, TModel, TXPOClass> validator = null)
		{
			if (db == null)
				throw new ArgumentNullException("db");
			if (mapper == null)
				throw new ArgumentNullException("mapper");
			DB = db;
			Mapper = mapper;
			Validator = validator;
		}
		public XpoDatabase DB { get; protected set; }

		public IXPDataMapper<TKey, TModel, TXPOClass> Mapper { get; protected set; }
		public IXPDataStoreValidator<TKey, TModel, TXPOClass> Validator { get; protected set; }

		public Type XpoType => typeof(TXPOClass);


		protected virtual IQueryable<TXPOClass> Query(Session s)
		{
			var result = from n in s.Query<TXPOClass>()
						 select n;
			return result;
		}

		protected virtual Func<TXPOClass, TModel> CreateModelInstance => Mapper.CreateModel;

		protected TXPOClass Assign(TModel source, TXPOClass destination)
		{
			return Mapper.Assign(source, destination);
		}

		public override TModel GetByKey(TKey key)
		{
			var result = DB.Execute((db, w) =>
			{
				TXPOClass item = w.GetObjectByKey<TXPOClass>(key);
				if (item != null)
					return CreateModelInstance(item);
				return null;
			});
			return result;
		}

		public override IDataValidationResults<TKey> Create(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			IDataValidationResults<TKey> result = new DataValidationResults<TKey>();

			result = DB.Execute((db, w) =>
			{
				// need to keep the xpo entities together with the model items so we can update 
				// the id's of the models afterwards.
				Dictionary<TXPOClass, TModel> batchPairs = new Dictionary<TXPOClass, TModel>();

				var r = new DataValidationResults<TKey>();
				foreach (var item in items)
				{
					var canInsert = Validator?.Inserting(item, r);
					if (canInsert.ResultType == DataValidationResultType.Error)
					{
						w.RollbackTransaction();
						r.Add(canInsert);
						break;
					}

					TXPOClass newItem = Assign(item, Activator.CreateInstance(typeof(TXPOClass), new object[] { w }) as TXPOClass);
					batchPairs.Add(newItem, item);

					var hasInserted = Validator?.Inserted(item, newItem, r);
					if (hasInserted.ResultType == DataValidationResultType.Error)
					{
						w.RollbackTransaction();
						r.Add(hasInserted);
						break; 
					}
				}

				try
				{
					w.ObjectSaved += (s, e) => {
						// sync the model ids with the newly generated xpo id's
						var xpoItem = e.Object as TXPOClass;
						if (xpoItem != null && batchPairs.ContainsKey(xpoItem))
						{
							//var model = batchPairs[xpoItem];
							batchPairs[xpoItem].ID = xpoItem.ID;
						}
					};
					w.CommitTransaction();
					
				}
				catch (Exception e)
				{
					w.RollbackTransaction();
					r.Add(new DataValidationResult<TKey>
					{
						ResultType = DataValidationResultType.Error,
						Message = e.InnerException != null ? e.InnerException.Message : e.Message
					});
				}
				return r;
			});

			return result;
		}

		public override IDataValidationResults<TKey> Update(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			IDataValidationResults<TKey> result = new DataValidationResults<TKey>();
			result = DB.Execute((db, w) =>
			{
				var r = new DataValidationResults<TKey>();
				foreach (var item in items)
				{
					var canUpdate = Validator?.Updating(item, r);
					if (canUpdate.ResultType == DataValidationResultType.Error)
					{
						w.RollbackTransaction();
						r.Add(canUpdate);
						break;
					}

					var updatedItem = w.GetObjectByKey<TXPOClass>(item.ID);
					if (updatedItem == null)
					{
						r.Add(DataValidationResultType.Error, item.ID, "KeyField", String.Format("Unable to locate {0}({1}) in datastore", typeof(TXPOClass).Name, item.ID), 0);
						break;
					}

					Assign(item, updatedItem);

					var hasUpdated = Validator?.Updated(item, updatedItem, r);
					if (hasUpdated.ResultType == DataValidationResultType.Error)
					{
						w.RollbackTransaction();
						r.Add(hasUpdated);
						break;
					}
				}
				try
				{
					w.CommitTransaction();
				}
				catch (Exception e)
				{
					w.RollbackTransaction();
					r.Add(new DataValidationResult<TKey>
					{
						ResultType = DataValidationResultType.Error,
						Message = e.InnerException != null ? e.InnerException.Message : e.Message
					});
				}
				return r;
			});

			return result;
		}

		public override IDataValidationResults<TKey> Delete(IEnumerable<TKey> ids)
		{
			var result = DB.Execute((db, w) =>
			{ 
				var r = new DataValidationResults<TKey>();
				foreach (var id in ids)
				{

					var item = w.GetObjectByKey<TXPOClass>(id);
					if (item == null)
					{
						r.Add(DataValidationResultType.Error, item.ID, "KeyField", String.Format("Unable to locate {0}({1}) in datastore", typeof(TXPOClass).Name, item.ID), 0);
						break;
					}
					var canDelete = Validator?.Deleting(id, item, r);
					if (canDelete.ResultType == DataValidationResultType.Error)
					{
						w.RollbackTransaction();
						r.Add(canDelete);
						break;
					}
					
					item.Delete();
					//val.Deleted(id, item, r);
					var hasDeleted = Validator?.Deleted(id, item, r);
					if (hasDeleted.ResultType == DataValidationResultType.Error)
					{
						w.RollbackTransaction();
						r.Add(hasDeleted);
						break; // throw new Exception(hasInserted.Message);
					}
				}
				try
				{
					w.CommitTransaction();
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
			});
			return result;
		}
	}
}
