using DevExpress.Xpo;

using System;
using System.Linq;


namespace DX.Data.Xpo.Identity
{
	[Obsolete("For legacy reasons only. Use DX.Data.AutoMapper or DX.Data.Mapster descendants")]
	public class XPUserMapper<TUser, TXPOUser> : XPUserMapper<string, TUser, TXPOUser>
	 where TUser : class, IXPUser<string>, new()
	 where TXPOUser : XPBaseObject, IXPUser<string>
	{
	}
	[Obsolete("For legacy reasons only. Use DX.Data.AutoMapper or DX.Data.Mapster descendants")]
	public class XPUserMapper<TKey, TUser, TXPOUser> : XPDataMapper<TKey, TUser, TXPOUser>
		 where TKey : IEquatable<TKey>
		 where TUser : class, IXPUser<TKey>, new()
		 where TXPOUser : XPBaseObject, IXPUser<TKey>
	{
		public override Func<TXPOUser, TUser> CreateModel => (source) => new TUser
		{
			Id = source.Id,
			UserName = source.UserName,
			Email = source.Email,
			EmailConfirmed = source.EmailConfirmed,
			PasswordHash = source.PasswordHash,
			SecurityStamp = source.SecurityStamp,
			PhoneNumber = source.PhoneNumber,
			PhoneNumberConfirmed = source.PhoneNumberConfirmed,
			TwoFactorEnabled = source.TwoFactorEnabled,
			LockoutEndDateUtc = source.LockoutEndDateUtc,
			LockoutEnabled = source.LockoutEnabled,
			NormalizedUserName = source.NormalizedUserName,
			NormalizedEmail = source.NormalizedEmail,
			RefreshToken = source.RefreshToken,
			RefreshTokenExpiryTime = source.RefreshTokenExpiryTime,
			AccessFailedCount = source.AccessFailedCount
			
		};

		public override TXPOUser Assign(TUser source, TXPOUser destination)
		{
			//destination.Id = source.Id;
			destination.UserName = source.UserName;
			destination.Email = source.Email;
			destination.EmailConfirmed = source.EmailConfirmed;
			destination.PasswordHash = source.PasswordHash;
			destination.SecurityStamp = source.SecurityStamp;
			destination.PhoneNumber = source.PhoneNumber;
			destination.PhoneNumberConfirmed = source.PhoneNumberConfirmed;
			destination.TwoFactorEnabled = source.TwoFactorEnabled;
			destination.LockoutEndDateUtc = source.LockoutEndDateUtc;
			destination.LockoutEnabled = source.LockoutEnabled;
			destination.AccessFailedCount = source.AccessFailedCount;
			destination.NormalizedUserName = source.NormalizedUserName;
			destination.NormalizedEmail = source.NormalizedEmail;
			destination.RefreshToken = source.RefreshToken;
			destination.RefreshTokenExpiryTime = source.RefreshTokenExpiryTime;
			if (destination is IAssignable)
				((IAssignable)destination)!.Assign(source);

			return destination;
		}

		public override string Map(string sourceField)
		{
			return sourceField;
		}

	}
}
