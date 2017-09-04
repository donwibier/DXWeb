using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;

namespace DX.Data.Xpo.Identity
{

	public partial class XpoDxRole : IDxRole<string>
	{
		public XpoDxRole(Session session) : base(session) { }
		public override void AfterConstruction() { base.AfterConstruction(); }

		#region Embedded Fields class

		public new class Fields : XpoDxBase.Fields
		{
			protected Fields() { }
			public static OperandProperty Name { get { return new OperandProperty("Name"); } }
			public static OperandProperty NameUpper { get { return new OperandProperty("NameUpper"); } }
			public static OperandProperty Users { get { return new OperandProperty("Users"); } }
		}
		#endregion

		protected override void OnChanged(string propertyName, object oldValue, object newValue)
		{
			if (propertyName == "Name")
				_NameUpper = ((string)newValue ?? "").ToUpperInvariant();

			base.OnChanged(propertyName, oldValue, newValue);
		}

		protected override void OnSaving()
		{
			if (!IsDeleted)
			{
				if (String.IsNullOrEmpty(Name))
					throw new Exception("Name is required");
			}
			base.OnSaving();
		}

		protected override void OnDeleting()
		{
			int userCount = (int)Session.Evaluate(typeof(XpoDxUser), 
				CriteriaOperator.Parse("Count"), 
				CriteriaOperator.Parse("Roles[Id == ?]", this.Id));
			if (userCount > 0)
				throw new Exception(String.Format("Role '{0}' cannot be deleted because there are users in this Role", this.Name));

			base.OnDeleting();
		}

		public override void Assign(object source, int loadingFlags)
		{
			base.Assign(source, loadingFlags);
			IDxRole<string> src = source as IDxRole<string>;
			if (src != null)
			{
				this.Name = src.Name;
				//if (Bits.Has(loadingFlags, DxIdentityUserFlags.FLAG_USERS))										
			}

		}
		public IList UsersList
		{
			get { return Users; }
		}
	}

}
