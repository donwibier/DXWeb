﻿using DX.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
                if (autoCommit && DbContext.ChangeTracker.HasChanges())
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
                if (autoCommit && DbContext.ChangeTracker.HasChanges())
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
                if (autoCommit && DbContext.ChangeTracker.HasChanges())
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
                if (autoCommit && DbContext.ChangeTracker.HasChanges())
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

        protected async virtual Task<IDataResult<TKey, TModel>> StoreAsync(StoreMode mode, params TModel[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var result = await TransactionalExecAsync(async (s, wrk) =>
            {
                TModel currentItem = default!;
                try
                {

                    ValidationResult validationResult = default!;
                    DataMode dataMode = DataMode.Create;
                    foreach (var item in items)
                    {
                        currentItem = item;
                        var modelKey = ModelKey(item);                            
                        if (modelKey == null || modelKey.Equals(EmptyKeyValue) || mode == StoreMode.Create)
                        {
                            dataMode = DataMode.Create;
                            var newDBItem = new TDBModel();
                            ToDBModel(item, newDBItem);
                            validationResult = await ValidateDBModelAsync(newDBItem, DataMode.Create, s);
                            if (!validationResult.IsValid)
                                throw CreateValidationException(item, validationResult);
#if (NETCOREAPP)
                            var r = await DbContext.Set<TDBModel>().AddAsync(newDBItem);
                            await DbContext.SaveChangesAsync();
                            ToModel(r.Entity, item); //reload changes in item
#else
                            var r = DbContext.Set<TDBModel>().Add(newDBItem);
                            await DbContext.SaveChangesAsync();
                            SetModelKey(item, DBModelKey(newDBItem));
#endif
                        }
                        else if (!modelKey.Equals(EmptyKeyValue) && (mode != StoreMode.Create))
                        {
                            dataMode = DataMode.Update;                                
                            var dbModel = EFGetByKey(modelKey);
                            if (dbModel == null)
                                throw new Exception($"Unable to locate {typeof(TDBModel).Name}({modelKey}) in datastore");

                            ToDBModel(item, dbModel);

                            validationResult = await ValidateDBModelAsync(dbModel, DataMode.Update, s);
                            if (!validationResult.IsValid)
                                throw CreateValidationException(item, validationResult);

                            DbContext.Entry(dbModel).State = EntityState.Modified;
                            await DbContext.SaveChangesAsync();                                
                        }
#if (NETCOREAPP)
                        await wrk.CommitAsync();
#else
                        wrk.Commit();
#endif
                    }

                    return new DataResult<TKey, TModel> { Success = true, Mode = dataMode, Data = new[] { currentItem } };
                }
                catch (Exception err)
                {
#if (NETCOREAPP)
                    await wrk.RollbackAsync();
#else
                    wrk.Rollback();
#endif
                    return new DataResult<TKey, TModel>(DataMode.Create, typeof(TDBModel).Name, err) { Data = new[] { currentItem } };
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

            var result = await TransactionalExecAsync(async (s, t) =>
            {
                try
                {
                    foreach (var id in ids)
                    {
                        var dbModel = EFGetByKey(id);
                        if (dbModel != null)
                        {
                            var validationResult = await ValidateDBModelAsync(dbModel, DataMode.Delete, s);
                            if (!validationResult.IsValid)
                                throw new ValidationException(validationResult.Errors);

                            DbContext.Entry(dbModel).State = EntityState.Deleted;
                            await DbContext.SaveChangesAsync();
                        }
                    }

                    await s.DbContext.SaveChangesAsync();
#if (NETCOREAPP)
                    await t.CommitAsync();
#else
                    t.Commit();
#endif
                    return new DataResult<TKey, TModel> { Success = true, Mode = DataMode.Delete };
                }
                catch (ValidationException err)
                {
                    return new DataResult<TKey, TModel>(DataMode.Delete, nameof(TDBModel), err);
                }
            }, false);
            return result;
        }		
	}
}
