using DevExpress.Data.Filtering;
using DevExpress.Xpo;
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
		// Created/Updated: DEV-RIG-DON\don on DEV-RIG-DON at 11-1-2024 10:01
		public new class FieldsClass : PersistentBase.FieldsClass
		{
			public FieldsClass()
			{

			}

			/// <doc>
			/// <assembly>
			/// <name>DevExpress.Xpo.v23.2</name>
			/// </assembly>
			/// <members>
			/// <member name="N:DevExpress.Xpo">
			/// <summary>
			/// <para>Contains classes that support the infrastructure of the eXpress Persistent Objects.</para>
			/// </summary>
			/// </member>
			/// <member name="T:DevExpress.Xpo.AggregatedAttribute">
			/// <summary>
			/// <para>Indicates that persistent objects referenced by the target property are aggregated.</para>
			/// </summary>
			/// </member>
			public FieldsClass(string propertyName) : base(propertyName)
			{

			}

			public const string IDFieldName = "ID";

			public OperandProperty ID => new OperandProperty(GetNestedName(IDFieldName));

			public const string _IdFieldName = "_Id";

			public OperandProperty _Id => new OperandProperty(GetNestedName(_IdFieldName));

			public const string IdFieldName = "Id";

			public OperandProperty Id => new OperandProperty(GetNestedName(IdFieldName));

			public const string _AddStampUTCFieldName = "_AddStampUTC";

			public OperandProperty _AddStampUTC => new OperandProperty(GetNestedName(_AddStampUTCFieldName));

			public const string AddStampUTCFieldName = "AddStampUTC";

			public OperandProperty AddStampUTC => new OperandProperty(GetNestedName(AddStampUTCFieldName));

			public const string _ModStampUTCFieldName = "_ModStampUTC";

			public OperandProperty _ModStampUTC => new OperandProperty(GetNestedName(_ModStampUTCFieldName));

			public const string ModStampUTCFieldName = "ModStampUTC";

			public OperandProperty ModStampUTC => new OperandProperty(GetNestedName(ModStampUTCFieldName));
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
