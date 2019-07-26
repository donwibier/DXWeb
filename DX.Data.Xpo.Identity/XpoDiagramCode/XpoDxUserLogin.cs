using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace DX.Data.Xpo.Identity.Persistent
{

    public partial class XpoDxUserLogin : IXPUserLogin<string>
    {
        public XpoDxUserLogin(Session session) : base(session) { }

		[PersistentAlias("[User!Key]")]
		public string UserId
		{
			get { return (string)(EvaluateAlias("UserId")); }
		}
		public override void AfterConstruction() { base.AfterConstruction(); }

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
        //public override void Assign(object source, int loadingFlags)
        //{
        //    base.Assign(source, loadingFlags);
        //    IDxUserLogin<string> src = source as IDxUserLogin<string>;
        //    if (src != null)
        //    {
        //        this.LoginProvider = src.LoginProvider;
        //        this.ProviderKey = src.ProviderKey;
        //        this.User = Session.FindObject(typeof(XpoDxUser), XpoDxUser.Fields.Id == src.UserId) as XpoDxUser;

        //    }
        //}

        #region Embedded Fields class
        public new class FieldsClass : XpoDxBase.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }

            public OperandProperty LoginProvider
            {
                get
                {
                    return new OperandProperty(GetNestedName("LoginProvider"));
                }
            }

            public OperandProperty ProviderKey
            {
                get
                {
                    return new OperandProperty(GetNestedName("ProviderKey"));
                }
            }

            public XpoDxUser.FieldsClass User
            {
                get
                {
                    return new XpoDxUser.FieldsClass(GetNestedName("User"));
                }
            }

            public OperandProperty UserId
            {
                get
                {
                    return new OperandProperty(GetNestedName("UserId"));
                }
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
