using AutoMapper;
using DevExpress.Xpo;
using DevExpress.XtraEditors.Filtering;
using DX.Data.Xpo.AutoMapper;
using DX.Data.Xpo.Identity.Persistent;
using FluentValidation;
using System.Reflection;
using System.Resources;

namespace DX.Data.Xpo.Identity.AutoMapper
{
#if (NETCOREAPP)
	using Microsoft.Extensions.Configuration;
	using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class RegisterServices
    {
        public static IdentityBuilder AddXpoAutoMapperIdentityStores(this IdentityBuilder builder, string connectionName)
        {
            return AddXpoAutoMapperIdentityStores<string,
                IdentityUser, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityUserRole<string>,
                IdentityRole, IdentityRoleClaim<string>,
                XpoDxUser, XpoDxUserClaim, XpoDxUserLogin, XpoDxUserToken, XpoDxRole, XpoDxRoleClaim>(builder, connectionName);

        }

		public static IdentityBuilder AddXpoAutoMapperIdentityStores<TUser, TXPOUser>(this IdentityBuilder builder, string connectionName)
            where TUser : IdentityUser<string>, new()
            where TXPOUser : XpoDxUser, IXPUser<string>
        {
            return AddXpoAutoMapperIdentityStores<string,
                        TUser, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityUserRole<string>,
                        IdentityRole, IdentityRoleClaim<string>,
                        TXPOUser, XpoDxUserClaim, XpoDxUserLogin, XpoDxUserToken, XpoDxRole, XpoDxRoleClaim>(builder, connectionName);
        }

        public static IdentityBuilder AddXpoAutoMapperIdentityStores<TUser, TXPOUser, TRole, TXPORole>
                            (this IdentityBuilder builder, string connectionName)
            where TUser : IdentityUser<string>, new()
            where TRole : IdentityRole<string>, new()
            where TXPOUser : XpoDxUser, IXPUser<string>
            where TXPORole : XpoDxRole, IXPRole<string>
        {
            return AddXpoAutoMapperIdentityStores<string, 
                TUser, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityUserRole<string>,
                TRole, IdentityRoleClaim<string>,
                TXPOUser, XpoDxUserClaim, XpoDxUserLogin, XpoDxUserToken, TXPORole, XpoDxRoleClaim>(builder, connectionName);
        }

        public static IdentityBuilder AddXpoAutoMapperIdentityStores<TKey, 
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
            if (!services.Any(sd => sd.ServiceType == typeof(IMapper)))
            {
                services.AddAutoMapper(cfg => cfg.AddProfile(new XPIdentityMapperProfile<TKey, TUser, TRole, 
                            TUserClaim, TUserRole, TUserLogin, TUserToken,  TRoleClaim, 
                            TXPOUser, TXPORole, TXPOUserLogin, TXPOUserClaim, TXPOUserToken, TXPORoleClaim>()));
            }
            // Default IDatalayer for Identity
            services.AddSingleton<IDataLayer>(sp => {
                var db = sp.GetRequiredService<XpoDatabase>();
                return db.GetDataLayer(connectionName);
            });


            services.RegisterXPIdentityValidators<TKey, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken, TXPORole, TXPORoleClaim>();

            services.AddScoped<IQueryableUserStore<TKey, TUser, TUserRole, TUserToken>, 
                XPAutoMapperUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
                                        TXPOUser, TXPORole, TXPOUserLogin, TXPOUserClaim, TXPOUserToken>>();

            services.AddScoped<IQueryableRoleStore<TKey, TRole>, XPAutoMapperRoleStore<TKey, TRole, TRoleClaim, TXPORole, TXPORoleClaim>>();


            services.AddScoped<IQueryableUserLoginStore<TKey, TUserLogin>, XPAutoMapperUserLoginStore<TKey, TUserLogin, TXPOUserLogin>>();
            services.AddScoped<IQueryableUserClaimStore<TKey, TUserClaim>, XPAutoMapperUserClaimStore<TKey, TUserClaim, TXPOUserClaim>>();
            services.AddScoped<IQueryableUserTokenStore<TKey, TUserToken>, XPAutoMapperUserTokenStore<TKey, TUserToken, TXPOUserToken>>();
            services.AddScoped<IQueryableRoleClaimStore<TKey, TRoleClaim>, XPAutoMapperRoleClaimStore<TKey, TRoleClaim, TXPORoleClaim>>();
           
            RegisterIdentityServices.AddStores<TKey>(builder.Services, connectionName,
                builder.UserType ?? typeof(TUser), typeof(TUserClaim) ,typeof(TUserLogin), typeof(TUserToken), typeof(TUserRole),
                 builder.RoleType ?? typeof(TRole), typeof(TRoleClaim),
                typeof(TXPOUser), typeof(TXPOUserLogin), typeof(TXPOUserClaim), typeof(TXPOUserToken),
                typeof(TXPORole), typeof(TXPORoleClaim));
            
            return builder;
        }
    }

    public class XPIdentityMapperProfile<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
                                    TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken, TXPORoleClaim> : Profile
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
        public XPIdentityMapperProfile()
        {
            CreateMap<TUser, TXPOUser>()
                    //.ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.NormalizedUserName))
                    .ForMember(dest => dest.RolesList, opt => opt.Ignore())
                    .ForMember(dest => dest.ClaimsList, opt => opt.Ignore())
                    .ForMember(dest => dest.TokenList, opt => opt.Ignore())
                    .ForMember(dest => dest.LoginsList, opt => opt.Ignore())
                    // IdentityUser.LockoutEnd (DateTimeOffset?) is XpoDxUser.LockoutEndDateUtc (DateTime?, UTC): the names differ
                    .ForMember(nameof(IXPUser<TKey>.LockoutEndDateUtc),
                        opt => opt.MapFrom(src => src.LockoutEnd.HasValue ? src.LockoutEnd.Value.UtcDateTime : (DateTime?)null))
				.ReverseMap()
                    .ForMember(dest => dest.LockoutEnd, opt => opt.MapFrom(src => src.LockoutEndDateUtc.HasValue
                        ? new DateTimeOffset(DateTime.SpecifyKind(src.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                        : (DateTimeOffset?)null))
			;

			CreateMap<TRole, TXPORole>()                    
                    .ForMember(dest => dest.ClaimsList, opt => opt.Ignore())					
				.ReverseMap();

            // the XPO login, claim and token have a get-only UserId (an alias of [User.Id]); the interface setter looks up the User
            CreateMap<TUserLogin, TXPOLogin>()
                .ForMember(nameof(IXPUserLogin<TKey>.UserId), opt => opt.Ignore())
                .AfterMap((src, dest) => dest.UserId = src.UserId)
                .ReverseMap();
            CreateMap<TUserClaim, TXPOClaim>()
                .ForMember(nameof(IXPUserClaim<TKey>.UserId), opt => opt.Ignore())
                .AfterMap((src, dest) => dest.UserId = src.UserId)
                .ReverseMap()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
            ;
            CreateMap<TUserToken, TXPOToken>()
                .ForMember(nameof(IXPUserToken<TKey>.UserId), opt => opt.Ignore())
                .AfterMap((src, dest) => dest.UserId = src.UserId)
                .ReverseMap();
            CreateMap<TRoleClaim, TXPORoleClaim>()
                .ReverseMap()
					.ForMember(dest => dest.Id, opt => opt.Ignore())
            ;
		}
	}
#endif
}
