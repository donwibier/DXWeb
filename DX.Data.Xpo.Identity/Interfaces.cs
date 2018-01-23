using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if(NETSTANDARD2_0)
using Microsoft.AspNetCore.Identity;
#else
using Microsoft.AspNet.Identity;
#endif

namespace DX.Data.Xpo.Identity
{
#if (NETSTANDARD2_0)
    public interface IUser<TKey> : IAssignable
         where TKey : IEquatable<TKey>
    {
        TKey Id { get; }
        string UserName { get; set; }
        string NormalizedName { get; set; }
    }
    public interface IRole<TKey> : IAssignable
        where TKey : IEquatable<TKey>
    {
        TKey Id { get; }
        string Name { get; set; }
        string NormalizedName { get; set; }
    }
#endif
    public interface IDxUser<TKey> : IXPOKey<TKey>, IUser<TKey>, IAssignable
		 where TKey : IEquatable<TKey>
	{
		//Id
		//UserName
		string Email { get; set; }
		bool EmailConfirmed { get; set; }

		string PhoneNumber { get; set; }
		bool PhoneNumberConfirmed { get; set; }

		bool TwoFactorEnabled { get; set; }

		int AccessFailedCount { get; set; }

		bool LockoutEnabled { get; set; }
		DateTime? LockoutEndDateUtc { get; set; }

		string SecurityStamp { get; set; }
		string PasswordHash { get; set; }
		IList RolesList { get; }
		IList ClaimsList { get; }
		IList LoginsList { get; }

		void AssignRoles(IList roles);
		void AssignClaims(IList claims);
		void AssignLogins(IList logins);
	}

	public interface IDxRole<TKey> : IXPOKey<TKey>, IRole<TKey>, IAssignable
	    where TKey : IEquatable<TKey>
	{
		//Id
		//Name
		IList UsersList { get; }
    }

    public interface IDxUserLogin<TKey> : IXPOKey<TKey>, IAssignable
        where TKey : IEquatable<TKey>
	{
		//Id
		TKey UserId { get; }
		string LoginProvider { get; set; }
		string ProviderKey { get; set; }
	}
	public interface IDxUserClaim<TKey> : IXPOKey<TKey>, IAssignable
	    where TKey : IEquatable<TKey>
	{
		//Id
		TKey UserId { get; }
		string ClaimType { get; set; }
		string ClaimValue { get; set; }
	}
}
