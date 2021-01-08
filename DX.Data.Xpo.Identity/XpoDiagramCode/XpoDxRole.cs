using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace DX.Data.Xpo.Identity.Persistent
{

	public partial class XpoDxRole : IXPRole<string>
	{
		public XpoDxRole(Session session) : base(session) { }
		public override void AfterConstruction() { base.AfterConstruction(); }
		//#if (NETSTANDARD2_1)
		//        public string NormalizedName
		//        {
		//            get { return NameUpper; }
		//            set { _NameUpper = (value??"").ToUpperInvariant(); }
		//        }
		//#endif       
		protected override void OnChanged(string propertyName, object oldValue, object newValue)
		{
			if (propertyName == nameof(Name))
				_NameUpper = ((string)newValue ?? string.Empty).ToUpperInvariant();

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
			var users = new XPCollection<XpoDxUser>(this.Session, CriteriaOperator.Parse("Roles[Id == ?]", Id), null);
			foreach (var u in users)
			{
				Users.Remove(u);
				if (!(Session is UnitOfWork))
					u.Save();
			}
			Session.Delete(new XPCollection<XpoDxRoleClaim>(this.Session, CriteriaOperator.Parse("Role.Id == ?", Id), null));
			//int userCount = (int)Session.Evaluate(typeof(XpoDxUser),
			//    CriteriaOperator.Parse("Count"),
			//    CriteriaOperator.Parse("Roles[Id == ?]", this.Id));
			//if (userCount > 0)
			//    throw new Exception(String.Format("Role '{0}' cannot be deleted because there are users in this Role", this.Name));

			base.OnDeleting();
		}

		//        public override void Assign(object source, int loadingFlags)
		//        {
		//            base.Assign(source, loadingFlags);
		//            IDxRole<string> src = source as IDxRole<string>;
		//            if (src != null)
		//            {
		//                this.Name = src.Name;
		//#if (NETSTANDARD2_1)
		//                this.NormalizedName = src.NormalizedName;
		//#endif
		//                //if (Bits.Has(loadingFlags, DxIdentityUserFlags.FLAG_USERS))										
		//            }

		//        }
		public IList UsersList
		{
			get { return Users; }
		}

		#region Embedded Fields class
		public new class FieldsClass : XpoDxBase.FieldsClass
		{
			public FieldsClass()
			{

			}

			public FieldsClass(string propertyName) : base(propertyName)
			{

			}

			public OperandProperty UsersList
			{
				get { return new OperandProperty(GetNestedName(nameof(UsersList))); }
			}

			public OperandProperty Name
			{
				get { return new OperandProperty(GetNestedName(nameof(Name))); }
			}

			public OperandProperty _NameUpper
			{
				get { return new OperandProperty(GetNestedName(nameof(_NameUpper))); }
			}

			public OperandProperty NameUpper
			{
				get { return new OperandProperty(GetNestedName(nameof(NameUpper))); }
			}

			public OperandProperty Users
			{
				get { return new OperandProperty(GetNestedName(nameof(Users))); }
			}
		}

		public new static FieldsClass Fields
		{
			get
			{
				if (ReferenceEquals(_Fields, null))
				{
					_Fields = new FieldsClass();
				}

				return _Fields;
			}
		}

		static FieldsClass _Fields;

		#endregion

	}

}
