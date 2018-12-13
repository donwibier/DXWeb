using DevExpress.Data.Filtering;
using DevExpress.Data.Linq;
using DevExpress.Data.Linq.Helpers;
using DevExpress.Xpo;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace DX.Data.Xpo.Identity
{
	//public abstract class XpoDtoBaseEntity<TKey, TXPOEntity> : IDataStoreModel<TKey>
	//	 where TKey : IEquatable<TKey>
	//	 where TXPOEntity : IXPSimpleObject, IAssignable
	//{
	//	public XpoDtoBaseEntity(TXPOEntity source)
	//		  : this(source, 0)
	//	{
	//	}

	//	public XpoDtoBaseEntity(TXPOEntity source, int loadingFlags)
	//		  : this()
	//	{
	//		if (source != null)
	//			Assign(source, loadingFlags);
	//	}

	//	public XpoDtoBaseEntity()
	//	{

	//	}
	//	public abstract TKey Key { get; }

	//	public static Type XPOType { get { return typeof(TXPOEntity); } }
	//	public static TXPOEntity CreateXPOInstance(Session session)
	//	{
	//		return (TXPOEntity)Activator.CreateInstance(typeof(TXPOEntity), session);
	//	}

	//	protected TXPOEntity CastSource(object source)
	//	{
	//		return (TXPOEntity)source;
	//	}
	//	//public abstract void Assign(TXPOEntity source, int loadingFlags);
	//	public abstract void Assign(object source, int loadingFlags);
	//}
	////[DataObject(true)]
	//public class XpoDtoDatasource<TKey, TXPOEntity, TDTOEntity>
	//	 where TKey : IEquatable<TKey>
	//	 where TXPOEntity : XPBaseObject, IXPOKey<TKey>, IAssignable
	//	 where TDTOEntity : XpoDtoBaseEntity<TKey, TXPOEntity>, IAssignable
	//{
	//	private readonly XpoDatabase _XpoDatabase = null;
	//	public XpoDtoDatasource()
	//	{

	//	}
	//	public XpoDtoDatasource(string connectionStringName)
	//		: this()
	//	{
	//		_XpoDatabase = new XpoDatabase(connectionStringName);
	//	}
	//	public XpoDtoDatasource(XpoDatabase database)
	//		  : this()
	//	{
	//		_XpoDatabase = database;
	//	}

	//	[DataObjectMethod(DataObjectMethodType.Select, false)]
	//	public List<TDTOEntity> Select(CriteriaOperator criteria = null, int pageSize = -1, int pageIndex = -1, params SortProperty[] sortProperties)
	//	{
	//		return Select(_XpoDatabase, criteria, pageSize, pageIndex, sortProperties);
	//	}
	//	[DataObjectMethod(DataObjectMethodType.Select, false)]
	//	public static List<TDTOEntity> Select(XpoDatabase database, CriteriaOperator criteria = null, int pageSize = -1, int pageIndex = -1, params SortProperty[] sortProperties)
	//	{
	//		List<TDTOEntity> result = new List<TDTOEntity>();
	//		using (UnitOfWork wrk = database.GetUnitOfWork())
	//		{
	//			XPCollection<TXPOEntity> items = new XPCollection<TXPOEntity>(wrk, criteria, sortProperties);
	//			if (pageIndex > -1)
	//			{
	//				if (pageSize > -1)
	//					items.SkipReturnedObjects = pageSize * pageIndex;
	//				items.TopReturnedObjects = pageSize;
	//			}
	//			foreach (TXPOEntity item in items)
	//			{
	//				TDTOEntity dto = Activator.CreateInstance(typeof(TDTOEntity), item) as TDTOEntity;
	//				if (dto != null)
	//					result.Add(dto);
	//			}
	//		}

	//		return result;
	//	}

	//	private static ICriteriaToExpressionConverter exprConverter = new CriteriaToExpressionConverter();
	//	public static IQueryable<TXPOEntity> Select(Session session, string filterExpression)
	//	{
	//		return (IQueryable<TXPOEntity>)session.Query<TXPOEntity>().AppendWhere(exprConverter, CriteriaOperator.Parse(filterExpression));
	//	}
	//	//==

	//	[DataObjectMethod(DataObjectMethodType.Select, false)]
	//	public static IQueryable<TDTOEntity> Select(XpoDatabase database, Expression<Func<TXPOEntity, bool>> filter = null, int pageSize = -1, int pageIndex = -1, params SortProperty[] sortProperties)
	//	{
	//		IQueryable<TDTOEntity> result;
	//		using (UnitOfWork wrk = database.GetUnitOfWork())
	//		{
	//			var tmp = wrk.Query<TXPOEntity>().Where(filter);
	//			if ((pageIndex > -1) && (pageSize > 0))
	//			{
	//				tmp = tmp.Skip(pageSize * pageIndex).Take(pageSize);
	//			}
	//			result = tmp.Select((xpo, i) => Activator.CreateInstance(typeof(TDTOEntity), xpo) as TDTOEntity);
	//		}

	//		return result;
	//	}

	//	//==
	//	[DataObjectMethod(DataObjectMethodType.Select, false)]
	//	public TDTOEntity Select(TKey keyValue, int loadingFlags = 0, bool raiseExceptionNotFound = false)
	//	{
	//		return Select(_XpoDatabase, keyValue, loadingFlags, raiseExceptionNotFound);
	//	}

	//	public TDTOEntity Select(CriteriaOperator criteria, int loadingFlags = 0, bool raiseExceptionNotFound = false)
	//	{
	//		return Select(_XpoDatabase, criteria, loadingFlags, raiseExceptionNotFound);
	//	}

	//	[DataObjectMethod(DataObjectMethodType.Select, false)]
	//	public static TDTOEntity Select(XpoDatabase database, TKey keyValue, int loadingFlags = 0, bool raiseExceptionNotFound = false)
	//	{
	//		return Select(database, CriteriaOperator.Parse("[Id]==?", keyValue), loadingFlags, raiseExceptionNotFound);
	//	}
	//	public static TDTOEntity Select(XpoDatabase database, CriteriaOperator criteria, int loadingFlags = 0, bool raiseExceptionNotFound = false)
	//	{
	//		TDTOEntity result = null;
	//		using (UnitOfWork wrk = database.GetUnitOfWork())
	//		{
	//			TXPOEntity item = wrk.FindObject<TXPOEntity>(criteria);
	//			if ((item == null) && raiseExceptionNotFound)
	//				throw new Exception(String.Format("{0} Item not found on '{1}' not found", typeof(TXPOEntity).FullName, criteria.ToString()));

	//			result = Activator.CreateInstance(typeof(TDTOEntity), item, loadingFlags) as TDTOEntity;
	//		}
	//		return result;
	//	}


	//	[DataObjectMethod(DataObjectMethodType.Insert, false)]
	//	public void Insert(TDTOEntity item, int savingFlags, Func<TDTOEntity, Exception[]> validateInsert, out TKey keyValue)
	//	{
	//		Insert(_XpoDatabase, item, savingFlags, validateInsert, out keyValue);
	//	}

	//	[DataObjectMethod(DataObjectMethodType.Insert, false)]
	//	public void Insert(XpoDatabase database, TDTOEntity items, int savingFlags, Func<TDTOEntity, Exception[]> validateInsert, out TKey keyValue)
	//	{
	//		TKey[] result;
	//		Insert(database, new TDTOEntity[] { items }, savingFlags, validateInsert, out result);
	//		if ((result != null) && (result.Length > 0))
	//			keyValue = result[0];
	//		else
	//			keyValue = default(TKey);
	//	}

	//	public static void Insert(XpoDatabase database, IEnumerable<TDTOEntity> items, int savingFlags, Func<TDTOEntity, Exception[]> validateInsert, out TKey[] keyValues)
	//	{
	//		// Validate your input here !!!!								
	//		if (validateInsert != null)
	//		{
	//			foreach (var item in items)
	//			{
	//				Exception[] validationResult = validateInsert(item);
	//				if (validationResult != null)
	//					throw new Exception(String.Format("Insert Validation of {0} item failed:\n{1}", typeof(TDTOEntity).FullName, item.Key,
	//						 String.Join("\n", (from e in validationResult select e.Message))));
	//			}
	//		}

	//		TKey[] ids = null;

	//		using (UnitOfWork wrk = database.GetUnitOfWork())
	//		{
	//			wrk.ObjectsSaved += (sender, e) =>
	//			{
	//				ids = (from n in e.Objects.OfType<TXPOEntity>()
	//					   select n.Key).ToArray();

	//			};
	//			foreach (var item in items)
	//			{
	//				TXPOEntity dbItem = Activator.CreateInstance(typeof(TXPOEntity), wrk) as TXPOEntity;
	//				dbItem.Assign(item, savingFlags);
	//			}
	//			wrk.CommitChanges();

	//			keyValues = ids;
	//		}
	//	}

	//	[DataObjectMethod(DataObjectMethodType.Update, false)]
	//	public void Update(TDTOEntity item, int savingFlags, Func<TDTOEntity, Exception[]> validateUpdate)
	//	{
	//		Update(_XpoDatabase, item, savingFlags, validateUpdate);
	//	}

	//	[DataObjectMethod(DataObjectMethodType.Update, false)]
	//	public void Update(XpoDatabase database, TDTOEntity item, int savingFlags, Func<TDTOEntity, Exception[]> validateUpdate,
	//		  bool insertOnNotFound = false, bool raiseExceptionOnNotFound = false)
	//	{
	//		Update(database, new TDTOEntity[] { item }, savingFlags, validateUpdate, insertOnNotFound, raiseExceptionOnNotFound);
	//	}

	//	public static void Update(XpoDatabase database, IEnumerable<TDTOEntity> items, int savingFlags, Func<TDTOEntity, Exception[]> validateUpdate,
	//		  bool insertOnNotFound = false, bool raiseExceptionOnNotFound = false)
	//	{
	//		// Validate your input here !!!!
	//		if (validateUpdate != null)
	//		{
	//			foreach (var item in items)
	//			{
	//				Exception[] validationResult = validateUpdate(item);
	//				if (validationResult != null)
	//					throw new Exception(String.Format("Update Validation of {0} item({1}) failed:\n{2}", typeof(TDTOEntity).FullName, item.Key,
	//						 String.Join("\n", (from e in validationResult select e.Message))));
	//			}
	//		}
	//		using (UnitOfWork wrk = database.GetUnitOfWork())
	//		{
	//			foreach (var item in items)
	//			{
	//				TXPOEntity dbItem = wrk.GetObjectByKey<TXPOEntity>(item.Key);
	//				if (dbItem == null)
	//				{
	//					if (insertOnNotFound)
	//						dbItem = Activator.CreateInstance(typeof(TXPOEntity), wrk) as TXPOEntity;
	//					else if (raiseExceptionOnNotFound)
	//						throw new Exception(String.Format("{0} Item with Id = '{1}' not found for updating", typeof(TXPOEntity).FullName, item.Key));
	//				}
	//				if (dbItem != null)
	//				{
	//					dbItem.Assign(item, savingFlags);
	//				}
	//			}
	//			wrk.CommitChanges();
	//		}
	//	}

	//	public void Delete(TDTOEntity item, Func<TDTOEntity, Exception[]> validateDelete)
	//	{
	//		Delete(_XpoDatabase, new TDTOEntity[] { item }, validateDelete);
	//	}
	//	public void Delete(IEnumerable<TDTOEntity> items, Func<TDTOEntity, Exception[]> validateDelete)
	//	{
	//		Delete(_XpoDatabase, items, validateDelete);
	//	}
	//	[DataObjectMethod(DataObjectMethodType.Delete, false)]
	//	public void Delete(XpoDatabase database, TDTOEntity item, Func<TDTOEntity, Exception[]> validateDelete)
	//	{
	//		Delete(database, new TDTOEntity[] { item }, validateDelete);
	//	}
	//	public static void Delete(XpoDatabase database, IEnumerable<TDTOEntity> items, Func<TDTOEntity, Exception[]> validateDelete)
	//	{
	//		// Validate your input here !!!!
	//		if (validateDelete != null)
	//		{
	//			foreach (var item in items)
	//			{
	//				Exception[] validationResult = validateDelete(item);
	//				if (validationResult != null)
	//					throw new Exception(String.Format("Delete Validation of {0} item({1}) failed:\n{2}", typeof(TDTOEntity).FullName, item.Key,
	//						 String.Join("\n", (from e in validationResult select e.Message))));
	//			}
	//		}

	//		using (UnitOfWork wrk = database.GetUnitOfWork())
	//		{
	//			wrk.Delete(wrk.GetObjectsByKey(wrk.GetClassInfo<TXPOEntity>(), (from o in items select o.Key).ToArray(), false));
	//			wrk.CommitChanges();
	//		}
	//	}
	//}
}
