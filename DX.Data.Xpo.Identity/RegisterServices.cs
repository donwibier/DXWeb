﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace DX.Data.Xpo.Identity
{
#if (NETCOREAPP)
    using FluentValidation;
    using DevExpress.Xpo;
    using DevExpress.Xpo.Metadata.Helpers;
    using DX.Data.Xpo.Identity.Persistent;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    public static class RegisterIdentityServices
    {
        static class Resources
        {
            public const string NotIdentityUser = "This class is not of type IdentityUser";
            public const string NotIdentityRole = "This class is not of type IdentityRole";
        }

        public static IServiceCollection RegisterXPIdentityValidators<TKey, TXPOUser, TXPOUserClaim, TXPOUserLogin, 
                TXPOUserToken,TXPORole, TXPORoleClaim>(this IServiceCollection services)
            where TKey:IEquatable<TKey>
            where TXPOUser : XPBaseObject, IXPUser<TKey>
            where TXPOUserLogin : XPBaseObject, IXPUserLogin<TKey>
            where TXPOUserClaim : XPBaseObject, IXPUserClaim<TKey>
            where TXPOUserToken : XPBaseObject, IXPUserToken<TKey>
            where TXPORole : XPBaseObject, IXPRole<TKey>
            where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey>
        {
            services.AddScoped<IValidator<TXPOUser>, XPIdentityUserValidator<TKey, TXPOUser>>();
            services.AddScoped<IValidator<TXPOUserLogin>, XPIdentityUserLoginValidator<TKey, TXPOUserLogin>>();
            services.AddScoped<IValidator<TXPOUserClaim>, XPIdentityUserClaimValidator<TKey, TXPOUserClaim>>();
            services.AddScoped<IValidator<TXPOUserToken>, XPIdentityUserTokenValidator<TKey, TXPOUserToken>>();
            services.AddScoped<IValidator<TXPORole>, XPIdentityRoleValidator<TKey, TXPORole>>();
            services.AddScoped<IValidator<TXPORoleClaim>, XPIdentityRoleClaimValidator<TKey, TXPORoleClaim>>();

            return services;
        }



        public static void AddStores<TKey>(IServiceCollection services, string connectionName,
            Type userType, Type userClaimType, Type userLoginType, Type userTokenType, Type userRoleType, Type roleType, Type roleClaimType,
            Type xpoUserType, Type xpoUserLoginType, Type xpoUserClaimType, Type xpoUserTokenType,
            Type xpoRoleType, Type xpoRoleClaimType)
            where TKey : IEquatable<TKey>
            //where TUser : IdentityUser<TKey>, new()
            //where TXPOUser : XPBaseObject, IXPUser<TKey>
            //where TRole : IdentityRole<TKey>, new()
            //where TXPORole : XPBaseObject, IXPRole<TKey>
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

            var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<>));
            if (identityUserType == null)
            {
                throw new InvalidOperationException(Resources.NotIdentityUser);
            }

            var keyType = identityUserType.GenericTypeArguments[0];
            var identityRoleType = FindGenericBaseType(roleType, typeof(IdentityRole<>));
            if (identityRoleType == null)
            {
                throw new InvalidOperationException(Resources.NotIdentityRole);
            }
            
            Type userStoreType = typeof(XPUserStore<,,,,,,,,,,,,>).MakeGenericType(
                typeof(TKey), userType, roleType, userClaimType, userRoleType,
                userLoginType, userTokenType, roleClaimType,
                xpoUserType, xpoRoleType, xpoUserLoginType, xpoUserClaimType, xpoUserTokenType);

            Type roleStoreType = typeof(XPRoleStore<,,,,,>).MakeGenericType(
                typeof(TKey), roleType, userRoleType, roleClaimType, xpoRoleType, xpoRoleClaimType);

            //=======================================
            //Type defaultUserMapperType = typeof(XPUserMapper<,,>).MakeGenericType(keyType, userType, xpoUserType);
            //Type defaultRoleMapperType = typeof(XPRoleMapper<,,>).MakeGenericType(keyType, roleType, xpoRoleType);

            //XPUserMapper<identityUserType, >

            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType/*, xpoUserType*/),
                (sp) =>
                {
                    XpoDatabase db = default!;
                    if (string.IsNullOrEmpty(connectionName))
                    {
                        db = (sp.GetRequiredService(typeof(XpoDatabase)) as XpoDatabase)!;
                        if (db == null)
                            throw new NullReferenceException("XpoDatabase service could not return an instance for IUserStore<>");
                    }
                    else
                    {
                        IConfiguration cfg = sp.GetRequiredService<IConfiguration>();
                        db = sp.GetRequiredService<XpoDatabase>();
                        if (db == null || db.DataLayerName != connectionName)
                        {
                            db = new XpoDatabase((o) => {
                                o.ConnectionString = cfg.GetConnectionString(connectionName)!;
                                o.Name = connectionName;
                            });
                        }
                    }
                    var userDataStoreType = typeof(IQueryableUserStore<,,,>).MakeGenericType(keyType, userType, typeof(IdentityUserRole<TKey>), typeof(IdentityUserToken<TKey>));
                    var userStore = sp.GetRequiredService(userDataStoreType);

                    var roleDataStoreType = typeof(IQueryableRoleStore<,>).MakeGenericType(keyType, roleType);
                    var roleStore = sp.GetRequiredService(roleDataStoreType);

                    var userClaimStoreType = typeof(IQueryableUserClaimStore<,>).MakeGenericType(keyType, typeof(IdentityUserClaim<TKey>));
                    var userClaimStore = sp.GetRequiredService(userClaimStoreType);
                    var userLoginStoreType = typeof(IQueryableUserLoginStore<,>).MakeGenericType(keyType, typeof(IdentityUserLogin<TKey>));
                    var userLoginStore = sp.GetRequiredService(userLoginStoreType);
                    var userTokenStoreType = typeof(IQueryableUserTokenStore<,>).MakeGenericType(keyType, typeof(IdentityUserToken<TKey>));
                    var userTokenStore = sp.GetRequiredService(userTokenStoreType);

                    return Activator.CreateInstance(userStoreType, userStore, roleStore, userClaimStore, userLoginStore, userTokenStore, new IdentityErrorDescriber())!;

                });
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType/*, xpoRoleType*/),
                (sp) =>
                {
                    XpoDatabase db = default!;
                    if (string.IsNullOrEmpty(connectionName))
                    {
                        db = sp.GetRequiredService<XpoDatabase>();
                        if (db == null)
                            throw new NullReferenceException("XpoDatabase service could not return an instance for IUserStore<>");
                        //return Activator.CreateInstance(roleStoreType, db, roleMapper ?? Activator.CreateInstance(defaultRoleMapperType), roleValidator);
                    }
                    else
                    {
                        IConfiguration cfg = sp.GetRequiredService<IConfiguration>();
                        db = sp.GetRequiredService<XpoDatabase>();
                        if (db == null || db.DataLayerName != connectionName)
                        {
                            db = new XpoDatabase((o) => {
                                o.ConnectionString = cfg.GetConnectionString(connectionName)!;
                                o.Name = connectionName;
                            });
                        }
                    }

                    var roleDataStoreType = typeof(IQueryableRoleStore<,>).MakeGenericType(keyType, roleType);
                    var roleStore = sp.GetRequiredService(roleDataStoreType);
                    var roleClaimStoreType = typeof(IQueryableRoleClaimStore<,>).MakeGenericType(keyType, typeof(IdentityRoleClaim<TKey>));
                    var roleClaimStore = sp.GetRequiredService(roleClaimStoreType);

                    return Activator.CreateInstance(roleStoreType, roleStore, roleClaimStore, new IdentityErrorDescriber())!;
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
            return null!;
        }


    }
#endif
#if (crap)

    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using DX.Data.Xpo.Identity.Persistent;
    using Microsoft.Extensions.Configuration;
    using DevExpress.Xpo;

    public static class XpoCoreIdentityExtensions
    {
        static class Resources
        {
            public const string NotIdentityUser = "This class is not of type IdentityUser";
            public const string NotIdentityRole = "This class is not of type IdentityRole";
        }
        /// <summary>
        /// Default quick config with standard User and Role classes
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static IdentityBuilder AddXpoIdentityStores(this IdentityBuilder builder, string connectionName)
        {
            return AddXpoIdentityStores(
                builder, connectionName,
                new XPUserMapper<string, XPIdentityUser, XpoDxUser>(), 
                new XPRoleMapper<string, XPIdentityRole, XpoDxRole>(),
                new XPUserStoreValidator<string, XPIdentityUser, XpoDxUser>(),
                new XPRoleStoreValidator<string, XPIdentityRole, XpoDxRole>());
        }
        /// <summary>
        /// Semi-quick config with custom User classes
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TXPOUser"></typeparam>
        /// <param name="builder"></param>
        /// <param name="connectionName"></param>
        /// <param name="userMapper"></param>
        /// <param name="userValidator"></param>
        /// <returns></returns>
        public static IdentityBuilder AddXpoIdentityStores<TUser, TXPOUser>
                            (this IdentityBuilder builder, string connectionName,
                                XPUserMapper<string, TUser, TXPOUser> userMapper,
                                XPUserStoreValidator<string, TUser, TXPOUser> userValidator)
            where TUser : class, IXPUser<string>, new()
            where TXPOUser : XpoDxUser, IXPUser<string>
        {
            return AddXpoIdentityStores<string, TUser, TXPOUser, XPIdentityRole, XpoDxRole, XpoDxUserLogin, XpoDxUserClaim, XpoDxUserToken, XpoDxRoleClaim>(
                builder, connectionName,
                userMapper,
                new XPRoleMapper<string, XPIdentityRole, XpoDxRole>(),
                userValidator,
                new XPRoleStoreValidator<string, XPIdentityRole, XpoDxRole>());
        }

        /// <summary>
        /// Custom config for usage with custom User and Role classes + validators + mappers etc.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TXPOUser"></typeparam>
        /// <typeparam name="TRole"></typeparam>
        /// <typeparam name="TXPORole"></typeparam>
        /// <param name="builder"></param>
        /// <param name="connectionName"></param>
        /// <param name="userMapper"></param>
        /// <param name="roleMapper"></param>
        /// <param name="userValidator"></param>
        /// <param name="roleValidator"></param>
        /// <returns></returns>
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
        where TUser : class, IXPUser<TKey>, new()
        where TRole : class, IXPRole<TKey>, new()
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
                    if (string.IsNullOrEmpty(connectionName))
                    {
                        var db = sp.GetRequiredService(typeof(XpoDatabase)) as XpoDatabase;
                        if (db == null)
                            throw new NullReferenceException("XpoDatabase service could not return an instance for IUserStore<>");
                        return Activator.CreateInstance(userStoreType,
                            db,
                            userMapper ?? Activator.CreateInstance(defaultUserMapperType),
                            userValidator);
                    }
                    else
                    {
                        IConfiguration cfg = sp.GetRequiredService<IConfiguration>();
                        XpoDatabase db = sp.GetRequiredService<XpoDatabase>();
                        if (db == null || db.DataLayerName != connectionName)
                        {
                            db = new XpoDatabase((o)=> {
                                o.ConnectionString = cfg.GetConnectionString(connectionName);
                                o.Name = connectionName;
                            });                            
                        }

                        return Activator.CreateInstance(userStoreType,
                            db,
                            userMapper ?? Activator.CreateInstance(defaultUserMapperType),
                            userValidator);
                    }
                });
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType/*, xpoRoleType*/),
                (sp) =>
                {
                    if (string.IsNullOrEmpty(connectionName))
                    {
                        var db = sp.GetRequiredService(typeof(XpoDatabase)) as XpoDatabase;
                        if (db == null)
                            throw new NullReferenceException("XpoDatabase service could not return an instance for IUserStore<>");
                        return Activator.CreateInstance(roleStoreType, db, roleMapper ?? Activator.CreateInstance(defaultRoleMapperType), roleValidator);
                    }
                    else
                    {
                        IConfiguration cfg = sp.GetRequiredService<IConfiguration>();
                        XpoDatabase db = sp.GetRequiredService<XpoDatabase>();
                        if (db == null || db.DataLayerName != connectionName)
                        {
                            db = new XpoDatabase((o) => {
                                o.ConnectionString = cfg.GetConnectionString(connectionName);
                                o.Name = connectionName;
                            });
                        }
                        return Activator.CreateInstance(roleStoreType,
                            db,
                            roleMapper ?? Activator.CreateInstance(defaultRoleMapperType),
                            roleValidator);
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
