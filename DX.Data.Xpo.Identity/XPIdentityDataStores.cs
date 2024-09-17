using DevExpress.Xpo.DB;
using DevExpress.Data.Filtering;
using DevExpress.Entity.Model.Metadata;
using DevExpress.Xpo;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static DevExpress.Data.Helpers.FindSearchRichParser;
using System.Timers;
using DevExpress.Data.Filtering.Helpers;
using System.Data;

#if(NETCOREAPP)
using Microsoft.AspNetCore.Identity;
#else
using Microsoft.AspNet.Identity;
#endif


namespace DX.Data.Xpo.Identity
{
    public abstract class XPBaseUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
                                    TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken> :
            XPDataStore<TKey, TUser, TXPOUser>, IQueryableUserStore<TKey, TUser, TUserRole, TUserToken>
                where TKey : IEquatable<TKey>
#if (NETCOREAPP)
                where TUser : IdentityUser<TKey>, new()
                where TRole : IdentityRole<TKey>, new()
                where TUserClaim : IdentityUserClaim<TKey>, new()
                where TUserRole : IdentityUserRole<TKey>, new()
                where TUserLogin : IdentityUserLogin<TKey>, new()
                where TUserToken : IdentityUserToken<TKey>, new()
                where TRoleClaim : IdentityRoleClaim<TKey>, new()
#else
				where TUser : class, IXPUser<TKey>, new()
				where TRole : class, IXPRole<TKey>, new()
				where TUserClaim : class, IXPUserClaim<TKey>, new()
				where TUserRole : class, IXPUserRole<TKey>, new()
				where TUserLogin : class, IXPUserLogin<TKey>, new()
				where TUserToken : class, IXPUserToken<TKey>, new()
				where TRoleClaim : class, IXPRoleClaim<TKey>, new()

#endif
				where TXPOUser : XPBaseObject, IXPUser<TKey>
                where TXPORole : XPBaseObject, IXPRole<TKey>
                where TXPOLogin : XPBaseObject, IXPUserLogin<TKey>
                where TXPOClaim : XPBaseObject, IXPUserClaim<TKey>
                where TXPOToken : XPBaseObject, IXPUserToken<TKey>
    {
        public XPBaseUserStore(IDataLayer dataLayer, IValidator<TXPOUser> validator) : base(dataLayer, validator)
        {

        }

        public override string KeyField => "Id"; //nameof(IdentityUser.Id);
        public override TKey ModelKey(TUser model) => model.Id;
        public override void SetModelKey(TUser model, TKey key) => model.Id = key;
        protected override TKey DBModelKey(TXPOUser model) => model.Id;

		//     public virtual async Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
		//     {
		//         await TransactionalExecAsync<TUser>(async (s, w) =>
		//         {
		//	var r = await s.DBQuery(w).FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
		//});
		//     }
		
		public virtual async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            ThrowIfNull(claims);

            await TransactionalExecAsync(async (s, w) => {
                //get user
                var usr = await w.GetObjectByKeyAsync<TXPOUser>(ModelKey(user), cancellationToken);
                //get current assigned claims
                var dbClaims = new XPCollection<TXPOClaim>(w, CriteriaOperator.FromLambda<TXPOClaim>(cl => cl.UserId.Equals(ModelKey(user))));

                if (usr != null)
                {
                    foreach (var claim in claims)
                    {
                        dbClaims.Filter = CriteriaOperator.FromLambda<TXPOClaim>(cl => cl.ClaimType == claim.Type && cl.ClaimValue == claim.Value);
                        if (dbClaims.Count() == 0)
                        {
                            var newClaim = Activator.CreateInstance(typeof(TXPOClaim), w) as TXPOClaim;
                            newClaim!.InitializeUserClaim(usr, claim);
                            usr.ClaimsList.Add(newClaim);
                            newClaim.Save();
                        }
                    }
                }
            });
        }

        public virtual async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            ThrowIfNull(login);

            await TransactionalExecAsync(async (s, w) => {
                //get user
                var usr = await w.GetObjectByKeyAsync<TXPOUser>(ModelKey(user), cancellationToken);
                //get current assigned claims
                var dbLogins = new XPCollection<TXPOLogin>(w, 
                                    CriteriaOperator.FromLambda<TXPOLogin>(lg => lg.UserId.Equals(user.Id) && 
                                                                                lg.ProviderKey == login.ProviderKey &&
                                                                                lg.LoginProvider == login.LoginProvider));

                if (usr != null && dbLogins.Count() == 0)
                {
                    var newLogin = Activator.CreateInstance(typeof(TXPOLogin), w) as TXPOLogin;
                    newLogin!.InitializeUserLogin(usr, login);
                    usr.LoginsList.Add(newLogin);
                    newLogin.Save();
                }
            });

        }

        public virtual async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            ThrowIfNullOrEmpty(normalizedRoleName);

            await TransactionalExecAsync(async (s, w) => {
                //get user
                var usr = await w.GetObjectByKeyAsync<TXPOUser>(ModelKey(user), cancellationToken);

                var role = await w.FindObjectAsync(typeof(TXPORole),
                    CriteriaOperator.Parse("(NormalizedName == ?) AND (NOT Users[ID == ?])", normalizedRoleName, usr.Id)) as TXPORole;
                if (role == null)
                    throw new InvalidOperationException($"Role {normalizedRoleName} was not found");
                
                usr.RolesList.Add(role);
            });
        }

        public virtual async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);

            var result = await TransactionalExecAsync(async (s, w) => {
                //get user
                var usr = await w.GetObjectByKeyAsync<TXPOUser>(ModelKey(user), cancellationToken);
                var r = new List<string>();
                foreach(var item in usr.RolesList)
                {
                    var role = item as IXPRole<TKey>;
                    if (role != null)
                        r.Add(role.Name);
                }
                return r.ToList();
            }, false, false);
            return result;
        }

        public virtual async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(claim);

            var result = await TransactionalExecAsync(async (s, w) => {

                var users = new XPCollection<TXPOUser>(w,
                    CriteriaOperator.Parse("Claims[ClaimType == ? AND ClaimValue ==?])", claim.Type, claim.Value),                     
                    new SortProperty(nameof(IXPUser<TKey>.UserName), SortingDirection.Ascending));
                await users.LoadAsync(cancellationToken);

                return users.Select(u => MapTo(u, new TUser())).ToList();
            }, false, false);
            return result!;            
        }

        public virtual async Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNullOrEmpty(normalizedRoleName);

            var result = await TransactionalExecAsync(async (s, w) => {

                var users = new XPCollection<TXPOUser>(w,
                    CriteriaOperator.Parse("Roles[NormalizedName == ?])", normalizedRoleName),
                    new SortProperty(nameof(IXPUser<TKey>.UserName), SortingDirection.Ascending));
                await users.LoadAsync(cancellationToken);
                return users.Select(u=>ToModel(u, new TUser())).ToList();
            });
            return result!;

        }

        public virtual async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            ThrowIfNullOrEmpty(normalizedRoleName);

            var result = await TransactionalExecAsync(async (s, w) => {

                var usr = await w.FindObjectAsync<TXPOUser>(CriteriaOperator.Parse("Id == ? AND (Roles[NormalizedName == ?].Count() > 0)", ModelKey(user), normalizedRoleName));                
                return usr != null;
            });
            return result;
        }

        public virtual async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            ThrowIfNull(claims);

            await TransactionalExecAsync(async (s, w) => {

                //get user
                var usr = await w.GetObjectByKeyAsync<TXPOUser>(ModelKey(user), cancellationToken);
                var userClaims = await w.Query<TXPOClaim>()
                        .Where(uc => uc.UserId.Equals(ModelKey(user)) && claims.Any(cl=>cl.Type == uc.ClaimType && cl.Value == uc.ClaimValue))
                        .ToListAsync(cancellationToken);
                foreach (var uc in userClaims)
                {
                    uc.SetMemberValue("User", null);
                    //uc.InitializeUserClaim(null!, null!); //need to set XPUser prop to null;
                }
                await w.DeleteAsync(userClaims, cancellationToken);
            });
        }

        public virtual async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            ThrowIfNullOrEmpty(normalizedRoleName);

            await TransactionalExecAsync(async (s, w) => {

                //get user
                var usr = await w.GetObjectByKeyAsync<TXPOUser>(ModelKey(user), cancellationToken);
                var r = usr.RolesList.Cast<TXPORole>().FirstOrDefault(r => r.NormalizedName == normalizedRoleName);
                if (r != null)
                    usr.RolesList.Remove(r);
            });
        }

        public virtual async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            ThrowIfNullOrEmpty(loginProvider);
            ThrowIfNullOrEmpty(providerKey);

            await TransactionalExecAsync(async (s, w) => {                
                var userLogins = await w.Query<TXPOLogin>()
                        .Where(ul => ul.UserId.Equals(ModelKey(user)) && ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey)
                        .ToListAsync(cancellationToken);
                foreach (var ul in userLogins)
                {
                    ul.SetMemberValue("User", null);                    
                }
                await w.DeleteAsync(userLogins, cancellationToken);
            });
        }

        public virtual async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(user);
            ThrowIfNull(claim);
            ThrowIfNull(newClaim);

            await TransactionalExecAsync(async (s, w) => {

                var userClaims = await w.Query<TXPOClaim>()
                        .Where(uc => uc.UserId.Equals(ModelKey(user)) && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value)
                        .ToListAsync(cancellationToken);
                
                foreach (var uc in userClaims)
                {
                    uc.ClaimType = newClaim.Type;
                    uc.ClaimValue = newClaim.Value;
                }                
            });
        }

        public virtual async Task<TUserRole?> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(userId);
            ThrowIfNull(roleId);

            var result = await TransactionalExecAsync(async (s, w) => {

                TUserRole r = null!;
                var role = await w.FindObjectAsync<TXPORole>(CriteriaOperator.Parse("Id == ? AND Users[Id == ?]", roleId, userId));
                if (role != null)
                {                    
                    r = new TUserRole() { UserId = userId, RoleId = role.Id };
                    MapTo(role, r);                    
                }
                return r;
            });
            return result!;
        }

        public virtual async Task RemoveUserTokenAsync(TUserToken token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(token);
            
            await TransactionalExecAsync(async (s, w) => {                
                var tk = await w.FindObjectAsync<TXPOToken>(
                                            CriteriaOperator.Parse("[User.Id] == ? AND [LoginProvider] == ? AND [Name] == ? AND [Value] == ?", 
                                                    token.UserId, token.LoginProvider, token.Name, token.Value));
                if (tk != null)
                {
                    tk.SetMemberValue("User", null);
                    w.Delete(tk);
                }
            });

        }

		public async Task<TUser?> FindByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNullOrEmpty(normalizedUserName);

            var result = await FindAsync(CriteriaOperator.FromLambda<TXPOUser>(u => u.NormalizedUserName == normalizedUserName));
            return result.FirstOrDefault();
        }
		public async Task<TUser?> FindByEmailAsync(string normalizedEmailAddress, CancellationToken cancellationToken = default)
		{
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			ThrowIfNullOrEmpty(normalizedEmailAddress);

            var result = await FindAsync(CriteriaOperator.FromLambda<TXPOUser>(u => u.NormalizedEmail == normalizedEmailAddress));
			return result.FirstOrDefault();
		}

        public async Task<TUser?> FindByIdAsync(object userId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
            if (userId == null)
				throw new ArgumentNullException("userId");			

            var result = await FindAsync(CriteriaOperator.Parse("Id == ?", userId));
            return result.FirstOrDefault();
		}

		public async virtual Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");
			
            //user.PasswordHash = passwordHash;
			await TransactionalExecAsync(async (s, w) => {
				var u = await w.GetObjectByKeyAsync<TXPOUser>(user.Id);
				if (u != null)
				{
					u.PasswordHash = passwordHash;
				}

			});
		}
		public async Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default) 
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			var result = await TransactionalExecAsync(async (s, w) => {
				
                var u = await w.GetObjectByKeyAsync<TXPOUser>(user.Id);
				return u?.PasswordHash ?? string.Empty;
			}, false, false);
            return result!;
		}

		public async Task<bool> HasPasswordHashAsync(TUser user, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			var result = await TransactionalExecAsync(async (s, w) => {

				var u = await w.GetObjectByKeyAsync<TXPOUser>(user.Id);
				return !string.IsNullOrEmpty(u?.PasswordHash ?? string.Empty);
			}, false, false);
			return result;
		}

		public async virtual Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			var result = await TransactionalExecAsync(async (s, w) => {

				var u = await w.GetObjectByKeyAsync<TXPOUser>(user.Id);
				return u?.SecurityStamp?? string.Empty;
			}, false, false);
			return result!;
		}

		public async virtual Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			await TransactionalExecAsync(async (s, w) => {
				var u = await w.GetObjectByKeyAsync<TXPOUser>(user.Id);
				if (u != null)
				{
					u.SecurityStamp= stamp;
				}
			});
		}
	}

	public abstract class XPBaseRoleStore<TKey, TRole, TRoleClaim, TXPORole, TXPOClaim> : XPDataStore<TKey, TRole, TXPORole>, 
            IQueryableRoleStore<TKey, TRole>
        where TKey : IEquatable<TKey>
#if (NETCOREAPP)
        where TRole : IdentityRole<TKey>, new()
        where TRoleClaim: IdentityRoleClaim<TKey>
#else
        where TRole : class, IXPRole<TKey>, new()
        where TRoleClaim : class, IXPRoleClaim<TKey>, new()
#endif
        where TXPORole : XPBaseObject, IXPRole<TKey>
        where TXPOClaim : XPBaseObject, IXPRoleClaim<TKey>
    {
        public XPBaseRoleStore(IDataLayer dataLayer, IValidator<TXPORole> validator) : base(dataLayer, validator)
        {

        }
        public override string KeyField => "Id"; // nameof(IdentityUser.Id);
        public override TKey ModelKey(TRole model) => model.Id;
        public override void SetModelKey(TRole model, TKey key) => model.Id = key;
        protected override TKey DBModelKey(TXPORole model) => model.Id;

		public async Task<TRole?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
		{
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			ThrowIfNullOrEmpty(id);

			var result = await FindAsync(CriteriaOperator.Parse("Id == ?", id));
			return result.FirstOrDefault();
		}

		public async Task<TRole?> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
		{
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			ThrowIfNullOrEmpty(normalizedName);

			var result = await FindAsync(CriteriaOperator.FromLambda<TXPORole>(u => u.NormalizedName == normalizedName));
			return result.FirstOrDefault();
		}


		public async virtual Task AddClaimsAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(role);
            ThrowIfNull(claim);

            await TransactionalExecAsync(async (s, w) => {
                //get role
                var rle = await w.GetObjectByKeyAsync<TXPORole>(ModelKey(role), cancellationToken);
                //get current assigned claims
                var dbClaims = new XPCollection<TXPOClaim>(w, CriteriaOperator.FromLambda<TXPOClaim>(cl => cl.RoleId.Equals(ModelKey(role))));

                if (rle != null)
                {
                    dbClaims.Filter = CriteriaOperator.FromLambda<TXPOClaim>(cl => cl.ClaimType == claim.Type && cl.ClaimValue == claim.Value);
                    if (dbClaims.Count() == 0)
                    {
                        var newClaim = Activator.CreateInstance(typeof(TXPOClaim), w) as TXPOClaim;
                        newClaim!.InitializeRoleClaim(rle, claim);
                        rle.ClaimsList.Add(newClaim);
                        newClaim.Save();
                    }
                }
            });
        }        

        public async virtual Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(role);
            ThrowIfNull(claim);

            await TransactionalExecAsync(async (s, w) => {

                //get user
                var rle = await w.GetObjectByKeyAsync<TXPORole>(ModelKey(role), cancellationToken);
                var roleClaims = await w.Query<TXPOClaim>()
                        .Where(rc => rc.RoleId.Equals(ModelKey(role)) && rc.ClaimType == claim.Type && rc.ClaimValue == claim.Value)
                        .ToListAsync(cancellationToken);
                foreach (var rc in roleClaims)
                {
                    rc.SetMemberValue("Role", null);                    
                }
                await w.DeleteAsync(roleClaims, cancellationToken);
            });
        }

        public async virtual Task ReplaceClaimAsync(TRole role, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(role);
            ThrowIfNull(claim);
            ThrowIfNull(newClaim);

            await TransactionalExecAsync(async (s, w) => {

                var roleClaims = await w.Query<TXPOClaim>()
                        .Where(rc => rc.RoleId.Equals(ModelKey(role)) && rc.ClaimType == claim.Type && rc.ClaimValue == claim.Value)
                        .ToListAsync(cancellationToken);

                foreach (var rc in roleClaims)
                {
                    rc.ClaimType = newClaim.Type;
                    rc.ClaimValue = newClaim.Value;
                }
            });
        }


	}


	public abstract class XPBaseUserLoginStore<TKey, TUserLogin, TXPOUserLogin> : XPDataStore<TKey, TUserLogin, TXPOUserLogin>,
		IQueryableUserLoginStore<TKey, TUserLogin>
	where TKey : IEquatable<TKey>
#if(NETCOREAPP)
    where TUserLogin : IdentityUserLogin<TKey>, new()
#else
	where TUserLogin : class, IXPUserLogin<TKey>, new()
#endif
	where TXPOUserLogin : XPBaseObject, IXPUserLogin<TKey>
	{
		public XPBaseUserLoginStore(IDataLayer dataLayer, IValidator<TXPOUserLogin> validator) : base(dataLayer, validator)
		{
		}
		public override string KeyField => "UserId";
		public override TKey ModelKey(TUserLogin model) => model.UserId;
		public override void SetModelKey(TUserLogin model, TKey key) => model.UserId = key;
		protected override TKey DBModelKey(TXPOUserLogin model) => model.UserId;


		public async virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TKey userId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			ThrowIfNull(userId);

            var results = await FindAsync(CriteriaOperator.Parse("[User.Id] == ?", userId));
#if (NETCOREAPP)
            return results.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToList();
#else
			return results.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList();
#endif
		}

		public async virtual Task<TUserLogin?> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			ThrowIfNull(userId);
			ThrowIfNullOrEmpty(loginProvider);
			ThrowIfNullOrEmpty(providerKey);

			var results = await FindAsync(CriteriaOperator.Parse("[User.Id] == ? AND [LoginProvider] == ? AND [ProviderKey] == ? ", userId, loginProvider, providerKey));
            return results.FirstOrDefault();
		}
		public async virtual Task<TUserLogin?> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
		{
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();			
			ThrowIfNullOrEmpty(loginProvider);
			ThrowIfNullOrEmpty(providerKey);

			var results = await FindAsync(CriteriaOperator.Parse("[LoginProvider] == ? AND [ProviderKey] == ? ", loginProvider, providerKey));
			return results.FirstOrDefault();
		}
	}


	public abstract class XPBaseUserClaimStore<TKey, TUserClaim, TXPOUserClaim> : XPDataStore<TKey, TUserClaim, TXPOUserClaim>,
			IQueryableUserClaimStore<TKey, TUserClaim>
		where TKey : IEquatable<TKey>
#if (NETCOREAPP)
        where TUserClaim : IdentityUserClaim<TKey>, new()
#else
        where TUserClaim : class, IXPUserClaim<TKey>, new()
#endif
		where TXPOUserClaim : XPBaseObject, IXPUserClaim<TKey>
	{
		protected XPBaseUserClaimStore(IDataLayer dataLayer, IValidator<TXPOUserClaim> validator) : base(dataLayer, validator)
		{
		}
		public override string KeyField => "Id";
		public override TKey ModelKey(TUserClaim model) => model.UserId;
		public override void SetModelKey(TUserClaim model, TKey key) => model.UserId = key;
		protected override TKey DBModelKey(TXPOUserClaim model) => model.UserId;

		public async virtual Task<List<Claim>> GetUserClaimsAsync(TKey userId, CancellationToken cancellationToken = default)
		{
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			ThrowIfNull(userId);


			var results = await FindAsync(CriteriaOperator.Parse("[User.Id] == ?", userId));
			return results.Select(cl => new Claim(cl.ClaimType!, cl.ClaimValue!)).ToList();
		}
	}

	public abstract class XPBaseUserTokenStore<TKey, TUserToken, TXPOUserToken> : XPDataStore<TKey, TUserToken, TXPOUserToken>,
			IQueryableUserTokenStore<TKey, TUserToken>
		where TKey : IEquatable<TKey>
#if (NETCOREAPP)
		where TUserToken : IdentityUserToken<TKey>, new()
#else
        where TUserToken : class, IXPUserToken<TKey>, new()
#endif
		where TXPOUserToken : XPBaseObject, IXPUserToken<TKey>
	{
		public XPBaseUserTokenStore(IDataLayer dataLayer, IValidator<TXPOUserToken> validator) : base(dataLayer, validator)
		{

		}
		public override string KeyField => "UserId";
		public override TKey ModelKey(TUserToken model) => model.UserId;
		public override void SetModelKey(TUserToken model, TKey key) => model.UserId = key;
		protected override TKey DBModelKey(TXPOUserToken model) => model.UserId;

		public async Task<TUserToken?> FindTokenAsync(TKey userId, string loginProvider, string name, CancellationToken cancellationToken = default)
		{
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			ThrowIfNull(userId);
			ThrowIfNullOrEmpty(loginProvider);
			ThrowIfNullOrEmpty(name);

			var results = await FindAsync(CriteriaOperator.Parse("[User.Id] == ? AND [LoginProvider] == ? AND [Name] == ?", userId, loginProvider, name));
			return results.FirstOrDefault();
		}

	}

	public abstract class XPBaseRoleClaimStore<TKey, TRoleClaim, TXPORoleClaim> : XPDataStore<TKey, TRoleClaim, TXPORoleClaim>,
		IQueryableRoleClaimStore<TKey, TRoleClaim>
	where TKey : IEquatable<TKey>
#if (NETCOREAPP)
		where TRoleClaim : IdentityRoleClaim<TKey>, new()
#else
		where TRoleClaim : class, IXPRoleClaim<TKey>, new()
#endif
	
	where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
	{
		public XPBaseRoleClaimStore(IDataLayer dataLayer, IValidator<TXPORoleClaim> validator) : base(dataLayer, validator)
		{

		}

		public override string KeyField => "RoleId";
		public override TKey ModelKey(TRoleClaim model) => model.RoleId;
		public override void SetModelKey(TRoleClaim model, TKey key) => model.RoleId = key;
		protected override TKey DBModelKey(TXPORoleClaim model) => model.RoleId;

		public virtual Task<List<Claim>> GetRoleClaimsAsync(TKey roleId, CancellationToken cancellationToken = default)
		{
            cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			ThrowIfNull(roleId);

			using (var wrk = new Session(DataLayer))
			{
				var results = new XPCollection<TXPORoleClaim>(wrk, CriteriaOperator.Parse("[Role.Id] == ?", roleId));
				return Task.FromResult(results.Select(cl => new Claim(cl.ClaimType, cl.ClaimValue)).ToList());
			}
		}

	}

}
