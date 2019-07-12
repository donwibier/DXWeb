using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;
using DX.Utils.Data;

namespace DX.Data.Xpo.Identity
{
	 /// <summary>
	 ///     Entity type for a user's login (i.e. facebook, google)
	 /// </summary>
	 public class XPIdentityUserLogin : XPIdentityUserLogin<string>
	 {
		  //public XPIdentityUserLogin(XpoDxUserLogin source)
				//: base(source)
		  //{
				
		  //}
		  //public XPIdentityUserLogin(XpoDxUserLogin source, int loadingFlags)
				//: base(source, loadingFlags)
		  //{
				
		  //}
		  public XPIdentityUserLogin()
			: base()
		  {
				
		  }
	 }

	 /// <summary>
	 ///     Entity type for a user's login (i.e. facebook, google)
	 /// </summary>
	 /// <typeparam name="TKey"></typeparam>
	 public abstract class XPIdentityUserLogin<TKey> : IDataStoreModel<TKey>//, IUserLogin<TKey>
		  where TKey : IEquatable<TKey>
		  //where TXPOLogin : XPBaseObject, IXPUserLogin<TKey>
	 {
		  //public XPIdentityUserLogin(TXPOLogin source)
				//: base(source)
		  //{
				
		  //}
		  //public XPIdentityUserLogin(TXPOLogin source, int loadingFlags)
				//: base(source, loadingFlags)
		  //{
				
		  //}
		  public XPIdentityUserLogin()
			:base()
		  {
				
		  }

		public virtual TKey ID { get => Id; set => Id = value; }
		
		  public virtual TKey Id { get; set; }

		  /// <summary>
		  ///     User Id for the user who owns this login
		  /// </summary>
		  public virtual TKey UserId { get; set; }

		  /// <summary>
		  ///     The login provider for the login (i.e. facebook, google)
		  /// </summary>
		  public virtual string LoginProvider { get; set; }

		  /// <summary>
		  ///     Key representing the login for the provider
		  /// </summary>
		  public virtual string ProviderKey { get; set; }

		//public override void Assign(object source, int loadingFlags)
		//{
		//	var src = CastSource(source);
		//	if (src != null)
		//	{
		//		this.Id = src.Key;
		//		this.UserId = src.UserId;
		//		this.LoginProvider = src.LoginProvider;
		//		this.ProviderKey = src.ProviderKey;
		//	}

		//}
	}


}
