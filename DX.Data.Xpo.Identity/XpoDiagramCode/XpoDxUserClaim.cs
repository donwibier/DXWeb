using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace DX.Data.Xpo.Identity
{

	public partial class XpoDxUserClaim : IDxUserClaim<string>
	{
		public XpoDxUserClaim(Session session) : base(session) { }
		public override void AfterConstruction() { base.AfterConstruction(); }

		#region Fields class
		public new class Fields : XpoDxBase.Fields
		{
			protected Fields() { }

			public static OperandProperty ClaimType { get { return new OperandProperty("ClaimType"); } }
			public static OperandProperty ClaimValue { get { return new OperandProperty("ClaimValue"); } }
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
				if (String.IsNullOrEmpty(ClaimType))
					throw new Exception("ClaimType is required");
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
			IDxUserClaim<string> src = source as IDxUserClaim<string>;
			if (src != null)
			{
				this.ClaimType = src.ClaimType;
				this.ClaimValue = src.ClaimValue;
				this.User = Session.FindObject(typeof(XpoDxUser), XpoDxUser.Fields.Id == src.UserId) as XpoDxUser;
				//this.UserId = src.UserId;

			}
		}
	}

}
