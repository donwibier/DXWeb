using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;


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

        public void InitializeUserClaim(XPBaseObject user, Claim claim)
        {            
            this.SetPropertyValue(nameof(User), user);
            base.InitializeFromClaim(claim);
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

        // Created/Updated: DEV-RIG-DON\don on DEV-RIG-DON at 6-1-2024 11:34
        public new class FieldsClass : XpoDxBaseClaim.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }

            public const string UserFieldName = "User";

            public XpoDxUser.FieldsClass User => new XpoDxUser.FieldsClass(GetNestedName(UserFieldName));

            public const string UserIdFieldName = "UserId";

            public OperandProperty UserId => new OperandProperty(GetNestedName(UserIdFieldName));
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

        string IXPUserClaim<string>.UserId 
        { 
            get => this.UserId;
            set => User = Session.GetObjectByKey< XpoDxUser>(value); 
        }

        static FieldsClass _Fields;
    }

}
