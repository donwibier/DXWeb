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

	[Persistent(@"DXUsers")]
	public partial class XpoDxUser : XpoDxBase
	{
		string _Email;
		[Indexed(Name = @"IdxEmail")]
		[Size(250)]
		[DevExpress.Xpo.DisplayName(@"E-mail adrress")]
		public string Email
		{
			get { return _Email; }
			set { SetPropertyValue<string>("Email", ref _Email, value); }
		}
		bool _EmailConfirmed;
		public bool EmailConfirmed
		{
			get { return _EmailConfirmed; }
			set { SetPropertyValue<bool>("EmailConfirmed", ref _EmailConfirmed, value); }
		}
		string _PasswordHash;
		[Size(SizeAttribute.Unlimited)]
		public string PasswordHash
		{
			get { return _PasswordHash; }
			set { SetPropertyValue<string>("PasswordHash", ref _PasswordHash, value); }
		}
		string _SecurityStamp;
		[Size(500)]
		public string SecurityStamp
		{
			get { return _SecurityStamp; }
			set { SetPropertyValue<string>("SecurityStamp", ref _SecurityStamp, value); }
		}
		string _PhoneNumber;
		[Size(50)]
		[DevExpress.Xpo.DisplayName(@"Phonenumber")]
		public string PhoneNumber
		{
			get { return _PhoneNumber; }
			set { SetPropertyValue<string>("PhoneNumber", ref _PhoneNumber, value); }
		}
		bool _PhoneNumberConfirmed;
		public bool PhoneNumberConfirmed
		{
			get { return _PhoneNumberConfirmed; }
			set { SetPropertyValue<bool>("PhoneNumberConfirmed", ref _PhoneNumberConfirmed, value); }
		}
		bool _TwoFactorEnabled;
		public bool TwoFactorEnabled
		{
			get { return _TwoFactorEnabled; }
			set { SetPropertyValue<bool>("TwoFactorEnabled", ref _TwoFactorEnabled, value); }
		}
		DateTime? _LockoutEndDateUtc;
		public DateTime? LockoutEndDateUtc
		{
			get { return _LockoutEndDateUtc; }
			set { SetPropertyValue<DateTime?>("LockoutEndDateUtc", ref _LockoutEndDateUtc, value); }
		}
		bool _LockoutEnabled;
		public bool LockoutEnabled
		{
			get { return _LockoutEnabled; }
			set { SetPropertyValue<bool>("LockoutEnabled", ref _LockoutEnabled, value); }
		}
		int _AccessFailedCount;
		public int AccessFailedCount
		{
			get { return _AccessFailedCount; }
			set { SetPropertyValue<int>("AccessFailedCount", ref _AccessFailedCount, value); }
		}
		string _UserName;
		[Indexed(Name = @"IdxUserName", Unique = true)]
		[Size(250)]
		[DevExpress.Xpo.DisplayName(@"Username")]
		public string UserName
		{
			get { return _UserName; }
			set { SetPropertyValue<string>("UserName", ref _UserName, value); }
		}
		[Indexed(Name = @"IdxEmailUpper")]
		[Size(250)]
		[Persistent(@"EmailUpper")]
		string _EmailUpper;
		[PersistentAlias("_EmailUpper")]
		public string EmailUpper
		{
			get { return _EmailUpper; }
		}
		[Indexed(Name = @"IdxUserNameUpper", Unique = true)]
		[Size(250)]
		[Persistent(@"UserNameUpper")]
		string _UserNameUpper;
		[PersistentAlias("_UserNameUpper")]
		public string UserNameUpper
		{
			get { return _UserNameUpper; }
		}
		string _NormalizedEmail;
		[Indexed(Name = @"IdxNormalizedEmail")]
		public string NormalizedEmail
		{
			get { return _NormalizedEmail; }
			set { SetPropertyValue<string>("NormalizedEmail", ref _NormalizedEmail, value); }
		}
		string _NormalizedUserName;
		[Indexed(Name = @"IdxNormalizedName")]
		[Persistent(@"NormalizedName")]
		public string NormalizedUserName
		{
			get { return _NormalizedUserName; }
			set { SetPropertyValue<string>("NormalizedUserName", ref _NormalizedUserName, value); }
		}
		string _RefreshToken;
		[Size(SizeAttribute.Unlimited)]
		public string RefreshToken
		{
			get { return _RefreshToken; }
			set { SetPropertyValue<string>("RefreshToken", ref _RefreshToken, value); }
		}
		DateTime? _RefreshTokenExpiryTime;
		public DateTime? RefreshTokenExpiryTime
		{
			get { return _RefreshTokenExpiryTime; }
			set { SetPropertyValue<DateTime?>("RefreshTokenExpiryTime", ref _RefreshTokenExpiryTime, value); }
		}
		string _ConcurrencyStamp;
		[Size(60)]
		public string ConcurrencyStamp
		{
			get { return _ConcurrencyStamp; }
			set { SetPropertyValue<string>("ConcurrencyStamp", ref _ConcurrencyStamp, value); }
		}
		[Association(@"XpoDxUsersRoles")]
		public XPCollection<XpoDxRole> Roles { get { return GetCollection<XpoDxRole>("Roles"); } }
		[Association(@"XpoDxUserLogins"), Aggregated]
		public XPCollection<XpoDxUserLogin> Logins { get { return GetCollection<XpoDxUserLogin>("Logins"); } }
		[Association(@"XpoDxUserTokenReferencesXpoDxUser"), Aggregated]
		public XPCollection<XpoDxUserToken> Tokens { get { return GetCollection<XpoDxUserToken>("Tokens"); } }
		[Association(@"XpoDxUserClaimReferencesXpoDxUser"), Aggregated]
		public XPCollection<XpoDxUserClaim> Claims { get { return GetCollection<XpoDxUserClaim>("Claims"); } }
	}

}
