using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DX.Utils.Data;
#if (NETSTANDARD2_0)
using Microsoft.AspNetCore.Identity;
#else
using Microsoft.AspNet.Identity;
#endif

namespace DX.Data.Xpo.Identity
{
#if (NETSTANDARD2_0)
    public interface IUser<TKey> 
         where TKey : IEquatable<TKey>
    {
        TKey Id { get; }
        string UserName { get; set; }
        string NormalizedName { get; set; }
        string NormalizedEmail { get; set; }
    }
    public interface IRole<TKey> 
        where TKey : IEquatable<TKey>
    {
        TKey Id { get; }
        string Name { get; set; }
        string NormalizedName { get; set; }
    }

#endif
    public interface IDxUser<TKey> : IDataStoreModel<TKey>, IUser<TKey>
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

    public interface IDxRole<TKey> : IDataStoreModel<TKey>, IRole<TKey>
        where TKey : IEquatable<TKey>
    {
        //Id
        //Name
        IList UsersList { get; }
    }

    public interface IDxUserLogin<TKey> : IDataStoreModel<TKey>
        where TKey : IEquatable<TKey>
    {
        //Id
        TKey UserId { get; }
        string LoginProvider { get; set; }
        string ProviderKey { get; set; }
    }

    public interface IDxBaseClaim<TKey> : IDataStoreModel<TKey>
        where TKey : IEquatable<TKey>
    {
        string ClaimType { get; set; }
        string ClaimValue { get; set; }

        Claim ToClaim();

        void InitializeFromClaim(Claim other);
    }
    public interface IDxUserClaim<TKey> : IDxBaseClaim<TKey>
        where TKey : IEquatable<TKey>
    {
        //Id
        TKey UserId { get; }
    }

    public interface IDxUserToken<TKey> : IDataStoreModel<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey UserId { get; }
        string LoginProvider { get; set; }
        string Name { get; set; }
        string Value { get; set; }
    }

    public interface IDxRoleClaim<TKey> : IDxBaseClaim<TKey>
        where TKey:IEquatable<TKey>
    {        
        TKey RoleId { get;  }
    }
}
