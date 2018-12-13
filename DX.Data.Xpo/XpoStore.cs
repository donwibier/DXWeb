using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace DX.Data.Xpo
{
	//public class XpoStore<TXPOMainEntity, TKey> : IDisposable
	//		where TXPOMainEntity : XPBaseObject
	//		where TKey : IEquatable<TKey>

	//{
	//	private readonly XpoDatabase _Database;

	//	public XpoStore()
	//		  : this("DefaultConnection")
	//	{
	//	}
	//	public XpoStore(string connectionName) :
	//		this(ConfigurationManager.ConnectionStrings[connectionName].ConnectionString, connectionName)
	//	{
	//	}
	//	public XpoStore(string connectionString, string name) :
	//		this(new XpoDatabase(connectionString, name))
	//	{
	//	}

	//	public XpoStore(XpoDatabase database)
	//	{
	//		_Database = database;
	//	}

	//	protected virtual string KeyField { get { return "Id"; } }
	//	protected virtual XpoDatabase Database { get { return _Database; } }

	//	protected Session GetSession(bool transactional = false)
	//	{
	//		return transactional ? _Database.GetUnitOfWork() : _Database.GetSession();
	//	}
	//	protected virtual void XPOExecute(Action<XpoDatabase, Session> work, bool transactional = true)
	//	{
	//		_Database.Execute(work, transactional);
	//	}
	//	protected virtual T XPOExecute<T>(Func<XpoDatabase, Session, T> work, bool transactional = true)
	//	{
	//		return _Database.Execute(work, transactional);
	//	}

	//	protected virtual TKey XPOSelectAndUpdate(TKey Id, Func<TXPOMainEntity, TKey> work, bool raiseNotFoundException = true)
	//	{
	//		TKey result = XPOSelectAndUpdate<TKey>(Id, work, raiseNotFoundException); ;
	//		return result;
	//	}

	//	protected virtual T XPOSelectAndUpdate<T>(TKey Id, Func<TXPOMainEntity, T> work, bool raiseNotFoundException = true)
	//	{
	//		return XPOSelectAndUpdate(CriteriaOperator.Parse($"{KeyField} == ?", Id), work, raiseNotFoundException);
	//	}
	//	protected virtual T XPOSelectAndUpdate<T>(CriteriaOperator selectCriteria, Func<TXPOMainEntity, T> work, bool raiseNotFoundException = true)
	//	{
	//		int affectedItems = 0;
	//		T[] results = XPOSelectAndUpdate<T>(selectCriteria, null, work, out affectedItems);

	//		if (affectedItems == 0 && raiseNotFoundException)
	//			throw new ArgumentNullException($"The requested object could not be found ({selectCriteria})!");

	//		return (results.Length > 0) ? results[0] : default(T);
	//	}
	//	protected virtual T[] XPOSelectAndUpdate<T>(CriteriaOperator selectCriteria, SortProperty[] sortProperties, Func<TXPOMainEntity, T> work, out int affectedItems)
	//	{
	//		int count = 0;
	//		List<T> results = new List<T>();
	//		using (UnitOfWork wrk = _Database.GetUnitOfWork())
	//		{
	//			XPCollection items = new XPCollection(wrk, typeof(TXPOMainEntity), selectCriteria, sortProperties);
	//			foreach (var item in items)
	//			{
	//				results.Add(work(item as TXPOMainEntity));
	//				count++;
	//				wrk.CommitChanges();
	//			}

	//		}
	//		affectedItems = count;
	//		return results.ToArray();
	//	}


	//	protected void ThrowIfDisposed()
	//	{
	//		if (disposedValue)
	//		{
	//			throw new ObjectDisposedException(GetType().Name);
	//		}
	//	}



	//	#region IDisposable Support
	//	private bool disposedValue = false; // To detect redundant calls

	//	protected virtual void Dispose(bool disposing)
	//	{
	//		if (!disposedValue)
	//		{
	//			if (disposing)
	//			{
	//				// TODO: dispose managed state (managed objects).
	//			}

	//			// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
	//			// TODO: set large fields to null.

	//			disposedValue = true;
	//		}
	//	}

	//	// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
	//	// ~XpoStore() {
	//	//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
	//	//   Dispose(false);
	//	// }

	//	// This code added to correctly implement the disposable pattern.
	//	public void Dispose()
	//	{
	//		// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
	//		Dispose(true);
	//		// TODO: uncomment the following line if the finalizer is overridden above.
	//		// GC.SuppressFinalize(this);
	//	}
	//	#endregion

	//}
}
