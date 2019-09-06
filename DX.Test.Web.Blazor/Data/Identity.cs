using DevExpress.Xpo;
using DX.Data.Xpo.Identity;
using DX.Data.Xpo.Identity.Persistent;
using System;


namespace DX.Test.Web.Blazor.Data
{
	public class ApplicationUserMapper : XPUserMapper<ApplicationUser, XpoApplicationUser>
	{
		public override XpoApplicationUser Assign(ApplicationUser source, XpoApplicationUser destination)
		{
			var result = base.Assign(source, destination);
			return result;
		}

		public override string Map(string sourceField)
		{
			return base.Map(sourceField);
		}

		public override Func<XpoApplicationUser, ApplicationUser> CreateModel => base.CreateModel;
	}

	// Add profile data for application users by adding properties to the ApplicationUser class
	public class ApplicationUser : XPIdentityUser
	{
		public ApplicationUser()
		{

		}
	}

	public class ApplicationRole : XPIdentityRole
	{
		public ApplicationRole()
		{ }
	}
	public class ApplicationRoleMapper : XPRoleMapper<string, ApplicationRole, XpoApplicationRole>
	{
		public override Func<XpoApplicationRole, ApplicationRole> CreateModel => base.CreateModel;

		public override XpoApplicationRole Assign(ApplicationRole source, XpoApplicationRole destination)
		{
			XpoApplicationRole result = base.Assign(source, destination);
			return result;
		}

		public override string Map(string sourceField)
		{
			return base.Map(sourceField);
		}
	}
	public class XpoApplicationUserMapper : XPUserMapper<ApplicationUser, XpoApplicationUser>
	{
		public override Func<XpoApplicationUser, ApplicationUser> CreateModel => base.CreateModel;
		public override XpoApplicationUser Assign(ApplicationUser source, XpoApplicationUser destination)
		{
			XpoApplicationUser result = base.Assign(source, destination);

			return result;
		}

		public override string Map(string sourceField)
		{
			return base.Map(sourceField);
		}
	}

	// This class will be persisted in the database by XPO
	// It should have the same properties as the ApplicationUser
	[MapInheritance(MapInheritanceType.ParentTable)]
	public class XpoApplicationUser : XpoDxUser
	{
		public XpoApplicationUser(Session session) : base(session)
		{
		}
	}

	[MapInheritance(MapInheritanceType.ParentTable)]
	public class XpoApplicationRole : XpoDxRole
	{
		public XpoApplicationRole(Session session) : base(session)
		{
		}
	}
}
