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
    public class XPIdentityRole : XPIdentityRole<string, XpoDxRole, XpoDxRoleClaim>
#else
    public class XPIdentityRole : XPIdentityRole<string, XpoDxRole>
#endif
    {
		//      public XPIdentityRole(XpoDxRole source)
		//	  : base(source)
		//{

		//}
		//public XPIdentityRole(XpoDxRole source, int loadingFlags)
		//	  : base(source, loadingFlags)
		//{

		//}
		public XPIdentityRole()
			: base()
		{

		}
    }
#if (NETSTANDARD2_0)
    public class XPIdentityRole<TXPORole> : XPIdentityRole<string, TXPORole, XpoDxRoleClaim>
         where TXPORole : XPBaseObject, IDxRole<string>
#else
    public class XPIdentityRole<TXPORole> : XPIdentityRole<string, TXPORole>
         where TXPORole : XPBaseObject, IDxRole<string>
#endif
    {
		public XPIdentityRole()
			:base()
		{

		}
		//public XPIdentityRole(TXPORole source, int loadingFlags) : base(source, loadingFlags)
		//{

		//}

		//public XPIdentityRole(TXPORole source) : base(source)
		//{

		//}

		//public XPIdentityRole()
		//{

		//}
	}
        /// <summary>
        ///     Represents a Role entity
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TUserRole"></typeparam>
#if (NETSTANDARD2_0)
    public abstract class XPIdentityRole<TKey, TXPORole, TXPORoleClaim> 
        : IDataStoreModel<TKey>, IDxRole<TKey>
		 where TKey : IEquatable<TKey>
		 where TXPORole : XPBaseObject, IDxRole<TKey>
         where TXPORoleClaim : XPBaseObject, IDxRoleClaim<TKey>
#else
    public abstract class XPIdentityRole<TKey, TXPORole> 
        : IDataStoreModel<TKey>, IDxRole<TKey>
		 where TKey : IEquatable<TKey>
		 where TXPORole : XPBaseObject, IDxRole<TKey>        
#endif
    {
        //public XPIdentityRole(TXPORole source, int loadingFlags)
		//	  : base(source, loadingFlags)
		//{
		//	Users = new List<IDxUser<TKey>>();
		//}

		//public XPIdentityRole(TXPORole source)
		//	  : this(source, 0)
		//{

		//}
		//public XPIdentityRole() :
		//	  this(null, 0)
		//{

		//}
		public XPIdentityRole()
		{

		}
        /// <summary>
        ///     Navigation property for users in the role
        /// </summary>
        //public virtual ICollection<IDxUser<TKey>> Users { get; protected set; }

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
//        public override void Assign(object source, int loadingFlags)
//		{
//			var src = CastSource(source);
//			if (src != null)
//			{
//				this.Id = src.Key;
//				this.Name = src.Name;
//#if (NETSTANDARD2_0)
//                this.NormalizedName = src.NormalizedName;
//#endif
//			}
//		}
	}
}
