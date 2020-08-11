using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace DX.Data.Xpo.IndentityServer.XPModels
{

	public partial class XpoDeviceCode
	{
		public XpoDeviceCode(Session session) : base(session) { }
		public override void AfterConstruction() { base.AfterConstruction(); }
	}

}
