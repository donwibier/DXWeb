using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace DX.Data.Xpo.Identity
{

	public partial class XpoDxBase : IXPOKey<string>, IXPSimpleObject, IAssignable
	{
		public XpoDxBase(Session session) : base(session) { }
		public override void AfterConstruction() { base.AfterConstruction(); }

		public string Key { get { return Id; } }
		#region Fields class

		public new class Fields
		{
			protected Fields() { }
			public static OperandProperty Id { get { return new OperandProperty("Id"); } }
		}

		#endregion

		protected override void OnSaving()
		{
			if (String.IsNullOrEmpty(_Id))
				_Id = Guid.NewGuid().ToString();

			if (Session.IsNewObject(this))
				_AddStampUTC = DateTime.UtcNow;
			_ModStampUTC = DateTime.UtcNow;

			base.OnSaving();
		}

		public virtual void Assign(object source, int loadingFlags)
		{
			IXPOKey<string> src = source as IXPOKey<string>;
			if (src != null)
			{
				this._Id = src.Key;
			}

		}
	}

}
