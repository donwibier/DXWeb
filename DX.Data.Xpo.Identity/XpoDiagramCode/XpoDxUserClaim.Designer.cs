﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace DX.Data.Xpo.Identity.Persistent
{

	[Persistent(@"DXUserClaims")]
	[MapInheritance(MapInheritanceType.ParentTable)]
	public partial class XpoDxUserClaim : XpoDxBaseClaim
	{
		XpoDxUser _User;
		[Association(@"XpoDxUserClaimReferencesXpoDxUser")]
		public XpoDxUser User
		{
			get { return _User; }
			set { SetPropertyValue<XpoDxUser>("User", ref _User, value); }
		}
		[PersistentAlias("[User.Id]")]
		public string UserId
		{
			get { return (string)(EvaluateAlias("UserId")); }
		}
	}

}
