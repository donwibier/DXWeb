using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace DX.Data.Xpo.Identity.Persistent
{

    public partial class XpoDxUserClaim: IDxUserClaim<string>
    {
        public XpoDxUserClaim(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

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

        #region Embedded Fields class

        public new class FieldsClass : XpoDxBase.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }

            public XpoDxUser.FieldsClass User
            {
                get
                {
                    return new XpoDxUser.FieldsClass(GetNestedName("User"));
                }
            }

            public OperandProperty ClaimType
            {
                get
                {
                    return new OperandProperty(GetNestedName("ClaimType"));
                }
            }

            public OperandProperty ClaimValue
            {
                get
                {
                    return new OperandProperty(GetNestedName("ClaimValue"));
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
