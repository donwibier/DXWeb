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

		string IXPRole<string>.Id { get => base.Id; set => setId(value); }
        //string IXPRole<string>.Name { get => Name; set => Name = value; }

        public IList ClaimsList { get { return Claims; } }

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
			var users = new XPCollection<XpoDxUser>(Session, CriteriaOperator.Parse("Roles[Id == ?]", Id), null);
			foreach (var u in users)
			{
				Users.Remove(u);
				if (!(Session is UnitOfWork))
					u.Save();
			}
			Session.Delete(new XPCollection<XpoDxRoleClaim>(Session, CriteriaOperator.Parse("Role.Id == ?", Id), null));
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
		//#if (NETSTANDARD2_1 || NETCOREAPP)
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
		// Created/Updated: DEV-RIG-DON\don on DEV-RIG-DON at 10-1-2024 14:27
		public new class FieldsClass : XpoDxBase.FieldsClass
		{
			public FieldsClass()
			{

			}

			/// <!-- Badly formed XML comment ignored for member "M:DX.Data.Xpo.Identity.Persistent.XpoDxBase.FieldsClass.#ctor(System.String)" -->
			public FieldsClass(string propertyName) : base(propertyName)
			{

			}

			public const string NameFieldName = "Name";

			public OperandProperty Name => new OperandProperty(GetNestedName(NameFieldName));

			public const string _NameUpperFieldName = "_NameUpper";

			public OperandProperty _NameUpper => new OperandProperty(GetNestedName(_NameUpperFieldName));

			public const string NameUpperFieldName = "NameUpper";

			public OperandProperty NameUpper => new OperandProperty(GetNestedName(NameUpperFieldName));

			public const string NormalizedNameFieldName = "NormalizedName";

			public OperandProperty NormalizedName => new OperandProperty(GetNestedName(NormalizedNameFieldName));

			public const string UsersFieldName = "Users";

			public OperandProperty Users => new OperandProperty(GetNestedName(UsersFieldName));

			public const string ClaimsFieldName = "Claims";

			public OperandProperty Claims => new OperandProperty(GetNestedName(ClaimsFieldName));

			public const string ClaimsListFieldName = "ClaimsList";

			public OperandProperty ClaimsList => new OperandProperty(GetNestedName(ClaimsListFieldName));

			public const string UsersListFieldName = "UsersList";

			public OperandProperty UsersList => new OperandProperty(GetNestedName(UsersListFieldName));
		}

        public new static FieldsClass Fields { get => _Fields; }

        static readonly FieldsClass _Fields = new FieldsClass();

        #endregion

    }

}
