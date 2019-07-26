using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace DX.Data.Xpo.Identity.Persistent
{

    public partial class XpoDxUserToken : IXPUserToken<string>
    {
        public XpoDxUserToken(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
		[PersistentAlias("[User!Key]")]
		public string UserId
		{
			get { return (string)(EvaluateAlias("UserId")); }
		}
		// Created/Updated: DESKTOP-KN2LOTV\don on DESKTOP-KN2LOTV at 2/8/2018 3:43 AM
		public new class FieldsClass : XpoDxBase.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }
            public XpoDxUser.FieldsClass User { get { return new XpoDxUser.FieldsClass(GetNestedName("User")); } }
            public OperandProperty LoginProvider { get { return new OperandProperty(GetNestedName("LoginProvider")); } }
            public OperandProperty Name { get { return new OperandProperty(GetNestedName("Name")); } }
            public OperandProperty Value { get { return new OperandProperty(GetNestedName("Value")); } }
            public OperandProperty UserId { get { return new OperandProperty(GetNestedName("UserId")); } }
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
