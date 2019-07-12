using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;
using DX.Utils.Data;
using System;
using System.Linq;

#if (NETSTANDARD2_0)
#else
using Microsoft.AspNet.Identity;
#endif

namespace DX.Data.Xpo.Identity
{
	public class XPRoleStoreValidator<TKey, TRole, TXPORole> : XPDataValidator<TKey, TRole, TXPORole>
		where TKey : IEquatable<TKey>
		where TRole : IDataStoreModel<TKey>
		where TXPORole : class, IXPSimpleObject, IXPRole<TKey>
	{

		public override IDataValidationResult<TKey> Deleting(TKey id, object arg, IDataValidationResults<TKey> validationResults)
		{
			IDataValidationResult<TKey> result = null;
			TXPORole role = arg as TXPORole;
			if (role != null)
			{
				int userCount = (int)role.Session.Evaluate(typeof(XpoDxUser),
					CriteriaOperator.Parse("Count"),
					CriteriaOperator.Parse("Roles[Id == ?]", role.ID));
				if (userCount > 0)
					result = new DataValidationResult<TKey>
					{
						ResultType = DataValidationResultType.Error,
						ID = role.ID,
						Message = String.Format("Role '{0}' cannot be deleted because there are users in this Role", role.Name)
					};
			}

			if (result == null)
			{
				result = base.Deleting(id, arg, validationResults);
			}
			validationResults.Add(result);
			return result;
		}


	}
}
