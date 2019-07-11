using DevExpress.Xpo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DX.Utils.Data;
#if (NETSTANDARD2_0)

#else
using Microsoft.AspNet.Identity;
#endif
using DX.Data.Xpo.Identity.Persistent;

namespace DX.Data.Xpo.Identity
{
#if (NETSTANDARD2_0)
    public class XPIdentityRole : XPIdentityRole<string, XpoDxRoleClaim>
#else
    public class XPIdentityRole : XPIdentityRole<string>
#endif
    {
		public XPIdentityRole()
			: base()
		{

		}
    }
//#if (NETSTANDARD2_0)
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
#if (NETSTANDARD2_0)
    public abstract class XPIdentityRole<TKey, TXPORoleClaim> : IDataStoreModel<TKey>, IDxRole<TKey>
		 where TKey : IEquatable<TKey>		 
         where TXPORoleClaim : XPBaseObject, IDxRoleClaim<TKey>
#else
    public abstract class XPIdentityRole<TKey> : IDataStoreModel<TKey>, IDxRole<TKey>
		 where TKey : IEquatable<TKey>
		 
#endif
    {
		public XPIdentityRole()
		{

		}

		private List<IDxUser<TKey>> _UserList = new List<IDxUser<TKey>>();
		public virtual IList UsersList { get => _UserList; }

		//public TKey ID { get; set; }
		
		public TKey ID { get => Id; set => Id = value; }

		/// <summary>
		///     Role id
		/// </summary>
		public TKey Id { get; set; }

		/// <summary>
		///     Role name
		/// </summary>
		public string Name { get; set; }

#if (NETSTANDARD2_0)
        public string NormalizedName { get; set; }

        public virtual Type XPORoleClaimType
        {
            get { return typeof(TXPORoleClaim); }
        }
#endif
	}
}
