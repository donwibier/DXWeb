using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;

using Mapster;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DX.Data.Xpo.Identity.Mapster
{
#if (NETCOREAPP)
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.AspNetCore.Identity;


	public static class RegisterServices
	{
		public static IdentityBuilder AddXpoMapsterIdentityStores(this IdentityBuilder builder, string connectionName)
		{
			return AddXpoMapsterIdentityStores<string,
				IdentityUser, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityUserRole<string>,
				IdentityRole, IdentityRoleClaim<string>,
				XpoDxUser, XpoDxUserClaim, XpoDxUserLogin, XpoDxUserToken, XpoDxRole, XpoDxRoleClaim>(builder, connectionName);

		}

		public static IdentityBuilder AddXpoMapsterIdentityStores<TUser, TXPOUser>(this IdentityBuilder builder, string connectionName)
			where TUser : IdentityUser<string>, new()
			where TXPOUser : XpoDxUser, IXPUser<string>
		{
			return AddXpoMapsterIdentityStores<string,
						TUser, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityUserRole<string>,
						IdentityRole, IdentityRoleClaim<string>,
						TXPOUser, XpoDxUserClaim, XpoDxUserLogin, XpoDxUserToken, XpoDxRole, XpoDxRoleClaim>(builder, connectionName);
		}

		public static IdentityBuilder AddXpoMapsterIdentityStores<TUser, TXPOUser, TRole, TXPORole>
							(this IdentityBuilder builder, string connectionName)
			where TUser : IdentityUser<string>, new()
			where TRole : IdentityRole<string>, new()
			where TXPOUser : XpoDxUser, IXPUser<string>
			where TXPORole : XpoDxRole, IXPRole<string>
		{
			return AddXpoMapsterIdentityStores<string,
				TUser, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityUserRole<string>,
				TRole, IdentityRoleClaim<string>,
				TXPOUser, XpoDxUserClaim, XpoDxUserLogin, XpoDxUserToken, TXPORole, XpoDxRoleClaim>(builder, connectionName);
		}

		public static IdentityBuilder AddXpoMapsterIdentityStores<TKey,
				TUser, TUserClaim, TUserLogin, TUserToken, TUserRole, TRole, TRoleClaim,
				TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken, TXPORole, TXPORoleClaim>
									(this IdentityBuilder builder, string connectionName)
			where TKey : IEquatable<TKey>
			where TUser : IdentityUser<TKey>, new()
			where TRole : IdentityRole<TKey>, new()
			where TUserClaim : IdentityUserClaim<TKey>, new()
			where TUserRole : IdentityUserRole<TKey>, new()
			where TUserLogin : IdentityUserLogin<TKey>, new()
			where TUserToken : IdentityUserToken<TKey>, new()
			where TRoleClaim : IdentityRoleClaim<TKey>, new()
			where TXPOUser : XPBaseObject, IXPUser<TKey>
			where TXPOUserLogin : XPBaseObject, IXPUserLogin<TKey>
			where TXPOUserClaim : XPBaseObject, IXPUserClaim<TKey>
			where TXPOUserToken : XPBaseObject, IXPUserToken<TKey>
			where TXPORole : XPBaseObject, IXPRole<TKey>
			where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
		{
			var services = builder.Services;
			services.AddTransient<IMapper, Mapper>();
			// The identity mapping rules go in a TypeAdapterConfig of their own (a copy of the application mapper's config,
			// with the rules added), not in the process-wide TypeAdapterConfig.GlobalSettings: rewriting those on every
			// registration broke mapping that was going on at the same time ("TypeAdapter.Adapt was already called")
			var identityMappers = new XPIdentityMapsterMappers(config => config.ApplyXPIdentityMapsterConfiguration<TKey, TUser, TRole,
						TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
						TXPOUser, TXPORole, TXPOUserLogin, TXPOUserClaim, TXPOUserToken, TXPORoleClaim>());

			// Default IDatalayer for Identity
			services.AddSingleton<IDataLayer>(sp => {
				var db = sp.GetRequiredService<XpoDatabase>();
				return db.GetDataLayer(connectionName);
			});


			services.RegisterXPIdentityValidators<TKey, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken, TXPORole, TXPORoleClaim>();

			services.AddScoped<IQueryableUserStore<TKey, TUser, TUserRole, TUserToken>>(sp => identityMappers.Create<
				XPMapsterUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
										TXPOUser, TXPORole, TXPOUserLogin, TXPOUserClaim, TXPOUserToken>>(sp));

			services.AddScoped<IQueryableRoleStore<TKey, TRole>>(sp => identityMappers.Create<XPMapsterRoleStore<TKey, TRole, TRoleClaim, TXPORole, TXPORoleClaim>>(sp));


			services.AddScoped<IQueryableUserLoginStore<TKey, TUserLogin>>(sp => identityMappers.Create<XPMapsterUserLoginStore<TKey, TUserLogin, TXPOUserLogin>>(sp));
			services.AddScoped<IQueryableUserClaimStore<TKey, TUserClaim>>(sp => identityMappers.Create<XPMapsterUserClaimStore<TKey, TUserClaim, TXPOUserClaim>>(sp));
			services.AddScoped<IQueryableUserTokenStore<TKey, TUserToken>>(sp => identityMappers.Create<XPMapsterUserTokenStore<TKey, TUserToken, TXPOUserToken>>(sp));
			services.AddScoped<IQueryableRoleClaimStore<TKey, TRoleClaim>>(sp => identityMappers.Create<XPMapsterRoleClaimStore<TKey, TRoleClaim, TXPORoleClaim>>(sp));

			RegisterIdentityServices.AddStores<TKey>(builder.Services, connectionName,
				builder.UserType ?? typeof(TUser), typeof(TUserClaim), typeof(TUserLogin), typeof(TUserToken), typeof(TUserRole),
				 builder.RoleType ?? typeof(TRole), typeof(TRoleClaim),
				typeof(TXPOUser), typeof(TXPOUserLogin), typeof(TXPOUserClaim), typeof(TXPOUserToken),
				typeof(TXPORole), typeof(TXPORoleClaim));

			return builder;
		}
	}
	/// <summary>
	/// The mapper of one AddXpoMapsterIdentityStores registration: a copy of the application mapper's TypeAdapterConfig
	/// (TypeAdapterConfig.GlobalSettings for the default Mapper) with the identity rules added, built on first use
	/// </summary>
	internal sealed class XPIdentityMapsterMappers
	{
		private readonly Action<TypeAdapterConfig> _configure;
		private readonly object _lock = new object();
		private TypeAdapterConfig? _source;
		private IMapper? _mapper;

		public XPIdentityMapsterMappers(Action<TypeAdapterConfig> configure) => _configure = configure;

		public IMapper For(IMapper applicationMapper)
		{
			var source = applicationMapper.Config;
			lock (_lock)
			{
				if (_mapper == null || !ReferenceEquals(_source, source))
				{
					var config = source.Clone();
					_configure(config);
					_mapper = new Mapper(config);
					_source = source;
				}
				return _mapper;
			}
		}

		public TStore Create<TStore>(IServiceProvider sp)
			=> ActivatorUtilities.CreateInstance<TStore>(sp, For(sp.GetRequiredService<IMapper>()));
	}

	public static class XPIdentityMapsterConfig
	{
		/// <summary>
		/// Adds the identity mapping rules to Mapster's process-wide TypeAdapterConfig.GlobalSettings.
		/// AddXpoMapsterIdentityStores no longer calls this: it keeps the rules in a TypeAdapterConfig of its own
		/// (see <see cref="ApplyXPIdentityMapsterConfiguration"/>). Call this only if your own code maps the identity
		/// types through GlobalSettings, and not while other code is mapping.
		/// </summary>
		public static void RegisterXPIdentityMapsterConfiguration<TKey, TUser, TRole, TUserClaim, TUserRole, 
			TUserLogin, TUserToken, TRoleClaim, TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken, TXPORoleClaim>
			(this IServiceCollection services)
				where TKey : IEquatable<TKey>
				where TUser : IdentityUser<TKey>
				where TRole : IdentityRole<TKey>
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
				where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
		{
			Configure<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
				TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken, TXPORoleClaim>(TypeAdapterConfig.GlobalSettings, true);
		}

		/// <summary>
		/// Adds the identity mapping rules to <paramref name="config"/> (merged with any rules it already has for these types)
		/// </summary>
		public static TypeAdapterConfig ApplyXPIdentityMapsterConfiguration<TKey, TUser, TRole, TUserClaim, TUserRole,
			TUserLogin, TUserToken, TRoleClaim, TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken, TXPORoleClaim>
			(this TypeAdapterConfig config)
				where TKey : IEquatable<TKey>
				where TUser : IdentityUser<TKey>
				where TRole : IdentityRole<TKey>
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
				where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
		{
			if (config == null)
				throw new ArgumentNullException(nameof(config));
			Configure<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
				TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken, TXPORoleClaim>(config, false);
			return config;
		}

		private static void Configure<TKey, TUser, TRole, TUserClaim, TUserRole,
			TUserLogin, TUserToken, TRoleClaim, TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken, TXPORoleClaim>
			(TypeAdapterConfig config, bool replace)
				where TKey : IEquatable<TKey>
				where TUser : IdentityUser<TKey>
				where TRole : IdentityRole<TKey>
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
				where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
		{
			TypeAdapterSetter<TSource, TDestination> For<TSource, TDestination>()
				=> replace ? config.NewConfig<TSource, TDestination>() : config.ForType<TSource, TDestination>();

			// IdentityUser.LockoutEnd (DateTimeOffset?) is XpoDxUser.LockoutEndDateUtc (DateTime?, UTC): the names differ
			For<TUser, TXPOUser>()
					.Ignore(dest => dest.RolesList, dest => dest.ClaimsList, dest => dest.TokenList, dest => dest.LoginsList)
					.Map(nameof(IXPUser<TKey>.LockoutEndDateUtc),
						src => src.LockoutEnd.HasValue ? src.LockoutEnd.Value.UtcDateTime : (DateTime?)null);
			For<TXPOUser, TUser>()
					.Map(dest => dest.LockoutEnd,
						src => src.LockoutEndDateUtc.HasValue
							? new DateTimeOffset(DateTime.SpecifyKind(src.LockoutEndDateUtc.Value, DateTimeKind.Utc))
							: (DateTimeOffset?)null);

			For<TRole, TXPORole>().Ignore(dest => dest.ClaimsList);

			For<TXPORoleClaim, TRoleClaim>()
				.Ignore(dest => dest.Id)
                .AfterMapping((src, dest) => dest.RoleId = src.RoleId);

			// as the role claims: the XPO claim's key is a string, IdentityUserClaim.Id an int
			For<TXPOClaim, TUserClaim>()
				.Ignore(dest => dest.Id)
				.AfterMapping((src, dest) => dest.UserId = src.UserId);

            For<TUserClaim, TXPOClaim>()
			    .Ignore(dest => dest.UserId)
				.AfterMapping((src, dest) => dest.UserId = src.UserId);
            For<TUserToken, TXPOToken>()
				.Ignore(dest => dest.UserId)
				.AfterMapping((src, dest) => dest.UserId = src.UserId);
            For<TUserLogin, TXPOLogin>()
                .Ignore(dest => dest.UserId)
                .AfterMapping((src, dest) => dest.UserId = src.UserId);
        }
    }
#endif

}
