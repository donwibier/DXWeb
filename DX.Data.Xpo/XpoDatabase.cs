using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using DX.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


#if (NETSTANDARD2_0)
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
#endif

namespace DX.Data.Xpo
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public class XpoDataLayerAttribute : Attribute
	{
		public XpoDataLayerAttribute(string dataLayerName)
		{
			DataLayerName = dataLayerName;
		}
		public string DataLayerName { get; private set; }
	}

	public class XpoDatabase : IDisposable
	{
		//private IDataLayer dataLayer;
		private readonly static object lockObj = new object();
		private readonly static ConcurrentDictionary<string, IDataLayer> dataLayers =
			new ConcurrentDictionary<string, IDataLayer>();

		private readonly string connectionString;
		public string ConnectionString { get { return connectionString; } }

		private readonly string dataLayerName;
		public string DataLayerName { get { return dataLayerName; } }
		/// Default constructor which uses the "DefaultConnection" connectionString
		/// </summary>
		
#if (NETSTANDARD2_0)
		public XpoDatabase(string connectionName, IConfiguration cfg) :
			this(cfg.GetConnectionString(connectionName), connectionName)
		{
		}
#else
		public XpoDatabase()
			: this("DefaultConnection")
		{
		}

		public XpoDatabase(string connectionName) :
			this(ConfigurationManager.ConnectionStrings[connectionName].ConnectionString, connectionName)
		{
		}
#endif

		/// <summary>
		/// Constructor which takes the connection string name
		/// </summary>
		/// <param name="connectionString"></param>
		public XpoDatabase(string connectionString, string connectionName)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString");
			if (String.IsNullOrEmpty(connectionName))
				throw new ArgumentNullException("connectionName");

			this.connectionString = connectionString;
			this.dataLayerName = connectionName;

			IDataLayer dataLayer = GetDataLayer(this.connectionString, this.dataLayerName);
		}

		public virtual Session GetSession()
		{
			return GetSession(ConnectionString, DataLayerName);
		}

		public virtual UnitOfWork GetUnitOfWork()
		{
			return GetUnitOfWork(ConnectionString, DataLayerName);
		}

		public virtual void Execute(Action<XpoDatabase, Session> work, bool transactional = true, bool commit = true)
		{
			using (Session s = transactional ? GetUnitOfWork() : GetSession())
			{
				work(this, s);
				if (transactional && commit && (s is UnitOfWork))
				{
					((UnitOfWork)s).CommitChanges();
				}
			}
		}
		public virtual T Execute<T>(Func<XpoDatabase, Session, T> work, bool transactional = true, bool commit = true)
		{
			T result = default(T);
			using (Session s = transactional ? GetUnitOfWork() : GetSession())
			{
				result = work(this, s);
				if (transactional && commit && (s is UnitOfWork))
					((UnitOfWork)s).CommitChanges();
			}
			return result;
		}

		public async virtual Task<T> ExecuteAsync<T>(Func<XpoDatabase, Session, T> work, bool transactional = true, bool commit = true)
		{
			return await Task.FromResult<T>(Execute<T>(work, transactional, commit));
		}

		public async virtual Task ExecuteAsync(Action<XpoDatabase, Session> work, bool transactional = true, bool commit = true)
		{
			await Task.Run(() => { Execute(work, transactional, commit); });
		}


#region Static Helpers
		public static Session GetSession(string connectionString, string dataLayerName)
		{
			return new Session(GetDataLayer(connectionString, dataLayerName));
		}

		public static UnitOfWork GetUnitOfWork(string connectionString, string dataLayerName)
		{
			return new UnitOfWork(GetDataLayer(connectionString, dataLayerName));
		}
		public static IDataLayer GetDataLayer(string connectionString, string dataLayerName)
		{
			IDataLayer result = null;
			if (!dataLayers.TryGetValue(dataLayerName, out result))
			{
				result = createDataLayer(connectionString, dataLayerName);
				dataLayers.AddOrUpdate(dataLayerName, result, (s, l) => l);
			}
			return result;
		}

		//public static T Execute<T>(string dataLayerName, Func<Session, T> work, bool transactional = true, bool commit = true)			
		//{

		//}

#endregion

		private static IDataLayer createDataLayer(string connectionString, string datalayerName)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString");
			// set XpoDefault.Session to null to prevent accidental use of XPO default session
			XpoDefault.Session = null;
			//ReflectionClassInfo.SuppressSuspiciousMemberInheritanceCheck = true;
			// Needed to run in Medium Trust Security Context
			XpoDefault.UseFastAccessors = false;
			XpoDefault.IdentityMapBehavior = IdentityMapBehavior.Strong;

			// autocreate option in connectionstring
			AutoCreateOption createOption = AutoCreateOption.None;
			bool enableCachingNode = false;
			try
			{
				createOption = Conversion.GetConfigOption<AutoCreateOption>(connectionString, "AutoCreateOption", AutoCreateOption.DatabaseAndSchema);
				enableCachingNode = Conversion.GetConfigOption(connectionString, "EnableCachingNode", false);
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("XpoDatabase was unable to parse connectionString!\n{0}",
					ex.InnerException == null ? ex.Message : ex.InnerException.Message));
			}

			XPDictionary dataDictionary = new ReflectionDictionary();
			IDataStore dataStore = XpoDefault.GetConnectionProvider(XpoDefault.GetConnectionPoolString(connectionString), createOption);

			// Initialize the XPO dictionary
			dataDictionary.GetDataStoreSchema(GetDataTypes(datalayerName));

			// make sure everything exists in the db
			if (createOption == AutoCreateOption.DatabaseAndSchema)
			{
				using (SimpleDataLayer dataLayer = new SimpleDataLayer(dataStore))
				{
					using (Session session = new Session(dataLayer))
					{
						// place code here to patch metadata              
						session.UpdateSchema();
						session.CreateObjectTypeRecords();
						XpoDefault.DataLayer = new ThreadSafeDataLayer(session.Dictionary, dataStore);
					}
				}
			}
			IDataLayer result;
			if (enableCachingNode)
				result = new ThreadSafeDataLayer(dataDictionary, new DataCacheNode(new DataCacheRoot(dataStore)));
			else
				result = new ThreadSafeDataLayer(dataDictionary, dataStore);

			return result;
		}
		static readonly string xpoName = XPClassInfo.GetShortAssemblyName(typeof(XPDictionary).Assembly);
		private static Type[] GetDataTypes(string name = "")
		{
			List<Type> result = new List<Type>();

			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (String.IsNullOrEmpty(name) ||
					(XPClassInfo.GetShortAssemblyName(asm) == xpoName) ||
					(asm.GetReferencedAssemblies().FirstOrDefault(a => a.Name == xpoName) != null))
				{
					foreach (Type tpe in asm.GetTypes())
					{
						if (typeof(PersistentBase).IsAssignableFrom(tpe))
						{
							if (String.IsNullOrEmpty(name))
								result.Add(tpe);
							else
							{
								object[] attrs = tpe.GetCustomAttributes(typeof(PersistentAttribute), true);
								if ((attrs != null) && (attrs.Length > 0))
								{
									if (!tpe.IsDefined(typeof(XpoDataLayerAttribute), true))
										result.Add(tpe);
									else
									{
										attrs = tpe.GetCustomAttributes(typeof(XpoDataLayerAttribute), true);
										if (attrs.FirstOrDefault(attr => ((XpoDataLayerAttribute)attr).DataLayerName == name) != null)
											result.Add(tpe);
									}
								}
							}
						}
					}
				}
			}
			return result.ToArray();
		}

#region Cloning

		public T[] CloneCollection<T>(CriteriaOperator sourceCriteria, SortProperty[] sortProperties,
			XpoDatabase target, bool synchronize = true,
			IEnumerable<XPClassInfo> excludedClasses = null, IEnumerable<string> synchronizeProperties = null)
			where T : IXPSimpleObject
		{
			if (target == null)
				throw new ArgumentNullException("target");
			List<T> result = new List<T>();
			using (Session sourceSession = this.GetSession())
			{
				XPCollection sourceList = new XPCollection(sourceSession, typeof(T), sourceCriteria, sortProperties);
				if (sourceList.Count > 0)
				{
					using (UnitOfWork targetSession = target.GetUnitOfWork())
					{
						Cloner c = new Cloner(sourceSession, targetSession, excludedClasses, synchronizeProperties);
						foreach (T sourceItem in sourceList)
						{
							result.Add(c.Clone<T>(sourceItem, synchronize));
						}
						targetSession.CommitChanges();
					}
				}
			}
			return result.ToArray();
		}

		public Task<T[]> CloneCollectionAsync<T>(CriteriaOperator sourceCriteria, SortProperty[] sortProperties,
			XpoDatabase target, bool synchronize = true,
			IEnumerable<XPClassInfo> excludedClasses = null, IEnumerable<string> synchronizeProperties = null)
			where T : IXPSimpleObject
		{
			return Task.FromResult(CloneCollection<T>(sourceCriteria, sortProperties, target,
				synchronize, excludedClasses, synchronizeProperties));
		}

		public T Clone<T>(T source, XpoDatabase target, bool synchronize = true,
			IEnumerable<XPClassInfo> excludedClasses = null, IEnumerable<string> synchronizeProperties = null)
			where T : IXPSimpleObject
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (target == null)
				throw new ArgumentNullException("target");

			using (Session sourceSession = this.GetSession())
			using (UnitOfWork targetSession = target.GetUnitOfWork())
			{
				Cloner c = new Cloner(sourceSession, targetSession, excludedClasses, synchronizeProperties);
				T result = c.Clone<T>(source, synchronize);
				targetSession.CommitChanges();
				return result;
			}
		}

		public virtual Task<T> CloneAsync<T>(T source, XpoDatabase target, bool synchronize = true,
			IEnumerable<XPClassInfo> excludedClasses = null, IEnumerable<string> synchronizeProperties = null)
			where T : IXPSimpleObject
		{
			//if (source == null)				
			//	 throw new ArgumentNullException("source");
			//if (target == null)
			//	throw new ArgumentNullException("targetSession");

			//T result;
			//using (Session sourceSession = this.GetSession())
			//using (UnitOfWork targetSession = target.GetUnitOfWork())
			//{
			//	Cloner c = new Cloner(sourceSession, targetSession, excludedClasses, synchronizeProperties);
			//	result = c.Clone<T>(source, synchronize);
			//}

			return Task.FromResult(Clone(source, target, synchronize, excludedClasses, synchronizeProperties));
		}

		class Cloner
		{
			/// <summary>
			/// A dictionary containing objects from the source session as key and objects from the 
			/// target session as values
			/// </summary>
			/// <returns></returns>
			Dictionary<object, object> clonedObjects = new Dictionary<object, object>();
			List<XPClassInfo> _excluded = new List<XPClassInfo>();
			List<String> _syncProps = new List<string>();
			Session _source = null;
			Session _target = null;

			/// <summary>
			/// Initializes a new instance of the CloneIXPSimpleObjectHelper class.
			/// </summary>
			public Cloner(Session source, Session target)
			{
				_source = source;
				_target = target;
			}

			public Cloner(Session source, Session target,
				IEnumerable<XPClassInfo> excludedclasses, IEnumerable<string> synchronizeproperties)
			{
				if (excludedclasses != null)
					_excluded.AddRange(excludedclasses);
				if (synchronizeproperties != null)
					_syncProps.AddRange(synchronizeproperties);

				_source = source;
				_target = target;
			}

			public T Clone<T>(T source) where T : IXPSimpleObject
			{
				return Clone<T>(source, _target, false);
			}
			public T Clone<T>(T source, bool synchronize) where T : IXPSimpleObject
			{
				return (T)Clone(source as IXPSimpleObject, _target, synchronize);
			}

			public object Clone(IXPSimpleObject source)
			{
				return Clone(source, _target, false);
			}
			public object Clone(IXPSimpleObject source, bool synchronize)
			{
				return Clone(source, _target, synchronize);
			}

			public T Clone<T>(T source, Session targetSession, bool synchronize) where T : IXPSimpleObject
			{
				return (T)Clone(source as IXPSimpleObject, targetSession, synchronize);
			}

			/// <summary>
			/// Clones and / or synchronizes the given IXPSimpleObject.
			/// </summary>
			/// <param name="source"></param>
			/// <param name="targetSession"></param>
			/// <param name="synchronize">If set to true, reference properties are only cloned in case
			/// the reference object does not exist in the targetsession. Otherwise the exising object will be
			/// reused and synchronized with the source. Set this property to false when knowing at forehand 
			/// that the targetSession will not contain any of the objects of the source.</param>
			/// <returns></returns>
			object Clone(IXPSimpleObject source, Session targetSession, bool synchronize)
			{
				if (source == null)
					return null;
				if (clonedObjects.ContainsKey(source))
					return clonedObjects[source];
				XPClassInfo targetClassInfo = targetSession.GetClassInfo(source.GetType());

				if (_excluded.Contains(targetClassInfo))
					return null;

				object clone;
				if (synchronize)
					clone = targetSession.GetObjectByKey(targetClassInfo, source.Session.GetKeyValue(source));
				else
					clone = null;

				if (clone == null)
					clone = targetClassInfo.CreateNewObject(targetSession);

				clonedObjects.Add(source, clone);

				foreach (XPMemberInfo m in targetClassInfo.PersistentProperties)
				{
					if (m is DevExpress.Xpo.Metadata.Helpers.ServiceField || m.IsKey)
						continue;
					object val;
					// makes sure when copying details entities in a master/detail relation, that the master is copied as well.
					if (m.ReferenceType != null)
					{
						object createdByClone = m.GetValue(clone);
						if ((createdByClone != null) && !synchronize)
							val = createdByClone;
						else if (_syncProps.Contains(m.MappingField))
						{
							object targetSource = targetSession.GetObjectByKey(targetClassInfo, source.Session.GetKeyValue(source));
							val = m.GetValue(targetSource);
						}
						else
						{
							val = Clone((IXPSimpleObject)m.GetValue(source), targetSession, synchronize);
						}
					}
					else
					{
						val = m.GetValue(source);
					}
					m.SetValue(clone, val);
				}
				foreach (XPMemberInfo m in targetClassInfo.CollectionProperties)
				{
					if (m.HasAttribute(typeof(AggregatedAttribute)))
					{
						XPBaseCollection col = (XPBaseCollection)m.GetValue(clone);
						XPBaseCollection colSource = (XPBaseCollection)m.GetValue(source);
						if (col != null)
						{
							foreach (IXPSimpleObject obj in new ArrayList(colSource))
								col.BaseAdd(Clone(obj, targetSession, synchronize));
						}
					}
				}
				return clone;
			}
		}

#endregion

#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).					
					//foreach (var key in dataLayers.Keys)
					//{
					//	IDataLayer item;
					//	if (dataLayers.TryRemove(key, out item)) { 
					//		item.Dispose();
					//		item = null;
					//	}
					//	else
					//	{
					//		throw new Exception($"Unable to remove datalayer {key} for destruction");
					//	}
					//}

				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~XpoDatabase() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
#endregion

	}
}

