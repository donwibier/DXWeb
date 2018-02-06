using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DX.Data.Xpo.Identity.Persistent;
using System.Threading;
#if (NETSTANDARD2_0)
using Microsoft.AspNetCore.Identity;
#else
using Microsoft.AspNet.Identity;
#endif

namespace DX.Data.Xpo.Identity
{
    public class XPUserStore<TUser> :
		XPUserStore<TUser, XpoDxUser>
		 where TUser : XPIdentityUser<string, XpoDxUser>, IUser<string>, new()
	{
		public XPUserStore() { }

		public XPUserStore(string connectionName) :
			base(connectionName)
		{
		}

		public XPUserStore(string connectionString, string connectionName) :
			base(connectionString, connectionName)
		{

		}

		public XPUserStore(XpoDatabase database) :
			base(database)
		{

		}
	}

	public class XPUserStore<TUser, TXPOUser> :
	 XPUserStore<string, TUser, TXPOUser, XpoDxRole, XpoDxUserLogin, XpoDxUserClaim>,
	 IUserStore<TUser>
	 where TUser : XPIdentityUser<string, TXPOUser>, IUser<string>, new()
	 where TXPOUser : XpoDxUser, IDxUser<string>, IUser<string>
	{
		public XPUserStore() :
			base()
		{

		}

		public XPUserStore(string connectionName) :
			base(connectionName)
		{

		}

		public XPUserStore(string connectionString, string connectionName) :
			base(connectionString, connectionName)
		{

		}

		public XPUserStore(XpoDatabase database) :
			base(database)
		{

		}
	}
#if (NETSTANDARD2_0)
    public class XPUserStore<TKey, TUser, TXPOUser, TXPORole, TXPOLogin, TXPOClaim> : XpoStore<TXPOUser, TKey>,
         IUserLoginStore<TUser>,
         IUserClaimStore<TUser>,
         IUserRoleStore<TUser>,
         IUserPasswordStore<TUser>,
         IUserSecurityStampStore<TUser>/*,
         IQueryableUserStore<TUser >,
         IUserEmailStore<TUser>,
         IUserPhoneNumberStore<TUser>,
         IUserTwoFactorStore<TUser>,
         IUserLockoutStore<TUser>*/
         where TKey : IEquatable<TKey>
         where TUser : XPIdentityUser<TKey, TXPOUser>, IUser<TKey>, new()
         where TXPOUser : XPBaseObject, IDxUser<TKey>, IUser<TKey>
         where TXPORole : XPBaseObject, IDxRole<TKey>, IRole<TKey>
         where TXPOLogin : XPBaseObject, IDxUserLogin<TKey>
         where TXPOClaim : XPBaseObject, IDxUserClaim<TKey>
#else
    public class XPUserStore<TKey, TUser, TXPOUser, TXPORole, TXPOLogin, TXPOClaim> : XpoStore<TXPOUser, TKey>,
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
		 where TUser : XPIdentityUser<TKey, TXPOUser>, IUser<TKey>
		 where TXPOUser : XPBaseObject, IDxUser<TKey>, IUser<TKey>
		 where TXPORole : XPBaseObject, IDxRole<TKey>, IRole<TKey>
		 where TXPOLogin : XPBaseObject, IDxUserLogin<TKey>
		 where TXPOClaim : XPBaseObject, IDxUserClaim<TKey>
#endif
    {

        public XPUserStore() :
			base()
		{

		}

		public XPUserStore(string connectionName) :
			base(connectionName)
		{

		}

		public XPUserStore(string connectionString, string connectionName) :
			base(connectionString, connectionName)
		{

		}

		public XPUserStore(XpoDatabase database) :
			base(database)
		{

		}


    #region Generic Helper methods and members
		protected static Type XPOUserType { get { return typeof(TXPOUser); } }
		protected static Type XPORoleType { get { return typeof(TXPORole); } }
		protected static Type XPOLoginType { get { return typeof(TXPOLogin); } }
		protected static Type XPOClaimType { get { return typeof(TXPOClaim); } }

		protected static TXPOUser XPOCreateUser(Session s) { return Activator.CreateInstance(typeof(TXPOUser), s) as TXPOUser; }
		protected static TXPORole XPOCreateRole(Session s) { return Activator.CreateInstance(typeof(TXPORole), s) as TXPORole; }
		protected static TXPOLogin XPOCreateLogin(Session s) { return Activator.CreateInstance(typeof(TXPOLogin), s) as TXPOLogin; }
		protected static TXPOClaim XPOCreateClaim(Session s) { return Activator.CreateInstance(typeof(TXPOClaim), s) as TXPOClaim; }
        #endregion


        #region IUserLoginStore<TUser, TKey>
#if (NETSTANDARD2_0)
        public async virtual Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddLoginAsync(user, login);
        }

        public async virtual Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RemoveLoginAsync(user, new UserLoginInfo(loginProvider, providerKey, ""));
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
                throw new ArgumentNullException("loginProvider");
            if (String.IsNullOrEmpty(providerKey))
                throw new ArgumentNullException("providerKey");

            var result = await Task.FromResult(XPOExecute((db, wrk) =>
            {
                var xpoUser = wrk.FindObject(XPOUserType, CriteriaOperator.Parse("Logins[(LoginProvider == ?) AND (ProviderKey == ?)]", loginProvider, providerKey));
                return xpoUser == null ? null : Activator.CreateInstance(typeof(TUser), xpoUser, DxIdentityUserFlags.FLAG_FULL) as TUser;
            }));
            return result;
        }

        public virtual string ConvertIdToString(TKey id)
        {
            if (id.Equals(default(TKey)))
            {
                return null;
            }
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

            var result = await FindByIdAsync(userId);
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
				throw new ArgumentNullException("user");
			}
			if (login == null)
			{
				throw new ArgumentNullException("login");
			}

			var result = await Task.FromResult(XPOExecute<int>((db, s) =>
			{
				TXPOLogin xpoLogin = XPOCreateLogin(s);
				xpoLogin.LoginProvider = login.LoginProvider;
				xpoLogin.ProviderKey = login.ProviderKey;
				xpoLogin.SetMemberValue("User", s.GetObjectByKey(XPOUserType, user.Id));

				//if (xpoLogin.User == null)
				//	 throw new Exception(String.Format("The user with id '{0}' could not be found in the database", user.Id));
				return 0;
			}));
		}

		public async virtual Task<TUser> FindAsync(UserLoginInfo login)
		{
			ThrowIfDisposed();
			if (login == null)
			{
				throw new ArgumentNullException("login");
			}
			var result = await Task.FromResult(XPOExecute<TUser>((db, s) =>
			{
				var userLogin = s.FindObject(XPOLoginType, CriteriaOperator.Parse("(LoginProvider == ?) AND (ProviderKey == ?)", login.LoginProvider, login.ProviderKey)) as XPBaseObject;
				return (userLogin == null) ? null : Activator.CreateInstance(typeof(TUser), userLogin.GetMemberValue("User"), DxIdentityUserFlags.FLAG_FULL) as TUser;
			}));
            return result;
		}

		public async virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			var result = await Task.FromResult(XPOExecute<IList<UserLoginInfo>>((db, wrk) =>
			{
				var results = new List<UserLoginInfo>();
				foreach (var r in new XPCollection(wrk, typeof(TXPOLogin), CriteriaOperator.Parse("[User!Key] == ?", user.Id)))
				{
					IDxUserLogin<TKey> xpoLogin = r as IDxUserLogin<TKey>;
#if (NETSTANDARD2_0)
                    results.Add(new UserLoginInfo(xpoLogin.LoginProvider, xpoLogin.ProviderKey, xpoLogin.LoginProvider));
#else
                    results.Add(new UserLoginInfo(xpoLogin.LoginProvider, xpoLogin.ProviderKey));
#endif
                }
				return results;
			}, false));
            return result;
		}

		public async virtual Task RemoveLoginAsync(TUser user, UserLoginInfo login)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (login == null)
			{
				throw new ArgumentNullException("login");
			}
			var result = await Task.FromResult(XPOExecute<int>((db, wrk) =>
			{
				wrk.Delete(wrk.FindObject(typeof(TXPOLogin),
						 CriteriaOperator.Parse("([User!Key] == ?) AND (LoginProvider == ?) AND )ProviderKey == ?)",
														 user.Id, login.LoginProvider, login.ProviderKey)));
				return 0;
			}));
		}

		public async virtual Task CreateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (String.IsNullOrEmpty(user.UserName))
				user.UserName = user.Email;

            var result = await Task.FromResult(XPOExecute((db, wrk) => 
            {
                var xpoUser = XPOCreateUser(wrk);

                xpoUser.Assign(user, DxIdentityUserFlags.FLAG_FULL);
                wrk.CommitTransaction();
                user.Assign(xpoUser, DxIdentityUserFlags.FLAG_FULL);
                return 0;
            }));
		}


		public async virtual Task DeleteAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			var result = await Task.FromResult(XPOExecute<int>((db, wrk) =>
			{
				wrk.Delete(wrk.GetObjectByKey(XPOUserType, user.Id));
				return 0;
			}));
		}

		public async virtual Task<TUser> FindByIdAsync(object userId)
		{
			ThrowIfDisposed();

			var result = await Task.FromResult(XPOExecute<TUser>((db, wrk) =>
			{
				var xpoUser = wrk.GetObjectByKey(XPOUserType, userId);
				return xpoUser == null ? null : Activator.CreateInstance(typeof(TUser), xpoUser, DxIdentityUserFlags.FLAG_FULL) as TUser;
			}));
            return result;
		}
        public async virtual Task<TUser> FindByIdAsync(TKey userId)
        {
            return await FindByIdAsync(userId);
        }

        public async virtual Task<TUser> FindByNameAsync(string userName)
		{
			ThrowIfDisposed();

			var result = await Task.FromResult(XPOExecute((db, wrk) =>
			{
				var xpoUser = wrk.FindObject(XPOUserType, XpoDxUser.Fields.UserNameUpper ==  userName.ToUpperInvariant());
				return xpoUser == null ? null : Activator.CreateInstance(typeof(TUser), xpoUser, DxIdentityUserFlags.FLAG_FULL) as TUser;
			}));
            return result;
		}

		public async virtual Task UpdateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			var result = await Task.FromResult(XPOExecute<object>((db, wrk) =>
			{
				TXPOUser u = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
				if (u != null)
				{
					u.Assign(user, DxIdentityUserFlags.FLAG_FULL);
				}

				return null;
			}));
		}
        #endregion

        #region IUserClaimStore<TUser, TKey>
#if (NETSTANDARD2_0)
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
                throw new ArgumentNullException("user");
            }
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            var result = await Task.FromResult(XPOExecute<object>((db, wrk) =>
            {
                foreach (var claim in claims)
                {
                    var xpoClaim = XPOCreateClaim(wrk);
                    xpoClaim.SetMemberValue("User", wrk.GetObjectByKey(XPOUserType, user.Id));
                    xpoClaim.ClaimType = claim.Type;
                    xpoClaim.ClaimValue = claim.Value;
                }
                return null;
            }));
            //return Task.FromResult(false);
        }

        public async virtual Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var result = await Task.FromResult(XPOExecute<object>((db, wrk) =>
            {
                XPCollection xpoClaims = new XPCollection(typeof(XpoDxUserClaim),
                    CriteriaOperator.Parse("[User!Id] == ?", user.Id) &
                    XpoDxUserClaim.Fields.ClaimValue == claim.Value &
                    XpoDxUserClaim.Fields.ClaimType == claim.Type, null);

                foreach (var item in xpoClaims)
                {
                    var xpoClaim = item as XpoDxUserClaim;
                    if (xpoClaim != null)
                    {
                        xpoClaim.ClaimType = newClaim.Type;
                        xpoClaim.ClaimValue = newClaim.Value;
                    }
                }
                return null;
            }));

        }

        public async virtual Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var result = await Task.FromResult(XPOExecute<object>((db, wrk) =>
            {
                foreach (var claim in claims)
                {
                    XPCollection xpoClaims = new XPCollection(wrk, typeof(XpoDxUserClaim),
                        CriteriaOperator.Parse("[User!Id] == ?", user.Id) &
                        XpoDxUserClaim.Fields.ClaimValue == claim.Value &
                        XpoDxUserClaim.Fields.ClaimType == claim.Type, null);
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
                return null;
            }));
        }

        public async virtual Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var result = await Task.FromResult(XPOExecute<IList<TUser>>((db, wrk) =>
            {
                XPCollection list = new XPCollection(wrk, typeof(XpoDxUser),
                    XpoDxUser.Fields.Claims[
                        XpoDxUserClaim.Fields.ClaimType == claim.Type &
                        XpoDxUserClaim.Fields.ClaimValue == claim.Value], null);

                List<TUser> users = new List<TUser>();
                foreach (var item in list)
                {
                    TUser usr = new TUser();
                    usr.Assign(item, 0);
                    users.Add(usr);
                }
                return users;
            }));

            return result;
        }
#endif

        public async virtual Task AddClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (claim == null)
			{
				throw new ArgumentNullException("claim");
			}
			var result = await Task.FromResult(XPOExecute<object>((db, wrk) =>
			{
				var xpoClaim = XPOCreateClaim(wrk);
				xpoClaim.SetMemberValue("User", wrk.FindObject(XPOUserType, CriteriaOperator.Parse($"{KeyField} == ?", user.Id)));
				xpoClaim.ClaimType = claim.Type;
				xpoClaim.ClaimValue = claim.Value;
				return null;
			}));            
		}

		public async virtual Task<IList<Claim>> GetClaimsAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			var result = await Task.FromResult(XPOExecute<IList<Claim>>((db, wrk) =>
			{
				var results = new List<Claim>();
				foreach (var c in new XPCollection(wrk, XPOClaimType, CriteriaOperator.Parse("[User!Key] == ?", user.Id)))
				{
					TXPOClaim xpoClaim = c as TXPOClaim;
					results.Add(new Claim(xpoClaim.ClaimType, xpoClaim.ClaimValue));
				}
				return results;
			}, false));
            return result;
		}

		public async virtual Task RemoveClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (claim == null)
			{
				throw new ArgumentNullException("claim");
			}
			var result = await Task.FromResult(XPOExecute<int>((db, wrk) =>
			{
				wrk.Delete(new XPCollection(wrk, XPOClaimType,
						 CriteriaOperator.Parse("([User!Key] == ?) AND (ClaimType == ?) AND (ClaimValue == ?)",
						 user.Id, claim.Type, claim.Value)));
				return 0;
			}));
		}
        #endregion

        #region IUserRoleStore<TUser, TKey>
#if (NETSTANDARD2_0)
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
#endif

        public async virtual Task AddToRoleAsync(TUser user, string roleName)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (String.IsNullOrWhiteSpace(roleName))
			{
				throw new ArgumentException("role cannot be empty");
			}

			string r = roleName.ToUpperInvariant();
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u =>
			{
				var role = u.Session.FindObject(typeof(TXPORole),
						 CriteriaOperator.Parse($"(NameUpper == ?) AND (NOT Users[{KeyField} == ?])", r, u.Id)) as TXPORole;
				if (role == null)
					throw new InvalidOperationException(String.Format("Role {0} was not found", roleName));
				u.RolesList.Add(role);
				return 0;
			}, true));
		}

		public async virtual Task<IList<string>> GetRolesAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			var userId = user.Id;

			var result = await Task.FromResult(XPOExecute<IList<string>>((db, wrk) =>
			{
				List<string> results = new List<string>();
				foreach (var role in new XPCollection(wrk, XPORoleType,
						 CriteriaOperator.Parse($"Users[{KeyField} == ?]", userId),
						 new SortProperty("Name", SortingDirection.Ascending)))
				{
					results.Add(((TXPORole)role).Name);
				}
				return results;
			}, false));
            return result;
		}

		public async virtual Task<bool> IsInRoleAsync(TUser user, string roleName)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (String.IsNullOrWhiteSpace(roleName))
			{
				throw new ArgumentException("roleName cannot be empty");
			}
			var result = await Task.FromResult(XPOExecute<bool>((db, wrk) =>
			{
				var role = wrk.FindObject(typeof(TXPORole),
						 CriteriaOperator.Parse($"(NameUpper == ?) AND (Users[{KeyField} == ?])", roleName.ToUpperInvariant(), user.Id)) as TXPORole;
				return (role != null);
			}, false));
            return result;
		}

		public async virtual Task RemoveFromRoleAsync(TUser user, string roleName)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (String.IsNullOrWhiteSpace(roleName))
			{
				throw new ArgumentException("role cannot be empty");
			}

			var result = await Task.FromResult(XPOExecute<int>((db, wrk) =>
			{
				var u = wrk.FindObject(typeof(TXPOUser), CriteriaOperator.Parse($"{KeyField} == ?", user.Id)) as TXPOUser;
				if (u == null)
					throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

				var role = wrk.FindObject(typeof(TXPORole),
						 CriteriaOperator.Parse($"(NameUpper == ?) AND (Users[{KeyField} == ?])", roleName.ToUpperInvariant(), user.Id)) as TXPORole;
				if (role != null)
					u.RolesList.Remove(role);
				return 0;
			}));
		}
        #endregion

        #region IUserPasswordStore<TUser, TKey>
#if (NETSTANDARD2_0)
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

        public async virtual Task<string> GetPasswordHashAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			var result = await Task.FromResult(XPOExecute<string>((db, wrk) =>
            {
                var u = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
                if (u == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

                return u.PasswordHash;
            }, false));
            return result;
		}

		public async virtual Task<bool> HasPasswordAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

            var result = await GetPasswordHashAsync(user);
            return !String.IsNullOrEmpty(result);
		}

		public async virtual Task SetPasswordHashAsync(TUser user, string passwordHash)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.PasswordHash = passwordHash;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.PasswordHash = user.PasswordHash, false));
        }
        #endregion

        #region IUserSecurityStampStore<TUser, TKey>
#if (NETSTANDARD2_0)
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
        public async virtual Task<string> GetSecurityStampAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            var result = await Task.FromResult(XPOExecute((db, wrk) => 
            {
                var u = wrk.GetObjectByKey(typeof(TXPOUser), user.Id) as TXPOUser;
                if (u == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));
                return u.SecurityStamp;
            }, false));
            return result;
        }

		public async virtual Task SetSecurityStampAsync(TUser user, string stamp)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.SecurityStamp = stamp;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.SecurityStamp = user.SecurityStamp, false));
		}
#endregion

#region IQueryableUserStore<TUser, TKey>
		public virtual IQueryable<TUser> Users
		{
			get
			{
				XPQuery<TXPOUser> q = new XPQuery<TXPOUser>(GetSession());
				var result = from u in q
							 select Activator.CreateInstance(typeof(TUser), u as TXPOUser) as TUser;
				return result;
			}
		}

#endregion

#region IUserEmailStore<TUser, TKey>

		public async virtual Task<TUser> FindByEmailAsync(string email)
		{
			ThrowIfDisposed();

			var result = await Task.FromResult(XPOExecute<TUser>((db, wrk) =>
			{
				var xpoUser = wrk.FindObject(XPOUserType, CriteriaOperator.Parse("EmailUpper == ?", email.ToUpperInvariant()));
				return xpoUser == null ? null : Activator.CreateInstance(typeof(TUser), xpoUser, DxIdentityUserFlags.FLAG_FULL) as TUser;
			}));
            return result;
		}

		public async virtual Task<string> GetEmailAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            var result = await Task.FromResult(XPOExecute<string>((db, wrk) =>
            {
                var xpoUser = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
                if (xpoUser == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

                return xpoUser.Email;
            }));
            return result;
        }

        public async virtual Task<bool> GetEmailConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            var result = await Task.FromResult(XPOExecute<bool>((db, wrk) =>
            {
                var xpoUser = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
                if (xpoUser == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

                return xpoUser.EmailConfirmed;
            }));
            return result;
        }

        public async virtual Task SetEmailAsync(TUser user, string email)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.Email = email;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.Email = user.Email, false));
		}

		public async virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.EmailConfirmed = confirmed;
            var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.EmailConfirmed = user.EmailConfirmed, false));
		}
#endregion

#region IUserPhoneNumberStore<TUser, TKey>

		public async virtual Task<string> GetPhoneNumberAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            var result = await Task.FromResult(XPOExecute<string>((db, wrk) =>
            {
                var xpoUser = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
                if (xpoUser == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

                return xpoUser.PhoneNumber;
            }));
            return result;            
		}

		public async virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            var result = await Task.FromResult(XPOExecute((db, wrk) =>
            {
                var xpoUser = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
                if (xpoUser == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

                return xpoUser.PhoneNumberConfirmed;
            }));
            return result;
        }

        public async virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.PhoneNumber = phoneNumber;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.PhoneNumber = user.PhoneNumber, false));
		}

		public async virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.PhoneNumberConfirmed = confirmed;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.PhoneNumberConfirmed = user.PhoneNumberConfirmed, false));
		}
#endregion

#region IUserTwoFactorStore<TUser, TKey>

		public async virtual Task<bool> GetTwoFactorEnabledAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            var result = await Task.FromResult(XPOExecute((db, wrk) =>
            {
                var xpoUser = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
                if (xpoUser == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

                return xpoUser.TwoFactorEnabled;
            }));
            return result;            
        }

		public async virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.TwoFactorEnabled = enabled;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.TwoFactorEnabled = user.TwoFactorEnabled, false));
		}
#endregion

#region IUserLockoutStore<TUser, TKey>

		public async virtual Task<int> GetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            var result = await Task.FromResult(XPOExecute((db, wrk) =>
            {
                var xpoUser = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
                if (xpoUser == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

                return xpoUser.AccessFailedCount;
            }));
            return result;
        }

        public async virtual Task<bool> GetLockoutEnabledAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            var result = await Task.FromResult(XPOExecute((db, wrk) =>
            {
                var xpoUser = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
                if (xpoUser == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

                return xpoUser.LockoutEnabled;
            }));
            return result;
        }

        public async virtual Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            var result = await Task.FromResult(XPOExecute((db, wrk) =>
            {
                var xpoUser = wrk.GetObjectByKey(XPOUserType, user.Id) as TXPOUser;
                if (xpoUser == null)
                    throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

                return xpoUser.LockoutEndDateUtc;
            }));

            return result.HasValue 
                ? new DateTimeOffset(DateTime.SpecifyKind(result.Value, DateTimeKind.Utc)) 
                : new DateTimeOffset();
		}

		public async virtual Task<int> IncrementAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.AccessFailedCount++;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.AccessFailedCount = user.AccessFailedCount, false));
			return user.AccessFailedCount;
		}

		public async virtual Task ResetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			user.AccessFailedCount = 0;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.AccessFailedCount = user.AccessFailedCount, false));
		}

		public async virtual Task SetLockoutEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.LockoutEnabled = enabled;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.LockoutEnabled = user.LockoutEnabled, false));
		}

		public async virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? (DateTime?)null : lockoutEnd.UtcDateTime;
			var result = await Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.LockoutEndDateUtc = user.LockoutEndDateUtc, false));

		}

        #endregion
    }
}
