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

        public void InitializeRoleClaim(XPBaseObject role, Claim claim)
        {
            SetPropertyValue(nameof(Role), role);
            base.InitializeFromClaim(claim);
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

        // Created/Updated: DEV-RIG-DON\don on DEV-RIG-DON at 6-1-2024 11:34
        public new class FieldsClass : XpoDxBaseClaim.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }

            public const string RoleFieldName = "Role";

            public XpoDxRole.FieldsClass Role => new XpoDxRole.FieldsClass(GetNestedName(RoleFieldName));

            public const string RoleIdFieldName = "RoleId";

            public OperandProperty RoleId => new OperandProperty(GetNestedName(RoleIdFieldName));
        }
        public new static FieldsClass Fields { get => _Fields; }

        static FieldsClass _Fields = new FieldsClass();


		string IXPRoleClaim<string>.RoleId 
        { 
            get => this.RoleId; 
            set => Role = Session.GetObjectByKey<XpoDxRole>(value); 
        }
    }

}
