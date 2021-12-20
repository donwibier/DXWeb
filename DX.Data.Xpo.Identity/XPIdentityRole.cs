using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;
using DX.Utils.Data;
using System;
using System.Collections;
using System.Collections.Generic;
#if (NETSTANDARD2_1 || NETCOREAPP)

#else
using Microsoft.AspNet.Identity;
#endif
using System.Linq;

namespace DX.Data.Xpo.Identity
{
	//#if (NETSTANDARD2_1 || NETCOREAPP)
	//    public class XPIdentityRole : XPIdentityRole<string, XpoDxRole, XpoDxRoleClaim, XpoDxUser, XpoDxUserClaim, XpoDxUserLogin, XpoDxUserToken>
	//#else
	//#endif
	public class XPIdentityRole : XPIdentityRole<string>
	{
		public XPIdentityRole()
			: base()
		{

		}
	}
	//#if (NETSTANDARD2_1 || NETCOREAPP)
	//    public class XPIdentityRole<TKey> : XPIdentityRole<string, XpoDxRoleClaim>
	//		where TKey : IEquatable<TKey>
	//#else
	//    public class XPIdentityRole<TKey> : XPIdentityRole<string>
	//         where TKey : IEquatable<TKey>	
	//#endif
	//	{
	//		public XPIdentityRole()
	//			:base()
	//		{

	//		}
	//	}
	/// <summary>
	///     Represents a Role entity
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TUserRole"></typeparam>
#if (NETSTANDARD2_1||NETCOREAPP)
	//public abstract class XPIdentityRole<TKey, TXPORole, TXPORoleClaim, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken> : IDataStoreModel<TKey>, IRole<TKey>
	public abstract class XPIdentityRole<TKey> : IXPRole<TKey>
		where TKey : IEquatable<TKey>
		//where TXPORole : XPBaseObject, IXPRole<TKey, TXPORole, TXPORoleClaim, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken>
		//where TXPORoleClaim : XPBaseObject, IXPRoleClaim<TKey, TXPORole, TXPORoleClaim, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken>
		//where TXPOUser : XPBaseObject, IXPUser<TKey, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken, TXPORole, TXPORoleClaim>
		//where TXPOUserClaim : XPBaseObject, IXPUserClaim<TKey, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken, TXPORole, TXPORoleClaim>
		//where TXPOUserLogin : XPBaseObject, IXPUserLogin<TKey, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken, TXPORole, TXPORoleClaim>
		//where TXPOUserToken : XPBaseObject, IXPUserToken<TKey, TXPOUser, TXPOUserClaim, TXPOUserLogin, TXPOUserToken, TXPORole, TXPORoleClaim>
	{
		private IList _UserList;
#else
    public abstract class XPIdentityRole<TKey> : IXPRole<TKey>
		 where TKey : IEquatable<TKey>
	{
		private List<IUser<TKey>> _UserList = new List<IUser<TKey>>();		 
#endif

		public XPIdentityRole()
		{

		}

		public virtual IList UsersList { get => _UserList; }

		//public TKey ID { get; set; }

		//public TKey ID { get => Id; set => Id = value; }

		/// <summary>
		///     Role id
		/// </summary>
		public TKey Id { get; set; }

		/// <summary>
		///     Role name
		/// </summary>
		public string Name { get; set; }

#if (NETSTANDARD2_1 || NETCOREAPP)
		public string NormalizedName { get; set; }

		//public virtual Type XPORoleClaimType
		//{
		//    get { return typeof(TXPORoleClaim); }
		//}
#endif
	}
}
