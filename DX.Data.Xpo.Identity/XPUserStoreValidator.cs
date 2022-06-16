using DevExpress.Xpo;
using DX.Utils.Data;
using System;
using System.Linq;


namespace DX.Data.Xpo.Identity
{
	public class XPUserStoreValidator<TUser, TXPOUser> : XPUserStoreValidator<string, TUser, TXPOUser>
		where TUser : class, IXPUser<string>, new()
		where TXPOUser : XPBaseObject, IXPUser<string>
	{
	}

	public class XPUserStoreValidator<TKey, TUser, TXPOUser> : XPDataValidator<TKey, TUser, TXPOUser>
		where TKey : IEquatable<TKey>
		where TUser : class, IXPUser<TKey>, new()
		where TXPOUser : XPBaseObject, IXPUser<TKey>
	{
		public override IDataValidationResults<TKey> Deleted(
			TKey id,
			TXPOUser dbModel,
			IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>();
			result.Add(
				new DataValidationResult<TKey>(
					DataValidationResultType.Success,
					id,
					string.Empty,
					string.Empty,
					0,
					DataValidationEventType.Deleted));

			validationResults.AddRange(result);
			return result;
		}

		public override IDataValidationResults<TKey> Deleting(
			TKey id,
			IDataValidationResults<TKey> validationResults,
			params object[] args)
		{
			var result = new DataValidationResults<TKey>();
			result.Add(
				new DataValidationResult<TKey>(
					DataValidationResultType.Success,
					id,
					string.Empty,
					string.Empty,
					0,
					DataValidationEventType.Deleting));

			validationResults.AddRange(result);
			return result;
		}

		public override IDataValidationResults<TKey> Inserted(
			TKey id,
			TUser model,
			TXPOUser dbModel,
			IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>();
			result.Add(
				new DataValidationResult<TKey>(
					DataValidationResultType.Success,
					id,
					string.Empty,
					string.Empty,
					0,
					DataValidationEventType.Inserted));

			validationResults.AddRange(result);
			return result;
		}

		public override IDataValidationResults<TKey> Inserting(
			TUser model,
			IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>();
			result.Add(
				new DataValidationResult<TKey>(
					DataValidationResultType.Success,
					default,
					string.Empty,
					string.Empty,
					0,
					DataValidationEventType.Inserting));

			validationResults.AddRange(result);
			return result;
		}

		public override IDataValidationResults<TKey> Updated(
			TKey id,
			TUser model,
			TXPOUser dbModel,
			IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>();
			result.Add(
				new DataValidationResult<TKey>(
					DataValidationResultType.Success,
					id,
					string.Empty,
					string.Empty,
					0,
					DataValidationEventType.Updated));

			validationResults.AddRange(result);
			return result;
		}

		public override IDataValidationResults<TKey> Updating(
			TKey id,
			TUser model,
			IDataValidationResults<TKey> validationResults)
		{
			var result = new DataValidationResults<TKey>();
			result.Add(
				new DataValidationResult<TKey>(
					DataValidationResultType.Success,
					id,
					string.Empty,
					string.Empty,
					0,
					DataValidationEventType.Updating));

			validationResults.AddRange(result);
			return result;
		}
	}
}
