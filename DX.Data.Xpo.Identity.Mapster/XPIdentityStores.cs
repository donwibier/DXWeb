using DevExpress.Xpo;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;

namespace DX.Data.Xpo.Identity.Mapster
{
	public class XPMapsterUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
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
		public XPMapsterUserStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPOUser> validator) : base(dataLayer, validator)
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

		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectToType<T>();

		protected override TDestination MapTo<TSource, TDestination>(TSource source, TDestination target) => Mapper.Map<TSource, TDestination>(source, target);
	}

	public class XPMapsterRoleStore<TKey, TRole, TRoleClaim, TXPORole, TXPOClaim>
			: XPBaseRoleStore<TKey, TRole, TRoleClaim, TXPORole, TXPOClaim>
		where TKey : IEquatable<TKey>
		where TRole : IdentityRole<TKey>, new()
		where TRoleClaim : IdentityRoleClaim<TKey>
		where TXPORole : XPBaseObject, IXPRole<TKey>
		where TXPOClaim : XPBaseObject, IXPRoleClaim<TKey>
	{
		public XPMapsterRoleStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPORole> validator) : base(dataLayer, validator)
		{
			Mapper = mapper;
		}

		public IMapper Mapper { get; }

		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectToType<T>();
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

	public class XPMapsterUserLoginStore<TKey, TUserLogin, TXPOUserLogin> : XPBaseUserLoginStore<TKey, TUserLogin, TXPOUserLogin>
		where TKey : IEquatable<TKey>
		where TUserLogin : IdentityUserLogin<TKey>, new()
		where TXPOUserLogin : XPBaseObject, IXPUserLogin<TKey>
	{
		public XPMapsterUserLoginStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPOUserLogin> validator) : base(dataLayer, validator)
		{
			Mapper = mapper;
		}

		public IMapper Mapper { get; }
		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectToType<T>();
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


	public class XPMapsterUserClaimStore<TKey, TUserClaim, TXPOUserClaim> : XPBaseUserClaimStore<TKey, TUserClaim, TXPOUserClaim>
		where TKey : IEquatable<TKey>
		where TUserClaim : IdentityUserClaim<TKey>, new()
		where TXPOUserClaim : XPBaseObject, IXPUserClaim<TKey>
	{
		public XPMapsterUserClaimStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPOUserClaim> validator) : base(dataLayer, validator)
		{
			Mapper = mapper;
		}
		public IMapper Mapper { get; }
		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectToType<T>();
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

	public class XPMapsterUserTokenStore<TKey, TUserToken, TXPOUserToken> : XPBaseUserTokenStore<TKey, TUserToken, TXPOUserToken>
		where TKey : IEquatable<TKey>
		where TUserToken : IdentityUserToken<TKey>, new()
		where TXPOUserToken : XPBaseObject, IXPUserToken<TKey>
	{
		public XPMapsterUserTokenStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPOUserToken> validator) : base(dataLayer, validator)
		{
			Mapper = mapper;
		}

		public IMapper Mapper { get; }
		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectToType<T>();
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

	public class XPMapsterRoleClaimStore<TKey, TRoleClaim, TXPORoleClaim> : XPBaseRoleClaimStore<TKey, TRoleClaim, TXPORoleClaim>
	where TKey : IEquatable<TKey>
	where TRoleClaim : IdentityRoleClaim<TKey>, new()
	where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
	{
		public XPMapsterRoleClaimStore(IDataLayer dataLayer, IMapper mapper, IValidator<TXPORoleClaim> validator) : base(dataLayer, validator)
		{
			Mapper = mapper;
		}

		public IMapper Mapper { get; }
		public override IQueryable<T> Query<T>() => DBQuery(UnitOfWork).ProjectToType<T>();
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
