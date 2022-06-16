using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DX.Data.Xpo.Identity.Persistent;
using DX.Data.Xpo.IndentityServer.XPModels;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
//using IdentityServer4.Models;
//using IdentityServer4.Stores;

namespace DX.Data.Xpo.IdentityServer.Stores
{
	/*
	public class XPPersistedGrant : PersistedGrant, IDataStoreModel<string>
	{		
		public XPPersistedGrant()
		{
			
		}

		public string ID { get => base.Key; set => base.Key = value; }
	}
	public class XPPersistedGrantStore : XPDataStore<string, PersistedGrant, XpoPersistedGrant>, IPersistedGrantStore
    {
        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;		

		/// <summary>
		/// Initializes a new instance of the <see cref="PersistedGrantStore"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="logger">The logger.</param>
		public XPPersistedGrantStore(XpoDatabase db, ILogger<XPPersistedGrantStore> logger, 
            IXPDataMapper<string, PersistedGrant, XpoPersistedGrant> mapper)
            : base(db)
        {
			DB = db;
			Logger = logger;
        }

        /// <inheritdoc/>
        //public virtual async Task StoreAsync(PersistedGrant token)
        //{
        //    var existing = await DB.ExecuteAsync((db, w) =>
        //    {
        //        var r = db.qu
        //        var r = Query(w).Select(CreateModelInstance);
        //        return r.ToList();
        //    });

        //    return results;

        //    Context.PersistedGrants.SingleOrDefaultAsync(x => x.Key == token.Key);
        //    if (existing == null)
        //    {
        //        Logger.LogDebug("{persistedGrantKey} not found in database", token.Key);

        //        var persistedGrant = token.ToEntity();
        //        Context.PersistedGrants.Add(persistedGrant);
        //    }
        //    else
        //    {
        //        Logger.LogDebug("{persistedGrantKey} found in database", token.Key);

        //        token.UpdateEntity(existing);
        //    }

        //    try
        //    {
        //        await Context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        Logger.LogWarning("exception updating {persistedGrantKey} persisted grant in database: {error}", token.Key, ex.Message);
        //    }
        //}

        ///// <inheritdoc/>
        //public virtual async Task<PersistedGrant> GetAsync(string key)
        //{
        //    var persistedGrant = await Context.PersistedGrants.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key);
        //    var model = persistedGrant?.ToModel();

        //    Logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

        //    return model;
        //}

        ///// <inheritdoc/>
        //public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        //{
        //    filter.Validate();

        //    var persistedGrants = await Filter(filter).ToArrayAsync();
        //    var model = persistedGrants.Select(x => x.ToModel());

        //    Logger.LogDebug("{persistedGrantCount} persisted grants found for {@filter}", persistedGrants.Length, filter);

        //    return model;
        //}

        ///// <inheritdoc/>
        //public virtual async Task RemoveAsync(string key)
        //{
        //    var persistedGrant = await Context.PersistedGrants.FirstOrDefaultAsync(x => x.Key == key);
        //    if (persistedGrant != null)
        //    {
        //        Logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

        //        Context.PersistedGrants.Remove(persistedGrant);

        //        try
        //        {
        //            await Context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            Logger.LogInformation("exception removing {persistedGrantKey} persisted grant from database: {error}", key, ex.Message);
        //        }
        //    }
        //    else
        //    {
        //        Logger.LogDebug("no {persistedGrantKey} persisted grant found in database", key);
        //    }
        //}

        ///// <inheritdoc/>
        //public async Task RemoveAllAsync(PersistedGrantFilter filter)
        //{
        //    filter.Validate();

        //    var persistedGrants = await Filter(filter).ToArrayAsync();

        //    Logger.LogDebug("removing {persistedGrantCount} persisted grants from database for {@filter}", persistedGrants.Length, filter);

        //    Context.PersistedGrants.RemoveRange(persistedGrants);

        //    try
        //    {
        //        await Context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        Logger.LogInformation("removing {persistedGrantCount} persisted grants from database for subject {@filter}: {error}", persistedGrants.Length, filter, ex.Message);
        //    }
        //}


        //private IQueryable<Entities.PersistedGrant> Filter(PersistedGrantFilter filter)
        //{
        //    var query = Context.PersistedGrants.AsQueryable();

        //    if (!String.IsNullOrWhiteSpace(filter.ClientId))
        //    {
        //        query = query.Where(x => x.ClientId == filter.ClientId);
        //    }
        //    if (!String.IsNullOrWhiteSpace(filter.SessionId))
        //    {
        //        query = query.Where(x => x.SessionId == filter.SessionId);
        //    }
        //    if (!String.IsNullOrWhiteSpace(filter.SubjectId))
        //    {
        //        query = query.Where(x => x.SubjectId == filter.SubjectId);
        //    }
        //    if (!String.IsNullOrWhiteSpace(filter.Type))
        //    {
        //        query = query.Where(x => x.Type == filter.Type);
        //    }

        //    return query;
        //}

    }
*/
}
