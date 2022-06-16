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
		string IXPUser<string>.Id { get => base.Id; set => setId(value); }
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
		//#if (NETSTANDARD2_1 || NETCOREAPP)
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
		//#if (NETSTANDARD2_1 || NETCOREAPP)
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
        // Created/Updated: DEV-RIG-DON\don on DEV-RIG-DON at 6/15/2022 3:40 PM
        public new class FieldsClass : XpoDxBase.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }

            public const string RolesListFieldName = "RolesList";

            public OperandProperty RolesList => new OperandProperty(GetNestedName(RolesListFieldName));

            public const string ClaimsListFieldName = "ClaimsList";

            public OperandProperty ClaimsList => new OperandProperty(GetNestedName(ClaimsListFieldName));

            public const string LoginsListFieldName = "LoginsList";

            public OperandProperty LoginsList => new OperandProperty(GetNestedName(LoginsListFieldName));

            public const string EmailFieldName = "Email";

            public OperandProperty Email => new OperandProperty(GetNestedName(EmailFieldName));

            public const string EmailConfirmedFieldName = "EmailConfirmed";

            public OperandProperty EmailConfirmed => new OperandProperty(GetNestedName(EmailConfirmedFieldName));

            public const string PasswordHashFieldName = "PasswordHash";

            public OperandProperty PasswordHash => new OperandProperty(GetNestedName(PasswordHashFieldName));

            public const string SecurityStampFieldName = "SecurityStamp";

            public OperandProperty SecurityStamp => new OperandProperty(GetNestedName(SecurityStampFieldName));

            public const string PhoneNumberFieldName = "PhoneNumber";

            public OperandProperty PhoneNumber => new OperandProperty(GetNestedName(PhoneNumberFieldName));

            public const string PhoneNumberConfirmedFieldName = "PhoneNumberConfirmed";

            public OperandProperty PhoneNumberConfirmed => new OperandProperty(GetNestedName(PhoneNumberConfirmedFieldName));

            public const string TwoFactorEnabledFieldName = "TwoFactorEnabled";

            public OperandProperty TwoFactorEnabled => new OperandProperty(GetNestedName(TwoFactorEnabledFieldName));

            public const string LockoutEndDateUtcFieldName = "LockoutEndDateUtc";

            public OperandProperty LockoutEndDateUtc => new OperandProperty(GetNestedName(LockoutEndDateUtcFieldName));

            public const string LockoutEnabledFieldName = "LockoutEnabled";

            public OperandProperty LockoutEnabled => new OperandProperty(GetNestedName(LockoutEnabledFieldName));

            public const string AccessFailedCountFieldName = "AccessFailedCount";

            public OperandProperty AccessFailedCount => new OperandProperty(GetNestedName(AccessFailedCountFieldName));

            public const string UserNameFieldName = "UserName";

            public OperandProperty UserName => new OperandProperty(GetNestedName(UserNameFieldName));

            public const string _EmailUpperFieldName = "_EmailUpper";

            public OperandProperty _EmailUpper => new OperandProperty(GetNestedName(_EmailUpperFieldName));

            public const string EmailUpperFieldName = "EmailUpper";

            public OperandProperty EmailUpper => new OperandProperty(GetNestedName(EmailUpperFieldName));

            public const string _UserNameUpperFieldName = "_UserNameUpper";

            public OperandProperty _UserNameUpper => new OperandProperty(GetNestedName(_UserNameUpperFieldName));

            public const string UserNameUpperFieldName = "UserNameUpper";

            public OperandProperty UserNameUpper => new OperandProperty(GetNestedName(UserNameUpperFieldName));

            public const string NormalizedEmailFieldName = "NormalizedEmail";

            public OperandProperty NormalizedEmail => new OperandProperty(GetNestedName(NormalizedEmailFieldName));

            public const string NormalizedNameFieldName = "NormalizedName";

            public OperandProperty NormalizedName => new OperandProperty(GetNestedName(NormalizedNameFieldName));

            public const string RefreshTokenFieldName = "RefreshToken";

            public OperandProperty RefreshToken => new OperandProperty(GetNestedName(RefreshTokenFieldName));

            public const string RefreshTokenExpiryTimeFieldName = "RefreshTokenExpiryTime";

            public OperandProperty RefreshTokenExpiryTime => new OperandProperty(GetNestedName(RefreshTokenExpiryTimeFieldName));

            public const string RolesFieldName = "Roles";

            public OperandProperty Roles => new OperandProperty(GetNestedName(RolesFieldName));

            public const string LoginsFieldName = "Logins";

            public OperandProperty Logins => new OperandProperty(GetNestedName(LoginsFieldName));

            public const string TokensFieldName = "Tokens";

            public OperandProperty Tokens => new OperandProperty(GetNestedName(TokensFieldName));

            public const string ClaimsFieldName = "Claims";

            public OperandProperty Claims => new OperandProperty(GetNestedName(ClaimsFieldName));
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
