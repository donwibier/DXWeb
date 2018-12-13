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
		public async virtual Task<TModel> GetByKeyAsync(TKey key)
		{
			var result = await Task.FromResult(GetByKey(key));
			return result;
		}
		protected abstract IEnumerable<TModel> Query();		

		//public async virtual Task<IEnumerable<TModel>> QueryAsync()
		//{
		//	var result = await Task.FromResult(Query());
		//	return result;
		//}

		public abstract IDataValidationResults<TKey> Create(IEnumerable<TModel> items);

		public virtual IDataValidationResults<TKey> Create(TModel item)
		{
			return Create(new TModel[] { item });
		}

		public async virtual Task<IDataValidationResults<TKey>> CreateAsync(IEnumerable<TModel> items)
		{
			var result = await Task.FromResult(Create(items));
			return result;
			
		}
		public async virtual Task<IDataValidationResults<TKey>> CreateAsync(TModel item)
		{
			return await CreateAsync(new TModel[] { item });
		}
		public abstract IDataValidationResults<TKey> Update(IEnumerable<TModel> items);
		public IDataValidationResults<TKey> Update(TKey id, TModel item)
		{
			item.ID = id;
			return Update(new TModel[] { item });
		}
		public IDataValidationResults<TKey> Update(TModel item)
		{
			return Update(new TModel[] { item });
		}
		public async virtual Task<IDataValidationResults<TKey>> UpdateAsync(IEnumerable<TModel> items)
		{
			var result = await Task.FromResult(Update(items));
			return result;			
		}
		public async virtual Task<IDataValidationResults<TKey>> UpdateAsync(TKey id, TModel item)
		{
			var result = await Task.FromResult(Update(id, item));
			return result;			
		}
		public async virtual Task<IDataValidationResults<TKey>> UpdateAsync(TModel item)
		{
			var result = await Task.FromResult(Update(item));
			return result;			
		}
		public abstract IDataValidationResults<TKey> Delete(IEnumerable<TKey> ids);
		public virtual IDataValidationResults<TKey> Delete(IEnumerable<TModel> items)
		{
			return Delete(from n in items select n.ID);
		}
		public virtual IDataValidationResults<TKey> Delete(TKey id)
		{
			return Delete(new TKey[] { id });
		}
		public async virtual Task<IDataValidationResults<TKey>> DeleteAsync(TKey id)
		{
			var result = await Task.FromResult(Delete(id));
			return result;
		}
		public async virtual Task<IDataValidationResults<TKey>> DeleteAsync(IEnumerable<TKey> ids)
		{
			var result = await Task.FromResult(Delete(ids));
			return result;
		}
		public async virtual Task<IDataValidationResults<TKey>> DeleteAsync(IEnumerable<TModel> items)
		{
			var result = await Task.FromResult(Delete(items));
			return result;
		}
		public async virtual Task<IDataValidationResults<TKey>> DeleteAsync(TModel item)
		{
			var result = await Task.FromResult(Delete(new TModel[] { item }));
			return result;
		}

		protected virtual void ThrowIfDisposed()
		{
			if (disposedValue)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
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
