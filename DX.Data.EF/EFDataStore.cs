using DX.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DX.Data;
using FluentValidation;
using FluentValidation.Results;

//using System.ComponentModel.DataAnnotations;




#if (NETCOREAPP)
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ChangeTracking;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#endif

namespace DX.Data.EF
{
    public abstract class EFDataStore<TEFContext, TKey, TModel, TDBModel> : IQueryableDataStore<TKey, TModel>
        where TEFContext : DbContext, new()
        where TKey : IEquatable<TKey>
        where TModel : class, new()
        where TDBModel : class, new()
    {
        public const string CtxMode = "datamode";
        public const string CtxStore = "datastore";

        public EFDataStore(TEFContext context, IValidator<TDBModel> validator)
        {
            //Mapper = mapper;
            DbContext = context;
            Validator = validator;
        }

        //protected IMapper Mapper { get; }
        public TEFContext DbContext { get; }
        public IValidator<TDBModel> Validator { get; }
        public virtual bool PaginateViaPrimaryKey { get => false; }
        public TModel CreateModel() => new TModel();
#if (NETCOREAPP)
        protected virtual TDBModel EFGetByKey(TKey key) => DbContext.Find<TDBModel>(key);
#else
        protected virtual TDBModel EFGetByKey(TKey key) => DbContext.Set<TDBModel>().Find(key);            
#endif

        /// <summary>
        /// Loads an entity for read-only use. On EF Core this is an AsNoTracking query, so reads do not grow
        /// the change tracker; it falls back to <see cref="EFGetByKey"/> when that is overridden, when the
        /// primary key is not a single <typeparamref name="TKey"/> property, and on EF6.
        /// </summary>
        protected virtual TDBModel EFGetByKeyNoTracking(TKey key)
        {
#if (NETCOREAPP)
            if (key != null)
            {
                var predicate = KeyPredicate(new[] { key });
                if (predicate != null)
                    return DbContext.Set<TDBModel>().AsNoTracking().FirstOrDefault(predicate);
            }
#endif
            return EFGetByKey(key);
        }

        /// <summary>
        /// Loads the tracked entities for a set of keys, keyed by <see cref="DBModelKey"/>; missing keys are absent.
        /// On EF Core several keys are loaded with one query per 64 keys; otherwise (EF6, an overridden
        /// <see cref="EFGetByKey"/>, a composite or differently typed key) it calls <see cref="EFGetByKey"/> per key.
        /// </summary>
        protected async virtual Task<IDictionary<TKey, TDBModel>> EFGetByKeysAsync(IEnumerable<TKey> keys)
        {
            var distinctKeys = keys.Where(k => k != null).Distinct().ToList();
            var result = new Dictionary<TKey, TDBModel>(distinctKeys.Count);
#if (NETCOREAPP)
            if (distinctKeys.Count > 1 && KeyPredicate(distinctKeys.Take(1).ToList()) != null)
            {
                for (int offset = 0; offset < distinctKeys.Count; offset += KeyBatchSize)
                {
                    var chunk = distinctKeys.Skip(offset).Take(KeyBatchSize).ToList();
                    // Pad to a power of two so only a handful of distinct SQL shapes reach the query cache.
                    int padded = 1;
                    while (padded < chunk.Count) padded <<= 1;
                    while (chunk.Count < padded) chunk.Add(chunk[chunk.Count - 1]);

                    var entities = await DbContext.Set<TDBModel>().Where(KeyPredicate(chunk)).ToListAsync();
                    foreach (var entity in entities)
                        result[DBModelKey(entity)] = entity;
                }
                // Keys the query did not match exactly (e.g. string keys under a case-insensitive collation)
                // get the same per-key lookup as before, so the outcome never differs from EFGetByKey.
                foreach (var key in distinctKeys.Where(k => !result.ContainsKey(k)).ToList())
                {
                    var entity = EFGetByKey(key);
                    if (entity != null)
                        result[key] = entity;
                }
                return result;
            }
#endif
            foreach (var key in distinctKeys)
            {
                var entity = EFGetByKey(key);
                if (entity != null)
                    result[key] = entity;
            }
            return await Task.FromResult<IDictionary<TKey, TDBModel>>(result);
        }

#if (NETCOREAPP)
        const int KeyBatchSize = 64;
        static readonly ConcurrentDictionary<Type, bool> getByKeyOverridden = new ConcurrentDictionary<Type, bool>();

        sealed class KeyHolder
        {
            public KeyHolder(TKey value) { Value = value; }
            public TKey Value { get; }
        }

        /// <summary>
        /// Builds e => e.Key == k1 || e.Key == k2 ... with every key as a SQL parameter, or returns null when
        /// a key query cannot replace <see cref="EFGetByKey"/>.
        /// </summary>
        Expression<Func<TDBModel, bool>> KeyPredicate(IList<TKey> keys)
        {
            var overridden = getByKeyOverridden.GetOrAdd(GetType(), t =>
            {
                var method = t.GetMethod(nameof(EFGetByKey), BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(TKey) }, null);
                return method == null || method.DeclaringType != typeof(EFDataStore<TEFContext, TKey, TModel, TDBModel>);
            });
            if (overridden)
                return null;

            var primaryKey = DbContext.Model.FindEntityType(typeof(TDBModel))?.FindPrimaryKey();
            if (primaryKey == null || primaryKey.Properties.Count != 1 || primaryKey.Properties[0].ClrType != typeof(TKey))
                return null;

            try
            {
                var e = Expression.Parameter(typeof(TDBModel), "e");
                var property = Expression.Call(typeof(Microsoft.EntityFrameworkCore.EF), nameof(Microsoft.EntityFrameworkCore.EF.Property), new[] { typeof(TKey) },
                    e, Expression.Constant(primaryKey.Properties[0].Name));
                Expression body = null;
                foreach (var key in keys)
                {
                    var value = Expression.Property(Expression.Constant(new KeyHolder(key)), nameof(KeyHolder.Value));
                    var equal = Expression.Equal(property, value);
                    body = body == null ? equal : Expression.OrElse(body, equal);
                }
                return Expression.Lambda<Func<TDBModel, bool>>(body, e);
            }
            catch (InvalidOperationException)
            {
                return null; // TKey has no equality operator
            }
        }
#endif

        protected virtual IQueryable<TDBModel> EFQuery() => DbContext.Set<TDBModel>();

		public virtual IQueryable<TModel> Query() => Query<TModel>();		
        public abstract IQueryable<T> Query<T>() where T : class, new();

        public abstract TModel GetByKey(TKey key);
        protected abstract TDBModel ToDBModel(TModel source, TDBModel target);
        protected abstract TModel ToModel(TDBModel source, TModel target);
        public abstract string KeyField { get; }
        public abstract void SetModelKey(TModel model, TKey key);
        public void Test()
        {
            var tran = DbContext.Database.BeginTransaction();
            tran.Commit();

        }

        public abstract TKey ModelKey(TModel model);

        protected abstract TKey DBModelKey(TDBModel model);

#if (NETCOREAPP)
        protected async virtual Task<T> TransactionalExecAsync<T>(
            Func<EFDataStore<TEFContext, TKey, TModel, TDBModel>,
            IDbContextTransaction, Task<T>> work,
            bool autoCommit = true)
        {
            T result = default!;

            using (var dbTrans = await DbContext.Database.BeginTransactionAsync())
            {
                result = await work(this, dbTrans);
                // Commit whenever the work succeeded, also when it already saved its own changes.
                if (autoCommit)
                {
                    await DbContext.SaveChangesAsync();
                    await dbTrans.CommitAsync();
                }
            }
            return result;
        }
        protected async virtual Task TransactionalExecAsync<T>(
            Func<EFDataStore<TEFContext, TKey, TModel, TDBModel>,
            IDbContextTransaction, Task> work, bool autoCommit = true)
        {
            using (var dbTrans = await DbContext.Database.BeginTransactionAsync())
            {
                await work(this, dbTrans);
                // Commit whenever the work succeeded, also when it already saved its own changes.
                if (autoCommit)
                {
                    await DbContext.SaveChangesAsync();
                    await dbTrans.CommitAsync();
                }
            }
        }
#else
        protected async virtual Task<T> TransactionalExecAsync<T>(
            Func<EFDataStore<TEFContext, TKey, TModel, TDBModel>,
            DbContextTransaction, Task<T>> work,
            bool autoCommit = true)
        {
            T result = default!;

            using (var dbTrans = DbContext.Database.BeginTransaction())
            {
                result = await work(this, dbTrans);
                // Commit whenever the work succeeded, also when it already saved its own changes.
                if (autoCommit)
                {
                    await DbContext.SaveChangesAsync();
                    dbTrans.Commit();
                }
            }
            return result;
        }
        protected async virtual Task TransactionalExecAsync<T>(
            Func<EFDataStore<TEFContext, TKey, TModel, TDBModel>,
            DbContextTransaction, Task> work, bool autoCommit = true)
        {
            using (var dbTrans = DbContext.Database.BeginTransaction())
            {
                await work(this, dbTrans);
                // Commit whenever the work succeeded, also when it already saved its own changes.
                if (autoCommit)
                {
                    await DbContext.SaveChangesAsync();
                    dbTrans.Commit();
                }
            }
        }


#endif
        protected async Task<ValidationResult> ValidateDBModelAsync(TDBModel item,
            DataMode mode,
            EFDataStore<TEFContext, TKey, TModel, TDBModel> store)
        {
            var validationContext = new ValidationContext<TDBModel>(item);
            validationContext.RootContextData[CtxMode] = mode;
            validationContext.RootContextData[CtxStore] = store;

            var result = await Validator.ValidateAsync(validationContext);
            return result;
        }

        class InsertHelper
        {
            public InsertHelper(TModel model, ValidationResult insertingResult, IDataResult<TKey, TModel> insertedResult)
            {
                Model = model;
                InsertingResult = insertingResult;
                InsertedResult = insertedResult;
            }
            public TModel Model { get; private set; }
            public ValidationResult InsertingResult { get; private set; }
            public IDataResult<TKey, TModel> InsertedResult { get; private set; }
        }
        protected virtual ValidationException CreateValidationException(TModel model, ValidationResult validationResult)
        {
            var err = new ValidationException("Validation Failed", validationResult.Errors);            
            return err;
        }
        //===============================
        protected enum StoreMode { Create, Update, Store }
        protected TKey EmptyKeyValue => default!;

        bool IsEmptyKey(TKey key) => key == null || key.Equals(EmptyKeyValue);

        static IDictionary<TKey, TDBModel> SingleOrEmpty(TKey key, TDBModel entity)
        {
            var result = new Dictionary<TKey, TDBModel>(1);
            if (entity != null)
                result[key] = entity;
            return result;
        }

        /// <summary>
        /// Updated entities are no longer forced to Modified (which rewrote every column); when automatic change
        /// detection is switched off, detect their changes explicitly so they are still saved.
        /// </summary>
        void DetectChangesIfDisabled(IEnumerable<TDBModel> updated)
        {
#if (NETCOREAPP)
            if (!DbContext.ChangeTracker.AutoDetectChangesEnabled)
                foreach (var entity in updated)
                    DbContext.Entry(entity).DetectChanges();
#else
            if (!DbContext.Configuration.AutoDetectChangesEnabled && updated.Any())
                DbContext.ChangeTracker.DetectChanges();
#endif
        }

        /// <summary>One item of a batch, with the entity it maps to and the operation applied to it.</summary>
        class PendingItem
        {
            public PendingItem(TModel item, TDBModel entity, DataMode mode)
            {
                Item = item;
                Entity = entity;
                Mode = mode;
            }
            public TModel Item { get; }
            public TDBModel Entity { get; }
            public DataMode Mode { get; }
        }

        /// <summary>
        /// Stops tracking the entities touched by a failed operation, so their (rolled back) changes
        /// are not retried by the next SaveChanges on the same DbContext.
        /// </summary>
        void DetachEntities(IEnumerable<TDBModel> entities)
        {
            foreach (var entity in entities)
            {
                var entry = DbContext.Entry(entity);
                if (entry.State != EntityState.Detached)
                    entry.State = EntityState.Detached;
            }
        }

        /// <summary>Returns the pending item whose entity caused a DbUpdateException, if it can be determined.</summary>
        static PendingItem FindFailedItem(Exception err, List<PendingItem> pending)
        {
            var updateException = err as DbUpdateException;
            if (updateException == null || updateException.Entries == null)
                return null;
            var failed = updateException.Entries
                .Select(e => pending.FirstOrDefault(p => ReferenceEquals(p.Entity, e.Entity)))
                .Where(p => p != null)
                .Distinct()
                .ToList();
            return failed.Count == 1 ? failed[0] : null;
        }

#if (NETCOREAPP)
        static async Task RollbackQuietlyAsync(IDbContextTransaction transaction)
        {
            try { await transaction.RollbackAsync(); }
            catch { /* keep the original error; disposing the transaction ends it anyway */ }
        }
#else
        static Task RollbackQuietlyAsync(DbContextTransaction transaction)
        {
            try { transaction.Rollback(); }
            catch { /* keep the original error; disposing the transaction ends it anyway */ }
            return Task.CompletedTask;
        }
#endif

        protected async virtual Task<IDataResult<TKey, TModel>> StoreAsync(StoreMode mode, params TModel[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var defaultMode = mode == StoreMode.Update ? DataMode.Update : DataMode.Create;
            if (items.Length == 0)
                return new DataResult<TKey, TModel> { Success = true, Mode = defaultMode, Data = new TModel[0] };

            var result = await TransactionalExecAsync(async (s, wrk) =>
            {
                TModel currentItem = default!;
                DataMode dataMode = defaultMode;
                var pending = new List<PendingItem>(items.Length);
                try
                {
                    // Load all entities to update up front (one query on EF Core instead of one per item).
                    var updateKeys = mode == StoreMode.Create ? new List<TKey>() : items.Select(ModelKey).Where(k => !IsEmptyKey(k)).ToList();
                    IDictionary<TKey, TDBModel> existing =
                        updateKeys.Count == 0 ? new Dictionary<TKey, TDBModel>()
                        : updateKeys.Count == 1 ? SingleOrEmpty(updateKeys[0], EFGetByKey(updateKeys[0]))
                        : await EFGetByKeysAsync(updateKeys);

                    foreach (var item in items)
                    {
                        currentItem = item;
                        var modelKey = ModelKey(item);
                        var emptyKey = IsEmptyKey(modelKey);
                        if (mode == StoreMode.Create || (emptyKey && mode == StoreMode.Store))
                        {
                            dataMode = DataMode.Create;
                            var newDBItem = new TDBModel();
                            ToDBModel(item, newDBItem);
                            var validationResult = await ValidateDBModelAsync(newDBItem, DataMode.Create, s);
                            if (!validationResult.IsValid)
                                throw CreateValidationException(item, validationResult);
                            pending.Add(new PendingItem(item, newDBItem, DataMode.Create));
#if (NETCOREAPP)
                            await DbContext.Set<TDBModel>().AddAsync(newDBItem);
#else
                            DbContext.Set<TDBModel>().Add(newDBItem);
#endif
                        }
                        else
                        {
                            dataMode = DataMode.Update;
                            if (emptyKey)
                                throw new InvalidOperationException($"Unable to update {typeof(TDBModel).Name}: the item has no key value");

                            TDBModel dbModel;
                            if (!existing.TryGetValue(modelKey, out dbModel) || dbModel == null)
                                throw new Exception($"Unable to locate {typeof(TDBModel).Name}({modelKey}) in datastore");

                            // The entity is normally tracked from the load, so EF's change detection updates only
                            // the columns that really changed. An untracked one (custom EFGetByKey) is attached first.
                            if (DbContext.Entry(dbModel).State == EntityState.Detached)
                                DbContext.Set<TDBModel>().Attach(dbModel);

                            // Registered before mutating, so a rejected item is detached (reverted) in the catch below.
                            pending.Add(new PendingItem(item, dbModel, DataMode.Update));
                            ToDBModel(item, dbModel);

                            var validationResult = await ValidateDBModelAsync(dbModel, DataMode.Update, s);
                            if (!validationResult.IsValid)
                                throw CreateValidationException(item, validationResult);
                        }
                    }

                    DetectChangesIfDisabled(pending.Where(p => p.Mode == DataMode.Update).Select(p => p.Entity));

                    // One SaveChanges and one commit for the whole batch.
                    await DbContext.SaveChangesAsync();
                    foreach (var p in pending.Where(p => p.Mode == DataMode.Create))
                    {
#if (NETCOREAPP)
                        ToModel(p.Entity, p.Item); //reload changes in item
#else
                        SetModelKey(p.Item, DBModelKey(p.Entity));
#endif
                    }
#if (NETCOREAPP)
                    await wrk.CommitAsync();
#else
                    wrk.Commit();
#endif
                    return new DataResult<TKey, TModel> { Success = true, Mode = dataMode, Data = items.ToArray() };
                }
                catch (Exception err)
                {
                    await RollbackQuietlyAsync(wrk);
                    DetachEntities(pending.Select(p => p.Entity));
                    var failed = FindFailedItem(err, pending);
                    if (failed != null)
                    {
                        currentItem = failed.Item;
                        dataMode = failed.Mode;
                    }
                    return new DataResult<TKey, TModel>(dataMode, typeof(TDBModel).Name, err) { Data = new[] { currentItem } };
                }
            }, false);
            return result;
        }

        public async virtual Task<IDataResult<TKey, TModel>> StoreAsync(params TModel[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            return await StoreAsync(StoreMode.Store, items);
        }

        public async virtual Task<IDataResult<TKey, TModel>> CreateAsync(params TModel[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            return await StoreAsync(StoreMode.Create, items);
        }

        public async virtual Task<IDataResult<TKey, TModel>> UpdateAsync(params TModel[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            return await StoreAsync(StoreMode.Update, items);
        }
        public async virtual Task<IDataResult<TKey, TModel>> DeleteAsync(params TModel[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));            
            return await DeleteAsync(items.Select(i => ModelKey(i)).ToArray());
        }

        public async virtual Task<IDataResult<TKey, TModel>> DeleteAsync(params TKey[] ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));
            if (ids.Length == 0)
                return new DataResult<TKey, TModel> { Success = true, Mode = DataMode.Delete };

            var result = await TransactionalExecAsync(async (s, t) =>
            {
                var deleted = new List<TDBModel>();
                try
                {
                    var keys = ids.Where(id => id != null).ToList();
                    IDictionary<TKey, TDBModel> existing = keys.Count == 1
                        ? SingleOrEmpty(keys[0], EFGetByKey(keys[0]))
                        : await EFGetByKeysAsync(keys);
                    foreach (var id in keys)
                    {
                        TDBModel dbModel;
                        if (existing.TryGetValue(id, out dbModel) && dbModel != null && !deleted.Any(d => ReferenceEquals(d, dbModel)))
                        {
                            var validationResult = await ValidateDBModelAsync(dbModel, DataMode.Delete, s);
                            if (!validationResult.IsValid)
                                throw new ValidationException(validationResult.Errors);

                            deleted.Add(dbModel);
                            DbContext.Entry(dbModel).State = EntityState.Deleted;
                        }
                    }

                    // One SaveChanges and one commit for the whole batch.
                    await DbContext.SaveChangesAsync();
#if (NETCOREAPP)
                    await t.CommitAsync();
#else
                    t.Commit();
#endif
                    return new DataResult<TKey, TModel> { Success = true, Mode = DataMode.Delete };
                }
                catch (Exception err)
                {
                    await RollbackQuietlyAsync(t);
                    DetachEntities(deleted);
                    return new DataResult<TKey, TModel>(DataMode.Delete, typeof(TDBModel).Name, err);
                }
            }, false);
            return result;
        }
	}
}
