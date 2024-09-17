using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;
using Microsoft.Extensions.DependencyInjection;
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
			// Mapster config
			services.RegisterXPIdentityMapsterConfiguration<TKey, TUser, TRole,
						TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
						TXPOUser, TXPORole, TXPOUserLogin, TXPOUserClaim, TXPOUserToken, TXPORoleClaim>();

			// Default IDatalayer for Identity
			services.AddSingleton<IDataLayer>(sp => {
				var db = sp.GetRequiredService<XpoDatabase>();
				return db.GetDataLayer(connectionName);
			});


			services.RegisterXPIdentityValidators<TKey, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken, TXPORole, TXPORoleClaim>();

			services.AddScoped<IQueryableUserStore<TKey, TUser, TUserRole, TUserToken>,
				XPMapsterUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
										TXPOUser, TXPORole, TXPOUserLogin, TXPOUserClaim, TXPOUserToken>>();

			services.AddScoped<IQueryableRoleStore<TKey, TRole>, XPMapsterRoleStore<TKey, TRole, TRoleClaim, TXPORole, TXPORoleClaim>>();


			services.AddScoped<IQueryableUserLoginStore<TKey, TUserLogin>, XPMapsterUserLoginStore<TKey, TUserLogin, TXPOUserLogin>>();
			services.AddScoped<IQueryableUserClaimStore<TKey, TUserClaim>, XPMapsterUserClaimStore<TKey, TUserClaim, TXPOUserClaim>>();
			services.AddScoped<IQueryableUserTokenStore<TKey, TUserToken>, XPMapsterUserTokenStore<TKey, TUserToken, TXPOUserToken>>();
			services.AddScoped<IQueryableRoleClaimStore<TKey, TRoleClaim>, XPMapsterRoleClaimStore<TKey, TRoleClaim, TXPORoleClaim>>();

			RegisterIdentityServices.AddStores<TKey>(builder.Services, connectionName,
				builder.UserType ?? typeof(TUser), typeof(TUserClaim), typeof(TUserLogin), typeof(TUserToken), typeof(TUserRole),
				 builder.RoleType ?? typeof(TRole), typeof(TRoleClaim),
				typeof(TXPOUser), typeof(TXPOUserLogin), typeof(TXPOUserClaim), typeof(TXPOUserToken),
				typeof(TXPORole), typeof(TXPORoleClaim));

			return builder;
		}
	}
	public static class XPIdentityMapsterConfig
	{
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
			TypeAdapterConfig<TUser, TXPOUser>.NewConfig()
					.Ignore(dest => dest.RolesList, dest => dest.ClaimsList, dest => dest.TokenList, dest => dest.LoginsList);

			TypeAdapterConfig<TRole, TXPORole>.NewConfig().Ignore(dest => dest.ClaimsList);

			TypeAdapterConfig<TXPOClaim, TUserClaim>.NewConfig().Ignore(dest => dest.Id);

			TypeAdapterConfig<TXPORoleClaim, TRoleClaim>.NewConfig().Ignore(dest => dest.Id);
		}
	}
#endif

}
