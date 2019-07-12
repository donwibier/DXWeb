using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace DX.Data.Xpo.Identity.Persistent
{

    public partial class XpoDxUserClaim: IXPUserClaim<string>
    {
        public XpoDxUserClaim(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        protected override void OnDeleting()
        {
            this.User = null;
            base.OnDeleting();
        }
        //public override void Assign(object source, int loadingFlags)
        //{
        //    base.Assign(source, loadingFlags);
        //    IDxUserClaim<string> src = source as IDxUserClaim<string>;
        //    if (src != null)
        //    {
        //        this.User = Session.GetObjectByKey(typeof(XpoDxUser), src.UserId) as XpoDxUser;
        //    }
        //}

        // Created/Updated: DESKTOP-KN2LOTV\don on DESKTOP-KN2LOTV at 2/9/2018 2:17 AM
        public new class FieldsClass : XpoDxBaseClaim.FieldsClass
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
    }

}
