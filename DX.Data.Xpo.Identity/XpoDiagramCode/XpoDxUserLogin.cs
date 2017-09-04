using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace DX.Data.Xpo.Identity
{

	public partial class XpoDxUserLogin : IDxUserLogin<string>
	{
		public XpoDxUserLogin(Session session) : base(session) { }
		public override void AfterConstruction() { base.AfterConstruction(); }

		#region Fields class
		public new class Fields : XpoDxBase.Fields
		{
			protected Fields() { }

			public static OperandProperty LoginProvider { get { return new OperandProperty("LoginProvider"); } }
			public static OperandProperty ProviderKey { get { return new OperandProperty("ProviderKey"); } }
			public static OperandProperty User { get { return new OperandProperty("User"); } }
			public static OperandProperty UserId { get { return new OperandProperty("UserId"); } }
		}
		#endregion

		protected override void OnSaving()
		{
			if (!IsDeleted)
			{
				if (User == null)
					throw new Exception("User cannot be null");
				if (String.IsNullOrEmpty(LoginProvider))
					throw new Exception("LoginProvider is required");
			}
			base.OnSaving();
		}
		protected override void OnDeleting()
		{
			User = null;
			base.OnDeleting();
		}
		public override void Assign(object source, int loadingFlags)
		{
			base.Assign(source, loadingFlags);
			IDxUserLogin<string> src = source as IDxUserLogin<string>;
			if (src != null)
			{
				this.LoginProvider = src.LoginProvider;
				this.ProviderKey = src.ProviderKey;
				this.User = Session.FindObject(typeof(XpoDxUser), XpoDxUser.Fields.Id == src.UserId) as XpoDxUser;

			}
		}
	}

}
