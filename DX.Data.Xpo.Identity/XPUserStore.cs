using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DX.Data.Xpo.Identity
{
	public class XPUserStore<TUser> :
		XPUserStore<TUser, XpoDxUser>
		 where TUser : XPIdentityUser<string, XpoDxUser>, IUser<string>
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
	 where TUser : XPIdentityUser<string, TXPOUser>, IUser<string>
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

		public virtual Task AddLoginAsync(TUser user, UserLoginInfo login)
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

			return Task.FromResult(XPOExecute<int>((db, s) =>
			{
				TXPOLogin xpoLogin = XPOCreateLogin(s);
				xpoLogin.LoginProvider = login.LoginProvider;
				xpoLogin.ProviderKey = login.ProviderKey;
				xpoLogin.SetMemberValue("User", s.FindObject(XPOUserType, CriteriaOperator.Parse($"{KeyField} == ?", user.Id)));

				//if (xpoLogin.User == null)
				//	 throw new Exception(String.Format("The user with id '{0}' could not be found in the database", user.Id));
				return 0;
			}));
		}

		public virtual Task<TUser> FindAsync(UserLoginInfo login)
		{
			ThrowIfDisposed();
			if (login == null)
			{
				throw new ArgumentNullException("login");
			}
			return Task.FromResult(XPOExecute<TUser>((db, s) =>
			{
				var userLogin = s.FindObject(XPOLoginType, CriteriaOperator.Parse("(LoginProvider == ?) AND (ProviderKey == ?)", login.LoginProvider, login.ProviderKey)) as XPBaseObject;
				return (userLogin == null) ? null : Activator.CreateInstance(typeof(TUser), userLogin.GetMemberValue("User"), DxIdentityUserFlags.FLAG_FULL) as TUser;
			}));
		}

		public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			return Task.FromResult(XPOExecute<IList<UserLoginInfo>>((db, wrk) =>
			{
				var results = new List<UserLoginInfo>();
				foreach (var r in new XPCollection(wrk, typeof(TXPOLogin), CriteriaOperator.Parse("[User!Key] == ?", user.Id)))
				{
					IDxUserLogin<TKey> xpoLogin = r as IDxUserLogin<TKey>;
					results.Add(new UserLoginInfo(xpoLogin.LoginProvider, xpoLogin.ProviderKey));
				}
				return results;
			}, false));
		}

		public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
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
			return Task.FromResult(XPOExecute<int>((db, wrk) =>
			{
				wrk.Delete(wrk.FindObject(typeof(TXPOLogin),
						 CriteriaOperator.Parse("([User!Key] == ?) AND (LoginProvider == ?) AND )ProviderKey == ?)",
														 user.Id, login.LoginProvider, login.ProviderKey)));
				return 0;
			}));
		}

		public Task CreateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (String.IsNullOrEmpty(user.UserName))
				user.UserName = user.Email;


			using (UnitOfWork wrk = Database.GetUnitOfWork())
			{
				var xpoUser = XPOCreateUser(wrk);

				xpoUser.Assign(user, DxIdentityUserFlags.FLAG_FULL);
				wrk.CommitChanges();
				user.Assign(xpoUser, DxIdentityUserFlags.FLAG_FULL);
			}

			return Task.FromResult(0);
		}


		public Task DeleteAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			return Task.FromResult(XPOExecute<int>((db, wrk) =>
			{
				wrk.Delete(wrk.FindObject(XPOUserType, CriteriaOperator.Parse($"{KeyField} == ?", user.Id)));
				return 0;
			}));
		}

		public Task<TUser> FindByIdAsync(TKey userId)
		{
			ThrowIfDisposed();

			return Task.FromResult(XPOExecute<TUser>((db, wrk) =>
			{
				var xpoUser = wrk.FindObject(XPOUserType, CriteriaOperator.Parse($"{KeyField} == ?", userId));
				return xpoUser == null ? null : Activator.CreateInstance(typeof(TUser), xpoUser, DxIdentityUserFlags.FLAG_FULL) as TUser;
			}));
		}

		public Task<TUser> FindByNameAsync(string userName)
		{
			ThrowIfDisposed();

			return Task.FromResult(XPOExecute((db, wrk) =>
			{
				var xpoUser = wrk.FindObject(XPOUserType, CriteriaOperator.Parse("UserNameUpper == ?", userName.ToUpperInvariant()));
				return xpoUser == null ? null : Activator.CreateInstance(typeof(TUser), xpoUser, DxIdentityUserFlags.FLAG_FULL) as TUser;
			}));
		}

		public Task UpdateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			return Task.FromResult(XPOExecute<object>((db, wrk) =>
			{
				TXPOUser u = wrk.FindObject(XPOUserType, CriteriaOperator.Parse($"{KeyField} == ?", user.Id)) as TXPOUser;
				if (u != null)
				{
					u.Assign(user, DxIdentityUserFlags.FLAG_FULL);
				}

				return null;
			}));
		}
		#endregion

		#region IUserClaimStore<TUser, TKey>

		public Task AddClaimAsync(TUser user, Claim claim)
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
			return Task.FromResult(XPOExecute<object>((db, wrk) =>
			{
				var xpoClaim = XPOCreateClaim(wrk);
				xpoClaim.SetMemberValue("User", wrk.FindObject(XPOUserType, CriteriaOperator.Parse($"{KeyField} == ?", user.Id)));
				xpoClaim.ClaimType = claim.Type;
				xpoClaim.ClaimValue = claim.Value;
				return null;
			}));
		}

		public Task<IList<Claim>> GetClaimsAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(XPOExecute<IList<Claim>>((db, wrk) =>
			{
				var results = new List<Claim>();
				foreach (var c in new XPCollection(wrk, XPOClaimType, CriteriaOperator.Parse("[User!Key] == ?", user.Id)))
				{
					TXPOClaim xpoClaim = c as TXPOClaim;
					results.Add(new Claim(xpoClaim.ClaimType, xpoClaim.ClaimValue));
				}
				return results;
			}, false));
		}

		public Task RemoveClaimAsync(TUser user, Claim claim)
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
			return Task.FromResult(XPOExecute<int>((db, wrk) =>
			{
				wrk.Delete(new XPCollection(wrk, XPOClaimType,
						 CriteriaOperator.Parse("([User!Key] == ?) AND (ClaimType == ?) AND (ClaimValue == ?)",
						 user.Id, claim.Type, claim.Value)));
				return 0;
			}));
		}
		#endregion

		#region IUserRoleStore<TUser, TKey>

		public Task AddToRoleAsync(TUser user, string roleName)
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
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u =>
			{
				var role = u.Session.FindObject(typeof(TXPORole),
						 CriteriaOperator.Parse($"(NameUpper == ?) AND (NOT Users[{KeyField} == ?])", r, u.Id)) as TXPORole;
				if (role == null)
					throw new InvalidOperationException(String.Format("Role {0} was not found", roleName));
				u.RolesList.Add(role);
				return 0;
			}, true));
		}

		public Task<IList<string>> GetRolesAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			var userId = user.Id;

			return Task.FromResult(XPOExecute<IList<string>>((db, wrk) =>
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
		}

		public Task<bool> IsInRoleAsync(TUser user, string roleName)
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
			return Task.FromResult(XPOExecute<bool>((db, wrk) =>
			{
				var role = wrk.FindObject(typeof(TXPORole),
						 CriteriaOperator.Parse($"(NameUpper == ?) AND (Users[{KeyField} == ?])", roleName.ToUpperInvariant(), user.Id)) as TXPORole;
				return (role != null);
			}, false));
		}

		public Task RemoveFromRoleAsync(TUser user, string roleName)
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

			return Task.FromResult(XPOExecute<int>((db, wrk) =>
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

		public Task<string> GetPasswordHashAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			//return Task.FromResult(XPOExecute<string>((wrk) =>
			//{
			//	 var u = wrk.FindObject(typeof(TXPOUser), CriteriaOperator.Parse("(Id == ?)", user.Id)) as TXPOUser;
			//	 if (u == null)
			//		  throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

			//	 return u.PasswordHash;
			//}, false));
			return Task.FromResult(user.PasswordHash);
		}

		public Task<bool> HasPasswordAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			//return Task.FromResult(XPOExecute<bool>((wrk) =>
			//{
			//	 var u = wrk.FindObject(typeof(TXPOUser), CriteriaOperator.Parse("(Id == ?)", user.Id)) as TXPOUser;
			//	 if (u == null)
			//		  throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

			//	 return !String.IsNullOrEmpty(u.PasswordHash);
			//}, false));
			return Task.FromResult(!String.IsNullOrEmpty(user.PasswordHash));
		}

		public Task SetPasswordHashAsync(TUser user, string passwordHash)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.PasswordHash = passwordHash;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.PasswordHash = user.PasswordHash, false));
		}
		#endregion

		#region IUserSecurityStampStore<TUser, TKey>

		public Task<string> GetSecurityStampAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			//return Task.FromResult(XPOExecute<string>((wrk) =>
			//{
			//	 var u = wrk.FindObject(typeof(TXPOUser), CriteriaOperator.Parse("(Id == ?)", user.Id)) as TXPOUser;
			//	 if (u == null)
			//		  throw new InvalidOperationException(String.Format("User '{0}' was not found", user.UserName));

			//	 return u.SecurityStamp;
			//}, false));
			return Task.FromResult(user.SecurityStamp);
		}

		public Task SetSecurityStampAsync(TUser user, string stamp)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.SecurityStamp = stamp;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.SecurityStamp = user.SecurityStamp, false));
		}
		#endregion

		#region IQueryableUserStore<TUser, TKey>
		public IQueryable<TUser> Users
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

		public Task<TUser> FindByEmailAsync(string email)
		{
			ThrowIfDisposed();

			return Task.FromResult(XPOExecute<TUser>((db, wrk) =>
			{
				var xpoUser = wrk.FindObject(XPOUserType, CriteriaOperator.Parse("EmailUpper == ?", email.ToUpperInvariant()));
				return xpoUser == null ? null : Activator.CreateInstance(typeof(TUser), xpoUser, DxIdentityUserFlags.FLAG_FULL) as TUser;
			}));
		}

		public Task<string> GetEmailAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.Email);
		}

		public Task<bool> GetEmailConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.EmailConfirmed);
		}

		public Task SetEmailAsync(TUser user, string email)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.Email = email;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.Email = user.Email, false));
		}

		public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.EmailConfirmed = confirmed;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.EmailConfirmed = user.EmailConfirmed, false));
		}
		#endregion

		#region IUserPhoneNumberStore<TUser, TKey>

		public Task<string> GetPhoneNumberAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.PhoneNumber);
		}

		public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.PhoneNumberConfirmed);
		}

		public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.PhoneNumber = phoneNumber;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.PhoneNumber = user.PhoneNumber, false));
		}

		public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.PhoneNumberConfirmed = confirmed;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.PhoneNumberConfirmed = user.PhoneNumberConfirmed, false));
		}
		#endregion

		#region IUserTwoFactorStore<TUser, TKey>

		public Task<bool> GetTwoFactorEnabledAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.TwoFactorEnabled);
		}

		public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.TwoFactorEnabled = enabled;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.TwoFactorEnabled = user.TwoFactorEnabled, false));
		}
		#endregion

		#region IUserLockoutStore<TUser, TKey>

		public Task<int> GetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.AccessFailedCount);
		}

		public Task<bool> GetLockoutEnabledAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.LockoutEnabled);
		}

		public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.LockoutEndDateUtc.HasValue
				? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
				: new DateTimeOffset());
		}

		public Task<int> IncrementAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.AccessFailedCount++;
			XPOSelectAndUpdate(user.Id, u => u.AccessFailedCount = user.AccessFailedCount, false);
			return Task.FromResult(user.AccessFailedCount);
		}

		public Task ResetAccessFailedCountAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			user.AccessFailedCount = 0;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.AccessFailedCount = user.AccessFailedCount, false));
		}

		public Task SetLockoutEnabledAsync(TUser user, bool enabled)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.LockoutEnabled = enabled;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.LockoutEnabled = user.LockoutEnabled, false));
		}

		public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? (DateTime?)null : lockoutEnd.UtcDateTime;
			return Task.FromResult(XPOSelectAndUpdate(user.Id, u => u.LockoutEndDateUtc = user.LockoutEndDateUtc, false));
		}
		#endregion
	}


}
