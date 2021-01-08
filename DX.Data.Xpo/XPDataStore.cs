using DevExpress.Xpo;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
		public override IDataValidationResults<TKey> Deleted(TKey id, TXPOClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, dbModel.ID, string.Empty, string.Empty, 0, DataValidationEventType.Deleted));

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

		public override IDataValidationResults<TKey> Inserted(TModel model, TXPOClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, dbModel.ID, string.Empty, string.Empty, 0, DataValidationEventType.Inserted));

			validationResults.AddRange(result.Results);
			return result;
		}

		public override IDataValidationResults<TKey> Inserting(TModel model, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, model.ID, string.Empty, string.Empty, 0, DataValidationEventType.Inserting));

			validationResults.AddRange(result.Results);
			return result;
		}

		public override IDataValidationResults<TKey> Updated(TModel model, TXPOClass dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, dbModel.ID, string.Empty, string.Empty, 0, DataValidationEventType.Updated));

			validationResults.AddRange(result.Results);
			return result;
		}

		public override IDataValidationResults<TKey> Updating(TModel model, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>(
				new DataValidationResult<TKey>(DataValidationResultType.Success, model.ID, string.Empty, string.Empty, 0, DataValidationEventType.Updating));

			validationResults.AddRange(result.Results);
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
				throw new ArgumentNullException(nameof(db));
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			DB = db;
			Mapper = mapper;
			Validator = validator;
		}
		public XpoDatabase DB { get; protected set; }

		public IDataMapper<TKey, TModel, TXPOClass> Mapper { get; protected set; }
		public IDataStoreValidator<TKey, TModel, TXPOClass> Validator { get; protected set; }

		public Type XpoType => typeof(TXPOClass);


		protected virtual IQueryable<TXPOClass> Query(Session s)
		{
			var result = from n in s.Query<TXPOClass>()
						 select n;
			return result;
		}
		protected override IEnumerable<TModel> Query()
		{
			var result = DB.Execute((db, w) =>
			{
				var r = Query(w).Select(CreateModelInstance);
				return r.ToList();
			});
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


		class InsertHelper
		{
			public InsertHelper(TModel model, IDataValidationResult<TKey> insertingResult, IDataValidationResult<TKey> insertedResult)
			{
				Model = model;
				InsertingResult = insertingResult;
				InsertedResult = insertedResult;
			}
			public TModel Model { get; private set; }
			public IDataValidationResult<TKey> InsertingResult { get; private set; }
			public IDataValidationResult<TKey> InsertedResult { get; private set; }
		}
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

			result = DB.Execute((db, w) =>
			{
				// need to keep the xpo entities together with the model items so we can update 
				// the id's of the models afterwards.
				Dictionary<TXPOClass, InsertHelper> batchPairs = new Dictionary<TXPOClass, InsertHelper>();
				var r = new DataValidationResults<TKey>();
				foreach (var item in items)
				{
					if (item.ID == null || item.ID.Equals(EmptyKeyValue) || mode == StoreMode.Create)
					{
						var canInsert = Validator?.Inserting(item, r);
						if (!canInsert.Success)
						{
							if (!continueOnError)
							{
								w.RollbackTransaction();
								break;
							}
						}
						else
						{
							TXPOClass newItem = Assign(
								item,
								Activator.CreateInstance(typeof(TXPOClass), new object[] { w }) as TXPOClass);

							var hasInserted = Validator?.Inserted(item, newItem, r);
							if (!hasInserted.Success)
							{
								if (!continueOnError)
								{
									w.RollbackTransaction();
									break;
								}
							}
							batchPairs.Add(newItem, new InsertHelper(item, canInsert.Results.FirstOrDefault(), hasInserted.Results.FirstOrDefault()));
						}
					}
					else if (!item.ID.Equals(EmptyKeyValue) && (mode != StoreMode.Create))
					{
						var canUpdate = Validator?.Updating(item, r);
						if (!canUpdate.Success)
						{
							if (!continueOnError)
							{
								w.RollbackTransaction();
								break;
							}
						}
						else
						{
							var updatedItem = w.GetObjectByKey<TXPOClass>(item.ID);
							if (updatedItem == null)
							{
								r.Add(
									DataValidationResultType.Error,
									item.ID,
									"KeyField",
									$"Unable to locate {typeof(TXPOClass).Name}({item.ID}) in datastore",
									0,
									DataValidationEventType.Updating);
								break;
							}

							Assign(item, updatedItem);

							var hasUpdated = Validator?.Updated(item, updatedItem, r);
							if (!hasUpdated.Success)
							{
								if (!continueOnError)
								{
									w.RollbackTransaction();
									break;
								}
							}
						}
					}

				}

				try
				{
					w.ObjectSaved += (s, e) =>
					{
						// sync the model ids with the newly generated xpo id's
						var xpoItem = e.Object as TXPOClass;
						if (xpoItem != null && batchPairs.ContainsKey(xpoItem))
						{
							batchPairs[xpoItem].Model.ID = xpoItem.ID;
							batchPairs[xpoItem].InsertingResult.ID = xpoItem.ID;
							batchPairs[xpoItem].InsertedResult.ID = xpoItem.ID;
						}
					};
					w.FailedCommitTransaction += (s, e) =>
					{
						r.Add(new DataValidationResult<TKey>
						{
							ResultType = DataValidationResultType.Error,
							Message = e.Exception.InnerException != null ? e.Exception.InnerException.Message : e.Exception.Message
						});

						e.Handled = true;
					};
					w.CommitTransaction();
				}
				catch
				{
					w.RollbackTransaction();
				}
				return r;
			});

			return result;
		}


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
			var result = DB.Execute((db, w) =>
			{
				var r = new DataValidationResults<TKey>();
				foreach (var id in ids)
				{

					var item = w.GetObjectByKey<TXPOClass>(id);
					if (item == null)
					{
						r.Add(DataValidationResultType.Error, item.ID, "KeyField", $"Unable to locate {typeof(TXPOClass).Name}({item.ID}) in datastore", 0, DataValidationEventType.Deleting);
						break;
					}
					var canDelete = Validator?.Deleting(id, r, item);
					if (!canDelete.Success)
					{
						w.RollbackTransaction();
						break;
					}

					item.Delete();
					//val.Deleted(id, item, r);
					var hasDeleted = Validator?.Deleted(id, item, r);
					if (!hasDeleted.Success)
					{
						w.RollbackTransaction();
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
