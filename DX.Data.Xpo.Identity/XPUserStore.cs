using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DX.Utils;
using DX.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace DX.Data.Xpo.Identity
{
	using DevExpress.Data;
#if (NETCOREAPP)
	using Microsoft.AspNetCore.Identity;

	public class XPUserStore<TKey, TUser, TRole, 
								TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
								TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken> :
			UserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
		where TUserToken : IdentityUserToken<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>, new()
        where TXPOUser : XPBaseObject, IXPUser<TKey>
        where TXPORole : XPBaseObject, IXPRole<TKey>
        where TXPOLogin : XPBaseObject, IXPUserLogin<TKey>
        where TXPOClaim : XPBaseObject, IXPUserClaim<TKey>
        where TXPOToken : XPBaseObject, IXPUserToken<TKey>
    {
        readonly IQueryableUserStore<TKey, TUser, TUserRole, TUserToken> _UserStore;
        readonly IQueryableRoleStore<TKey, TRole> _RoleStore;
        readonly IQueryableUserClaimStore<TKey, TUserClaim> _ClaimStore;
        readonly IQueryableUserLoginStore<TKey, TUserLogin> _LoginStore;
        readonly IQueryableUserTokenStore<TKey, TUserToken> _TokenStore;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="describer">The <see cref="Microsoft.AspNetCore.Identity.IdentityErrorDescriber"/> used to describe store errors.</param>
        public XPUserStore(IQueryableUserStore<TKey, TUser, TUserRole, TUserToken> userStore, IQueryableRoleStore<TKey, TRole> roleStore,
					 IQueryableUserClaimStore<TKey, TUserClaim> userClaimStore,
					 IQueryableUserLoginStore<TKey, TUserLogin> userLoginStore, IQueryableUserTokenStore<TKey, TUserToken> userTokenStore,
            IdentityErrorDescriber? describer = null) : base(describer ?? new IdentityErrorDescriber())
        {
			_TokenStore = userTokenStore;
			_LoginStore = userLoginStore;
			_ClaimStore = userClaimStore;
			_RoleStore = roleStore;
			_UserStore = userStore;
			//var test IdentityResult.Failed(new IdentityError { Code = "100", Description = err.Message });
		}

        protected void ThrowIfNull(object? obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
        }
		
        public override IQueryable<TUser> Users => _UserStore.Query();

        public async override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {            
            await _UserStore.AddClaimsAsync(user, claims, cancellationToken);
        }

        public override async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {            
			await _UserStore.AddLoginAsync(user, login, cancellationToken);
        }

        public override async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
			await _UserStore.AddToRoleAsync(user, normalizedRoleName, cancellationToken);
        }

        public async override Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            var r = await _UserStore.CreateAsync(user);
			//TODO: Check appropriate error
			return r.Success ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.DefaultError());
        }

        public async override Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            //TODO: Check appropriate error
            var r = await _UserStore.DeleteAsync(user);
            return r.Success ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.DefaultError());
        }

        public async override Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
			return await _UserStore.FindByEmailAsync(normalizedEmail);			
        }

        public async override Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
			return await _UserStore.FindByIdAsync(userId);
        }

		protected async Task<TUser?> FindByIdAsync(TKey userId, CancellationToken cancellationToken = default)
		{			
			return await _UserStore.FindByIdAsync(userId);
		}

		public async override Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
			return await _UserStore.FindByUserNameAsync(normalizedUserName);			
        }

        public async override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
        {			
			return await _ClaimStore.GetUserClaimsAsync(user.Id);
		}

        public async override Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default)
        {
			return await _LoginStore.GetLoginsAsync(user.Id);			
		}

        public override async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default)
        {
			return await _UserStore.GetRolesAsync(user, cancellationToken);
        }

        public override async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
			return await _UserStore.GetUsersForClaimAsync(claim, cancellationToken);
        }

        public override async Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
			return await _UserStore.GetUsersInRoleAsync(normalizedRoleName, cancellationToken);
        }

        public override async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            return await _UserStore.IsInRoleAsync(user, normalizedRoleName, cancellationToken);
        }

        public async override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
			await _UserStore.RemoveClaimsAsync(user, claims, cancellationToken);
        }

        public override async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
			await _UserStore.RemoveFromRoleAsync(user, normalizedRoleName, cancellationToken);
        }

        public async override Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
			await _UserStore.RemoveLoginAsync(user, loginProvider, providerKey, cancellationToken);
        }

        public async override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
			await _UserStore.ReplaceClaimAsync(user, claim, newClaim, cancellationToken);
        }

        public async override Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);

            user.ConcurrencyStamp = Guid.NewGuid().ToString();

			var r = await _UserStore.UpdateAsync(user);
			//TODO Do something with result
            return r.Success ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.DefaultError());
			//try
            //{
            //    await SaveChanges(cancellationToken);
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            //}
        }

        protected async override Task AddUserTokenAsync(TUserToken token)
        {			
			await _TokenStore.CreateAsync(token);
        }

        protected async override Task<TRole?> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
			return await _RoleStore.FindByNameAsync(normalizedRoleName);
        }

        protected async override Task<TUserToken?> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
        {
			return await _TokenStore.FindTokenAsync(user.Id, loginProvider, name, cancellationToken);
        }

        protected override Task<TUser?> FindUserAsync(TKey userId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(userId);

            var result = _UserStore.GetByKey(userId);
			return Task.FromResult(result)!;
        }

        protected async override Task<TUserLogin?> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
			return await _LoginStore.FindUserLoginAsync(userId, loginProvider, providerKey, cancellationToken);				
        }

        protected async override Task<TUserLogin?> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
			return await _LoginStore.FindUserLoginAsync(loginProvider, providerKey, cancellationToken);
        }

		protected async override Task<TUserRole?> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken = default)
		{
			return await FindUserRoleAsync(userId, roleId, cancellationToken);
		}

		protected async override Task RemoveUserTokenAsync(TUserToken token)
        {
			await _UserStore.RemoveUserTokenAsync(token);
        }
    }


#else

	using Microsoft.AspNet.Identity;
	using System.Data;

	public class IdentityError
	{
		public string Code { get; set; } = default!;
		public string Description { get; set; } = default!;
	}

	//public class XPUserStore<TUser> : XPUserStore<TUser, XpoDxUser>
	//	 where TUser : class, IXPUser<string>, new()
	//{
	//	public XPUserStore(XpoDatabase db, XPDataMapper<string, TUser, XpoDxUser> mapper, XPDataValidator<string, TUser, XpoDxUser> validator) : base(db, mapper, validator)
	//	{

	//	}
	//}

	//public class XPUserStore<TUser, TXPOUser> : XPUserStore<string, TUser, TXPOUser, XpoDxRole, XpoDxUserLogin, XpoDxUserClaim>,
	//	IUserStore<TUser>
	//	 where TUser :  class, IXPUser<string>, new()
	//	 where TXPOUser : XpoDxUser, IXPUser<string>, IUser<string>
	//{
	//	public XPUserStore(XpoDatabase db, XPDataMapper<string, TUser, TXPOUser> mapper, XPDataValidator<string, TUser, TXPOUser> validator)
	//		: base(db, mapper, validator)
	//	{

	//	}

	//	//public XPUserStore(string connectionName, XPDataMapper<string, TUser, TXPOUser> mapper, XPDataValidator<string, TUser, TXPOUser> validator) 
	//	//	: base(connectionName, mapper, validator)
	//	//{

	//	//}
	//}
	public class XPUserStore<TKey, TUser, TRole,
								TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
								TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken> : /*XPDataStore<TKey, TUser, TXPOUser>, 	*/
		 IUserStore<TUser, TKey>,
		 IUserLoginStore<TUser, TKey>,
		 IUserClaimStore<TUser, TKey>,
		 IUserRoleStore<TUser, TKey>,
		 IUserPasswordStore<TUser, TKey>,
		 IUserSecurityStampStore<TUser, TKey>,
		 IQueryableUserStore<TUser, TKey>,
		 IUserEmailStore<TUser, TKey>,
		 IUserPhoneNumberStore<TUser, TKey>,
		 IUserTwoFactorStore<TUser, TKey>,
		 IUserLockoutStore<TUser, TKey>
		//where TKey : IEquatable<TKey>
		//where TUser : class, IUser<TKey>, IXPUser<TKey>, new()
		//where TXPOUser : XPBaseObject, IXPUser<TKey>, IUser<TKey>
		//where TXPORole : XPBaseObject, IRole<TKey>
		//where TXPOLogin : XPBaseObject, IXPUserLogin<TKey>
		//where TXPOClaim : XPBaseObject, IXPUserClaim<TKey>
		where TKey : IEquatable<TKey>
		where TUser : class, IXPUser<TKey>
		where TRole : class, IXPRole<TKey>
		where TUserClaim : class, IXPUserClaim<TKey>, new()
		where TUserRole : class, IXPUserRole<TKey>, new()
		where TUserLogin : class, IXPUserLogin<TKey>, new()
		where TUserToken : class, IXPUserToken<TKey>, new()
		where TRoleClaim : class, IXPRoleClaim<TKey>, new()
		where TXPOUser : XPBaseObject, IXPUser<TKey>
		where TXPORole : XPBaseObject, IXPRole<TKey>
		where TXPOLogin : XPBaseObject, IXPUserLogin<TKey>
		where TXPOClaim : XPBaseObject, IXPUserClaim<TKey>
		where TXPOToken : XPBaseObject, IXPUserToken<TKey>
	{
		readonly IQueryableUserStore<TKey, TUser, TUserRole, TUserToken> _UserStore;
		readonly IQueryableRoleStore<TKey, TRole> _RoleStore;
		readonly IQueryableUserClaimStore<TKey, TUserClaim> _ClaimStore;
		readonly IQueryableUserLoginStore<TKey, TUserLogin> _LoginStore;
		readonly IQueryableUserTokenStore<TKey, TUserToken> _TokenStore;
		private bool isDisposed = false;
		public XPUserStore(
				IQueryableUserStore<TKey, TUser, TUserRole, TUserToken> userStore,
				IQueryableRoleStore<TKey, TRole> roleStore,
				IQueryableUserClaimStore<TKey, TUserClaim> claimStore,
				IQueryableUserLoginStore<TKey, TUserLogin> loginStore,
				IQueryableUserTokenStore<TKey, TUserToken> tokenStore
			)
		{
			_UserStore = userStore;
			_RoleStore = roleStore;
			_ClaimStore = claimStore;
			_LoginStore = loginStore;
			_TokenStore = tokenStore;
		}

		protected void ThrowIfNull(object? obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));
		}

		//public override TKey GetModelKey(TUser model) => model.Id;
		//public override void SetModelKey(TUser model, TKey key) => model.Id = key;

		//protected override IQueryable<TXPOUser> XPQuery()
		//{
		//	var r = XPQuery(UnitOfWork);
		//	return r;

		//}

		//protected override IQueryable<TXPOUser> XPQuery(Session s )
		//{
		//	var r = from n in s.Query<TXPOUser>()
		//			select n;
		//	return r;

		//}
		//public override IQueryable<TUser> Query() => XPQuery().Select(CreateModelInstance).AsQueryable();

		protected virtual void ThrowIfDisposed()
		{
			if (isDisposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		public void Dispose()
		{
			isDisposed = true;
		}

		#region Generic Helper methods and members
		protected static Type XPOUserType { get { return typeof(TXPOUser); } }
		protected static Type XPORoleType { get { return typeof(TXPORole); } }
		protected static Type XPOLoginType { get { return typeof(TXPOLogin); } }
		protected static Type XPOClaimType { get { return typeof(TXPOClaim); } }

		public IQueryable<TUser> Users => throw new NotImplementedException();

		protected static TXPOUser XPOCreateUser(Session s) { return (Activator.CreateInstance(typeof(TXPOUser), s) as TXPOUser)!; }
		protected static TXPORole XPOCreateRole(Session s) { return (Activator.CreateInstance(typeof(TXPORole), s) as TXPORole)!; }
		protected static TXPOLogin XPOCreateLogin(Session s) { return (Activator.CreateInstance(typeof(TXPOLogin), s) as TXPOLogin)!; }
		protected static TXPOClaim XPOCreateClaim(Session s) { return (Activator.CreateInstance(typeof(TXPOClaim), s) as TXPOClaim)!; }
		#endregion

		public async Task CreateAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			var r = await _UserStore.CreateAsync(user);
			//TODO: Check appropriate error
			//return r.Success ? IdentityResult.Success : IdentityResult.Failed(r.Exception.Message);
		}

		public async virtual Task UpdateAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			var r = await _UserStore.UpdateAsync(user);
			if (!r.Success)
				throw r.Exception;
		}

		public async virtual Task DeleteAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			var r = await _UserStore.DeleteAsync(user);
			if (!r.Success)
				throw r.Exception;
		}

		public async virtual Task<TUser> FindByIdAsync(TKey userId)
		{
			ThrowIfDisposed();
			ThrowIfNull(userId);
			var result = await _UserStore.FindByIdAsync(userId);
			return result!;
		}

		public async virtual Task<TUser> FindByNameAsync(string normalizedUserName)
		{
			ThrowIfDisposed();
			ThrowIfNull(normalizedUserName);
			var result = await _UserStore.FindByUserNameAsync(normalizedUserName);
			return result!;
		}

		public async virtual Task AddLoginAsync(TUser user, UserLoginInfo login)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			ThrowIfNull(login);
			await _UserStore.AddLoginAsync(user, login);
		}

		public async virtual Task RemoveLoginAsync(TUser user, UserLoginInfo login)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			ThrowIfNull(login);
			await _UserStore.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
		}

		public async virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);

			return await _LoginStore.GetLoginsAsync(user.Id);			
		}

		public async virtual Task<TUser> FindAsync(UserLoginInfo login)
		{
			ThrowIfDisposed();
			ThrowIfNull(login);
			//TODO: Do this in one call
			var result = await _LoginStore.FindUserLoginAsync(login.LoginProvider, login.ProviderKey);
			return (result != null)? _UserStore.GetByKey(result.UserId):null!;				
		}

		public async virtual Task<IList<Claim>> GetClaimsAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);

			var result = await _ClaimStore.GetUserClaimsAsync(user.Id);
			return result;
		}

		public async virtual Task AddClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			ThrowIfNull(claim);
			await _ClaimStore.CreateAsync(new TUserClaim { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value });
		}

		public async virtual Task RemoveClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			await _ClaimStore.DeleteAsync(new TUserClaim { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value });
		}

		public async virtual Task AddToRoleAsync(TUser user, string roleName)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			await _UserStore.AddToRoleAsync(user, roleName);
		}

		public async virtual Task RemoveFromRoleAsync(TUser user, string roleName)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			await _UserStore.RemoveFromRoleAsync(user, roleName);
		}

		public async virtual Task<IList<string>> GetRolesAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return await _UserStore.GetRolesAsync(user);
		}

		public async virtual Task<bool> IsInRoleAsync(TUser user, string roleName)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return await _UserStore.IsInRoleAsync(user, roleName);
		}

		public async virtual Task SetPasswordHashAsync(TUser user, string passwordHash)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			user.PasswordHash = passwordHash;
			await _UserStore.SetPasswordHashAsync(user, passwordHash);
		}

		public async virtual Task<string> GetPasswordHashAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return await _UserStore.GetPasswordHashAsync(user);
		}

		public async Task<bool> HasPasswordAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return await _UserStore.HasPasswordHashAsync(user);
		}

		public async Task SetSecurityStampAsync(TUser user, string stamp)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			await SetSecurityStampAsync(user, stamp);
		}

		public async Task<string> GetSecurityStampAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return await _UserStore.GetSecurityStampAsync(user);
		}

		public async Task SetEmailAsync(TUser user, string email)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			user.Email = email;
			await _UserStore.UpdateAsync(user);
		}

		public virtual Task<string> GetEmailAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return Task.FromResult(user.Email);
		}

		public virtual Task<bool> GetEmailConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return Task.FromResult(user.EmailConfirmed);
		}

		public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			user.EmailConfirmed = confirmed;
			return Task.CompletedTask;// FromResult<object>(null!);
		}

		public async virtual Task<TUser> FindByEmailAsync(string email)
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(email))
				throw new ArgumentNullException(nameof(email));
			
			var result = await _UserStore.FindByEmailAsync(email);
			return result!;
		}

		public virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			user.PhoneNumber = phoneNumber;
			return Task.CompletedTask;
		}

		public virtual Task<string> GetPhoneNumberAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return Task.FromResult(user.PhoneNumber);			
		}

		public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return Task.FromResult(user.PhoneNumberConfirmed);
		}

		public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			user.PhoneNumberConfirmed = confirmed;
			return Task.CompletedTask;

		}

		public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			user.TwoFactorEnabled = enabled;
			return Task.CompletedTask;
		}

		public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return Task.FromResult(user.TwoFactorEnabled);			
		}

		public virtual Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return Task.FromResult(new DateTimeOffset(user.LockoutEndDateUtc??DateTime.UtcNow, TimeSpan.Zero));
		}

		public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);

			DateTime utcDateTime = DateTime.UtcNow + lockoutEnd.Offset;
			user.LockoutEndDateUtc = utcDateTime;
			return Task.CompletedTask;
		}

		public virtual Task<int> IncrementAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			user.AccessFailedCount++;
			return Task.FromResult(user.AccessFailedCount);
		}

		public virtual Task ResetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			user.AccessFailedCount = 0;
			return Task.CompletedTask;
		}

		public virtual Task<int> GetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);			
			return Task.FromResult(user.AccessFailedCount);
		}

		public Task<bool> GetLockoutEnabledAsync(TUser user)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			return Task.FromResult(user.LockoutEnabled);
		}

		public Task SetLockoutEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();
			ThrowIfNull(user);
			user.LockoutEnabled= enabled;
			return Task.CompletedTask;

		}
	}
#endif
}
