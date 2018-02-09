using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;

namespace DX.Data.Xpo.Identity
{
	 /// <summary>
	 ///     EntityType that represents one specific user claim
	 /// </summary>
	 public class XPIdentityUserClaim : XPIdentityUserClaim<string, XpoDxUserClaim>
	 {
		  public XPIdentityUserClaim(XpoDxUserClaim source)
				: base(source)
		  {
				
		  }
		  public XPIdentityUserClaim(XpoDxUserClaim source, int loadingFlags)
				: base(source, loadingFlags)
		  {
				
		  }
		  public XPIdentityUserClaim()
		  {
				
		  }
	 }

	/// <summary>
	///     EntityType that represents one specific user claim
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	public abstract class XPIdentityUserClaim<TKey, TXPOClaim> : XpoDtoBaseEntity<TKey, TXPOClaim>, IDxUserClaim<TKey>
		 where TKey : IEquatable<TKey>
		 where TXPOClaim : XPBaseObject, IDxUserClaim<TKey>
	{
		public XPIdentityUserClaim(TXPOClaim source)
			  : base(source)
		{

		}
		public XPIdentityUserClaim(TXPOClaim source, int loadingFlags)
			  : base(source, loadingFlags)
		{

		}
		public XPIdentityUserClaim()
		{

		}
		public override TKey Key { get { return Id; } }
		/// <summary>
		///     Primary key
		/// </summary>
		public virtual TKey Id { get; set; }

		/// <summary>
		///     User Id for the user who owns this login
		/// </summary>
		public virtual TKey UserId { get; set; }

		/// <summary>
		///     Claim type
		/// </summary>
		public virtual string ClaimType { get; set; }

		/// <summary>
		///     Claim value
		/// </summary>
		public virtual string ClaimValue { get; set; }

		public override void Assign(object source, int loadingFlags)
		{
			var src = CastSource(source);
			if (src != null)
			{
				this.Id = src.Key;
				this.UserId = src.UserId;
				this.ClaimType = src.ClaimType;
				this.ClaimValue = src.ClaimValue;
			}
		}

        public Claim ToClaim()
        {
            return new Claim(this.ClaimType, this.ClaimValue);
        }

        public void InitializeFromClaim(Claim other)
        {
            this.ClaimType = other.Type;
            this.ClaimValue = other.Value;
        }
    }

}
