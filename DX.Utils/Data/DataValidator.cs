using System;
using System.Collections.Generic;
using System.Text;

namespace DX.Utils.Data
{
	public abstract class DataValidator<TKey, TModel, TDBModel> : IDataStoreValidator<TKey, TModel, TDBModel>
			where TKey : IEquatable<TKey>
			where TModel : IDataStoreModel<TKey>
			where TDBModel : class, IDataStoreModel<TKey>
	{
		public abstract bool Deleting(TDBModel model);
		public abstract bool Deleted(TDBModel model);
		public abstract bool Inserting(TModel model);
		public abstract bool Inserted(TModel model, TDBModel dbModel);
		public abstract bool Updating(TModel model);
		public abstract bool Updated(TModel model, TDBModel dbModel);
	}
}
