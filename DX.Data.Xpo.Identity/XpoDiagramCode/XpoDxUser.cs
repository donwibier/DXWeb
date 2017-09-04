using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using DX.Utils;

namespace DX.Data.Xpo.Identity
{

	public partial class XpoDxUser : IDxUser<string>
	{
		public XpoDxUser(Session session) : base(session) { }
		public override void AfterConstruction() { base.AfterConstruction(); }

		#region Embedded Fields class

		public new class Fields : XpoDxBase.Fields
		{
			protected Fields() { }
			public static OperandProperty UserName { get { return new OperandProperty("UserName"); } }
			public static OperandProperty UserNameUpper { get { return new OperandProperty("UserNameUpper"); } }
			public static OperandProperty PasswordHash { get { return new OperandProperty("PasswordHash"); } }
			public static OperandProperty SecurityStamp { get { return new OperandProperty("SecurityStamp"); } }
			public static OperandProperty Email { get { return new OperandProperty("Email"); } }
			public static OperandProperty EmailUpper { get { return new OperandProperty("EmailUpper"); } }
			public static OperandProperty EmailConfirmed { get { return new OperandProperty("EmailConfirmed"); } }
			public static OperandProperty PhoneNumber { get { return new OperandProperty("PhoneNumber"); } }
			public static OperandProperty PhoneNumberConfirmed { get { return new OperandProperty("PhoneNumberConfirmed"); } }
			public static OperandProperty TwoFactorEnabled { get { return new OperandProperty("TwoFactorEnabled"); } }
			public static OperandProperty LockoutEndDateUtc { get { return new OperandProperty("LockoutEndDateUtc"); } }
			public static OperandProperty LockoutEnabled { get { return new OperandProperty("LockoutEnabled"); } }
			public static OperandProperty AccessFailedCount { get { return new OperandProperty("AccessFailedCount"); } }

			public static OperandProperty Roles { get { return new OperandProperty("Roles"); } }
			public static OperandProperty Logins { get { return new OperandProperty("Logins"); } }
			public static OperandProperty Claims { get { return new OperandProperty("Claims"); } }

		}
		#endregion

		protected override void OnChanged(string propertyName, object oldValue, object newValue)
		{
			if (propertyName == "Email")
				_EmailUpper = ((string)newValue ?? "").ToUpperInvariant();
			else if (propertyName == "UserName")
				_UserNameUpper = ((string)newValue ?? "").ToUpperInvariant();

			base.OnChanged(propertyName, oldValue, newValue);
		}

		public IList RolesList { get { return Roles; } }

		public IList ClaimsList { get { return Claims; } }

		public IList LoginsList { get { return Logins; } }

		public override void Assign(object source, int loadingFlags)
		{
			base.Assign(source, loadingFlags);
			IDxUser<string> src = source as IDxUser<string>;
			if (src != null)
			{
				this.UserName = src.UserName;
				//this.UserNameUpper = src.UserNameUpper;
				this.PasswordHash = src.PasswordHash;
				this.SecurityStamp = src.SecurityStamp;
				this.Email = src.Email;
				//this.EmailUpper = src.EmailUpper;
				this.EmailConfirmed = src.EmailConfirmed;
				this.PhoneNumber = src.PhoneNumber;
				this.PhoneNumberConfirmed = src.PhoneNumberConfirmed;
				this.TwoFactorEnabled = src.TwoFactorEnabled;
				this.LockoutEndDateUtc = src.LockoutEndDateUtc;
				this.LockoutEnabled = src.LockoutEnabled;
				this.AccessFailedCount = src.AccessFailedCount;

				if (loadingFlags.BitHas(DxIdentityUserFlags.FLAG_ROLES))
					AssignRoles(src.RolesList);
				if (loadingFlags.BitHas(DxIdentityUserFlags.FLAG_LOGINS))
					AssignLogins(src.LoginsList);
				if (loadingFlags.BitHas(DxIdentityUserFlags.FLAG_CLAIMS))
					AssignClaims(src.ClaimsList);
			}

		}
		public void AssignRoles(IList roles)
		{
			if (roles == null)
				return;
			foreach (var role in new XPCollection(Session, typeof(XpoDxRole), XpoDxRole.Fields.Users[XpoDxUser.Fields.Id == Id], null))
			{
				Roles.Remove(role as XpoDxRole);
			}
			foreach (var r in roles)
			{
				IDxRole<string> role = r as IDxRole<string>;
				if (role != null)
					Roles.Add(Session.FindObject(typeof(XpoDxRole), XpoDxRole.Fields.NameUpper == role.Name.ToUpperInvariant()) as XpoDxRole);
			}
		}

		public void AssignClaims(IList claims)
		{
			if (claims == null)
				return;
			foreach (var claim in new XPCollection(Session, typeof(XpoDxUserClaim), CriteriaOperator.Parse("[User!Key] == ?", Id), null))
			{
				Claims.Remove(claim as XpoDxUserClaim);
			}
			foreach (var c in claims)
			{
				IDxUserClaim<string> claim = c as IDxUserClaim<string>;
				if (claim != null)
				{
					Claims.Add(new XpoDxUserClaim(Session)
					{
						User = this,
						ClaimType = claim.ClaimType,
						ClaimValue = claim.ClaimValue

					});
				}
			}
		}

		public void AssignLogins(IList logins)
		{
			if (logins == null)
				return;
			foreach (var login in new XPCollection(Session, typeof(XpoDxUserLogin), CriteriaOperator.Parse("[User!Key] == ?", Id), null))
				Logins.Remove(login as XpoDxUserLogin);
			foreach (var l in logins)
			{
				IDxUserLogin<string> login = l as IDxUserLogin<string>;
				if (l != null)
				{
					Logins.Add(new XpoDxUserLogin(Session)
					{
						User = this,
						LoginProvider = login.LoginProvider,
						ProviderKey = login.ProviderKey
					});
				}
			}
		}
	}

}
