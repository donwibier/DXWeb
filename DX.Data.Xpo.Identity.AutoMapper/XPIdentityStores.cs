using AutoMapper;
using AutoMapper.QueryableExtensions;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Data.Xpo.AutoMapper;
using DX.Data.Xpo.Identity.Persistent;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DX.Data.Xpo.Identity.AutoMapper
{
    public class XPAutoMapperUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
                                    TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken> :
            XPBaseUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
                                    TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken>, IQueryableUserStore<TKey, TUser, TUserRole, TUserToken>
                where TKey : IEquatable<TKey>
                where TUser : IdentityUser<TKey>, new()
                where TRole : IdentityRole<TKey>, new()
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
        public XPAutoMapperUserStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPOUser> validator) : base(dataLayer, validator)
        {
            Mapper = mapper;
        }

        public IMapper Mapper { get; }

        public override TUser GetByKey(TKey key)
        {
            using (var wrk = new Session(DataLayer))
            {
                TUser result = CreateModel();
                return MapTo(XPOGetByKey(key, wrk), result);
            }
        }

        public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectTo<T>(Mapper.ConfigurationProvider);

		protected override TDestination MapTo<TSource, TDestination>(TSource source, TDestination target) => Mapper.Map<TSource, TDestination>(source, target);
    }

    public class XPAutoMapperRoleStore<TKey, TRole, TRoleClaim, TXPORole, TXPOClaim> 
            : XPBaseRoleStore<TKey, TRole, TRoleClaim, TXPORole, TXPOClaim>
        where TKey : IEquatable<TKey>
        where TRole : IdentityRole<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>
        where TXPORole : XPBaseObject, IXPRole<TKey>
        where TXPOClaim : XPBaseObject, IXPRoleClaim<TKey>
    {
        public XPAutoMapperRoleStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPORole> validator) : base(dataLayer, validator)
        {
            Mapper = mapper;
        }

        public IMapper Mapper { get; }
        
        public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectTo<T>(Mapper.ConfigurationProvider);
        protected override TDestination MapTo<TSource, TDestination>(TSource source, TDestination target) => Mapper.Map<TSource, TDestination>(source, target);
        
        public override TRole GetByKey(TKey key)
        {
            using (var wrk = new Session(DataLayer))
            {
                TRole result = CreateModel();
                return MapTo(XPOGetByKey(key, wrk), result);
            }
        }
	}

    public class XPAutoMapperUserLoginStore<TKey, TUserLogin, TXPOUserLogin> : XPBaseUserLoginStore<TKey, TUserLogin, TXPOUserLogin>
		where TKey : IEquatable<TKey>
        where TUserLogin:IdentityUserLogin<TKey>, new()
        where TXPOUserLogin : XPBaseObject, IXPUserLogin<TKey>
    {
        public XPAutoMapperUserLoginStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPOUserLogin> validator) : base(dataLayer, validator)
        {
			Mapper = mapper;
		}

		public IMapper Mapper { get; }
		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectTo<T>(Mapper.ConfigurationProvider);
		protected override TDestination MapTo<TSource, TDestination>(TSource source, TDestination target) => Mapper.Map<TSource, TDestination>(source, target);

		public override TUserLogin GetByKey(TKey key)
		{
			using (var wrk = new Session(DataLayer))
			{
				var result = CreateModel();
				return MapTo(XPOGetByKey(key, wrk), result);
			}
		}

	}


	public class XPAutoMapperUserClaimStore<TKey, TUserClaim, TXPOUserClaim> : XPBaseUserClaimStore<TKey, TUserClaim, TXPOUserClaim> 
		where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TXPOUserClaim: XPBaseObject, IXPUserClaim<TKey>
    {
        public XPAutoMapperUserClaimStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPOUserClaim> validator) : base(dataLayer, validator)
        {
			Mapper = mapper;
		}
		public IMapper Mapper { get; }
		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectTo<T>(Mapper.ConfigurationProvider);
		protected override TDestination MapTo<TSource, TDestination>(TSource source, TDestination target) => Mapper.Map<TSource, TDestination>(source, target);

		public override TUserClaim GetByKey(TKey key)
		{
			using (var wrk = new Session(DataLayer))
			{
				var result = CreateModel();
				return MapTo(XPOGetByKey(key, wrk), result);
			}
		}
	}

	public class XPAutoMapperUserTokenStore<TKey, TUserToken, TXPOUserToken> : XPBaseUserTokenStore<TKey, TUserToken, TXPOUserToken>
		where TKey : IEquatable<TKey>
        where TUserToken : IdentityUserToken<TKey>, new()
        where TXPOUserToken : XPBaseObject, IXPUserToken<TKey>
    {
        public XPAutoMapperUserTokenStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPOUserToken> validator) : base(dataLayer, validator)
        {
			Mapper = mapper;
		}

		public IMapper Mapper { get; }
		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectTo<T>(Mapper.ConfigurationProvider);
		protected override TDestination MapTo<TSource, TDestination>(TSource source, TDestination target) => Mapper.Map<TSource, TDestination>(source, target);

		public override TUserToken GetByKey(TKey key)
		{
			using (var wrk = new Session(DataLayer))
			{
				var result = CreateModel();
				return MapTo(XPOGetByKey(key, wrk), result);
			}
		}
	}

	public class XPAutoMapperRoleClaimStore<TKey, TRoleClaim, TXPORoleClaim> : XPBaseRoleClaimStore<TKey, TRoleClaim, TXPORoleClaim>
	where TKey : IEquatable<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
    where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
    {
        public XPAutoMapperRoleClaimStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPORoleClaim> validator) : base(dataLayer, validator)
        {
			Mapper = mapper;
		}

		public IMapper Mapper { get; }
		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectTo<T>(Mapper.ConfigurationProvider);
		protected override TDestination MapTo<TSource, TDestination>(TSource source, TDestination target) => Mapper.Map<TSource, TDestination>(source, target);

		public override TRoleClaim GetByKey(TKey key)
		{
			using (var wrk = new Session(DataLayer))
			{
				var result = CreateModel();
				return MapTo(XPOGetByKey(key, wrk), result);
			}
		}

	}
}
