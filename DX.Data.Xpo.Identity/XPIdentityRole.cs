using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;
using DX.Utils.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace DX.Data.Xpo.Identity
{
	public class XPIdentityRole : XPIdentityRole<string>
	{
		public XPIdentityRole()
			: base()
		{

		}
	}
	/// <summary>
	///     Represents a Role entity
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TUserRole"></typeparam>
#if (NETCOREAPP)	
	public abstract class XPIdentityRole<TKey> : IXPRole<TKey>
		where TKey : IEquatable<TKey>		
	{
		private IList _UserList = default;
#else
    public abstract class XPIdentityRole<TKey> : IXPRole<TKey>
		 where TKey : IEquatable<TKey>
	{
		private List<Microsoft.AspNet.Identity.IUser<TKey>> _UserList = new List<Microsoft.AspNet.Identity.IUser<TKey>>();		 
#endif

		public XPIdentityRole()
		{

		}		
		public virtual IList UsersList { get => _UserList; }

		/// <summary>
		///     Role id
		/// </summary>
		public TKey Id { get; set; }

		/// <summary>
		///     Role name
		/// </summary>
		public string Name { get; set; }

#if (NETCOREAPP)
		public string NormalizedName { get; set; }		
#endif
	}
}
