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

        //public override void Assign(object source, int loadingFlags)
        //{
        //    base.Assign(source, loadingFlags);
        //    IDxBaseClaim<string> src = source as IDxBaseClaim<string>;
        //    if (src != null)
        //    {
        //        this.ClaimType = src.ClaimType;
        //        this.ClaimValue = src.ClaimValue;
        //    }
        //}

        public virtual void InitializeFromClaim(Claim other)
        {
            this.ClaimType = other.Type;
            this.ClaimValue = other.Value;
        }

        public virtual Claim ToClaim()
        {
            return new Claim(this.ClaimType, this.ClaimValue);
        }

        // Created/Updated: DESKTOP-KN2LOTV\don on DESKTOP-KN2LOTV at 2/9/2018 2:16 AM
        public new class FieldsClass : XpoDxBase.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

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
