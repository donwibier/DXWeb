using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if (NETSTANDARD2_1)
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
#else
using System.Data.Entity;
#endif

namespace DX.Data.EF
{
	public class EFDatabase<TEFContext>
		where TEFContext: DbContext, new()
	{
		public virtual void Execute(
#if (NETSTANDARD2_1)
			Action<EFDatabase<TEFContext>, TEFContext, IDbContextTransaction> work,
#else
			Action<EFDatabase<TEFContext>, TEFContext, DbContextTransaction> work, 
#endif
			bool transactional = true, bool commit = true)
		{
			using (TEFContext ctx = new TEFContext())
			{
				using (var dbTransaction = ctx.Database.BeginTransaction())
				{
					work(this, ctx, dbTransaction);
					if (transactional && commit && ctx.ChangeTracker.HasChanges())
					{
						ctx.SaveChanges();
						dbTransaction.Commit();
					}
				}
			}
		}

		public T Execute<T>(
#if (NETSTANDARD2_1)
			Func<EFDatabase<TEFContext>, TEFContext, IDbContextTransaction, T> work,
#else
			Func<EFDatabase<TEFContext>, TEFContext, DbContextTransaction, T> work, 
#endif
			bool transactional = true, bool commit = true)
		{
			T result = default;
			using (TEFContext ctx = new TEFContext())
			{
				using (var dbTransaction = ctx.Database.BeginTransaction())
				{
					result = work(this, ctx, dbTransaction);
					if (transactional && commit && ctx.ChangeTracker.HasChanges())
					{						
						ctx.SaveChanges();
						dbTransaction.Commit();
					}
				}
			}
			return result;
		}

		public async virtual Task<T> ExecuteAsync<T>(
#if (NETSTANDARD2_1)
			Func<EFDatabase<TEFContext>, TEFContext, IDbContextTransaction, T> work,
#else
			Func<EFDatabase<TEFContext>, TEFContext, DbContextTransaction, T> work, 
#endif
			bool transactional = true, bool commit = true)
		{
			return await Task.FromResult<T>(Execute<T>(work, transactional, commit));
		}

		public async virtual Task ExecuteAsync(
#if (NETSTANDARD2_1)
			Action<EFDatabase<TEFContext>, TEFContext, IDbContextTransaction> work,
#else
			Action<EFDatabase<TEFContext>, TEFContext, DbContextTransaction> work, 
#endif
			bool transactional = true, bool commit = true)
		{
			await Task.Run(() => { Execute(work, transactional, commit); });
		}

	}

}
