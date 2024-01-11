using DevExpress.Xpo;
using DevExpress.Xpo.Metadata.Helpers;
using DX.Data.Xpo.Identity.Persistent;
using FluentValidation;
using System;
using System.Linq;


namespace DX.Data.Xpo.Identity
{
	public class XPIdentityUserValidator<TKey, TDBUser> : AbstractValidator<TDBUser>
		where TKey : IEquatable<TKey>
		where TDBUser : IXPSimpleObject, IXPUser<TKey>
	{
		public XPIdentityUserValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty()
				.EmailAddress();
			RuleFor(x => x.UserName)
				.NotEmpty();
		}
	}

    public class XPIdentityUserValidator : XPIdentityUserValidator<string, XpoDxUser>
    {
        public XPIdentityUserValidator() : base()
        {

        }
    }

    public class XPIdentityUserClaimValidator<TKey, TDBUserClaim> : AbstractValidator<TDBUserClaim>
        where TKey : IEquatable<TKey>
        where TDBUserClaim : IXPSimpleObject, IXPUserClaim<TKey>
    {
        public XPIdentityUserClaimValidator()
        {
            RuleFor(x=>x.UserId)
                .NotEmpty();
            RuleFor(x=>x.ClaimType)
                .NotEmpty();
        }
    }

    public class XPIdentityUserClaimValidator : XPIdentityUserClaimValidator<string, XpoDxUserClaim>
    {
        public XPIdentityUserClaimValidator() : base()
        {

        }
    }

    public class XPIdentityUserLoginValidator<TKey, TDBUserLogin> : AbstractValidator<TDBUserLogin>
        where TKey : IEquatable<TKey>
        where TDBUserLogin : IXPSimpleObject, IXPUserLogin<TKey>
    {
        public XPIdentityUserLoginValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();
            RuleFor(x => x.LoginProvider)
                .NotEmpty();
            RuleFor(x => x.ProviderKey)
                .NotEmpty();
        }
    }

    public class XPIdentityUserLoginValidator : XPIdentityUserLoginValidator<string, XpoDxUserLogin>
    {
        public XPIdentityUserLoginValidator() : base()
        {

        }
    }

    public class XPIdentityUserTokenValidator<TKey, TDBUserToken> : AbstractValidator<TDBUserToken>
        where TKey : IEquatable<TKey>
        where TDBUserToken : IXPSimpleObject, IXPUserToken<TKey>
    {
        public XPIdentityUserTokenValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();
            RuleFor(x => x.Name)
                .NotEmpty();
            RuleFor(x => x.Value)
                .NotEmpty();
        }
    }

    public class XPIdentityUserTokenValidator : XPIdentityUserTokenValidator<string, XpoDxUserToken>
    {
        public XPIdentityUserTokenValidator() : base()
        {

        }
    }

    public class XPIdentityRoleValidator<TKey, TDBRole> : AbstractValidator<TDBRole>
        where TKey : IEquatable<TKey>
        where TDBRole : IXPSimpleObject, IXPRole<TKey>
    {
        public XPIdentityRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();
        }
    }
    public class XPIdentityRoleValidator : XPIdentityRoleValidator<string, XpoDxRole>
    {
        public XPIdentityRoleValidator() : base()
        {

        }
    }

    public class XPIdentityRoleClaimValidator<TKey, TDBRoleClaim> : AbstractValidator<TDBRoleClaim>
        where TKey : IEquatable<TKey>
        where TDBRoleClaim : IXPSimpleObject, IXPRoleClaim<TKey>
    {
        public XPIdentityRoleClaimValidator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty();
            RuleFor(x => x.ClaimType)
                .NotEmpty();
            RuleFor(x => x.ClaimValue)
                .NotEmpty();
        }
    }
    public class XPIdentityRoleClaimValidator : XPIdentityRoleClaimValidator<string, XpoDxRoleClaim>
    {
        public XPIdentityRoleClaimValidator() : base()
        {

        }
    }

}
