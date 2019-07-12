using DevExpress.Xpo;
using DX.Utils.Data;
using System;
using System.Linq;

#if (NETSTANDARD2_0)
#else
using Microsoft.AspNet.Identity;
#endif

namespace DX.Data.Xpo.Identity
{
	public class XPUserStoreValidator<TUser, TXPOUser> : XPUserStoreValidator<string, TUser, TXPOUser>
		where TUser : IXPUser<string>
		where TXPOUser : XPBaseObject, IXPUser<string>
	{

	}

	public class XPUserStoreValidator<TKey, TUser, TXPOUser> : XPDataValidator<TKey, TUser, TXPOUser>
		where TKey : IEquatable<TKey>
		where TUser : IXPUser<TKey>
		where TXPOUser : XPBaseObject, IXPUser<TKey>
	{
		public override IDataValidationResult<TKey> Deleted(TKey id, TXPOUser dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = id
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Deleting(TKey id, object arg, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = id
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Inserted(TUser model, TXPOUser dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = dbModel.ID
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Inserting(TUser model, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = model.ID
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Updated(TUser model, TXPOUser dbModel, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = model.ID
			};
			validationResults.Add(result);
			return result;
		}

		public override IDataValidationResult<TKey> Updating(TUser model, IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResult<TKey>
			{
				ResultType = DataValidationResultType.Success,
				ID = model.ID
			};
			validationResults.Add(result);
			return result;
		}
	}
}
