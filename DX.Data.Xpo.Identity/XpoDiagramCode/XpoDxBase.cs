using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace DX.Data.Xpo.Identity.Persistent
{

    public partial class XpoDxBase : IXPOKey<string>, IXPSimpleObject, IAssignable
    {
        public XpoDxBase(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        public string Key { get { return Id; } }

        protected override void OnSaving()
        {
            if (String.IsNullOrEmpty(_Id))
                _Id = Guid.NewGuid().ToString();

            if (Session.IsNewObject(this))
                _AddStampUTC = DateTime.UtcNow;
            _ModStampUTC = DateTime.UtcNow;

            base.OnSaving();
        }

        public virtual void Assign(object source, int loadingFlags)
        {
            IXPOKey<string> src = source as IXPOKey<string>;
            if (src != null)
            {
                this._Id = src.Key;
            }

        }

        #region Embedded Fields class
        public new class FieldsClass : PersistentBase.FieldsClass
        {
            public FieldsClass()
            {

            }

            public FieldsClass(string propertyName) : base(propertyName)
            {

            }

            public OperandProperty _Id
            {
                get
                {
                    return new OperandProperty(GetNestedName("_Id"));
                }
            }

            public OperandProperty Id
            {
                get
                {
                    return new OperandProperty(GetNestedName("Id"));
                }
            }

            public OperandProperty _AddStampUTC
            {
                get
                {
                    return new OperandProperty(GetNestedName("_AddStampUTC"));
                }
            }

            public OperandProperty AddStampUTC
            {
                get
                {
                    return new OperandProperty(GetNestedName("AddStampUTC"));
                }
            }

            public OperandProperty _ModStampUTC
            {
                get
                {
                    return new OperandProperty(GetNestedName("_ModStampUTC"));
                }
            }

            public OperandProperty ModStampUTC
            {
                get
                {
                    return new OperandProperty(GetNestedName("ModStampUTC"));
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
