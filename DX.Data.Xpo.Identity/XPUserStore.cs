﻿using DevExpress.Xpo;
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
using Microsoft.AspNetCore.Identity;

namespace DX.Data.Xpo.Identity
{

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

		}

        protected void ThrowIfNull(object? obj)
        {
#if (!NETCOREAPP)
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
#else
            ArgumentNullException.ThrowIfNull(obj);
#endif
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





#if (crap)





#if (NETCOREAPP)
	public class XPUserStore<TUser> : XPUserStore<TUser, XpoDxUser>
		 where TUser : class, IXPUser<string>, new()
	{
		public XPUserStore(XpoDatabase db, XPDataMapper<string, TUser, XpoDxUser> mapper, XPDataValidator<string, TUser, XpoDxUser> validator) : base(db, mapper, validator)
		{

		}
	}


	public class XPUserStore<TUser, TXPOUser> : XPUserStore<string, TUser, TXPOUser, XpoDxRole, XpoDxUserLogin, XpoDxUserClaim, XpoDxUserToken>
		 where TUser : class, IXPUser<string>, new()
		 where TXPOUser : XPBaseObject, IXPUser<string>
#else
	public class XPUserStore<TUser> : XPUserStore<TUser, XpoDxUser>
		 where TUser : class, IXPUser<string>, new()
	{
		public XPUserStore(XpoDatabase db, XPDataMapper<string, TUser, XpoDxUser> mapper, XPDataValidator<string, TUser, XpoDxUser> validator) : base(db, mapper, validator)
		{

		}
	}

	public class XPUserStore<TUser, TXPOUser> : XPUserStore<string, TUser, TXPOUser, XpoDxRole, XpoDxUserLogin, XpoDxUserClaim>,
		IUserStore<TUser>
		 where TUser :  class, IXPUser<string>, new()
		 where TXPOUser : XpoDxUser, IXPUser<string>, IUser<string>
#endif
	{
		public XPUserStore(XpoDatabase db, XPDataMapper<string, TUser, TXPOUser> mapper, XPDataValidator<string, TUser, TXPOUser> validator)
			: base(db, mapper, validator)
		{

		}

		//public XPUserStore(string connectionName, XPDataMapper<string, TUser, TXPOUser> mapper, XPDataValidator<string, TUser, TXPOUser> validator) 
		//	: base(connectionName, mapper, validator)
		//{

		//}
	}
#if (NETCOREAPP)
	public class XPUserStore<TKey, TUser, TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken> : XPDataStore<TKey, TUser, TXPOUser>, // XpoStore<TXPOUser, TKey>, 
		IUserLoginStore<TUser>,
		 IUserClaimStore<TUser>,
		 IUserRoleStore<TUser>,
		 IUserPasswordStore<TUser>,
		 IUserSecurityStampStore<TUser>,
		 IQueryableUserStore<TUser>,
		 IUserEmailStore<TUser>,
		 IUserPhoneNumberStore<TUser>,
		 IUserTwoFactorStore<TUser>,
		 IUserLockoutStore<TUser>,
		 IUserAuthenticationTokenStore<TUser>,
		 IUserAuthenticatorKeyStore<TUser>,
		 IUserTwoFactorRecoveryCodeStore<TUser>
		 where TKey : IEquatable<TKey>
		 where TUser : class, IXPUser<TKey>, new()
		 where TXPOUser : XPBaseObject, IXPUser<TKey>
		 where TXPORole : XPBaseObject, IXPRole<TKey>
		 where TXPOLogin : XPBaseObject, IXPUserLogin<TKey>
		 where TXPOClaim : XPBaseObject, IXPUserClaim<TKey>
		 where TXPOToken : XPBaseObject, IXPUserToken<TKey>
#else
	public class XPUserStore<TKey, TUser, TXPOUser, TXPORole, TXPOLogin, TXPOClaim> : XPDataStore<TKey, TUser, TXPOUser>, 	
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
		 where TKey : IEquatable<TKey>
		 where TUser : class, IXPUser<TKey>, new()
		 where TXPOUser : XPBaseObject, IXPUser<TKey>, IUser<TKey>
		 where TXPORole : XPBaseObject, IRole<TKey>
		 where TXPOLogin : XPBaseObject, IXPUserLogin<TKey>
		 where TXPOClaim : XPBaseObject, IXPUserClaim<TKey>
#endif
	{
		public XPUserStore(XpoDatabase db, XPDataMapper<TKey, TUser, TXPOUser> mapper, XPDataValidator<TKey, TUser, TXPOUser> validator)
			: base(db, mapper, validator)
		{

		}

		public override TKey GetModelKey(TUser model) => model.Id;
		public override void SetModelKey(TUser model, TKey key) => model.Id = key;

		protected override IQueryable<TXPOUser> XPQuery()
		{
			var r = XPQuery(UnitOfWork);
			return r;

		}

		protected override IQueryable<TXPOUser> XPQuery(Session s )
		{
			var r = from n in s.Query<TXPOUser>()
					select n;
			return r;

		}
		public override IQueryable<TUser> Query() => XPQuery().Select(CreateModelInstance).AsQueryable();


		#region Generic Helper methods and members
		protected static Type XPOUserType { get { return typeof(TXPOUser); } }
		protected static Type XPORoleType { get { return typeof(TXPORole); } }
		protected static Type XPOLoginType { get { return typeof(TXPOLogin); } }
		protected static Type XPOClaimType { get { return typeof(TXPOClaim); } }

		protected static TXPOUser XPOCreateUser(Session s) { return Activator.CreateInstance(typeof(TXPOUser), s) as TXPOUser; }
		protected static TXPORole XPOCreateRole(Session s) { return Activator.CreateInstance(typeof(TXPORole), s) as TXPORole; }
		protected static TXPOLogin XPOCreateLogin(Session s) { return Activator.CreateInstance(typeof(TXPOLogin), s) as TXPOLogin; }
		protected static TXPOClaim XPOCreateClaim(Session s) { return Activator.CreateInstance(typeof(TXPOClaim), s) as TXPOClaim; }
#if (NETSTANDARD2_1 || NETCOREAPP)
		protected static Type XPOTokenType { get { return typeof(TXPOToken); } }
		protected static TXPOToken XPOCreateToken(Session s) { return Activator.CreateInstance(typeof(TXPOToken), s) as TXPOToken; }
#endif
#endregion


		#region IUserLoginStore<TUser, TKey>
#if (NETSTANDARD2_1 || NETCOREAPP)
		public async virtual Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AddLoginAsync(user, login);
		}

		public async virtual Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await RemoveLoginAsync(user, new UserLoginInfo(loginProvider, providerKey, string.Empty));
		}

		public async virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var result = await GetLoginsAsync(user);
			return result;
		}

		public async virtual Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (String.IsNullOrEmpty(loginProvider))
				throw new ArgumentNullException(nameof(loginProvider));
			if (String.IsNullOrEmpty(providerKey))
				throw new ArgumentNullException(nameof(providerKey));

			var result = await DB.ExecuteAsync((db, wrk) =>
			{
				var xpoUser = wrk.FindObject(XPOUserType, CriteriaOperator.Parse("Logins[(LoginProvider == ?) AND (ProviderKey == ?)]", loginProvider, providerKey));

				//return xpoUser == null ? null : Activator.CreateInstance(typeof(TUser), xpoUser, DxIdentityUserFlags.FLAG_FULL) as TUser;
				return (xpoUser == null) ? null : Mapper.CreateModel(xpoUser as TXPOUser);
			});
			return result;
		}

		public virtual string ConvertIdToString(TKey id)
		{
			if (id == null)
				return null;
			else if (id.Equals(default(TKey)))
				return null;

			return id.ToString();
		}

		public virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(ConvertIdToString(user.Id));
		}

		public virtual Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(user.UserName);
		}

		public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user == null");
			if (String.IsNullOrEmpty(userName))
				throw new ArgumentNullException("userName = empty");

			user.UserName = userName;
			return Task.FromResult<object>(null);
		}

		public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user == null");

			return Task.FromResult(user.NormalizedName);
		}

		public virtual Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user == null");
			if (String.IsNullOrEmpty(normalizedName))
				throw new ArgumentNullException("normalizedName = empty");

			user.NormalizedName = normalizedName;
			return Task.FromResult<object>(null);

		}

		public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			try
			{
				await CreateAsync(user);
			}
			catch (Exception err)
			{
				return IdentityResult.Failed(new IdentityError { Code = "100", Description = err.Message });
			}
			return IdentityResult.Success;
		}

		public async virtual Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			try
			{
				await UpdateAsync(user);
			}
			catch (Exception err)
			{
				return IdentityResult.Failed(new IdentityError { Code = "100", Description = err.Message });
			}
			return IdentityResult.Success;
		}

		public async virtual Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			try
			{
				await DeleteAsync(user);
			}
			catch (Exception err)
			{
				return IdentityResult.Failed(new IdentityError { Code = "100", Description = err.Message });
			}
			return IdentityResult.Success;
		}

		public async virtual Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			//Change by picopixel 
			var id = TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromString(userId);
			var result = await FindByIdAsync(id);
			//var result = await FindByIdAsync(userId);
			return result;
		}

		public async virtual Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			var result = await FindByNameAsync(normalizedUserName);
			return result;
		}
#endif


		public async virtual Task AddLoginAsync(TUser user, UserLoginInfo login)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (login == null)
			{
				throw new ArgumentNullException(nameof(login));
			}

			var result = await DB.ExecuteAsync((db, wrk) =>
			{
				TXPOLogin xpoLogin = XPOCreateLogin(wrk);
				xpoLogin.LoginProvider = login.LoginProvider;
				xpoLogin.ProviderKey = login.ProviderKey;

				var usr = wrk.GetObjectByKey(XPOUserType, user.Id);
				if (usr != null)
					xpoLogin.SetMemberValue("User", usr);
				else
					throw new Exception($"User '{user.UserName}' could not be found in the database");
				return 0;
			});
		}

		public async virtual Task<TUser> FindAsync(UserLoginInfo login)
		{
			ThrowIfDisposed();
			if (login == null)
			{
				throw new ArgumentNullException(nameof(login));
			}
			var result = await DB.ExecuteAsync((db, s) =>
			{
				var xpoUser = s.FindObject(XPOUserType, CriteriaOperator.Parse("Logins[(LoginProvider == ?) AND (ProviderKey == ?)]", login.LoginProvider, login.ProviderKey)) as TXPOUser;
				return xpoUser == null ? null : Mapper.CreateModel(xpoUser);
				//var userLogin = s.FindObject(XPOLoginType, CriteriaOperator.Parse("(LoginProvider == ?) AND (ProviderKey == ?)", login.LoginProvider, login.ProviderKey)) as XPBaseObject;
				//if (userLogin != null)
				//{
				//	var xpoUser = userLogin.GetMemberValue("User") as TXPOUser;
				//	return xpoUser == null ? null : Mapper.CreateModel(xpoUser);
				//}
				//return null;
			});
			return result;
		}

		public async virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var result = await DB.ExecuteAsync<IList<UserLoginInfo>>((db, wrk) =>
			{
				var results = new List<UserLoginInfo>();
				foreach (var r in new XPCollection(wrk, typeof(TXPOLogin), CriteriaOperator.Parse("[User!Key] == ?", user.Id)))
				{
					IXPUserLogin<TKey> xpoLogin = r as IXPUserLogin<TKey>;
#if (NETSTANDARD2_1 || NETCOREAPP)
					results.Add(new UserLoginInfo(xpoLogin.LoginProvider, xpoLogin.ProviderKey, xpoLogin.LoginProvider));
#else
					results.Add(new UserLoginInfo(xpoLogin.LoginProvider, xpoLogin.ProviderKey));
#endif
				}
				return results;
			}, false);
			return result;
		}

		public async virtual Task RemoveLoginAsync(TUser user, UserLoginInfo login)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (login == null)
			{
				throw new ArgumentNullException(nameof(login));
			}
			await DB.ExecuteAsync((db, wrk) =>
			{
				//Typo in CriteriaOperator found by picopixel
				wrk.Delete(wrk.FindObject(typeof(TXPOLogin),
						 CriteriaOperator.Parse("([User!Key] == ?) AND (LoginProvider == ?) AND (ProviderKey == ?)",
														 user.Id, login.LoginProvider, login.ProviderKey)));
				//return 0;
			});
		}

		//Task IUserStore<TUser, TKey>.CreateAsync(TUser user)
		//{
		//	throw new NotImplementedException();
		//}

		//Task IUserStore<TUser, TKey>.DeleteAsync(TUser user)
		//{
		//	throw new NotImplementedException();
		//}

		public new async virtual Task CreateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (String.IsNullOrEmpty(user.UserName))
				user.UserName = user.Email;

			var result = await base.CreateAsync(user);
			if (!result.Success)
				throw new DataValidationException<TKey>(result);
		}


		public new async virtual Task DeleteAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			var result = await base.DeleteAsync(user.Id);
			if (!result.Success)
				throw new DataValidationException<TKey>(result);
		}

		public async virtual Task<TUser> FindByIdAsync(object userId)
		{
			ThrowIfDisposed();

			var result = await FindByIdAsync((TKey)userId);
			return result;
		}
		public async virtual Task<TUser> FindByIdAsync(TKey userId)
		{
			ThrowIfDisposed();
			var result = await base.GetByKeyAsync(userId);

			return result;
		}

		public async virtual Task<TUser> FindByNameAsync(string userName)
		{
			ThrowIfDisposed();

			var result = await DB.ExecuteAsync((db, wrk) =>
			{
				try
				{
#if (NETSTANDARD2_1 || NETCOREAPP)

					var xpoUser = wrk.FindObject(XPOUserType, CriteriaOperator.Parse("NormalizedName == ?", userName));
#else
				var xpoUser = wrk.FindObject(XPOUserType, XpoDxUser.Fields.UserNameUpper == userName.ToUpperInvariant());
#endif
					if (xpoUser != null)
					{
						TUser r = Mapper.CreateModel(xpoUser as TXPOUser);
						return r;
					}
				}
				catch(Exception ex)
                {
					string s = ex.Message;

                }
				
				return null;
			});
			return result;
		}

		public new async virtual Task UpdateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var result = await base.UpdateAsync(user);
		}
		#endregion

		#region IUserClaimStore<TUser, TKey>
#if (NETSTANDARD2_1 || NETCOREAPP)
		public async virtual Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();

			var result = await GetClaimsAsync(user);
			return result;
		}

		public async virtual Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (claims == null)
			{
				throw new ArgumentNullException(nameof(claims));
			}

			await DB.ExecuteAsync((db, wrk) =>
			{
				foreach (var claim in claims)
				{
					var xpoClaim = XPOCreateClaim(wrk);
					xpoClaim.SetMemberValue("User", wrk.GetObjectByKey(XPOUserType, user.Id));
					xpoClaim.ClaimType = claim.Type;
					xpoClaim.ClaimValue = claim.Value;
				}
			});
		}

		public async virtual Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();

			await DB.ExecuteAsync((db, wrk) =>
			{
				XPCollection xpoClaims = new XPCollection(typeof(XpoDxUserClaim),
					CriteriaOperator.Parse("[User!Key] == ? AND ClaimValue == ? AND ClaimType == ?",
											user.Id, claim.Value, claim.Type), null);

				foreach (var item in xpoClaims)
				{
					var xpoClaim = item as XpoDxUserClaim;
					if (xpoClaim != null)
					{
						xpoClaim.ClaimType = newClaim.Type;
						xpoClaim.ClaimValue = newClaim.Value;
					}
				}
			});
		}

		public async virtual Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();

			await DB.ExecuteAsync((db, wrk) =>
			{
				foreach (var claim in claims)
				{
					XPCollection xpoClaims = new XPCollection(wrk, typeof(XpoDxUserClaim),
						CriteriaOperator.Parse("[User!Key] == ? AND ClaimValue == ? AND ClaimType == ?",
											user.Id, claim.Value, claim.Type), null);
					foreach (var item in xpoClaims)
					{
						var xpoClaim = item as XpoDxUserClaim;
						if (xpoClaim != null)
						{
							xpoClaim.User = null;
						}
					}
					wrk.Delete(xpoClaims);
				}
			});
		}

		public async virtual Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (claim == null)
			{
				throw new ArgumentNullException(nameof(claim));
			}

			var result = await DB.ExecuteAsync((db, wrk) =>
			{
				XPCollection list = new XPCollection(wrk, typeof(XpoDxUser),
					XpoDxUser.Fields.Claims[
						XpoDxUserClaim.Fields.ClaimType == claim.Type &
						XpoDxUserClaim.Fields.ClaimValue == claim.Value], null);

				List<TUser> users = new List<TUser>();
				foreach (var item in list)
				{
					TUser usr = Mapper.CreateModel(item as TXPOUser);
					users.Add(usr);
				}
				return users;
			});

			return result;
		}
#endif

		public async virtual Task AddClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (claim == null)
			{
				throw new ArgumentNullException(nameof(claim));
			}
			await DB.ExecuteAsync((db, wrk) =>
			{
				var xpoClaim = XPOCreateClaim(wrk);
				xpoClaim.SetMemberValue("User", wrk.GetObjectByKey(XPOUserType, user.Id));
				xpoClaim.ClaimType = claim.Type;
				xpoClaim.ClaimValue = claim.Value;
			});
		}

		public async virtual Task<IList<Claim>> GetClaimsAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			var result = await DB.ExecuteAsync((db, wrk) =>
			{
				var results = new List<Claim>();
				foreach (var c in new XPCollection(wrk, XPOClaimType, CriteriaOperator.Parse("[User!Key] == ?", user.Id)))
				{
					TXPOClaim xpoClaim = c as TXPOClaim;
					results.Add(new Claim(xpoClaim.ClaimType, xpoClaim.ClaimValue));
				}
				return results;
			}, false);
			return result;
		}

		public async virtual Task RemoveClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (claim == null)
			{
				throw new ArgumentNullException(nameof(claim));
			}
			await DB.ExecuteAsync((db, wrk) =>
			{
				wrk.Delete(new XPCollection(wrk, XPOClaimType,
						 CriteriaOperator.Parse("([User!Key] == ?) AND (ClaimType == ?) AND (ClaimValue == ?)",
													user.Id, claim.Type, claim.Value), null));
			});
		}
		#endregion

		#region IUserRoleStore<TUser, TKey>
#if (NETSTANDARD2_1 || NETCOREAPP)
		public async virtual Task AddToRoleAsync(TUser user, string roleName,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AddToRoleAsync(user, roleName);
		}

		public async virtual Task RemoveFromRoleAsync(TUser user, string roleName,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			await RemoveFromRoleAsync(user, roleName);
		}

		public async virtual Task<IList<string>> GetRolesAsync(TUser user,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			return await GetRolesAsync(user);
		}

		public async virtual Task<bool> IsInRoleAsync(TUser user, string roleName,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			return await IsInRoleAsync(user, roleName);
		}

		public async virtual Task<IList<TUser>> GetUsersInRoleAsync(string roleName,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			return await GetUsersInRoleAsync(roleName);
		}
		public virtual async Task<IList<TUser>> GetUsersInRoleAsync(string roleName)
		{
			ThrowIfDisposed();
			IList<TUser> result = await DB
				.ExecuteAsync(
					(db, wrk) =>
					{
						var role = wrk.FindObject(
							typeof(TXPORole),
							CriteriaOperator.Parse("(NormalizedName == ?)", roleName)) as TXPORole;
						var member = (XPBaseCollection)role.GetMemberValue(nameof(Users));
						List<TUser> users = new List<TUser>();
						foreach (var item in member)
						{
							TUser usr = Mapper.CreateModel(item as TXPOUser);
							users.Add(usr);
						}
						return users;
					},
					false);
			return result;
		}
#endif

		public async virtual Task AddToRoleAsync(TUser user, string roleName)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (String.IsNullOrWhiteSpace(roleName))
			{
				throw new ArgumentException("role cannot be empty");
			}

			string r = roleName.ToUpperInvariant();
			await DB.ExecuteAsync((db, wrk) =>
			{
				var u = wrk.GetObjectByKey<TXPOUser>(user.Id);
				if (u != null)
				{
#if (NETSTANDARD2_1 || NETCOREAPP)
					var role = wrk.FindObject(typeof(TXPORole),
						CriteriaOperator.Parse("(NormalizedName == ?) AND (NOT Users[ID == ?])", roleName, u.Id)) as TXPORole;
#else
					var role = u.Session.FindObject(typeof(TXPORole),
						CriteriaOperator.Parse("(NameUpper == ?) AND (NOT Users[ID == ?])", r, u.Id)) as TXPORole;
#endif
					if (role == null)
						throw new InvalidOperationException($"Role {roleName} was not found");
					u.RolesList.Add(role);

				}


			}, true);
		}

		public async virtual Task<IList<string>> GetRolesAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			var userId = user.Id;
			var result = await DB.ExecuteAsync((db, wrk) =>
			{
				List<string> r = new List<string>();
				foreach (var role in new XPCollection(wrk, XPORoleType,
						 CriteriaOperator.Parse("Users[ID == ?]", userId),
						 new SortProperty("Name", SortingDirection.Ascending)))
				{
					r.Add(((TXPORole)role).Name);
				}
				return r;
			}, false);
			return result;
		}

		public async virtual Task<bool> IsInRoleAsync(TUser user, string roleName)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (String.IsNullOrWhiteSpace(roleName))
			{
				throw new ArgumentException("roleName cannot be empty");
			}
			var result = await DB.ExecuteAsync((db, wrk) =>
			{
#if (NETSTANDARD2_1 || NETCOREAPP)
				var role = wrk.FindObject(typeof(TXPORole),
						 CriteriaOperator.Parse("(NormalizedName == ?) AND (Users[ID == ?])", roleName, user.Id)) as TXPORole;
#else
				var role = wrk.FindObject(typeof(TXPORole),
						 CriteriaOperator.Parse("(NameUpper == ?) AND (Users[ID == ?])", roleName.ToUpperInvariant(), user.Id)) as TXPORole;
#endif
				return (role != null);
			}, false);
			return result;
		}

		public async virtual Task RemoveFromRoleAsync(TUser user, string roleName)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (String.IsNullOrWhiteSpace(roleName))
			{
				throw new ArgumentException("role cannot be empty");
			}

			var result = await DB.ExecuteAsync((db, wrk) =>
			{
				var u = wrk.FindObject(typeof(TXPOUser), CriteriaOperator.Parse("ID == ?", user.Id)) as TXPOUser;
				if (u == null)
					throw new InvalidOperationException($"User '{user.UserName}' was not found");
#if (NETSTANDARD2_1 || NETCOREAPP)
				var role = wrk.FindObject(typeof(TXPORole),
						 CriteriaOperator.Parse("(NormalizedName == ?) AND (Users[ID == ?])", roleName, user.Id)) as TXPORole;
#else
				var role = wrk.FindObject(typeof(TXPORole),
						 CriteriaOperator.Parse("(NameUpper == ?) AND (Users[ID == ?])", roleName.ToUpperInvariant(), user.Id)) as TXPORole;
#endif
				if (role != null)
					u.RolesList.Remove(role);
				return 0;
			});
		}
		#endregion

		#region IUserPasswordStore<TUser, TKey>
#if (NETSTANDARD2_1 || NETCOREAPP)
		public async virtual Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await SetPasswordHashAsync(user, passwordHash);
		}

		public async virtual Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var result = await GetPasswordHashAsync(user);
			return result;
		}

		public async virtual Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var result = await HasPasswordAsync(user);
			return result;
		}
#endif

		public virtual Task<string> GetPasswordHashAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			return Task.FromResult(user.PasswordHash);
		}

		public virtual Task<bool> HasPasswordAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}


			return Task.FromResult(!String.IsNullOrEmpty(user.PasswordHash));
		}

		public async virtual Task SetPasswordHashAsync(TUser user, string passwordHash)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.PasswordHash = passwordHash;

			await DB.ExecuteAsync((db, wrk) =>
			{
				var u = wrk.GetObjectByKey<TXPOUser>(user.Id);
				if (u != null)
				{
					u.PasswordHash = passwordHash;
				}
			});

			//var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.PasswordHash = user.PasswordHash, false));
		}
		#endregion

		#region IUserSecurityStampStore<TUser, TKey>
#if (NETSTANDARD2_1 || NETCOREAPP)
		public async virtual Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await SetSecurityStampAsync(user, stamp);
		}

		public async virtual Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var result = await GetSecurityStampAsync(user);
			return result;
		}
#endif
		public virtual Task<string> GetSecurityStampAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(user.SecurityStamp);
		}

		public async virtual Task SetSecurityStampAsync(TUser user, string stamp)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.SecurityStamp = stamp;
			await DB.ExecuteAsync((db, wrk) =>
			{
				var u = wrk.GetObjectByKey<TXPOUser>(user.Id);
				if (u != null)
				{
					u.SecurityStamp = stamp;
				}
			});

			//var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.SecurityStamp = user.SecurityStamp, false));
		}
		#endregion

		#region IQueryableUserStore<TUser, TKey>
		//protected virtual Func<TXPOUser, TUser> CreateUserInstance => (x) =>
		//{
		//	var result = Activator.CreateInstance(typeof(TUser), x) as TUser;
		//	//result.Assign(x, 0);
		//	return result;
		//};
		public virtual IQueryable<TUser> Users
		{
			get
			{
				//TODO: Might need to check this for memoryleak
				var session = DB.GetSession();
				session.Disposed += (s, e) =>
				{
					Trace.WriteLine("IQueryable<TUser> Users session disposed");
				};

				var r = session.Query<TXPOUser>().Select(CreateModelInstance);
				return r.AsQueryable();
			}
		}

		#endregion

		#region IUserEmailStore<TUser, TKey>
#if (NETSTANDARD2_1 || NETCOREAPP)
		public virtual Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return SetEmailAsync(user, email);
		}

		public virtual Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return GetEmailAsync(user);
		}

		public virtual Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return GetEmailConfirmedAsync(user);
		}

		public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return SetEmailConfirmedAsync(user, confirmed);
		}

		public async virtual Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return await FindByEmailAsync(normalizedEmail);
		}

		public virtual Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(user.NormalizedEmail);
		}

		public virtual Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.NormalizedEmail = normalizedEmail;
			return Task.CompletedTask;
		}
#endif

		public async virtual Task<TUser> FindByEmailAsync(string email)
		{
			ThrowIfDisposed();

			var result = await DB.ExecuteAsync((db, wrk) =>
			{
				var xpoUser = wrk.FindObject(XPOUserType, CriteriaOperator.Parse("EmailUpper == ?", email.ToUpperInvariant()));
				return (xpoUser == null) ? null : Mapper.CreateModel(xpoUser as TXPOUser);
			});
			return result;
		}

		public virtual Task<string> GetEmailAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			return Task.FromResult(user.Email);
		}

		public virtual Task<bool> GetEmailConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(user.EmailConfirmed); ;
		}

		public virtual Task SetEmailAsync(TUser user, string email)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.Email = email;
			return Task.CompletedTask;
		}

		public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.EmailConfirmed = confirmed;
			return Task.CompletedTask;
		}
		#endregion

		#region IUserPhoneNumberStore<TUser, TKey>
#if (NETSTANDARD2_1 || NETCOREAPP)
		public virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return SetPhoneNumberAsync(user, phoneNumber);
		}

		public virtual Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return GetPhoneNumberAsync(user);
		}

		public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return GetPhoneNumberConfirmedAsync(user);
		}

		public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return SetPhoneNumberConfirmedAsync(user, confirmed);
		}

#endif
		public virtual Task<string> GetPhoneNumberAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(user.PhoneNumber);
		}

		public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(user.PhoneNumberConfirmed);
		}

		public virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.PhoneNumber = phoneNumber;
			return Task.CompletedTask;
		}

		public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.PhoneNumberConfirmed = confirmed;
			return Task.CompletedTask;
		}
		#endregion

		#region IUserTwoFactorStore<TUser, TKey>
#if (NETSTANDARD2_1 || NETCOREAPP)
		public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return SetTwoFactorEnabledAsync(user, enabled);
		}

		public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return GetTwoFactorEnabledAsync(user);
		}

#endif
		public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(user.TwoFactorEnabled);
		}

		public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.TwoFactorEnabled = enabled;
			return Task.CompletedTask;
		}
		#endregion

		#region IUserLockoutStore<TUser, TKey>
#if (NETSTANDARD2_1 || NETCOREAPP)
		public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			DateTimeOffset? result = null;
			if (user.LockoutEndDateUtc.HasValue)
				result = new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc));
			//	result = new DateTimeOffset(user.LockoutEndDateUtc.Value);            


			return Task.FromResult(result);
		}

		public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return SetLockoutEndDateAsync(user, lockoutEnd.Value.UtcDateTime);
		}

		public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return IncrementAccessFailedCountAsync(user);
		}

		public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return ResetAccessFailedCountAsync(user);
		}

		public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return GetAccessFailedCountAsync(user);
		}

		public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return GetLockoutEnabledAsync(user);
		}

		public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return SetLockoutEnabledAsync(user, enabled);
		}

#endif
		public virtual Task<int> GetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(user.AccessFailedCount);
		}

		public virtual Task<bool> GetLockoutEnabledAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			return Task.FromResult(user.LockoutEnabled);
		}

		public virtual Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var result = user.LockoutEndDateUtc.HasValue
				? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
				: new DateTimeOffset();

			return Task.FromResult(result);
		}

		public virtual Task<int> IncrementAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.AccessFailedCount++;
			return Task.FromResult(user.AccessFailedCount);
		}

		public virtual Task ResetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			user.AccessFailedCount = 0;
			return Task.CompletedTask;
		}

		public virtual Task SetLockoutEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.LockoutEnabled = enabled;
			return Task.CompletedTask;
		}

		public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
			return Task.CompletedTask;
		}


		#endregion

#if (NETCOREAPP)


		#region IUserAuthenticationTokenStore<TUser>

		protected virtual TXPOToken FindToken(Session wrk, TUser user, string loginProvider, string name, bool createIfNotFound = false)
		{
			if (wrk == null)
				throw new ArgumentNullException(nameof(wrk));

			TXPOToken xpoToken = wrk.FindObject(XPOTokenType,
				CriteriaOperator.Parse("[User!Key] == ? AND LoginProvider == ? AND Name == ?", user.Id, loginProvider, name)) as TXPOToken;

			if (createIfNotFound && (xpoToken == null))
			{
				xpoToken = Activator.CreateInstance(XPOTokenType, wrk) as TXPOToken;
				xpoToken.Name = name;
				xpoToken.LoginProvider = loginProvider;

				var xpoUser = wrk.GetObjectByKey(XPOUserType, user.Id);
				if (xpoUser != null)
					xpoToken.SetMemberValue("User", xpoUser);
				else
					throw new Exception($"User '{user.Id}' could not be found in the database");
			}
			return xpoToken;
		}
		public async virtual Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			await DB.ExecuteAsync((db, wrk) =>
			{
				var xpoToken = FindToken(wrk, user, loginProvider, name, true);
				xpoToken.Value = value;

			});
		}

		public async virtual Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			await DB.ExecuteAsync((db, wrk) =>
			{
				var xpoToken = FindToken(wrk, user, loginProvider, name, false);
				if (xpoToken != null)
					wrk.Delete(xpoToken);
			});

		}

		public virtual async Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			var result = await DB.ExecuteAsync((db, wrk) =>
			{
				var xpoToken = FindToken(wrk, user, loginProvider, name, false);
				return xpoToken?.Value;
			});
			return result;
		}
		#endregion

		#region IUserAuthenticatorKeyStore<TUser>
		private const string InternalLoginProvider = "[AspNetUserStore]";
		private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
		private const string RecoveryCodeTokenName = "RecoveryCodes";

		public async virtual Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
		{
			await SetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, key, cancellationToken);
		}

		public async virtual Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
		{
			var result = await GetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, cancellationToken);
			return result;
		}
		#endregion

		#region IUserTwoFactorRecoveryCodeStore<TUser>
		public virtual Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
		{
			var mergedCodes = string.Join(";", recoveryCodes);
			return SetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, mergedCodes, cancellationToken);
		}

		public async virtual Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			if (code == null)
			{
				throw new ArgumentNullException(nameof(code));
			}

			var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? string.Empty;
			var splitCodes = mergedCodes.Split(';');
			if (splitCodes.Contains(code))
			{
				var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
				await ReplaceCodesAsync(user, updatedCodes, cancellationToken);
				return true;
			}
			return false;
		}

		public async virtual Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? string.Empty;
			if (mergedCodes.Length > 0)
			{
				return mergedCodes.Split(';').Length;
			}
			return 0;
		}
		#endregion

#endif

	}
#endif
}
