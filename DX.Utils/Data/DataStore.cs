using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DX.Utils.Data
{
	public abstract class DataStore<TKey, TModel> : IDataStore<TKey, TModel>, IDisposable
		where TKey : IEquatable<TKey>
		where TModel : IDataStoreModel<TKey>
	{
		public virtual Type KeyType => typeof(TKey);
		public virtual Type ModelType => typeof(TModel);

		public abstract TModel GetByKey(TKey key);
		protected abstract IEnumerable<TModel> Query();		

		//public async virtual Task<IEnumerable<TModel>> QueryAsync()
		//{
		//	var result = await Task.FromResult(Query());
		//	return result;
		//}

		public abstract void Create(IEnumerable<TModel> items);

		public virtual void Create(TModel item)
		{
			Create(new TModel[] { item });
		}

		public async virtual Task CreateAsync(IEnumerable<TModel> items)
		{
			await Task.Run(() => Create(items));
		}
		public async virtual Task CreateAsync(TModel item)
		{
			await CreateAsync(new TModel[] { item });
		}
		public abstract void Update(IEnumerable<TModel> items);
		public void Update(TKey id, TModel item)
		{
			item.ID = id;
			Update(new TModel[] { item });
		}
		public async virtual Task UpdateAsync(IEnumerable<TModel> items)
		{
			await Task.Run(() => Update(items));
		}
		public async virtual Task UpdateAsync(TKey id, TModel item)
		{
			await Task.Run(() => Update(id, item));
		}
		public abstract void Delete(IEnumerable<TKey> ids);
		public virtual void Delete(IEnumerable<TModel> items)
		{
			Delete(from n in items select n.ID);
		}
		public virtual void Delete(TKey id)
		{
			Delete(new TKey[] { id });
		}
		public async virtual Task DeleteAsync(TKey id)
		{
			await Task.Run(() => Delete(id));
		}
		public async virtual Task DeleteAsync(IEnumerable<TKey> ids)
		{
			await Task.Run(() => Delete(ids));
		}
		public async virtual Task DeleteAsync(IEnumerable<TModel> items)
		{
			await Task.Run(() => Delete(items));
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~XpoStore() {
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
