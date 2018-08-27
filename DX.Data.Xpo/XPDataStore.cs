using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DX.Data.Xpo
{
	public abstract class XPDataValidator<TKey, TModel, TXPOClass> : DataValidator<TKey, TModel, TXPOClass>
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
		where TXPOClass : XPBaseObject, IDataStoreModel<TKey>
	{
		
	}

	public abstract class XPDataStore<TKey, TModel, TXPOClass> : DataStore<TKey, TModel>
		where TKey : IEquatable<TKey>
		where TModel : class, IDataStoreModel<TKey>
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

		public XPDataValidator<TKey, TModel, TXPOClass> Validator => val;

		protected virtual TModel CreateModel(TXPOClass item)
		{
			TModel result = Activator.CreateInstance(typeof(TModel)) as TModel;
			return Assign(item, result);
		}

		protected abstract TXPOClass Assign(TModel source, TXPOClass destination);
		protected abstract TModel Assign(TXPOClass source, TModel destination);
		public override TModel GetByKey(TKey key)
		{
			var result = DB.Execute((db, w) => {
				TXPOClass item = w.GetObjectByKey<TXPOClass>(key);
				if (item != null)
					return CreateModel(item);
				return null;
			});
			return result;
		}

		public override void Create(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");
			try
			{
				var result = xpo.Execute((db, w) =>
				{
					foreach (var item in items)
					{
						bool ok = val.Inserting(item);
						if (!ok)
							throw new Exception(String.Format("Validator failed on Inserting on '{0}'", XpoType.Name));

						var newItem = Assign(item, Activator.CreateInstance(typeof(TXPOClass), new object[] { w }) as TXPOClass);
						//TODO: Move to Validator Inserted
						//newItem.AddStampUTC = DateTime.UtcNow;
						//newItem.ModStampUTC = DateTime.UtcNow;
						ok = val.Inserted(item, newItem);
						if (!ok)
							throw new Exception(String.Format("Validator failed on Inserted on '{0}'", XpoType.Name));
					}
					return 0;
				});
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		public override void Update(IEnumerable<TModel> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");
			try
			{
				xpo.Execute((db, w) =>
				{
					foreach (var item in items)
					{
						bool ok = val.Updating(item);
						if (!ok)
							throw new Exception(String.Format("{0}.OnUpdating failed on '{1}'", this.GetType().Name, XpoType.Name));

						var updatedItem = w.GetObjectByKey<TXPOClass>(item.ID);
						if (updatedItem == null)
							throw new Exception(String.Format("Unable to locate {0}({1}) in datastore", typeof(TXPOClass).Name, item.ID));

						Assign(item, updatedItem);
						// Move to Validator Updated
						//updatedItem.ModStampUTC = DateTime.UtcNow;
						ok = val.Updated(item, updatedItem);
						if (!ok)
							throw new Exception(String.Format("{0}.OnUpdated failed on '{1}'", this.GetType().Name, XpoType.Name));

					}
					return 0;
				});
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		public override void Delete(IEnumerable<TKey> ids)
		{
			var r = xpo.Execute((db, w) =>
			{
				foreach (var id in ids)
				{

					var item = w.GetObjectByKey<TXPOClass>(id);
					if (item == null)
						throw new Exception(String.Format("Unable to locate {0}({1}) in datastore", typeof(TXPOClass).Name, id));
					bool ok = val.Deleting(item);
					if (!ok)
						throw new Exception(String.Format("{0}.OnDeleting failed on '{1}'", this.GetType().Name, XpoType.Name));

					item.Delete();
					ok = val.Deleted(item);
					if (!ok)
						throw new Exception(String.Format("{0}.OnDeleted failed on '{1}'", this.GetType().Name, XpoType.Name));
				}
				return 0;
			});

		}

		protected virtual IEnumerable<TModel> Query(CriteriaOperator filter, SortProperty[] sorting = null, int pageNo = -1, int pageSize = -1)
		{
			List<TModel> results = new List<TModel>();

			DB.Execute((db, w) =>
			{
				XPCollection<TXPOClass> items = new XPCollection<TXPOClass>(w, filter, sorting);
				if (pageNo > -1 && pageSize > 0)
				{
					items.SkipReturnedObjects = pageSize * pageNo;
					items.TopReturnedObjects = pageSize;
				}
				foreach (TXPOClass item in items)
				{
					results.Add(CreateModel(item));
				}
			});

			return results;
		}
		protected async virtual Task<IEnumerable<TModel>> QueryAsync(CriteriaOperator filter, SortProperty[] sorting, int pageNo = -1, int pageSize = -1)
		{
			var result = await Task.FromResult(Query(filter, sorting, pageNo, pageSize));
			return result;
		}
		public class XPOrderBy
		{

			public XPOrderBy(Func<TXPOClass, object> orderBy, bool ascending = true)
			{
				OrderBy = orderBy;
				Ascending = ascending;
			}
			public Func<TXPOClass, object> OrderBy { get; private set; }
			public bool Ascending { get; private set; }
		}

		protected async virtual Task<IEnumerable<TModel>> QueryAsync(
			Func<TXPOClass, bool> filter = null,
			XPOrderBy[] orderBy = null,
			int pageNo = -1, int pageSize = -1)
		{
			var result = await xpo.ExecuteAsync((db, w) =>
			{
				var q = (filter == null) ? w.Query<TXPOClass>() : w.Query<TXPOClass>().Where(filter);
				if (orderBy != null)
				{
					foreach (var o in orderBy)
					{
						bool first = true;
						if (first)
						{
							q = o.Ascending ? q.OrderBy(o.OrderBy) : q.OrderByDescending(o.OrderBy);
							first = false;
						}
						else
						{
							var z = q as IOrderedEnumerable<TXPOClass>;
							q = o.Ascending ? z.ThenBy(o.OrderBy) : z.ThenByDescending(o.OrderBy);
						}
					}
					//only works when OrderBy is used ??

					if ((pageNo > -1) && (pageSize > 0))
						q = q.Skip(pageNo * pageSize).Take(pageSize);

				}

				var r = q.Select(s => CreateModel(s));
				return r;
			});
			return result;
		}

	}
}
