using DevExpress.Xpo;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
#if (NETCOREAPP)
using Microsoft.AspNetCore.Identity;
#else
using Microsoft.AspNet.Identity;
#endif

namespace DX.Data.Xpo.Identity
{
#if (NETCOREAPP)
    public interface IXPUser<TKey>
         where TKey : IEquatable<TKey>
	{
		TKey Id { get; set; }
        string UserName { get; set; }
#else
	public interface IXPUser<TKey> : IUser<TKey>
		 where TKey : IEquatable<TKey>
	{
		new TKey Id { get; set; }
#endif
		string NormalizedUserName { get; set; }
        string NormalizedEmail { get; set; }

        string Email { get; set; }
        bool EmailConfirmed { get; set; }
        string PasswordHash { get; set; }
        string SecurityStamp { get; set; }

        string PhoneNumber { get; set; }
        bool PhoneNumberConfirmed { get; set; }

        bool TwoFactorEnabled { get; set; }

        bool LockoutEnabled { get; set; }
        DateTime? LockoutEndDateUtc { get; set; }

        int AccessFailedCount { get; set; }

        //net8
        string? ConcurrencyStamp { get; set; }

        string RefreshToken { get; set; }
        DateTime? RefreshTokenExpiryTime { get; set; }

        IList RolesList { get; }

        IList ClaimsList { get; }

        IList LoginsList { get; }

        IList TokenList { get; }

    }

    public interface IXPUserRole<TKey>
		where TKey : IEquatable<TKey>
	{
		TKey UserId { get; set; }
		TKey RoleId { get; set; }
	}

    public interface IXPUserLogin<TKey>
        where TKey : IEquatable<TKey>
    {
        //Id
        TKey UserId { get; set; }
        string LoginProvider { get; set; }
        string ProviderKey { get; set; }
        string ProviderDisplayName { get; set; }

        void InitializeUserLogin(XPBaseObject user, UserLoginInfo login);
    }

    public interface IXPBaseClaim<TKey>
        where TKey : IEquatable<TKey>
    {
        string ClaimType { get; set; }
        string ClaimValue { get; set; }

        Claim ToClaim();

        void InitializeFromClaim(Claim other);
    }
    public interface IXPUserClaim<TKey> : IXPBaseClaim<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey UserId { get; set; }
        void InitializeUserClaim(XPBaseObject user, Claim claim);
    }

    public interface IXPUserToken<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey UserId { get; set; }
        string LoginProvider { get; set; }
        string Name { get; set; }
        string Value { get; set; }
    }
#if (NETCOREAPP)
    public interface IXPRole<TKey>
        where TKey : IEquatable<TKey>
    {
		TKey Id { get; set; }
        string Name { get; set; }
#else
	public interface IXPRole<TKey> : IRole<TKey>
		where TKey : IEquatable<TKey>
	{
		new TKey Id { get;  set; }
#endif

		string NormalizedName { get; set; }
        string? ConcurrencyStamp { get; set; }
        IList ClaimsList { get; }
    }

    public interface IXPRoleClaim<TKey> : IXPBaseClaim<TKey>
    where TKey : IEquatable<TKey>
    {
        TKey RoleId { get; set;  }

        void InitializeRoleClaim(XPBaseObject role, Claim claim);
    }


    public interface IQueryableUserStore<TKey, TUser, TUserRole, TUserToken> : IQueryableDataStore<TKey, TUser>
        where TKey : IEquatable<TKey>
#if (NETCOREAPP)
        where TUser : IdentityUser<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserToken: IdentityUserToken<TKey>
#else
        where TUser : class, IUser<TKey>
        where TUserRole : class, IXPUserRole<TKey>
        where TUserToken: class, IXPUserToken<TKey>
#endif
	{
		Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
		Task<TUser?> FindByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default);
		Task<TUser?> FindByIdAsync(object userId, CancellationToken cancellationToken = default);
		Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default);
        Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default);
        Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default);
        Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default);
        Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default);
        Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default);
        Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default);

        Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default);
        Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default);
        Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default);

        Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default);
        Task<TUserRole?> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken = default);
        Task RemoveUserTokenAsync(TUserToken token, CancellationToken cancellationToken = default);
		Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default);
        Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default);
		Task<bool> HasPasswordHashAsync(TUser user, CancellationToken cancellationToken = default);
		Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default);
		Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default);
	}

    public interface IQueryableRoleStore<TKey, TRole> : IQueryableDataStore<TKey, TRole>
        where TKey : IEquatable<TKey>
#if (NETCOREAPP)
        where TRole : IdentityRole<TKey>
#else
		where TRole : class, IRole<TKey>
#endif
    {
        Task AddClaimsAsync(TRole role, Claim claim, CancellationToken cancellationToken = default);
        Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default);
        Task ReplaceClaimAsync(TRole role, Claim claim, Claim newClaim, CancellationToken cancellationToken = default);
				
		Task<TRole?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
		Task<TRole?> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default);
	}

    public interface IQueryableUserClaimStore<TKey, TUserClaim> : IQueryableDataStore<TKey, TUserClaim>
        where TKey : IEquatable<TKey>
#if (NETCOREAPP)
        where TUserClaim : IdentityUserClaim<TKey>
#else
        where TUserClaim : class, IXPUserClaim<TKey>
#endif
    {
        Task<List<Claim>> GetUserClaimsAsync(TKey userId, CancellationToken cancellationToken = default);
    }

	public interface IQueryableRoleClaimStore<TKey, TRoleClaim> : IQueryableDataStore<TKey, TRoleClaim>
    	where TKey : IEquatable<TKey>
#if (NETCOREAPP)
        where TRoleClaim : IdentityRoleClaim<TKey>
#else
		where TRoleClaim : class, IXPRoleClaim<TKey>
#endif
	{
		Task<List<Claim>> GetRoleClaimsAsync(TKey roleId, CancellationToken cancellationToken = default);
	}

	public interface IQueryableUserLoginStore<TKey, TUserLogin> : IQueryableDataStore<TKey, TUserLogin>
		where TKey : IEquatable<TKey>
#if (NETCOREAPP)
        where TUserLogin : IdentityUserLogin<TKey>
#else
        where TUserLogin : class, IXPUserLogin<TKey>
#endif
	{
		Task<TUserLogin?> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken = default);
		Task<TUserLogin?> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default);
		Task<IList<UserLoginInfo>> GetLoginsAsync(TKey userId, CancellationToken cancellationToken = default);
	}

	public interface IQueryableUserTokenStore<TKey, TUserToken> : IQueryableDataStore<TKey, TUserToken>
		where TKey : IEquatable<TKey>
#if (NETCOREAPP)
		where TUserToken : IdentityUserToken<TKey>
#else
		where TUserToken : class, IXPUserToken<TKey>
#endif
		
	{
		Task<TUserToken?> FindTokenAsync(TKey userId, string loginProvider, string name, CancellationToken cancellationToken);
	}

}
