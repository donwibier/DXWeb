using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using System;

namespace DX.Data.Xpo
{
	/// <summary>
	/// A connection taken from the data layer's connection pool for the exclusive use of one ExplicitUnitOfWork, so it can
	/// hold a database level transaction (a ThreadSafeDataLayer does not support those, and sharing the pool would let
	/// other requests run inside the transaction). Released to the pool on Dispose.
	/// </summary>
	internal sealed class XPExclusiveConnection : IDisposable
	{
		private readonly DataStorePool _pool;
		private IDataStore? _provider;

		private XPExclusiveConnection(DataStorePool pool, IDataStore provider, XPDictionary dictionary)
		{
			_pool = pool;
			_provider = provider;
			DataLayer = new SimpleDataLayer(dictionary, provider);
		}

		public IDataLayer DataLayer { get; }

		/// <summary>
		/// An exclusive connection for <paramref name="dataLayer"/>, or null when it is not a pooled data layer
		/// (in-memory stores, caching nodes, custom layers): then use a normal UnitOfWork
		/// </summary>
		public static XPExclusiveConnection? TryAcquire(IDataLayer dataLayer)
		{
			if (dataLayer is ThreadSafeDataLayer threadSafe && threadSafe.ConnectionProvider is DataStorePool pool)
				return new XPExclusiveConnection(pool, pool.AcquireChangeProvider(), threadSafe.Dictionary);
			return null;
		}

		public void Dispose()
		{
			// the SimpleDataLayer is not disposed: that would dispose the pooled provider
			var provider = _provider;
			_provider = null;
			if (provider != null)
				_pool.ReleaseChangeProvider(provider);
		}
	}
}
