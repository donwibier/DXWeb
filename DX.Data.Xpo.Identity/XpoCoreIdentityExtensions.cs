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

    public static class XpoCoreIdentityExtensions
    {
        static class Resources
        {
            public const string NotIdentityUser = "This class is not of type IdentityUser";
            public const string NotIdentityRole = "This class is not of type IdentityRole";
        }
        //TODO: Add some overloads in case you want to go nuts on custom XPO classes
        public static IdentityBuilder AddXpoIdentityStores<TXPOUser, TXPORole>(this IdentityBuilder builder, string connectionName, string connectionString)
            where TXPOUser : XpoDxUser, IDxUser<string>, IUser<string>
            where TXPORole : XpoDxRole, IDxRole<string>, IRole<string>
        {
            AddStores(builder.Services, connectionName, connectionString, builder.UserType, builder.RoleType,
                typeof(TXPOUser), typeof(XpoDxUserLogin), typeof(XpoDxUserClaim), typeof(XpoDxUserToken),
                typeof(TXPORole)/*, typeof(XpoDxRoleClaim)*/);
            return builder;
        }

        private static void AddStores(IServiceCollection services, string connectionName, string connectionString, 
            Type userType, Type roleType,
            Type xpoUserType, Type xpoUserLoginType, Type xpoUserClaimType, Type xpoUserTokenType,
            Type xpoRoleType/*, Type xpoRoleClaimType*/)
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
            //if (xpoRoleClaimType== null)
            //    throw new ArgumentNullException("xpoRoleClaimType");

            var identityUserType = FindGenericBaseType(userType, typeof(XPIdentityUser<,,,,>));
            if (identityUserType == null)
            {
                throw new InvalidOperationException(Resources.NotIdentityUser);
            }

            var keyType = identityUserType.GenericTypeArguments[0];

            //var userXpoLoginType = identityUserType.GenericTypeArguments[2];
            //var userXpoClaimType = identityUserType.GenericTypeArguments[3];
            //var userXpoTokenType = identityUserType.GenericTypeArguments[4];

            var identityRoleType = FindGenericBaseType(roleType, typeof(XPIdentityRole<,,>));
            if (identityRoleType == null)
            {
                throw new InvalidOperationException(Resources.NotIdentityRole);
            }
            var xpoRoleClaimType = identityRoleType.GenericTypeArguments[2];

            Type userStoreType = typeof(XPUserStore<,,,,,,>).MakeGenericType(
                keyType, userType, xpoUserType, xpoRoleType, xpoUserLoginType, xpoUserClaimType, xpoUserTokenType);
            Type roleStoreType = typeof(XPRoleStore<,,,>).MakeGenericType(
                keyType, roleType, xpoRoleType, xpoRoleClaimType);
            
            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType/*, xpoUserType*/),
                (sp) => Activator.CreateInstance(userStoreType, connectionString, connectionName));
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType/*, xpoRoleType*/),
                (sp) => Activator.CreateInstance(roleStoreType, connectionString, connectionName));

            //services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType, xpoUserType), userStoreType);
            //services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType, xpoRoleType), roleStoreType);
        }



        //private static void AddStores(IServiceCollection services, string connectionName, string connectionString, Type userType, Type xpoUserType, Type roleType, Type xpoRoleType)
        //{
        //    // no roles is not supported
        //    if (userType == null)
        //        throw new ArgumentNullException("userType");

        //    if (xpoUserType == null)
        //        throw new ArgumentNullException("xpoUserType");

        //    if (roleType == null)
        //        throw new ArgumentNullException("roleType");

        //    if (roleType == null)
        //        throw new ArgumentNullException("xpoRoleType");

        //    var identityUserType = FindGenericBaseType(userType, typeof(XPIdentityUser<,,,,>));
        //    if (identityUserType == null)
        //    {
        //        throw new InvalidOperationException(Resources.NotIdentityUser);
        //    }

        //    var keyType = identityUserType.GenericTypeArguments[0];

        //    var userXpoLoginType = identityUserType.GenericTypeArguments[2];
        //    var userXpoClaimType = identityUserType.GenericTypeArguments[3];
        //    var userXpoTokenType = identityUserType.GenericTypeArguments[4];

        //    var identityRoleType = FindGenericBaseType(roleType, typeof(XPIdentityRole<,,>));
        //    if (identityRoleType == null)
        //    {
        //        throw new InvalidOperationException(Resources.NotIdentityRole);
        //    }
        //    var roleXpoClaimType = identityRoleType.GenericTypeArguments[2];
        //    Type userStoreType = typeof(XPUserStore<,,,,,,>).MakeGenericType(
        //        keyType, userType, xpoUserType, xpoRoleType, userXpoLoginType, userXpoClaimType, userXpoTokenType);
        //    Type roleStoreType = typeof(XPRoleStore<,,,>).MakeGenericType(
        //        keyType, roleType, xpoRoleType, roleXpoClaimType);

        //    //var userStore = Activator.CreateInstance(typeof(IUserStore<>).MakeGenericType(userType, xpoUserType), connectionName);
        //    //var roleStore = Activator.CreateInstance(typeof(IUser))
        //    //services.AddScoped((sp) => Activator.CreateInstance(typeof(IUserStore<>).MakeGenericType(userType, xpoUserType), connectionName));

        //    services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType, xpoUserType),
        //        (sp) => Activator.CreateInstance(userStoreType, connectionString, connectionName));
        //    services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType, xpoRoleType), 
        //        (sp)=> Activator.CreateInstance(roleStoreType, connectionString, connectionName));

        //    //services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType, xpoUserType), userStoreType);
        //    //services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType, xpoRoleType), roleStoreType);
        //}

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
