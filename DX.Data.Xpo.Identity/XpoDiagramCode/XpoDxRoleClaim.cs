using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;

namespace DX.Data.Xpo.Identity.Persistent
{

    public partial class XpoDxRoleClaim : IXPRoleClaim<string>
    {
        public XpoDxRoleClaim(Session session) : base(session) { }

        public override void AfterConstruction() { base.AfterConstruction(); }
        protected override void OnDeleting()
        {
            this.Role = null;
            base.OnDeleting();
        }
        //public override void Assign(object source, int loadingFlags)
        //{
        //    base.Assign(source, loadingFlags);
        //    IDxRoleClaim<string> src = source as IDxRoleClaim<string>;
        //    if (src != null)
        //    {
        //        this.Role = Session.GetObjectByKey(typeof(XpoDxRole), src.RoleId) as XpoDxRole;
        //    }

        //}

        // Created/Updated: DESKTOP-KN2LOTV\don on DESKTOP-KN2LOTV at 2/9/2018 2:16 AM
        public new class FieldsClass : XpoDxBaseClaim.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }

            public XpoDxRole.FieldsClass Role
            {
                get
                {
                    return new XpoDxRole.FieldsClass(GetNestedName("Role"));
                }
            }

            public OperandProperty RoleId
            {
                get
                {
                    return new OperandProperty(GetNestedName("RoleId"));
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
