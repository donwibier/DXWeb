using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DX.Utils.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DX.Data.Xpo.Identity.Persistent
{

	public partial class XpoDxBase
	{
		public XpoDxBase(Session session) : base(session) { }
		public override void AfterConstruction() { base.AfterConstruction(); }
		protected void setId(string s) => _Id = s;

		[PersistentAlias(nameof(Id))]
		public virtual string ID { get => (string)EvaluateAlias(nameof(ID)); }
		//[NonPersistent]
		//public virtual string ID { get => _Id; set => _Id = value; }

		protected override void OnSaving()
		{
			if (String.IsNullOrEmpty(_Id))
				_Id = Guid.NewGuid().ToString();

			if (Session.IsNewObject(this))
				_AddStampUTC = DateTime.UtcNow;
			_ModStampUTC = DateTime.UtcNow;

			base.OnSaving();
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
				get { return new OperandProperty(GetNestedName(nameof(_Id))); }
			}

			public OperandProperty Id
			{
				get { return new OperandProperty(GetNestedName(nameof(Id))); }
			}

			public OperandProperty _AddStampUTC
			{
				get { return new OperandProperty(GetNestedName(nameof(_AddStampUTC))); }
			}

			public OperandProperty AddStampUTC
			{
				get { return new OperandProperty(GetNestedName(nameof(AddStampUTC))); }
			}

			public OperandProperty _ModStampUTC
			{
				get { return new OperandProperty(GetNestedName(nameof(_ModStampUTC))); }
			}

			public OperandProperty ModStampUTC
			{
				get { return new OperandProperty(GetNestedName(nameof(ModStampUTC))); }
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
