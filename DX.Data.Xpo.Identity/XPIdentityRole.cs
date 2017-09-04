using DevExpress.Xpo;
using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DX.Data.Xpo.Identity
{	
	public class XPIdentityRole : XPIdentityRole<string, XpoDxRole>
	{
		public XPIdentityRole(XpoDxRole source)
			  : base(source)
		{

		}
		public XPIdentityRole(XpoDxRole source, int loadingFlags)
			  : base(source, loadingFlags)
		{

		}
		public XPIdentityRole()
		{

		}
	}

	/// <summary>
	///     Represents a Role entity
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TUserRole"></typeparam>
	public abstract class XPIdentityRole<TKey, TXPORole> : XpoDtoBaseEntity<TKey, TXPORole>, IRole<TKey>, IDxRole<TKey>
		 where TKey : IEquatable<TKey>
		 where TXPORole : XPBaseObject, IDxRole<TKey>
	{
		public XPIdentityRole(TXPORole source, int loadingFlags)
			  : base(source, loadingFlags)
		{
			Users = new List<IDxUser<TKey>>();
		}

		public XPIdentityRole(TXPORole source)
			  : this(source, 0)
		{

		}
		public XPIdentityRole() :
			  this(null, 0)
		{

		}

		/// <summary>
		///     Navigation property for users in the role
		/// </summary>
		public virtual ICollection<IDxUser<TKey>> Users { get; protected set; }
		public virtual IList UsersList { get { return Users.ToList(); } }

		public override TKey Key { get { return Id; } }

		/// <summary>
		///     Role id
		/// </summary>
		public TKey Id { get; set; }

		/// <summary>
		///     Role name
		/// </summary>
		public string Name { get; set; }

		public override void Assign(object source, int loadingFlags)
		{
			var src = CastSource(source);
			if (src != null)
			{
				this.Id = src.Key;
				this.Name = src.Name;
			}
		}
	}
}
