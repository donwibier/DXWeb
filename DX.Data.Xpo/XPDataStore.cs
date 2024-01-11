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

        //public XPDataStore(string connectionName, IValidator<TDBModel> validator)
        //{
            
        //}
        

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
        protected async virtual Task TransactionalExecAsync<T>(
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

        class InsertHelper
        {
            public InsertHelper(TModel model, ValidationResult insertingResult, IDataResult<TKey> insertedResult)
            {
                Model = model;
                InsertingResult = insertingResult;
                InsertedResult = insertedResult;
            }
            public TModel Model { get; private set; }
            public ValidationResult InsertingResult { get; private set; }
            public IDataResult<TKey> InsertedResult { get; private set; }
        }

        protected enum StoreMode { Create, Update, Store }
        protected TKey EmptyKeyValue => default!;

        protected async virtual Task<IDataResult<TKey>> StoreAsync(StoreMode mode, params TModel[] items)
        {
            ThrowIfNull(items);

            var result = await TransactionalExecAsync(
                async (s, wrk) =>
                {
                    // need to keep the xpo entities together with the model items so we can update 
                    // the id's of the models afterwards.
                    Dictionary<TDBModel, InsertHelper> batchPairs = new Dictionary<TDBModel, InsertHelper>();
                    try
                    {
                        ValidationResult validationResult = default!;
                        DataMode dataMode = DataMode.Create;
                        foreach (var item in items)
                        {
                            var modelKey = ModelKey(item);
                            TDBModel dbItem = null!;
                            if (modelKey == null || modelKey.Equals(EmptyKeyValue) || mode == StoreMode.Create)
                            {
                                dataMode = DataMode.Create;
                                dbItem = (Activator.CreateInstance(typeof(TDBModel), wrk) as TDBModel)!;
                                batchPairs.Add(dbItem, new InsertHelper(item, validationResult, new DataResult<TKey>()));
                            }
                            else if (!modelKey.Equals(EmptyKeyValue) && (mode != StoreMode.Create))
                            {
                                dataMode = DataMode.Update;
                                dbItem = XPOGetByKey(modelKey, wrk)!;
                                if (dbItem == null)
                                    throw new ValidationException($"Unable to locate {typeof(TDBModel).Name}({modelKey}) in datastore");
                            }
                            //Mapper.Map(item, dbItem);
                            ToDBModel(item, dbItem);
                            validationResult = await ValidateDBModelAsync(dbItem, dataMode, s);
                            if (!validationResult.IsValid)
                                throw new ValidationException(validationResult.Errors);
                            await wrk.SaveAsync(dbItem);
                        }


                        wrk.ObjectSaved += (s, e) =>
                        {
                            // sync the model ids with the newly generated xpo id's
                            if (e.Object is TDBModel xpoItem && batchPairs.ContainsKey(xpoItem))
                            {
                                TKey k = (TKey)xpoItem.Session.GetKeyValue(xpoItem);
                                SetModelKey(batchPairs[xpoItem].Model, k);
                                if (batchPairs[xpoItem].InsertedResult != null)
                                    batchPairs[xpoItem].InsertedResult.Key = k;
                                // INFO: We need to map back the XPO Item into the model.							
                                ToModel(xpoItem, batchPairs[xpoItem].Model);                                
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
                            return new DataResult<TKey> { Success = false, Mode = dataMode, Exception = commitFailure };
                        else
                            return new DataResult<TKey> { Success = true, Mode = dataMode };
                    }
                    catch (Exception err)
                    {
                        wrk.RollbackTransaction();
                        return new DataResult<TKey>(DataMode.Create, nameof(TDBModel), err);
                    }
                },
                true, false);
            return result;
        }

        public async virtual Task<IDataResult<TKey>> StoreAsync(params TModel[] items)
        {
            ThrowIfNull(items);
            return await StoreAsync(StoreMode.Store, items);

        }

        public async virtual Task<IDataResult<TKey>> CreateAsync(params TModel[] items)
        {
            ThrowIfNull(items);
            return await StoreAsync(StoreMode.Create, items);
        }

        public async virtual Task<IDataResult<TKey>> UpdateAsync(params TModel[] items)
        {
            ThrowIfNull(items);
            return await StoreAsync(StoreMode.Update, items);
        }
        public async virtual Task<IDataResult<TKey>> DeleteAsync(params TModel[] items)
        {
            ThrowIfNull(items);
            return await DeleteAsync(items.Select(i => ModelKey(i)).ToArray());
        }

        public async virtual Task<IDataResult<TKey>> DeleteAsync(params TKey[] ids)
        {
            ThrowIfNull(ids);

            var result = await TransactionalExecAsync(async (s, wrk) =>
            {
                try
                {
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
                        return new DataResult<TKey> { Success = false, Mode = DataMode.Delete, Exception = commitFailure };
                    else
                        return new DataResult<TKey> { Success = true, Mode = DataMode.Delete };
                }
                catch (ValidationException err)
                {
                    wrk.RollbackTransaction();
                    return new DataResult<TKey>(DataMode.Delete, nameof(TDBModel), err);
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
