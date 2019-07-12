using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;
using DX.Utils;
using DX.Utils.Data;
#if (NETSTANDARD2_0)
using Microsoft.AspNetCore.Identity;
#else
using Microsoft.AspNet.Identity;
#endif


namespace DX.Data.Xpo.Identity
{
	public class XPIdentityUser : XPIdentityUser<string>
	{
		public XPIdentityUser()
			: base()
		{

		}
	}

	public class XPIdentityUser<TKey> : XPIdentityUser<TKey, XPIdentityUserLogin<TKey>, XPIdentityRole<TKey>, XPIdentityUserClaim<TKey>>
		 where TKey : IEquatable<TKey>
		 
	{

		public XPIdentityUser()
			  : base()
		{

		}

	}

	/// <summary>
	///     Default XPO IUser implementation
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TLogin"></typeparam>
	/// <typeparam name="TRole"></typeparam>
	/// <typeparam name="TClaim"></typeparam>
	public class XPIdentityUser<TKey, TLogin, TRole, TClaim> : IDataStoreModel<TKey>, IXPUser<TKey>, IUser<TKey>
		 where TKey : IEquatable<TKey>		 
		 where TRole : class
		 where TLogin : class
		 where TClaim : class		
	{
		public XPIdentityUser()
		{
			Claims = new List<TClaim>();
			Roles = new List<TRole>();
			Logins = new List<TLogin>();

		}

		public TKey ID { get => Id; set => Id = value; }
		/// <summary>
		///     User ID (Primary Key)
		/// </summary>
		public virtual TKey Id { get; set; }

		/// <summary>
		///     User name
		/// </summary>
		public virtual string UserName { get; set; }
#if (NETSTANDARD2_0)
		public virtual string NormalizedName { get; set; }
		public virtual string NormalizedEmail { get; set; }
#endif
		/// <summary>
		///     Email
		/// </summary>
		public virtual string Email { get; set; }

		/// <summary>
		///     True if the email is confirmed, default is false
		/// </summary>
		public virtual bool EmailConfirmed { get; set; }

		/// <summary>
		///     The salted/hashed form of the user password
		/// </summary>
		public virtual string PasswordHash { get; set; }

		/// <summary>
		///     A random value that should change whenever a users credentials have changed (password changed, login removed)
		/// </summary>
		public virtual string SecurityStamp { get; set; }

		/// <summary>
		///     PhoneNumber for the user
		/// </summary>
		public virtual string PhoneNumber { get; set; }

		/// <summary>
		///     True if the phone number is confirmed, default is false
		/// </summary>
		public virtual bool PhoneNumberConfirmed { get; set; }

		/// <summary>
		///     Is two factor enabled for the user
		/// </summary>
		public virtual bool TwoFactorEnabled { get; set; }

		/// <summary>
		///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
		/// </summary>
		public virtual DateTime? LockoutEndDateUtc { get; set; }

		/// <summary>
		///     Is lockout enabled for this user
		/// </summary>
		public virtual bool LockoutEnabled { get; set; }

		/// <summary>
		///     Used to record failures for the purposes of lockout
		/// </summary>
		public virtual int AccessFailedCount { get; set; }

		private ICollection<TRole> _Roles = new List<TRole>();
		/// <summary>
		///     Navigation property for user roles
		/// </summary>
		public virtual ICollection<TRole> Roles { get => _Roles; set => _Roles = value; }

		/// <summary>
		///     Navigation property for user claims
		/// </summary>
		private ICollection<TClaim> _Claims = new List<TClaim>();
		public virtual ICollection<TClaim> Claims { get => _Claims; set => _Claims = value; }

		private ICollection<TLogin> _Logins = new List<TLogin>();
		/// <summary>
		///     Navigation property for user logins
		/// </summary>
		public virtual ICollection<TLogin> Logins { get=>_Logins; set=>_Logins = value; }

		/// <summary>
		///     Navigation property for user roles
		/// </summary>
		public virtual IList RolesList { get; protected set; }

		/// <summary>
		///     Navigation property for user claims
		/// </summary>
		public virtual IList ClaimsList { get; protected set; }

		/// <summary>
		///     Navigation property for user logins
		/// </summary>
		public virtual IList LoginsList { get; protected set; }


//		public override void Assign(object source, int loadingFlags)
//		{
//			var src = CastSource(source);
//			if (src != null)
//			{
//				Id = src.Id;
//				UserName = src.UserName;
//				Email = src.Email;
//				EmailConfirmed = src.EmailConfirmed;
//				PasswordHash = src.PasswordHash;
//				SecurityStamp = src.SecurityStamp;
//				PhoneNumber = src.PhoneNumber;
//				PhoneNumberConfirmed = src.PhoneNumberConfirmed;
//				TwoFactorEnabled = src.TwoFactorEnabled;
//				LockoutEndDateUtc = src.LockoutEndDateUtc;
//				LockoutEnabled = src.LockoutEnabled;
//				AccessFailedCount = src.AccessFailedCount;
//#if (NETSTANDARD2_0)
//                this.NormalizedName = src.NormalizedName;
//                this.NormalizedEmail = src.NormalizedEmail;
//#endif

//				if (loadingFlags.BitHas(DxIdentityUserFlags.FLAG_ROLES))
//				{
//					AssignRoles(src.RolesList);
//				}
//				if (loadingFlags.BitHas(DxIdentityUserFlags.FLAG_CLAIMS))
//				{
//					AssignClaims(src.ClaimsList);
//				}
//				if (loadingFlags.BitHas(DxIdentityUserFlags.FLAG_LOGINS))
//				{
//					AssignLogins(src.LoginsList);
//				}
//			}
//		}

		public void AssignRoles(IList roles)
		{
			Roles = new List<TRole>();
			foreach (var role in roles)
				Roles.Add(Activator.CreateInstance(typeof(TRole), role as XpoDxRole) as TRole);
		}

		public void AssignClaims(IList claims)
		{
			Claims = new List<TClaim>();
			foreach (var claim in claims)
				Claims.Add(Activator.CreateInstance(typeof(TClaim), claim as XpoDxUserClaim) as TClaim);
		}

		public void AssignLogins(IList logins)
		{
			Logins = new List<TLogin>();
			foreach (var login in logins)
				Logins.Add(Activator.CreateInstance(typeof(TLogin), login as XpoDxUserLogin) as TLogin);
		}
	}

}

