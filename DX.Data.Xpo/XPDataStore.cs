using DevExpress.Entity.Model.Metadata;
using DevExpress.Xpo;
using DX.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DX.Data;
using FluentValidation;
using FluentValidation.Results;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DevExpress.Data.Filtering;
using System.Threading;

namespace DX.Data.Xpo
{


    public abstract class XPDataStore<TKey, TModel, TDBModel> : IQueryableDataStore<TKey, TModel>, IDisposable
        where TKey : IEquatable<TKey>
        where TModel : class, new()
        where TDBModel : class, IXPSimpleObject
    {
        public const string CtxMode = "datamode";
        public const string CtxStore = "datastore";

        public XPDataStore(IDataLayer dataLayer, IValidator<TDBModel> validator)
        {
            
            DataLayer = dataLayer;
            Validator = validator;
        }

        private UnitOfWork _uow = default!;
        private bool _disposed;
        protected bool IsUOWDisposed { get => _uow == default; }
        protected UnitOfWork UnitOfWork
        {
            get
            {
                if (_uow == default)
                    _uow = GetNewUnitOfWork();

                return _uow;
            }
        }
        protected UnitOfWork GetNewUnitOfWork()
        {
            var result = new UnitOfWork(DataLayer);
			result.Disposed += UoW_Disposed;
            return result;
        }

		private void UoW_Disposed(object? sender, EventArgs e) => _uow = default!;

		public IDataLayer DataLayer { get; }
        public IValidator<TDBModel> Validator { get; }
        public virtual bool PaginateViaPrimaryKey { get => false; }

        public TModel CreateModel() => (Activator.CreateInstance(typeof(TModel)) as TModel)!;
        protected virtual TDBModel XPOGetByKey(TKey key, Session session) => session.GetObjectByKey<TDBModel>(key);

        protected virtual IQueryable<TDBModel> DBQuery(Session uow) => uow.Query<TDBModel>();
        
        public virtual IQueryable<TModel> Query() => Query<TModel>();

		public abstract IQueryable<T> Query<T>() where T : class, new();

        public abstract TModel GetByKey(TKey key);
        
        protected virtual TDBModel ToDBModel(TModel source, TDBModel target) => MapTo(source, target);
        protected virtual TModel ToModel(TDBModel source, TModel target) => MapTo(source, target);
        protected abstract TDestination MapTo<TSource, TDestination>(TSource source, TDestination target)
            where TSource : class
            where TDestination : class;

		protected async Task<List<TModel>> FindAsync(CriteriaOperator filter)
		{            
			ThrowIfDisposed();
			ThrowIfNull(filter);

			var result = await TransactionalExecAsync(async (s, w) =>
			{
                await Task.Delay(0);
				var r = new XPCollection<TDBModel>(w, filter);                
				return r.Select(u => ToModel(u, new TModel())).ToList();
			});
			return result;
		}


		protected async virtual Task<T> TransactionalExecAsync<T>(
            Func<XPDataStore<TKey, TModel, TDBModel>, Session, Task<T>> work,
            bool transactional = true, bool autoCommit = true)
        {
            T result = default!;
            using (var wrk = transactional ? new UnitOfWork(DataLayer) : new Session(DataLayer))
            {
                if (transactional)
                    wrk.BeginTransaction();

                result = await work(this, wrk);

                if (autoCommit && transactional)
                    await wrk.CommitTransactionAsync();
            }
            return result;
        }
        protected async virtual Task TransactionalExecAsync(
            Func<XPDataStore<TKey, TModel, TDBModel>, Session, Task> work,
            bool transactional = true, bool autoCommit = true)
        {
            using (var wrk = transactional ? new UnitOfWork(DataLayer) : new Session(DataLayer))
            {
                if (transactional)
                    wrk.BeginTransaction();

                await work(this, wrk);

                if (autoCommit && transactional)
                    await wrk.CommitTransactionAsync();
            }
            
        }
        public abstract string KeyField { get; }

        public abstract void SetModelKey(TModel model, TKey key);
        public abstract TKey ModelKey(TModel model);
        protected abstract TKey DBModelKey(TDBModel model);

        protected async virtual Task<ValidationResult> ValidateDBModelAsync(TDBModel item,
        DataMode mode,
            XPDataStore<TKey, TModel, TDBModel> store)
        {            
            var validationContext = new ValidationContext<TDBModel>(item);
            validationContext.RootContextData[CtxMode] = mode;
            validationContext.RootContextData[CtxStore] = store;

            var result = await Validator.ValidateAsync(validationContext);
            return result;
        }

        protected virtual ValidationException CreateValidationException(TModel model, ValidationResult validationResult)
        {
            var err = new ValidationException("Validation Failed", validationResult.Errors);            
            return err;
        }

        protected enum StoreMode { Create, Update, Store }
        protected TKey EmptyKeyValue => default!;

        private bool IsEmptyKey(TKey key) => key == null || key.Equals(EmptyKeyValue);

        // public TDBModel(Session session) constructor, compiled once per TDBModel; null when there is none
        // (CreateDBModel then falls back to Activator so the exception is the same as before)
        private static readonly Lazy<Func<Session, TDBModel>?> dbModelFactory = new Lazy<Func<Session, TDBModel>?>(() =>
        {
            var ctor = typeof(TDBModel).GetConstructor(new[] { typeof(Session) });
            if (ctor == null || typeof(TDBModel).IsAbstract)
                return null;
            var p = System.Linq.Expressions.Expression.Parameter(typeof(Session), "session");
            return System.Linq.Expressions.Expression.Lambda<Func<Session, TDBModel>>(
                System.Linq.Expressions.Expression.New(ctor, p), p).Compile();
        });

        private static TDBModel CreateDBModel(Session session)
        {
            var factory = dbModelFactory.Value;
            if (factory != null)
                return factory(session);
            return (Activator.CreateInstance(typeof(TDBModel), session) as TDBModel)!;
        }

        private bool? _usesDefaultXPOGetByKey;

        /// <summary>
        /// Loads the objects with the given keys into the session with one round-trip, so the XPOGetByKey calls
        /// that follow are served from the session's identity map. Only done when XPOGetByKey is not overridden,
        /// because an override may interpret the keys differently.
        /// </summary>
        protected virtual void XPOPrefetchByKeys(IEnumerable<TKey> keys, Session session)
        {
            if (!UsesDefaultXPOGetByKey())
                return;
            var list = new List<object>();
            var seen = new HashSet<TKey>();
            foreach (var key in keys)
            {
                if (!IsEmptyKey(key) && seen.Add(key))
                    list.Add(key!);
            }
            if (list.Count > 1)
                session.GetObjectsByKey(session.GetClassInfo<TDBModel>(), list, false);
        }

        private bool UsesDefaultXPOGetByKey()
        {
            if (_usesDefaultXPOGetByKey == null)
            {
                var m = GetType().GetMethod(nameof(XPOGetByKey), BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new[] { typeof(TKey), typeof(Session) }, null);
                _usesDefaultXPOGetByKey = m != null && m.DeclaringType == typeof(XPDataStore<TKey, TModel, TDBModel>);
            }
            return _usesDefaultXPOGetByKey.Value;
        }

        protected async virtual Task<IDataResult<TKey, TModel>> StoreAsync(StoreMode mode, params TModel[] items)
        {
            ThrowIfNull(items);

            var result = await TransactionalExecAsync(
                async (s, wrk) =>
                {
                    TModel currentItem = default!;
                    DataMode dataMode = mode == StoreMode.Update ? DataMode.Update : DataMode.Create;
                    // need to keep the xpo entities together with the model items so we can update 
                    // the id's (created items) and server side changes (all items) of the models afterwards.
                    // An updated db object can occur more than once in one batch.
                    var batchPairs = new Dictionary<TDBModel, List<TModel>>();
                    try
                    {
                        if (mode != StoreMode.Create)
                            XPOPrefetchByKeys(items.Where(i => i != null).Select(i => ModelKey(i)), wrk);

                        foreach (var item in items)
                        {
                            currentItem = item;
                            var modelKey = ModelKey(item);
                            TDBModel dbItem = null!;
                            if (mode == StoreMode.Create || (mode == StoreMode.Store && IsEmptyKey(modelKey)))
                            {
                                dataMode = DataMode.Create;
                                dbItem = CreateDBModel(wrk);
                            }
                            else
                            {
                                dataMode = DataMode.Update;
                                dbItem = IsEmptyKey(modelKey) ? null! : XPOGetByKey(modelKey, wrk)!;
                                if (dbItem == null)
                                    throw new ValidationException($"Unable to locate {typeof(TDBModel).Name}({modelKey}) in datastore");
                            }
                            if (!batchPairs.TryGetValue(dbItem, out var models))
                                batchPairs.Add(dbItem, models = new List<TModel>(1));
                            models.Add(item);

                            ToDBModel(item, dbItem);
                            var validationResult = await ValidateDBModelAsync(dbItem, dataMode, s);
                            if (!validationResult.IsValid)
                                throw CreateValidationException(item, validationResult);
                            await wrk.SaveAsync(dbItem);
                        }


                        wrk.ObjectSaved += (s, e) =>
                        {
                            // sync the model ids with the newly generated xpo id's and
                            // map back the XPO Item into the model (OnSaving etc.)
                            if (e.Object is TDBModel xpoItem && batchPairs.TryGetValue(xpoItem, out var models))
                            {
                                TKey k = (TKey)xpoItem.Session.GetKeyValue(xpoItem);
                                foreach (var model in models)
                                {
                                    SetModelKey(model, k);
                                    ToModel(xpoItem, model);
                                }
                            }
                        };
                        ValidationException commitFailure = null!;
                        wrk.FailedCommitTransaction += (s, e) =>
                        {
                            commitFailure = new ValidationException(e.Exception.InnerException != null ? e.Exception.InnerException.Message : e.Exception.Message);
                            e.Handled = true;
                        };
                        await wrk.CommitTransactionAsync();
                        if (commitFailure != null)
                            return new DataResult<TKey, TModel> { 
                                Success = false, 
                                Mode = dataMode, 
                                Exception = commitFailure ,
                                Data = new[] { currentItem } 
                            };
                        else
                            return new DataResult<TKey, TModel> { 
                                Success = true, 
                                Mode = dataMode,
                                Data = items.ToArray()
                            };
                    }
                    catch (Exception err)
                    {
                        wrk.RollbackTransaction();
                        return new DataResult<TKey, TModel>(dataMode, typeof(TDBModel).Name, err) { Data = new[] { currentItem } };
                    }
                },
                true, false);
            return result;
        }

        public async virtual Task<IDataResult<TKey, TModel>> StoreAsync(params TModel[] items)
        {
            ThrowIfNull(items);
            return await StoreAsync(StoreMode.Store, items);

        }

        public async virtual Task<IDataResult<TKey, TModel>> CreateAsync(params TModel[] items)
        {
            ThrowIfNull(items);
            return await StoreAsync(StoreMode.Create, items);
        }

        public async virtual Task<IDataResult<TKey, TModel>> UpdateAsync(params TModel[] items)
        {
            ThrowIfNull(items);
            return await StoreAsync(StoreMode.Update, items);
        }
        public async virtual Task<IDataResult<TKey, TModel>> DeleteAsync(params TModel[] items)
        {
            ThrowIfNull(items);
            return await DeleteAsync(items.Select(i => ModelKey(i)).ToArray());
        }

        public async virtual Task<IDataResult<TKey, TModel>> DeleteAsync(params TKey[] ids)
        {
            ThrowIfNull(ids);

            var result = await TransactionalExecAsync(async (s, wrk) =>
            {
                try
                {
                    XPOPrefetchByKeys(ids, wrk);
                    foreach (var id in ids)
                    {
                        var dbModel = XPOGetByKey(id, wrk);
                        if (dbModel != null)
                        {
                            var validationResult = await ValidateDBModelAsync(dbModel, DataMode.Delete, s);
                            if (!validationResult.IsValid)
                                throw new ValidationException(validationResult.Errors);

                            await wrk.DeleteAsync(dbModel);
                        }
                    }
                    ValidationException commitFailure = null!;
                    wrk.FailedCommitTransaction += (s, e) =>
                    {
                        commitFailure = new ValidationException(e.Exception.InnerException != null ? e.Exception.InnerException.Message : e.Exception.Message);
                        e.Handled = true;
                    };

                    await wrk.CommitTransactionAsync();
                    if (commitFailure != null)
                        return new DataResult<TKey, TModel> { Success = false, Mode = DataMode.Delete, Exception = commitFailure };
                    else
                        return new DataResult<TKey, TModel> { Success = true, Mode = DataMode.Delete };
                }
                catch (Exception err)
                {
                    wrk.RollbackTransaction();
                    return new DataResult<TKey, TModel>(DataMode.Delete, typeof(TDBModel).Name, err);
                }
            }, true, false);
            return result;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected void ThrowIfNull(object? obj)
        {
#if (!NETCOREAPP)
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
#else
            ArgumentNullException.ThrowIfNull (obj);
#endif
        }

        protected void ThrowIfNullOrEmpty(string? value, string? name = null)
        {
            if (value == null || string.IsNullOrEmpty(value.Trim()))
                throw new ArgumentNullException(name ?? nameof(value));
        }

		protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (_uow != default)
                        _uow.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
