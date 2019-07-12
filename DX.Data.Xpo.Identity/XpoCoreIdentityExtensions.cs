using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace DX.Data.Xpo.Identity
{
#if (NETSTANDARD2_0)
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;
	using DX.Data.Xpo.Identity.Persistent;
	using Microsoft.Extensions.Configuration;
    using DevExpress.Xpo;

    public class XpoIdentityStoreOptions<TKey, TXPOUser, TXPOUserLogin, TXPOUserClaim, TXPOUserToken, TXPORole, TXPORoleClaim>
			where TKey : IEquatable<TKey>
			where TXPOUser : XpoDxUser, IXPUser<TKey>, IUser<TKey>
			where TXPOUserLogin : XpoDxUserLogin, IXPUserLogin<TKey>
			where TXPOUserClaim : XpoDxUserClaim, IXPUserClaim<TKey>
			where TXPOUserToken : XpoDxUserToken, IXPUserToken<TKey>
			where TXPORole : XpoDxRole, IXPRole<TKey>, IRole<TKey>
			where TXPORoleClaim : XpoDxRoleClaim, IXPRoleClaim<TKey>
	{
		//XPDataMapper<TKey, >
	}



	//public class XpoIdentityUserMapper<TKey>
	//	where TKey: IEquatable<TKey>
	//{
	//	public Action<TKey, TModel, TXPOModel> ConfigureModel<TModel, TXPOModel> 			{ get; }
	//		//where TModel: IUser<TKey>, IDxUser<TKey>
	//		//where TXPOModel: XpoDxUser, IDxUser<TKey>

	//}

	//public class XpoIdentityStoreOptions<TKey>
	//	where TKey: IEquatable<TKey>
	//{
	//	public XpoIdentityStoreOptions()
	//	{
	//		UserMapper = new XpoIdentityUserMapper<TKey>();
	//		//UserValidator = new XpoIdentityUserValidator();

	//		//RoleMapper = new XpoIdentityRoleMapper();
	//		//RoleValidator = new XpoIdentityRoleValidator();
	//	}
	//	public XpoIdentityUserMapper<TKey> UserMapper { get; }
	//	//public XpoIdentityUserValidator UserValidator { get; }

	//	//public XpoIdentityRoleMapper RoleMapper { get; }
	//	//public XpoIdentityRoleValidator RoleValidator { get; }

	//}

	//public class XpoIdentityStoreOptions<TKey, TXPOUser, TXPOUserLogin, TXPOUserClaim, TXPOUserToken, TXPORole, TXPORoleClaim>
	//	where TKey : IEquatable<TKey>
	//		where TXPOUser : XpoDxUser, IDxUser<TKey>, IUser<TKey>
	//		where TXPOUserLogin : XpoDxUserLogin, IDxUserLogin<TKey>
	//		where TXPOUserClaim : XpoDxUserClaim, IDxUserClaim<TKey>
	//		where TXPOUserToken : XpoDxUserToken, IDxUserToken<TKey>
	//		where TXPORole : XpoDxRole, IDxRole<TKey>, IRole<TKey>
	//		where TXPORoleClaim : XpoDxRoleClaim, IDxRoleClaim<TKey>
	//{

	//}




	public static class XpoCoreIdentityExtensions
    {
        static class Resources
        {
            public const string NotIdentityUser = "This class is not of type IdentityUser";
            public const string NotIdentityRole = "This class is not of type IdentityRole";
		}
		//TODO: Add some overloads in case you want to go nuts on custom XPO classes
		//public static IdentityBuilder AddXpoIdentityStores<TXPOUser>(this IdentityBuilder builder)
		//    where TXPOUser : XpoDxUser, IDxUser<string>, IUser<string>
		//{
		//    return AddXpoIdentityStores<string, TXPOUser, XpoDxUserLogin, XpoDxUserClaim, XpoDxUserToken, XpoDxRole, XpoDxRoleClaim>(builder, String.Empty);
		//}
		//public static IdentityBuilder AddXpoIdentityStores<TXPOUser>(this IdentityBuilder builder, string connectionName, string connectionString)
		//    where TXPOUser : XpoDxUser, IDxUser<string>, IUser<string>
		//{
		//    return AddXpoIdentityStores<string, TXPOUser, XpoDxUserLogin, XpoDxUserClaim, XpoDxUserToken, XpoDxRole, XpoDxRoleClaim>(builder, connectionName);
		//}

		//public static IdentityBuilder AddXpoIdentityStores<TXPOUser, TXPORole>(this IdentityBuilder builder)
		//    where TXPOUser : XpoDxUser, IDxUser<string>, IUser<string>
		//    where TXPORole : XpoDxRole, IDxRole<string>, IRole<string>
		//{
		//    return AddXpoIdentityStores<string, TXPOUser, XpoDxUserLogin, XpoDxUserClaim, XpoDxUserToken, TXPORole, XpoDxRoleClaim>(builder, String.Empty);
		//}

		//				.AddXpoIdentityUserMapper<ApplicationUser, XpoApplicationUser>(new ApplicationUserMapper())
		//	.AddXpoIdentityRoleMapper<ApplicationRole, XpoApplicationRole>(new ApplicationRoleMapper())

		//public static IdentityBuilder AddXpoIdentityRoleMapper<TRole, TXPORole>(this IdentityBuilder builder, XPRoleMapper<string, TRole, TXPORole> mapper)
		//	where TRole : class, IXPRole<string>, new()
		//	where TXPORole : XPBaseObject, IXPRole<string>, IRole<string>
		//{
		//	builder.Services.TryAddSingleton<XPRoleMapper<string, TRole, TXPORole>>(mapper);
		//	return builder;
		//}

		//public static IdentityBuilder AddXpoIdentityRoleMapper<TKey, TRole, TXPORole, TXPORoleClaim>(this IdentityBuilder builder, XPDataMapper<TKey, TRole, TXPORole> mapper)
		//	where TKey : IEquatable<TKey>
		//	where TRole : IRole<TKey>
		//	where TXPORole : XPBaseObject, IXPRole<TKey>
		//	where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
		//{
		//	builder.Services.TryAddSingleton<XPDataMapper<TKey, TRole, TXPORole>>(mapper);
		//	return builder;
		//}

		//public static IdentityBuilder AddXpoIdentityUserMapper<TUser, TXPOUser>(this IdentityBuilder builder, XPUserMapper<string, TUser, TXPOUser> mapper)
		//	where TUser : class, IXPUser<string>, new()
		//	where TXPOUser : XPBaseObject, IXPUser<string>
		//{
		//	builder.Services.TryAddSingleton<XPUserMapper<string, TUser, TXPOUser>>(mapper);
		//	return builder;
		//}

		//public static IdentityBuilder AddXpoIdentityUserMapper<TKey, TUser, TXPOUser>(this IdentityBuilder builder, XPUserMapper<TKey, TUser, TXPOUser> mapper)
		//	where TKey : IEquatable<TKey>
		//	where TUser : class, IXPUser<TKey>, new()
		//	where TXPOUser : XPBaseObject, IXPUser<TKey>
		//{
		//	builder.Services.TryAddSingleton<XPDataMapper<TKey, TUser, TXPOUser>>(mapper);
		//	return builder;
		//}

		//public static IdentityBuilder AddXpoIdentityStores<TUser, TXPOUser, TRole, TXPORole>(this IdentityBuilder builder)
		//	where TUser : class, IXPUser<string>, new()
		//	where TRole : class, IXPRole<string>, new()
		//	where TXPOUser : XPBaseObject, IXPUser<string>
		//	where TXPORole : XPBaseObject, IXPRole<string>
		//{
		//	return AddXpoIdentityStores<string, TUser, TRole, TXPOUser, XpoDxUserLogin, XpoDxUserClaim, XpoDxUserToken, TXPORole, XpoDxRoleClaim>(builder);
		//}

		//public static IdentityBuilder AddXpoIdentityStores<TUser, TXPOUser, TRole, TXPORole>(this IdentityBuilder builder, 
		//		XPUserMapper<string, TUser, TXPOUser> userMapper,
		//		XPRoleMapper<string, TRole, TXPORole> roleMapper)
		//	where TUser : class, IXPUser<string>, new()
		//	where TRole : class, IXPRole<string>, new()
		//	where TXPOUser : XPBaseObject, IXPUser<string>
		//          where TXPORole : XPBaseObject, IXPRole<string>
		//      {
		//          return AddXpoIdentityStores<string, TUser, TRole, TXPOUser, XpoDxUserLogin, XpoDxUserClaim, XpoDxUserToken, TXPORole, XpoDxRoleClaim>(builder, "",
		//				userMapper, roleMapper);
		//      }

		public static IdentityBuilder AddXpoIdentityStores<TUser, TXPOUser, TRole, TXPORole>
							(this IdentityBuilder builder, string connectionName,
								XPUserMapper<string, TUser, TXPOUser> userMapper,
								XPRoleMapper<string, TRole, TXPORole> roleMapper,
								XPUserStoreValidator<string, TUser, TXPOUser> userValidator,
								XPRoleStoreValidator<string, TRole, TXPORole> roleValidator)			
			where TUser : class, IXPUser<string>, new()
			where TRole : class, IXPRole<string>, new()
			where TXPOUser : XpoDxUser, IXPUser<string>
			where TXPORole : XpoDxRole, IXPRole<string>
		{
			return AddXpoIdentityStores<string, TUser, TXPOUser, TRole, TXPORole, XpoDxUserLogin, XpoDxUserClaim, XpoDxUserToken, XpoDxRoleClaim>(builder, connectionName,
				userMapper, roleMapper, userValidator, roleValidator);
		}

			public static IdentityBuilder AddXpoIdentityStores<TKey, TUser, TXPOUser, TRole, TXPORole, TXPOUserLogin, TXPOUserClaim, TXPOUserToken, TXPORoleClaim>
										(this IdentityBuilder builder, string connectionName, 
											XPUserMapper<TKey, TUser, TXPOUser> userMapper, 
											XPRoleMapper<TKey, TRole, TXPORole> roleMapper,
											XPUserStoreValidator<TKey, TUser, TXPOUser> userValidator,
											XPRoleStoreValidator<TKey, TRole, TXPORole> roleValidator)
			where TKey : IEquatable<TKey>
			where TUser: class, IXPUser<TKey>, new()
			where TRole: class, IXPRole<TKey>, new()
            where TXPOUser : XPBaseObject, IXPUser<TKey>
            where TXPOUserLogin : XPBaseObject, IXPUserLogin<TKey>
            where TXPOUserClaim : XPBaseObject, IXPUserClaim<TKey>
            where TXPOUserToken : XPBaseObject, IXPUserToken<TKey>
            where TXPORole : XPBaseObject, IXPRole<TKey>
            where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
        {

			AddStores(builder.Services, connectionName, 
				userMapper, roleMapper, userValidator, roleValidator,
				builder.UserType, builder.RoleType,
                typeof(TXPOUser), typeof(TXPOUserLogin), typeof(TXPOUserClaim), typeof(TXPOUserToken),
                typeof(TXPORole), typeof(TXPORoleClaim));
            return builder;
        }

        private static void AddStores<TKey, TUser, TXPOUser, TRole, TXPORole>(IServiceCollection services, string connectionName,
			XPDataMapper<TKey, TUser, TXPOUser> userMapper, XPDataMapper<TKey, TRole, TXPORole> roleMapper,
			XPUserStoreValidator<TKey, TUser, TXPOUser> userValidator,
			XPRoleStoreValidator<TKey, TRole, TXPORole> roleValidator,
			Type userType, Type roleType,
			Type xpoUserType, Type xpoUserLoginType, Type xpoUserClaimType, Type xpoUserTokenType,
			Type xpoRoleType, Type xpoRoleClaimType)
			where TKey : IEquatable<TKey>
			where TUser : class, IXPUser<TKey>, new()
			where TXPOUser : XPBaseObject, IXPUser<TKey>
			where TRole : class, IXPRole<TKey>, new()
			where TXPORole : XPBaseObject, IXPRole<TKey>
		{
			// no roles is not supported
			if (userType == null)
                throw new ArgumentNullException("userType");

            if (roleType == null)
                throw new ArgumentNullException("roleType");

            if (xpoUserType == null)
                throw new ArgumentNullException("xpoUserType");
            if (xpoUserLoginType == null)
                throw new ArgumentNullException("xpoUserLoginType");
            if (xpoUserClaimType == null)
                throw new ArgumentNullException("xpoUserClaimType");
            if (xpoUserTokenType == null)
                throw new ArgumentNullException("xpoUserTokenType");

			var identityUserType = FindGenericBaseType(userType, typeof(XPIdentityUser<,,,>));
            if (identityUserType == null)
            {
                throw new InvalidOperationException(Resources.NotIdentityUser);
            }

            var keyType = identityUserType.GenericTypeArguments[0];
            var identityRoleType = FindGenericBaseType(roleType, typeof(XPIdentityRole<>));
            if (identityRoleType == null)
            {
                throw new InvalidOperationException(Resources.NotIdentityRole);
            }
            var xpoRoleClaimTpe = xpoRoleClaimType ?? identityRoleType.GenericTypeArguments[2];

            Type userStoreType = typeof(XPUserStore<,,,,,,>).MakeGenericType(
                keyType, userType, xpoUserType, xpoRoleType, xpoUserLoginType, xpoUserClaimType, xpoUserTokenType);
            Type roleStoreType = typeof(XPRoleStore<,,,>).MakeGenericType(
                keyType, roleType, xpoRoleType, xpoRoleClaimTpe);

			Type defaultUserMapperType = typeof(XPUserMapper<,,>).MakeGenericType(keyType, userType, xpoUserType);
			Type defaultRoleMapperType = typeof(XPRoleMapper<,,>).MakeGenericType(keyType, roleType, xpoRoleType);

			//XPUserMapper<identityUserType, >

			services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType/*, xpoUserType*/),
                (sp) =>
                {
                    if (String.IsNullOrEmpty(connectionName))
                    {
                        var db = sp.GetRequiredService(typeof(XpoDatabase)) as XpoDatabase;
                        if (db == null)
                            throw new NullReferenceException("XpoDatabase service could not return an instance for IUserStore<>");
						return Activator.CreateInstance(userStoreType, db, userMapper ?? Activator.CreateInstance(defaultUserMapperType), userValidator);
                    }
                    else
                    {
						IConfiguration cfg = sp.GetRequiredService<IConfiguration>();
                        return Activator.CreateInstance(userStoreType, connectionName, userMapper ?? Activator.CreateInstance(defaultUserMapperType), userValidator);
                    }
                });
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType/*, xpoRoleType*/),
                (sp) =>
                {
                    if ( String.IsNullOrEmpty(connectionName))
                    {
                        var db = sp.GetRequiredService(typeof(XpoDatabase)) as XpoDatabase;
                        if (db == null)
                            throw new NullReferenceException("XpoDatabase service could not return an instance for IUserStore<>");
                        return Activator.CreateInstance(roleStoreType, db, roleMapper ?? Activator.CreateInstance(defaultRoleMapperType), roleValidator);
                    }
                    else
                    {
                        return Activator.CreateInstance(roleStoreType, connectionName, roleMapper ?? Activator.CreateInstance(defaultRoleMapperType), roleValidator);
                    }
                });
        }

        private static TypeInfo FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            var type = currentType;
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType != null && genericType == genericBaseType)
                {
                    return typeInfo;
                }
                type = type.BaseType;
            }
            return null;
        }
    }
#endif
}
