using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace DX.Data.Xpo.Identity.Persistent
{

	public partial class XpoDxUser : IXPUser<string>
	{
		public XpoDxUser(Session session) : base(session) { }
		public override void AfterConstruction() { base.AfterConstruction(); }

		protected override void OnDeleting()
		{
			var roles = new XPCollection<XpoDxRole>(CriteriaOperator.Parse("Users[Id == ?]", Id), null);
			foreach (var r in roles)
			{
				r.Users.Remove(this);
				if (!(Session is UnitOfWork))
					r.Save();

			}
			Session.Delete(new XPCollection<XpoDxUserClaim>(CriteriaOperator.Parse("User.Id == ?", Id), null));

			base.OnDeleting();
		}
		protected override void OnChanged(string propertyName, object oldValue, object newValue)
		{
			if (propertyName == nameof(Email))
				_EmailUpper = ((string)newValue ?? string.Empty).ToUpperInvariant();
			else if (propertyName == nameof(UserName))
				_UserNameUpper = ((string)newValue ?? string.Empty).ToUpperInvariant();

			base.OnChanged(propertyName, oldValue, newValue);
		}
		//#if (NETSTANDARD2_1)
		//        public string NormalizedName
		//        {
		//            get { return UserNameUpper; }
		//            set { _UserNameUpper = (value ?? "").ToUpperInvariant(); }
		//        }

		//        public string NormalizedEmail
		//        {
		//            get { return EmailUpper; }
		//            set { _EmailUpper = (value ?? "").ToUpperInvariant(); }
		//        }
		//#endif       

		public IList RolesList { get { return Roles; } }

		public IList ClaimsList { get { return Claims; } }

		public IList LoginsList { get { return Logins; } }

		//        public override void Assign(object source, int loadingFlags)
		//        {
		//            base.Assign(source, loadingFlags);
		//            IDxUser<string> src = source as IDxUser<string>;
		//            if (src != null)
		//            {
		//                this.UserName = src.UserName;
		//                //this.UserNameUpper = src.UserNameUpper;
		//                this.PasswordHash = src.PasswordHash;
		//                this.SecurityStamp = src.SecurityStamp;
		//                this.Email = src.Email;
		//                //this.EmailUpper = src.EmailUpper;
		//                this.EmailConfirmed = src.EmailConfirmed;
		//                this.PhoneNumber = src.PhoneNumber;
		//                this.PhoneNumberConfirmed = src.PhoneNumberConfirmed;
		//                this.TwoFactorEnabled = src.TwoFactorEnabled;
		//                this.LockoutEndDateUtc = src.LockoutEndDateUtc;
		//                this.LockoutEnabled = src.LockoutEnabled;
		//                this.AccessFailedCount = src.AccessFailedCount;
		//#if (NETSTANDARD2_1)
		//                this.NormalizedName = src.NormalizedName;
		//                this.NormalizedEmail = src.NormalizedEmail;
		//#endif
		//                if (loadingFlags.BitHas(DxIdentityUserFlags.FLAG_ROLES))
		//                    AssignRoles(src.RolesList);
		//                if (loadingFlags.BitHas(DxIdentityUserFlags.FLAG_LOGINS))
		//                    AssignLogins(src.LoginsList);
		//                if (loadingFlags.BitHas(DxIdentityUserFlags.FLAG_CLAIMS))
		//                    AssignClaims(src.ClaimsList);
		//            }

		//        }
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
				IXPRole<string> role = r as IXPRole<string>;
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
				IXPUserClaim<string> claim = c as IXPUserClaim<string>;
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
				IXPUserLogin<string> login = l as IXPUserLogin<string>;
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

		#region Embedded Fields class
		public new class FieldsClass : XpoDxBase.FieldsClass
		{
			public FieldsClass()
			{

			}

			public FieldsClass(string propertyName) : base(propertyName)
			{

			}

			public OperandProperty RolesList
			{
				get { return new OperandProperty(GetNestedName(nameof(RolesList))); }
			}

			public OperandProperty ClaimsList
			{
				get { return new OperandProperty(GetNestedName(nameof(ClaimsList))); }
			}

			public OperandProperty LoginsList
			{
				get { return new OperandProperty(GetNestedName(nameof(LoginsList))); }
			}

			public OperandProperty Email
			{
				get { return new OperandProperty(GetNestedName(nameof(Email))); }
			}

			public OperandProperty EmailConfirmed
			{
				get { return new OperandProperty(GetNestedName(nameof(EmailConfirmed))); }
			}

			public OperandProperty PasswordHash
			{
				get { return new OperandProperty(GetNestedName(nameof(PasswordHash))); }
			}

			public OperandProperty SecurityStamp
			{
				get { return new OperandProperty(GetNestedName(nameof(SecurityStamp))); }
			}

			public OperandProperty PhoneNumber
			{
				get { return new OperandProperty(GetNestedName(nameof(PhoneNumber))); }
			}

			public OperandProperty PhoneNumberConfirmed
			{
				get { return new OperandProperty(GetNestedName(nameof(PhoneNumberConfirmed))); }
			}

			public OperandProperty TwoFactorEnabled
			{
				get { return new OperandProperty(GetNestedName(nameof(TwoFactorEnabled))); }
			}

			public OperandProperty LockoutEndDateUtc
			{
				get { return new OperandProperty(GetNestedName(nameof(LockoutEndDateUtc))); }
			}

			public OperandProperty LockoutEnabled
			{
				get { return new OperandProperty(GetNestedName(nameof(LockoutEnabled))); }
			}

			public OperandProperty AccessFailedCount
			{
				get { return new OperandProperty(GetNestedName(nameof(AccessFailedCount))); }
			}

			public OperandProperty UserName
			{
				get { return new OperandProperty(GetNestedName(nameof(UserName))); }
			}

			public OperandProperty _EmailUpper
			{
				get { return new OperandProperty(GetNestedName(nameof(_EmailUpper))); }
			}

			public OperandProperty EmailUpper
			{
				get { return new OperandProperty(GetNestedName(nameof(EmailUpper))); }
			}

			public OperandProperty _UserNameUpper
			{
				get { return new OperandProperty(GetNestedName(nameof(_UserNameUpper))); }
			}

			public OperandProperty UserNameUpper
			{
				get { return new OperandProperty(GetNestedName(nameof(UserNameUpper))); }
			}

			public OperandProperty Roles
			{
				get { return new OperandProperty(GetNestedName(nameof(Roles))); }
			}

			public OperandProperty Logins
			{
				get { return new OperandProperty(GetNestedName(nameof(Logins))); }
			}

			public OperandProperty Claims
			{
				get { return new OperandProperty(GetNestedName(nameof(Claims))); }
			}
		}

		public new static FieldsClass Fields
		{
			get
			{
				if (ReferenceEquals(_Fields, null))
				{
					_Fields = new FieldsClass();
				}

				return _Fields;
			}
		}

		static FieldsClass _Fields;

		#endregion

	}

}
