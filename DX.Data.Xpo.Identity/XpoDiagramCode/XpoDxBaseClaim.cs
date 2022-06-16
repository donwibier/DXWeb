using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;


namespace DX.Data.Xpo.Identity.Persistent
{

    public partial class XpoDxBaseClaim : IXPBaseClaim<string>
    {
        public XpoDxBaseClaim(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
       
        public virtual void InitializeFromClaim(Claim other)
        {
            this.ClaimType = other.Type;
            this.ClaimValue = other.Value;
        }

        public virtual Claim ToClaim()
        {
            return new Claim(this.ClaimType, this.ClaimValue);
        }

        // Created/Updated: DEV-RIG-DON\don on DEV-RIG-DON at 6/16/2022 10:34 AM
        public new class FieldsClass : XpoDxBase.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }

            public const string ClaimTypeFieldName = "ClaimType";

            public OperandProperty ClaimType => new OperandProperty(GetNestedName(ClaimTypeFieldName));

            public const string ClaimValueFieldName = "ClaimValue";

            public OperandProperty ClaimValue => new OperandProperty(GetNestedName(ClaimValueFieldName));
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
