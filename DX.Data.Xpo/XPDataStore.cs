using DevExpress.Xpo;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DX.Data.Xpo
{
	public class XPDataValidator<TKey, TModel, TXPOClass> : DataValidator<TKey, TModel, TXPOClass>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TXPOClass : XPBaseObject, IDataStoreModel<TKey>
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
				ID = model.ID
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
				ID = model.ID
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
		private readonly XpoDatabase xpo;
		private readonly XPDataValidator<TKey, TModel, TXPOClass> val;
		public XPDataStore(XpoDatabase db, XPDataValidator<TKey, TModel, TXPOClass> validator = null)
		{
			xpo = db;
			val = validator;
		}
		public XpoDatabase DB => xpo;

		public Type XpoType => typeof(TXPOClass);

		//public XPDataValidator<TKey, TModel, TXPOClass> Validator => val;

		protected virtual IQueryable<TXPOClass> Query(Session s)
		{
			var result = from n in s.Query<TXPOClass>()
						 select n;
			return result;
		}


		//protected virtual TModel CreateModel(TXPOClass item)
		//{
		//	return CreateInstance(item);
		//	//TModel result = Activator.CreateInstance(typeof(TModel)) as TModel;
		//	//return Assign(item, result);
		//}
		protected virtual Func<TXPOClass, TModel> CreateModelInstance
		{
			get
			{
				return (x) => { return Assign(x, new TModel()); };
			}
		}

		protected abstract TXPOClass Assign(TModel source, TXPOClass destination);
		protected abstract TModel Assign(TXPOClass source, TModel destination);
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
			try
			{
				result = xpo.Execute((db, w) =>
				{
					var r = new DataValidationResults<TKey>();
					foreach (var item in items)
					{						
						val?.Inserting(item, r);

						//bool ok = val.Inserting(item);
						//if (!ok)
						//	throw new Exception(String.Format("Validator failed on Inserting on '{0}'", XpoType.Name));

						var newItem = Assign(item, Activator.CreateInstance(typeof(TXPOClass), new object[] { w }) as TXPOClass);
						//TODO: Move to Validator Inserted
						//newItem.AddStampUTC = DateTime.UtcNow;
						//newItem.ModStampUTC = DateTime.UtcNow;
						
						//ok = val.Inserted(item, newItem);
						//if (!ok)
						//	throw new Exception(String.Format("Validator failed on Inserted on '{0}'", XpoType.Name));
						//w.FailedCommitTransaction += (s, e) =>
						//{
							
						//	e.Handled = true;
						//};
						try
						{
							w.CommitTransaction();
							item.ID = newItem.ID;
							val?.Inserted(item, newItem, r);
						}
						catch(Exception e)
						{
							r.Add(new DataValidationResult<TKey>
							{
								ResultType = DataValidationResultType.Error,
								Message = e.InnerException != null ? e.InnerException.Message : e.Message
							});
						}
					}
					return r;
				});
			}
			catch (Exception ex)
			{
				result.Add(new DataValidationResult<TKey>
				{
					ResultType = DataValidationResultType.Error,
					Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
				});
			}
			return result;
		}

		public override IDataValidationResults<TKey> Update(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			IDataValidationResults<TKey> result = new DataValidationResults<TKey>();
			try
			{
				result = xpo.Execute((db, w) =>
				{
					var r = new DataValidationResults<TKey>();
					foreach (var item in items)
					{
						val?.Updating(item, r);

						var updatedItem = w.GetObjectByKey<TXPOClass>(item.ID);
						if (updatedItem == null)
							r.Add(DataValidationResultType.Error, item.ID, "KeyField", String.Format("Unable to locate {0}({1}) in datastore", typeof(TXPOClass).Name, item.ID), 0);

						Assign(item, updatedItem);
						// Move to Validator Updated
						//updatedItem.ModStampUTC = DateTime.UtcNow;
						
						try
						{
							w.CommitTransaction();
							//item.ID = updatedItem.ID;
							val?.Updated(item, updatedItem, r);
						}
						catch (Exception e)
						{
							r.Add(new DataValidationResult<TKey>
							{
								ResultType = DataValidationResultType.Error,
								Message = e.InnerException != null ? e.InnerException.Message : e.Message
							});
						}
					}
					return r;
				});
			}
			catch (Exception ex)
			{
				result.Add(new DataValidationResult<TKey>
				{
					ResultType = DataValidationResultType.Error,
					Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
				});
			}
			return result;
		}

		public override IDataValidationResults<TKey> Delete(IEnumerable<TKey> ids)
		{
			var result = xpo.Execute((db, w) =>
			{ 
				var r = new DataValidationResults<TKey>();
				foreach (var id in ids)
				{

					var item = w.GetObjectByKey<TXPOClass>(id);
					if (item == null)
						r.Add(DataValidationResultType.Error, item.ID, "KeyField", String.Format("Unable to locate {0}({1}) in datastore", typeof(TXPOClass).Name, item.ID), 0);

					val.Deleting(id, item, r);
					item.Delete();
					val.Deleted(id, item, r);
				}
				return r;
			});
			return result;
		}

		


	}
}
